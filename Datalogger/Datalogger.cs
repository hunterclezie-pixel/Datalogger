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

        // PIC Connection ---------------------------------------

        // Sampling and ADC Reading -----------------------------

        // Graph Drawing ---------------------------------------

        // Saving and Opening Log Files -------------------------

        // Button Events ---------------------------------------

        // Form Closing ----------------------------------------
    }
}