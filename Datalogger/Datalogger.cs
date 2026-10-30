using System.IO.Ports;

namespace Datalogger
{
    public partial class Datalogger : Form
    {
        public partial class SerialData : Form
        {
            static readonly byte[] IdRequest = { 0x24, 0x49, 0x00 };
            static readonly byte[] IdReply = { 0x24, 0x49, 0x02, 0x33, 0x75 };
            static readonly byte[] AdcRequest = { 0x24, 0x41, 0x01, 0x00 };

            SerialPort _serialPort = new SerialPort();
            byte[] rxBuffer = new byte[11];
            int rxCount = 0;
            bool waiting = false, verified = false, scanning = false, centerSent = false;
            volatile bool closing = false;
            byte expectedReply = 0;
            int pendingServo = -1, lastSliderAngle = 90;
            double measuredVref = 5.00;            // CHANGE to your measured reference voltage
            DateTime deadline, nextScan = DateTime.MinValue,
                     nextADC = DateTime.MinValue, nextPortCheck = DateTime.MinValue;

            public SerialData()
            {
                InitializeComponent();

                StatusTimer.Interval = 20;
                StatusTimer.Tick += StatusTimer_Tick;

                PortsComboBox.Enabled = false;
                ConnectButton.Enabled = false;
                FramingErrorButton.Enabled = false;
                StatusLabel.Text = "COM ERR";
                TemperatureLabel.Text = "--- °F";

                ServoTrackBar.Enabled = false;
                ServoDegreeLabel.Text = "90°";

                ServoTrackBar.Scroll += ServoTrackBar_Scroll;
                FormClosing += SerialData_FormClosing;
                StatusTimer.Start();
            }

            // ---------- Logging ----------

            void AddToList(string dir, byte[] data, int count, string note,
                           string port = null, DateTime? time = null)
            {
                string hex = count > 0 ? BitConverter.ToString(data, 0, count).Replace("-", " ") : "";
                port ??= _serialPort.IsOpen ? _serialPort.PortName : "none";
                ComListBox.Items.Add($"{(time ?? DateTime.Now):HH:mm:ss.fff}  {port}  {dir}  {hex}  {note}");
                ComListBox.TopIndex = ComListBox.Items.Count - 1;
            }

            void Info(string note, string port = null) => AddToList("INFO", new byte[0], 0, note, port);

