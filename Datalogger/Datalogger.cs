using System.IO.Ports;

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
        ToolTip tips = new ToolTip();
        int highByte = adcValue >> 8;
        int lowByte = adcValue & 0xFF;

        public Datalogger()
        {
            InitializeComponent();

            // Form Setup

            // Button Events

            // Timer Setup

            // PIC Connection Setup
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
                    picPort = new SerialPort(portName, 9600);
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
                }
                catch
                {
                    if (picPort.IsOpen)
                        picPort.Close();
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

        // Sampling and ADC Reading -----------------------------

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

        // Graph Drawing ---------------------------------------

        void DrawGraph()
        {
            if (GraphPictureBox.Width <= 0 ||
                GraphPictureBox.Height <= 0)
                return;

            Bitmap graph = new Bitmap(
                GraphPictureBox.Width,
                GraphPictureBox.Height);

            using (Graphics g = Graphics.FromImage(graph))
            {
                g.Clear(Color.White);

                int width = graph.Width;
                int height = graph.Height;

                // Draw Graph Axes
                g.DrawLine(Pens.Black, 35, 10, 35, height - 25);
                g.DrawLine(Pens.Black, 35, height - 25,
                    width - 10, height - 25);

                if (adcValues.Count > 1 && width > 50 && height > 40)
                {
                    DateTime endTime = sampleTimes.Last();
                    DateTime startTime = sampleTimes.First();

                    // Last 30 Seconds View
                    if (GraphViewComboBox.SelectedIndex == 1)
                        startTime = endTime.AddSeconds(-30);

                    double totalTime =
                        (endTime - startTime).TotalMilliseconds;

                    if (totalTime <= 0)
                        totalTime = 1;

                    Point? lastPoint = null;

                    for (int i = 0; i < adcValues.Count; i++)
                    {
                        if (sampleTimes[i] < startTime)
                            continue;

                        double elapsed =
                            (sampleTimes[i] - startTime).TotalMilliseconds;

                        int x = 35 + (int)(elapsed / totalTime *
                            (width - 45));

                        int y = height - 25 -
                            (int)(adcValues[i] / 1023.0 *
                            (height - 35));

                        Point currentPoint = new Point(x, y);

                        if (lastPoint != null)
                            g.DrawLine(Pens.Blue,
                                lastPoint.Value, currentPoint);

                        lastPoint = currentPoint;
                    }
                }
            }

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

        // Button Events ---------------------------------------

        private void StartButton_Click(object sender, EventArgs e)
        {

        }

        private void StopButton_Click(object sender, EventArgs e)
        {

        }

        private void SaveButton_Click(object sender, EventArgs e)
        {

        }


        private void ExitButton_Click(object sender, EventArgs e)
        {

        }

        // Form Closing ----------------------------------------
    }
}