using System.IO.Ports;

/*
Hunter Clezie
Fall 2026
RCET 3371
Datalogger
github url: https://github.com/hunterclezie-pixel/Datalogger.git
*/

namespace Datalogger
{
    public partial class Datalogger : Form
    {
        public Datalogger()
        {
            InitializeComponent();

            // Form Setup
            this.Text = "PIC Data Logger";
            StartButton.Enabled = true;
            StopButton.Enabled = false;

            // Only allow the sample rates listed below
            SampleRateComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            SampleRateComboBox.Items.Clear();
            SampleRateComboBox.Items.AddRange(new string[]
            {
                "10 Minutes", "5 Minutes", "1 Minute", "30 Seconds",
                "10 Seconds", "5 Seconds", "1 Second", "500 ms",
                "200 ms", "100 ms"
            });
            SampleRateComboBox.SelectedIndex = 9;

            GraphViewComboBox.Items.Clear();
            GraphViewComboBox.Items.Add("Entire History");
            GraphViewComboBox.Items.Add("Last 30 Seconds");
            GraphViewComboBox.SelectedIndex = 0;

            PicStatusLabel.Text = "PIC: Searching...";
            RateStatusLabel.Text = "Rate: 100 ms";
            FileStatusLabel.Text = "File: None";

            // Event handlers for the ComboBoxes and Graph
            SampleRateComboBox.SelectedIndexChanged += SampleRateComboBox_SelectedIndexChanged;
            GraphViewComboBox.SelectedIndexChanged += GraphViewComboBox_SelectedIndexChanged;
            GraphPictureBox.Resize += GraphPictureBox_Resize;

            // Event handlers for the StripMenuItems
            StartStripMenuItem.Click += StartButton_Click;
            StopStripMenuItem.Click += StopButton_Click;
            SaveStripMenuItem.Click += SaveButton_Click;
            ExitStripMenuItem.Click += ExitButton_Click;
            OpenStripMenuItem.Click += OpenContextMenu_Click;

            // Event handlers for the ContextMenuStrip
            StartContextMenu.Click += StartButton_Click;
            StopContextMenu.Click += StopButton_Click;
            SaveContextMenu.Click += SaveButton_Click;
            ExitContextMenu.Click += ExitButton_Click;

            // Timer and Form Events
            SampleTimer.Interval = 20;
            SampleTimer.Tick += SampleTimer_Tick;
            this.FormClosing += Datalogger_FormClosing;
            SampleTimer.Start();

            UpdateStatusBar();
            DrawGraph();
        }

        //Global Variables below here --------------------------------------------------------------
        SerialPort picPort = new SerialPort();
        bool picConnected = false;
        bool picScanning = false;
        bool picWaiting = false;
        bool isLogging = false;
        volatile bool closing = false;
        bool openedLog = false;
        int sampleRate = 100;
        int rxCount = 0;
        byte[] rxBuffer = new byte[11];
        DateTime nextScan = DateTime.MinValue;
        DateTime nextSample = DateTime.MinValue;
        DateTime replyDeadline = DateTime.MinValue;
        List<int> adcValues = new List<int>();
        List<DateTime> sampleTimes = new List<DateTime>();
        string logFile = "";
        ToolTip tips = new ToolTip();

        //Custom Methods below here --------------------------------------------------------------

        // Updates the connection, sample rate, and current log file
        void UpdateStatusBar()
        {
            if (picConnected)
                PicStatusLabel.Text = "PIC: Connected (" + picPort.PortName + ")";
            else if (picScanning)
                PicStatusLabel.Text = "PIC: Searching...";
            else
                PicStatusLabel.Text = "PIC: Disconnected";

            RateStatusLabel.Text = "Rate: " + SampleRateComboBox.Text;

            if (logFile == "")
                FileStatusLabel.Text = "File: None";
            else
                FileStatusLabel.Text = "File: " + logFile;

            FileStatusLabel.ToolTipText = logFile;
        }

        // Converts the selected sample rate to milliseconds
        int GetSampleRate()
        {
            switch (SampleRateComboBox.SelectedIndex)
            {
                case 0: return 600000;
                case 1: return 300000;
                case 2: return 60000;
                case 3: return 30000;
                case 4: return 10000;
                case 5: return 5000;
                case 6: return 1000;
                case 7: return 500;
                case 8: return 200;
                case 9: return 100;
                default: return 0;
            }
        }

        // Checks a serial port to see if it belongs to the PIC
        SerialPort TryPort(string portName)
        {
            SerialPort port = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);

