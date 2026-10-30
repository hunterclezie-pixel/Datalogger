using System.Diagnostics.Metrics;
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
        // Variables Below Here ---------------------------------

        SerialPort picPort = new SerialPort();
        bool picConnected = false;
        bool isLogging = false;
        bool picWaiting = false;
        int sampleRate = 100;
        List<int> adcValues = new List<int>();
        List<DateTime> sampleTimes = new List<DateTime>();
        string logFile = "";

        public Datalogger()
        {
            InitializeComponent();

            // Form Setup
            this.Text = "PIC Data Logger";

            StopButton.Enabled = false;

            SampleRateComboBox.Items.AddRange(new string[]
            {
            "10 Minutes",
            "5 Minutes",
            "1 Minute",
            "30 Seconds",
            "10 Seconds",
            "5 Seconds",
            "1 Second",
            "500 ms",
            "200 ms",
            "100 ms"
            });

            SampleRateComboBox.SelectedIndex = 9;

            GraphViewComboBox.Items.Add("Entire History");
            GraphViewComboBox.Items.Add("Last 30 Seconds");
            GraphViewComboBox.SelectedIndex = 0;

            PicStatusLabel.Text = "PIC: Searching...";
            RateStatusLabel.Text = "Rate: 100 ms";
            FileStatusLabel.Text = "File: None";

            // Button Events
            StartButton.Click += StartButton_Click;
            StopButton.Click += StopButton_Click;
            SaveButton.Click += SaveButton_Click;

            SampleRateComboBox.SelectedIndexChanged += SampleRateComboBox_SelectedIndexChanged;
            GraphViewComboBox.SelectedIndexChanged += GraphViewComboBox_SelectedIndexChanged;

            // Timer Setup
            SampleTimer.Interval = sampleRate;
            SampleTimer.Tick += SampleTimer_Tick;

            // PIC Connection Setup
            FindPIC();
            this.FormClosing += Datalogger_FormClosing;
        }

        // PIC Connection ---------------------------------------

        // Find PIC Device on Serial Ports
        void FindPIC()
        {
            picConnected = false;
            PicStatusLabel.Text = "PIC: Searching...";

            foreach (string portName in SerialPort.GetPortNames())
            {
                try
                {
                    picPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
                    picPort.ReadTimeout = 500;
                    picPort.WriteTimeout = 500;
                    picPort.Open();

                    picPort.DiscardInBuffer();

                    // Request PIC Device ID
                    picPort.Write(new byte[] { 0x24, 0x49, 0x00 }, 0, 3);

                    byte[] response = new byte[5];
                    int count = 0;

                    while (count < response.Length)
                    {
                        count += picPort.Read(response, count,
                            response.Length - count);
                    }

                    // Verify Device ID
                    if (response[0] == 0x24 &&
                        response[1] == 0x49 &&
                        response[2] == 0x02 &&
                        response[3] == 0x33 &&
                        response[4] == 0x75)
                    {
                        picConnected = true;
                        PicStatusLabel.Text = "PIC: Connected (" + portName + ")";
                        return;
                    }

                    picPort.Close();
                    picPort.Dispose();
                }
                catch
                {
                    if (picPort.IsOpen)
                        picPort.Close();
                    picPort.Dispose();
                }
            }

            PicStatusLabel.Text = "PIC: Disconnected";
        }

        // Close PIC Connection
        void ClosePIC()
        {
            SampleTimer.Stop();
            isLogging = false;
            picConnected = false;
            picWaiting = false;

            if (picPort.IsOpen)
                picPort.Close();

            PicStatusLabel.Text = "PIC: Disconnected";
            StartButton.Enabled = true;
            StopButton.Enabled = false;
        }

        // Graph Drawing ---------------------------------------
        void DrawGraph()
        {
            int width = GraphPictureBox.Width;
            int height = GraphPictureBox.Height;

            if (width < 150 || height < 100)
                return;

            Bitmap graph = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(graph);

            g.Clear(Color.White);

            // Graph Size
            int left = 95;
            int right = width - 15;
            int top = 10;
            int bottom = height - 40;

            // Draw Graph Lines and Voltage Labels
            for (int i = 0; i <= 4; i++)
            {
                int adc = i * 1023 / 4;

                int y = bottom - (int)(adc / 1023.0 * (bottom - top));
                double voltage = adc * 5.0 / 1023.0;
                g.DrawLine(Pens.LightGray, left, y, right, y);
                g.DrawString(adc + " (" + voltage.ToString("F2") + " V)",
                    this.Font, Brushes.Black, 5, y - 8);
            }

            // Draw Graph Data
            if (adcValues.Count > 0)
            {
                DateTime startTime = sampleTimes[0];
                DateTime endTime = sampleTimes[sampleTimes.Count - 1];

                // Last 30 Seconds View
                if (GraphViewComboBox.SelectedIndex == 1)
                    startTime = endTime.AddSeconds(-30);

                double totalTime = (endTime - startTime).TotalMilliseconds;

                if (totalTime <= 0)
                    totalTime = 1000;

                // Draw Time Labels
                for (int i = 0; i <= 5; i++)
                {
                    int x = left + i * (right - left) / 5;
                    g.DrawLine(Pens.LightGray, x, top, x, bottom);
                    DateTime time = startTime.AddMilliseconds(totalTime * i / 5);
                    g.DrawString(time.ToString("HH:mm:ss"),
                        this.Font, Brushes.Black, x - 25, bottom + 5);
                }

                // Draw ADC Values
                Point? lastPoint = null;

                for (int i = 0; i < adcValues.Count; i++)
                {
                    if (sampleTimes[i] < startTime)
                        continue;

                    int x = left + (int)((sampleTimes[i] - startTime).TotalMilliseconds / totalTime * (right - left));
                    int y = bottom - (int)(adcValues[i] / 1023.0 * (bottom - top));

                    Point currentPoint = new Point(x, y);

                    // Connect Points
                    if (lastPoint != null)
                        g.DrawLine(Pens.Blue, lastPoint.Value, currentPoint);

                    // Draw Individual Points
                    g.FillEllipse(Brushes.Blue, x - 2, y - 2, 4, 4);
                    lastPoint = currentPoint;
                }
            }
            else
            {
                g.DrawString("No data yet - press Start",
                    this.Font, Brushes.Gray, left + 20, top + 20);
            }

            // Draw Graph Border
            g.DrawRectangle(Pens.Black, left, top, right - left, bottom - top);
            g.Dispose();

            // Update PictureBox
            Image oldGraph = GraphPictureBox.Image;
            GraphPictureBox.Image = graph;
            oldGraph?.Dispose();
        }

        // Saving and Opening Log Files -------------------------

        void SaveLog()
        {
            if (adcValues.Count == 0)
            {
                MessageBox.Show("No data to save.");
                return;
            }

            try
            {
                // Group samples by hour
                var hours = sampleTimes
                    .Select(time => time.ToString("yyMMddHH"))
                    .Distinct();

                foreach (string hour in hours)
                {
                    logFile = Path.Combine(
                        Application.StartupPath,
                        "log_" + hour + ".log");

                    using (StreamWriter writer = new StreamWriter(logFile))
                    {
                        for (int i = 0; i < adcValues.Count; i++)
                        {
                            if (sampleTimes[i].ToString("yyMMddHH") != hour)
                                continue;

                            int highByte = adcValues[i] >> 8;
                            int lowByte = adcValues[i] & 0xFF;

                            string timestamp =
                                sampleTimes[i].ToString("yyMMddHHmmssff");

                            writer.WriteLine("\"$$AN0\"," +
                                highByte + "," +
                                lowByte + "," +
                                timestamp);
                        }
                    }
                }

                FileStatusLabel.Text = "File: " +
                    Path.GetFileName(logFile);

                MessageBox.Show("Data saved successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving file: " + ex.Message);
            }
        }

        void OpenLog()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Log Files (*.log)|*.log";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string[] lines = File.ReadAllLines(dialog.FileName);

                        adcValues.Clear();
                        sampleTimes.Clear();

                        foreach (string line in lines)
                        {
                            string[] data = line.Split(',');

                            if (data.Length != 4)
                                continue;

                            if (data[0].Trim('"') != "$$AN0")
                                continue;

                            int highByte = int.Parse(data[1]);
                            int lowByte = int.Parse(data[2]);

                            int adcValue = (highByte << 8) | lowByte;

                            DateTime time = DateTime.ParseExact(
                                data[3], "yyMMddHHmmssff",
                                System.Globalization.CultureInfo.InvariantCulture);

                            adcValues.Add(adcValue);
                            sampleTimes.Add(time);
                        }

                        logFile = dialog.FileName;
                        FileStatusLabel.Text = "File: " +
                            Path.GetFileName(logFile);

                        DrawGraph();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error opening file: " + ex.Message);
                    }
                }
            }
        }

        // Event Handlers below here ---------------------------------------

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (!picConnected)
            {
                FindPIC();

                if (!picConnected)
                {
                    MessageBox.Show("PIC not connected.");
                    return;
                }
            }

            isLogging = true;
            SampleTimer.Start();

            StartButton.Enabled = false;
            StopButton.Enabled = true;
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            isLogging = false;
            SampleTimer.Stop();

            StartButton.Enabled = true;
            StopButton.Enabled = false;
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
            switch (SampleRateComboBox.SelectedIndex)
            {
                case 0: sampleRate = 600000; break;
                case 1: sampleRate = 300000; break;
                case 2: sampleRate = 60000; break;
                case 3: sampleRate = 30000; break;
                case 4: sampleRate = 10000; break;
                case 5: sampleRate = 5000; break;
                case 6: sampleRate = 1000; break;
                case 7: sampleRate = 500; break;
                case 8: sampleRate = 200; break;
                case 9: sampleRate = 100; break;

                default:
                    MessageBox.Show("Invalid sample rate.");
                    return;
            }

            SampleTimer.Interval = sampleRate;

            RateStatusLabel.Text =
                "Rate: " + SampleRateComboBox.Text;
        }

        private void GraphViewComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DrawGraph();
        }

        private void SampleTimer_Tick(object sender, EventArgs e)
        {
            if (!picConnected || !isLogging || picWaiting)
                return;

            try
            {
                picWaiting = true;

                // Request AN0
                byte[] request = { 0x24, 0x41, 0x01, 0x00 };

                picPort.DiscardInBuffer();
                picPort.Write(request, 0, request.Length);

                // Read ADC Response
                byte[] response = new byte[6];
                int count = 0;

                while (count < response.Length)
                {
                    count += picPort.Read(response, count,
                        response.Length - count);
                }

                // Check Response
                if (response[0] == 0x24 &&
                    response[1] == 0x41 &&
                    response[2] == 0x03 &&
                    response[3] == 0x00)
                {
                    int adcValue = (response[4] << 8) | response[5];

                    if (adcValue >= 0 && adcValue <= 1023)
                    {
                        adcValues.Add(adcValue);
                        sampleTimes.Add(DateTime.Now);

                        DrawGraph();
                    }
                }
            }
            catch
            {
                ClosePIC();

                MessageBox.Show("PIC connection lost.",
                    "Connection Error");
            }
            finally
            {
                picWaiting = false;
            }
        }

        private void Datalogger_FormClosing(object sender, FormClosingEventArgs e)
        {
            SampleTimer.Stop();

            if (picPort.IsOpen)
                picPort.Close();

            picPort.Dispose();
        }

        private void OpenContextMenu_Click(object sender, EventArgs e)
        {
            OpenLog();
        }
    }
}