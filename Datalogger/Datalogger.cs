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
        System.Windows.Forms.Timer sampleTimer = new System.Windows.Forms.Timer();
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

        private void StartButton_Click(object sender, EventArgs e)
        {

        }

        private void StopButton_Click(object sender, EventArgs e)
        {

        }

        private void SaveButton_Click(object sender, EventArgs e)
        {

        }

        // PIC Connection ---------------------------------------

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

        void ClosePIC()
        {
            sampleTimer.Stop();
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

        // Graph Drawing ---------------------------------------

        // Saving and Opening Log Files -------------------------

        // Button Events ---------------------------------------

        // Form Closing ----------------------------------------
    }
}