            try
            {
                port.ReadTimeout = 50;
                port.WriteTimeout = 500;
                port.Open();
                Thread.Sleep(100);

                for (int attempt = 0; attempt < 2; attempt++)
                {
                    if (closing)
                        break;

                    port.DiscardInBuffer();
                    byte[] request = { 0x24, 0x49, 0x00 };
                    port.Write(request, 0, request.Length);

                    byte[] response = new byte[5];
                    int count = 0;
                    DateTime timeout = DateTime.Now.AddMilliseconds(500);

                    while (count < 5 && DateTime.Now < timeout && !closing)
                    {
                        try
                        {
                            response[count] = (byte)port.ReadByte();
                            count++;
                        }
                        catch (TimeoutException)
                        {
                            // Keep waiting for the rest of the reply
                        }
                    }

                    if (count == 5 &&
                        response[0] == 0x24 && response[1] == 0x49 &&
                        response[2] == 0x02 && response[3] == 0x33 &&
                        response[4] == 0x75)
                    {
                        Thread.Sleep(50);

                        if (port.BytesToRead == 0 && !closing)
                            return port;
                    }
                }
            }
            catch
            {
                // Try another port
            }

            try
            {
                port.Close();
                port.Dispose();
            }
            catch { }

            return null;
        }

        // Finds the PIC by checking available serial ports
        SerialPort FindPIC()
        {
            foreach (string portName in SerialPort.GetPortNames())
            {
                if (closing)
                    break;

                SerialPort found = TryPort(portName);

                if (found != null)
                    return found;
            }

            return null;
        }

        // Searches for the PIC without freezing the Windows Form
        async void SearchForPIC()
        {
            if (picConnected || picScanning || closing)
                return;

            picScanning = true;
            UpdateStatusBar();
            SerialPort found = null;

            try
            {
                found = await Task.Run(() => FindPIC());
            }
            catch
            {
                // Try searching again later
            }

            picScanning = false;

            if (closing)
            {
                if (found != null)
                    found.Dispose();
                return;
            }

            if (found != null)
            {
                picPort.Dispose();
                picPort = found;
                picConnected = true;
                picWaiting = false;
                rxCount = 0;
                nextSample = DateTime.Now;
                UpdateStatusBar();
            }
            else
            {
                nextScan = DateTime.Now.AddSeconds(2);
                UpdateStatusBar();
            }
        }

        // Disconnects from the PIC
        void ClosePIC()
        {
            picConnected = false;
            picWaiting = false;
            rxCount = 0;

            try
            {
                if (picPort.IsOpen)
                    picPort.Close();
            }
            catch { }

            nextScan = DateTime.Now.AddSeconds(2);
            UpdateStatusBar();
        }

        // Requests the AN1 value from the PIC
        void RequestADC()
        {
            if (!picConnected || picWaiting)
                return;

            try
            {
                byte[] request = { 0x24, 0x41, 0x01, 0x01 };
                picPort.Write(request, 0, request.Length);
                picWaiting = true;
                replyDeadline = DateTime.Now.AddMilliseconds(500);
            }
            catch
            {
                ClosePIC();
            }
        }

        // Collects bytes until a complete PIC response arrives
        void ReceiveByte(byte value)
        {
            if (rxCount == 0 && value != 0x24)
                return;

            rxBuffer[rxCount] = value;
            rxCount++;

            if (rxCount < 3)
                return;

            // The third byte tells us how many data bytes follow
            if (rxBuffer[2] > 8)
            {
                rxCount = 0;
                return;
            }

            int packetLength = rxBuffer[2] + 3;

            if (rxCount < packetLength)
                return;

            // PIC error response
            if (rxBuffer[1] == 0x45)
            {
                rxCount = 0;
                ClosePIC();
                return;
            }

            // AN1 response: 24 41 03 01 HighByte LowByte
            if (rxBuffer[1] == 0x41 && rxBuffer[2] == 0x03 &&
                rxBuffer[3] == 0x01 && picWaiting)
            {
                int adcValue = (rxBuffer[4] << 8) | rxBuffer[5];
                StoreSample(adcValue);
                picWaiting = false;
            }

            rxCount = 0;
        }

        // Records one valid ADC sample, saves it, and updates the graph
        void StoreSample(int adcValue)
        {
            if (!isLogging || adcValue < 0 || adcValue > 1023)
                return;

            DateTime time = DateTime.Now;
            adcValues.Add(adcValue);
            sampleTimes.Add(time);
            SaveSample(adcValue, time);
            DrawGraph();
        }