            // Thread-safe version for the discovery thread
            void LogScan(string dir, byte[] data, int count, string note, string port)
            {
                DateTime time = DateTime.Now;
                if (closing || !IsHandleCreated) return;
                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        if (!closing) AddToList(dir, data, count, note, port, time);
                    }));
                }
                catch (InvalidOperationException) { }
            }

            // ---------- Sending ----------

            void SendBytes(byte[] data)
            {
                if (!verified || !_serialPort.IsOpen || waiting || data.Length == 0) return;

                try
                {
                    _serialPort.Write(data, 0, data.Length);

                    string note = "Custom request";
                    if (data.Length == 3 && data[1] == 0x49) note = "Device ID request";
                    if (data.Length == 4 && data[1] == 0x41) note = "AN0 request";
                    AddToList("TX", data, data.Length, note);

                    expectedReply = data.Length > 1 ? data[1] : (byte)0;
                    waiting = true;
                    deadline = DateTime.Now.AddMilliseconds(500);
                }
                catch (Exception ex) { LostConnection("Send failed: " + ex.Message); }
            }

            void SerialSend()
            {
                string[] parts = OutputTextBox.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                byte[] data = new byte[parts.Length];

                try
                {
                    for (int i = 0; i < parts.Length; i++) data[i] = Convert.ToByte(parts[i], 16);
                }
                catch (Exception) { MessageBox.Show("Type hex like: 24 49 00"); return; }

                if (data.Length < 3 || data[0] != 0x24 || data[2] > 8 || data.Length != 3 + data[2])
                {
                    MessageBox.Show("Enter one complete packet, like: 24 49 00");
                    return;
                }
                if (data[1] == 0x53)
                {
                    MessageBox.Show("Use the servo slider to send servo commands.");
                    return;
                }
                SendBytes(data);
            }

            void SendServo(int angle)
            {
                if (!verified || !_serialPort.IsOpen) return;
                if (waiting) { pendingServo = angle; return; }

                int pulseWidth = (int)Math.Round(500 + angle * 2000.0 / 180.0);
                int match = 0xB1E0 + pulseWidth;
                byte[] packet = { 0x24, 0x53, 0x02, (byte)(match >> 8), (byte)(match & 0xFF) };

                try
                {
                    _serialPort.Write(packet, 0, packet.Length);
                    AddToList("TX", packet, packet.Length, $"Servo {angle}° = {pulseWidth} us");
                }
                catch (Exception ex) { LostConnection("Servo send failed: " + ex.Message); }
            }

            // ---------- Decoding ----------

            string Decode()
            {
                byte cmd = rxBuffer[1], len = rxBuffer[2];

                if (cmd == 0x49)
                {
                    if (len != 2) return "Invalid Device ID length";
                    int id = rxBuffer[3] * 256 + rxBuffer[4];
                    return (id != 0x3375 ? "Wrong Device ID = 0x" : "Device ID = 0x") + id.ToString("X4");
                }

                if (cmd == 0x41)
                {
                    if (len != 3 || rxBuffer[3] != 0) return "Invalid AN0 reply";
                    int adc = rxBuffer[4] * 256 + rxBuffer[5];
                    if (adc > 1023) return "Invalid 10-bit ADC value";

                    // Lab formula: V = ADC * Vref / 1024, LM34 = 10 mV per °F
                    double f = adc * (measuredVref / 1024.0) / 0.010;

                    if (waiting && expectedReply == 0x41)
                    {
                        TemperatureLabel.Text = f.ToString("F1") + " °F";
                        ADCButton.Text = "ADC: " + adc;
                    }
                    return $"AN0 ADC = {adc}, Temperature = {f:F1} °F";
                }

                if (cmd == 0x45)
                {
                    if (len != 2) return "Invalid error packet";
                    if (rxBuffer[3] == 1) return "FRAMING ERROR, detail 0x" + rxBuffer[4].ToString("X2");
                    if (rxBuffer[3] == 2) return "OVERRUN ERROR";
                    return "PIC error code " + rxBuffer[3];
                }

                return "Unknown reply";
            }

            // Build packets one byte at a time (works across multiple reads)
            void ReceiveByte(byte value)
            {
                if (rxCount == 0 && value != 0x24)
                {
                    AddToList("RX", new[] { value }, 1, "Unexpected byte");
                    return;
                }

                rxBuffer[rxCount++] = value;

                if (rxCount == 3 && rxBuffer[2] > 8)
                {
                    AddToList("RX", rxBuffer, rxCount, "Invalid packet length");
                    rxCount = 0;
                    return;
                }

                if (rxCount < 3 || rxCount < 3 + rxBuffer[2]) return;

                string meaning = Decode();
                AddToList("RX", rxBuffer, rxCount, meaning);

                bool error = rxBuffer[1] == 0x45 && rxBuffer[2] == 2;
                bool matches = waiting && rxBuffer[1] == expectedReply;
                bool valid = true;

                if (rxBuffer[1] == 0x49)
                    valid = rxBuffer[2] == 2 && rxBuffer[3] == 0x33 && rxBuffer[4] == 0x75;
                if (rxBuffer[1] == 0x41)
                    valid = rxBuffer[2] == 3 && rxBuffer[3] == 0 &&
                            (rxBuffer[4] * 256 + rxBuffer[5]) <= 1023;

                rxCount = 0;

                if (error) { LostConnection(meaning); return; }
                if (matches && valid) { waiting = false; expectedReply = 0; }
            }

            // ---------- Automatic PIC discovery (background thread) ----------

            // Returns the open port if it answers with the exact ID reply, otherwise null
            SerialPort TryPort(string name)
            {
                SerialPort p = null;
                bool accepted = false;

                try
                {
                    p = new SerialPort(name, 9600, Parity.None, 8, StopBits.One)
                    { ReadTimeout = 50, WriteTimeout = 500 };

                    p.Open();
                    Thread.Sleep(100);                     // let the adapter settle after opening
                    LogScan("INFO", new byte[0], 0, "Checking for PIC", name);

                    // Two attempts: the first can be spoiled by line noise right after opening
                    for (int attempt = 0; attempt < 2 && !closing && !accepted; attempt++)
                    {
                        p.DiscardInBuffer();
                        p.Write(IdRequest, 0, IdRequest.Length);
                        LogScan("TX", IdRequest, IdRequest.Length, "Device ID check", name);

                        byte[] rx = new byte[11];
                        int n = 0;
                        DateTime end = DateTime.UtcNow.AddMilliseconds(500);

                        while (DateTime.UtcNow < end && n < 5 && !closing)
                        {
                            try { rx[n++] = (byte)p.ReadByte(); }
                            catch (TimeoutException) { }
                        }

                        if (n == 5)                        // catch any extra trailing bytes
                        {
                            Thread.Sleep(50);
                            while (p.BytesToRead > 0 && n < rx.Length) rx[n++] = (byte)p.ReadByte();
                        }

                        bool ok = n == IdReply.Length && rx.AsSpan(0, n).SequenceEqual(IdReply);

                        if (n > 0)
                            LogScan("RX", rx, n, ok ? "Device ID = 0x3375" : "Unexpected Device ID reply", name);

                        if (ok && !closing)
                        {
                            LogScan("INFO", new byte[0], 0, "PIC verified: 0x3375", name);
                            accepted = true;
                        }
                    }

                    if (!accepted)
                        LogScan("INFO", new byte[0], 0, "Rejected: wrong or missing Device ID", name);
                }
                catch (Exception ex)
                {
                    LogScan("INFO", new byte[0], 0, "Rejected: " + ex.Message, name);
                }
                finally
                {
                    if (!accepted && p != null)
                    {
                        try { p.Close(); p.Dispose(); } catch (Exception) { }
                    }
                }

                return accepted ? p : null;
            }

            SerialPort FindPIC()
            {
                string[] ports;
                try { ports = SerialPort.GetPortNames(); }
                catch (Exception ex)
                {
                    LogScan("INFO", new byte[0], 0, "Port scan failed: " + ex.Message, "none");
                    return null;
                }

                if (ports.Length == 0)
                    LogScan("INFO", new byte[0], 0, "No serial ports found", "none");

                foreach (string name in ports)
                {
                    if (closing) return null;
                    SerialPort found = TryPort(name);
                    if (found != null) return found;
                }
                return null;
            }

            async void StartDiscovery()
            {
                if (scanning || verified || closing) return;
                scanning = true;

                SerialPort found = null;
                try { found = await Task.Run(FindPIC); }
                catch (Exception ex) { Info("Discovery failed: " + ex.Message); }

                scanning = false;

                if (closing)
                {
                    found?.Close();
                    found?.Dispose();
                    return;
                }

                if (found == null)
                {
                    StatusLabel.Text = "COM ERR";
                    nextScan = DateTime.Now.AddSeconds(1);
                    return;
                }

                try { _serialPort.Close(); _serialPort.Dispose(); } catch (Exception) { }
                _serialPort = found;

                verified = true;
                waiting = false;
                expectedReply = 0;
                rxCount = 0;
                pendingServo = -1;

                PortsComboBox.Items.Clear();
                PortsComboBox.Items.Add(found.PortName);
                PortsComboBox.SelectedIndex = 0;

                StatusLabel.Text = "CONNECTED: " + found.PortName;
                Info("Connected to verified PIC");

                // Centered servo command only once per application run
                if (!centerSent)
                {
                    centerSent = true;
                    SendServo(90);
                }

                nextADC = DateTime.Now.AddSeconds(1);
                nextPortCheck = DateTime.Now.AddSeconds(1);
            }

            // ---------- Communication loss ----------

            void LostConnection(string reason)
            {
                Info("Disconnected: " + reason, _serialPort.PortName);

                try { _serialPort.Close(); } catch (Exception) { }

                verified = false;
                waiting = false;
                expectedReply = 0;
                rxCount = 0;
                pendingServo = -1;

                StatusLabel.Text = "COM ERR";
                TemperatureLabel.Text = "--- °F";
                ADCButton.Text = "ADC: ---";
                PortsComboBox.Items.Clear();

                nextScan = DateTime.Now.AddSeconds(1);
            }

            // ---------- Timer: every 20 ms, never blocks ----------

            private void StatusTimer_Tick(object sender, EventArgs e)
            {
                if (closing) return;

                if (!verified)
                {
                    StatusLabel.Text = "COM ERR";
                    if (!scanning && DateTime.Now >= nextScan) StartDiscovery();
                    UpdateButtons();
                    return;
                }

                try
                {
                    if (!_serialPort.IsOpen) { LostConnection("Port closed"); UpdateButtons(); return; }

                    if (DateTime.Now >= nextPortCheck)
                    {
                        nextPortCheck = DateTime.Now.AddSeconds(1);
                        if (Array.IndexOf(SerialPort.GetPortNames(), _serialPort.PortName) == -1)
                        {
                            LostConnection("Port disappeared");
                            UpdateButtons();
                            return;
                        }
                    }

                    while (verified && _serialPort.BytesToRead > 0)
                        ReceiveByte((byte)_serialPort.ReadByte());

                    if (!verified) { UpdateButtons(); return; }

                    // Request timeout
                    if (waiting && DateTime.Now > deadline)
                    {
                        if (rxCount > 0) AddToList("RX", rxBuffer, rxCount, "Incomplete packet");
                        Info("TIMEOUT: no reply");

                        byte timedOut = expectedReply;
                        waiting = false;
                        expectedReply = 0;
                        rxCount = 0;

                        if (timedOut == 0x41 || timedOut == 0x49)
                        {
                            LostConnection("Reply timeout");
                            UpdateButtons();
                            return;
                        }
                    }

                    // Send a slider change that arrived while waiting
                    if (!waiting && pendingServo >= 0)
                    {
                        int angle = pendingServo;
                        pendingServo = -1;
                        SendServo(angle);
                    }

                    if (!verified) { UpdateButtons(); return; }

                    // AN0 request every second
                    if (!waiting && DateTime.Now >= nextADC)
                    {
                        nextADC = DateTime.Now.AddSeconds(1);
                        SendBytes(AdcRequest);
                    }

                    if (verified) StatusLabel.Text = "CONNECTED: " + _serialPort.PortName;
                }
                catch (Exception ex) { LostConnection("Communication failed: " + ex.Message); }

                UpdateButtons();
            }

            void UpdateButtons()
            {
                bool ready = verified && _serialPort.IsOpen && !waiting;
                IDButton.Enabled = ADCButton.Enabled = ReadButton.Enabled = WriteButton.Enabled = ready;
                ServoTrackBar.Enabled = verified;
            }

            // ---------- Part 3 framing error test (button is disabled in Part 4) ----------

            void ForceAFramingError()
            {
                if (!_serialPort.IsOpen) { MessageBox.Show("Connect first."); return; }

                _serialPort.BaudRate = 4800;
                _serialPort.Write(new byte[] { 0x00 }, 0, 1);
                AddToList("TX", new byte[] { 0x00 }, 1, "Sent at 4800 to force FRAMING");
                _serialPort.BaudRate = 9600;

                Thread.Sleep(20);
                _serialPort.DiscardInBuffer();
                rxCount = 0;
                waiting = true;
                deadline = DateTime.Now.AddMilliseconds(1000);
            }

            // ---------- Event handlers (names must match the Designer) ----------

            private void ExitButton_Click(object sender, EventArgs e) => Close();
            private void ConnectButton_Click(object sender, EventArgs e) { }   // unused in Part 4
            private void ReadButton_Click(object sender, EventArgs e) => SendBytes(AdcRequest);
            private void ADCButton_Click(object sender, EventArgs e) => SendBytes(AdcRequest);
            private void IDButton_Click(object sender, EventArgs e) => SendBytes(IdRequest);
            private void WriteButton_Click(object sender, EventArgs e) => SerialSend();
            private void FramingErrorButton_Click(object sender, EventArgs e) => ForceAFramingError();

            private void ServoTrackBar_Scroll(object sender, EventArgs e)
            {
                int angle = ServoTrackBar.Value;
                ServoDegreeLabel.Text = angle + "°";

                if (angle == lastSliderAngle) return;
                lastSliderAngle = angle;

                if (verified) SendServo(angle);
            }

            private void SerialData_FormClosing(object sender, FormClosingEventArgs e)
            {
                closing = true;
                StatusTimer.Stop();
                try { _serialPort.Close(); _serialPort.Dispose(); } catch (Exception) { }
            }
        }
    }
}