        // Draws the ADC readings onto the PictureBox
        void DrawGraph()
        {
            int width = GraphPictureBox.Width;
            int height = GraphPictureBox.Height;

            if (width < 150 || height < 100)
                return;

            Bitmap graph = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(graph);
            g.Clear(Color.White);

            int left = 95;
            int right = width - 15;
            int top = 10;
            int bottom = height - 40;

            // Draw the voltage and ADC labels
            for (int i = 0; i <= 4; i++)
            {
                int adc = i * 1023 / 4;
                int y = bottom - (int)(adc / 1023.0 * (bottom - top));
                double volts = adc * 5.0 / 1024.0;

                g.DrawLine(Pens.LightGray, left, y, right, y);
                g.DrawString(adc + " (" + volts.ToString("F2") + " V)",
                    this.Font, Brushes.Black, 5, y - 8);
            }

            if (adcValues.Count > 0)
            {
                DateTime startTime = sampleTimes[0];
                DateTime endTime = sampleTimes[sampleTimes.Count - 1];

                if (GraphViewComboBox.SelectedIndex == 1)
                    startTime = endTime.AddSeconds(-30);

                if (endTime <= startTime)
                    endTime = startTime.AddSeconds(1);

                double totalTime = (endTime - startTime).TotalMilliseconds;

                // Draw the time labels
                for (int i = 0; i <= 5; i++)
                {
                    int x = left + i * (right - left) / 5;
                    DateTime time = startTime.AddMilliseconds(totalTime * i / 5);
                    g.DrawLine(Pens.LightGray, x, top, x, bottom);
                    g.DrawString(time.ToString("HH:mm:ss"),
                        this.Font, Brushes.Black, x - 25, bottom + 5);
                }

                // Connect each ADC reading with a blue line
                Point? lastPoint = null;

                for (int i = 0; i < adcValues.Count; i++)
                {
                    if (sampleTimes[i] < startTime)
                        continue;

                    int x = left + (int)((sampleTimes[i] - startTime).TotalMilliseconds /
                        totalTime * (right - left));
                    int y = bottom - (int)(adcValues[i] / 1023.0 * (bottom - top));
                    Point currentPoint = new Point(x, y);

                    if (lastPoint != null)
                        g.DrawLine(Pens.Blue, lastPoint.Value, currentPoint);

                    g.FillEllipse(Brushes.Blue, x - 2, y - 2, 4, 4);
                    lastPoint = currentPoint;
                }
            }
            else
            {
                g.DrawString("No data yet - press Start",
                    this.Font, Brushes.Gray, left + 20, top + 20);
            }

            g.DrawRectangle(Pens.Black, left, top, right - left, bottom - top);
            g.Dispose();

            Image oldGraph = GraphPictureBox.Image;
            GraphPictureBox.Image = graph;
            oldGraph?.Dispose();
        }

        // Automatically saves each sample to an hourly log file
        void SaveSample(int adcValue, DateTime time)
        {
            try
            {
                string hour = time.ToString("yyMMddHH");
                logFile = Path.Combine(Application.StartupPath, "log_" + hour + ".log");

                int highByte = adcValue >> 8;
                int lowByte = adcValue & 0xFF;
                // fff stores the full three-digit millisecond value
                string timestamp = time.ToString("yyMMddHHmmssfff");

                string line = "\"$$AN1\"," + highByte + "," + lowByte + "," + timestamp;
                File.AppendAllText(logFile, line + Environment.NewLine);
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                isLogging = false;
                StartButton.Enabled = true;
                StopButton.Enabled = false;
                MessageBox.Show("Error saving data: " + ex.Message);
            }
        }

        // Starts recording the ADC values
        void StartLogging()
        {
            if (!picConnected)
            {
                MessageBox.Show("PIC not connected. Searching for device.");
                SearchForPIC();
                return;
            }

            if (openedLog)
            {
                adcValues.Clear();
                sampleTimes.Clear();
                openedLog = false;
                GraphPictureBox.Invalidate();
            }

            int interval = GetSampleRate();
            if (interval <= 0)
            {
                MessageBox.Show("Please choose a valid sample rate.");
                return;
            }

            sampleRate = interval;
            isLogging = true;
            nextSample = DateTime.Now;

            StartButton.Enabled = false;
            StopButton.Enabled = true;
        }

        // Stops recording new ADC values
        void StopLogging()
        {
            isLogging = false;

            StartButton.Enabled = true;
            StopButton.Enabled = false;
        }

        // Confirms or saves the most recent hour to its hourly log file
        void SaveLog()
        {
            if (adcValues.Count == 0)
            {
                MessageBox.Show("No data to save.");
                return;
            }

            // Every hourly file contains only readings from that hour
            DateTime selectedHour = sampleTimes[sampleTimes.Count - 1];
            string hour = selectedHour.ToString("yyMMddHH");
            string fileName = "log_" + hour + ".log";
            string hourlyPath = Path.Combine(Application.StartupPath, fileName);

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Log Files (*.log)|*.log";
            dialog.InitialDirectory = Application.StartupPath;
            dialog.FileName = fileName;

            // The hourly file may already exist from automatic saving
            dialog.OverwritePrompt = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // The assignment requires the application's folder and hourly name
                if (!string.Equals(Path.GetFullPath(dialog.FileName),
                    Path.GetFullPath(hourlyPath), StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Please save as " + fileName +
                        " in the application folder.");
                    dialog.Dispose();
                    return;
                }

                try
                {
                    // Samples were already added by SaveSample while logging.
                    // Only create the file here if it does not already exist.
                    if (!File.Exists(hourlyPath))
                    {
                        using (StreamWriter writer = new StreamWriter(hourlyPath))
                        {
                            for (int i = 0; i < adcValues.Count; i++)
                            {
                                if (sampleTimes[i].ToString("yyMMddHH") != hour)
                                    continue;

                                int highByte = adcValues[i] >> 8;
                                int lowByte = adcValues[i] & 0xFF;
                                string timestamp = sampleTimes[i].ToString("yyMMddHHmmssfff");
                                writer.WriteLine("\"$$AN1\"," + highByte + "," + lowByte + "," + timestamp);
                            }
                        }
                    }

                    logFile = hourlyPath;
                    UpdateStatusBar();
                    MessageBox.Show("Log saved: " + fileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving file: " + ex.Message);
                }
            }

            dialog.Dispose();
        }

        // Opens a previously saved log file and displays its readings
        void OpenLog()
        {
            if (isLogging)
            {
                MessageBox.Show("Stop logging before opening a file.");
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Log Files (*.log)|*.log";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string[] lines = File.ReadAllLines(dialog.FileName);
                    List<int> newValues = new List<int>();
                    List<DateTime> newTimes = new List<DateTime>();

                    foreach (string line in lines)
                    {
                        string[] data = line.Split(',');

                        if (data.Length != 4 || data[0].Trim('"') != "$$AN1")
                            continue;

                        int highByte = int.Parse(data[1]);
                        int lowByte = int.Parse(data[2]);
                        int adcValue = (highByte << 8) | lowByte;
                        // Old logs may have only two fractional-second digits
                        string timeFormat = "yyMMddHHmmssfff";
                        if (data[3].Length == 14)
                            timeFormat = "yyMMddHHmmssff";

                        DateTime time = DateTime.ParseExact(data[3], timeFormat, null);

                        if (adcValue >= 0 && adcValue <= 1023)
                        {
                            newValues.Add(adcValue);
                            newTimes.Add(time);
                        }
                    }

                    adcValues = newValues;
                    sampleTimes = newTimes;
                    openedLog = true;
                    logFile = dialog.FileName;
                    UpdateStatusBar();
                    DrawGraph();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening file: " + ex.Message);
                }
            }

            dialog.Dispose();
        }

        //Event Handlers below here --------------------------------------------------------------

        private void StartButton_Click(object sender, EventArgs e)
        {
            StartLogging();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            StopLogging();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveLog();
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SampleRateComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            sampleRate = GetSampleRate();
            if (sampleRate <= 0)
            {
                MessageBox.Show("Please choose a valid sample rate.");
                return;
            }

            nextSample = DateTime.Now.AddMilliseconds(sampleRate);
            UpdateStatusBar();
        }

        private void GraphViewComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DrawGraph();
        }

        private void GraphPictureBox_Resize(object sender, EventArgs e)
        {
            DrawGraph();
        }

        private void SampleTimer_Tick(object sender, EventArgs e)
        {
            if (closing)
                return;

            // Automatically search for the PIC if it disconnects
            if (!picConnected)
            {
                if (!picScanning && DateTime.Now >= nextScan)
                    SearchForPIC();
                return;
            }

            try
            {
                if (!picPort.IsOpen)
                {
                    ClosePIC();
                    return;
                }

                // Read bytes already received from the PIC
                while (picConnected && picPort.BytesToRead > 0)
                {
                    byte value = (byte)picPort.ReadByte();
                    ReceiveByte(value);
                }

                if (picWaiting && DateTime.Now > replyDeadline)
                {
                    ClosePIC();
                    return;
                }

                if (isLogging && !picWaiting && DateTime.Now >= nextSample)
                {
                    nextSample = DateTime.Now.AddMilliseconds(sampleRate);
                    RequestADC();
                }
            }
            catch
            {
                ClosePIC();
            }
        }

        private void OpenContextMenu_Click(object sender, EventArgs e)
        {
            OpenLog();
        }

        private void Datalogger_FormClosing(object sender, FormClosingEventArgs e)
        {
            closing = true;
            isLogging = false;
            SampleTimer.Stop();

            try
            {
                if (picPort.IsOpen)
                    picPort.Close();
                picPort.Dispose();
            }
            catch { }
        }

        private void AboutStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This program was designed to log data from a PIC microcontroller.\n" +
                "It provides a simple interface for configuring logging parameters and viewing the logged data.");
        }
    }
}