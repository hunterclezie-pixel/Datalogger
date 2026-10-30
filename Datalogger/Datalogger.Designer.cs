namespace Datalogger
{
    partial class Datalogger
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            MenuStrip = new MenuStrip();
            GraphicsContextMenu = new ContextMenuStrip(components);
            SampleRateComboBox = new ComboBox();
            GraphicsViewComboBox = new ComboBox();
            SaveButton = new Button();
            StopButton = new Button();
            StartButton = new Button();
            GraphPictureBox = new PictureBox();
            ADCValueLabel = new Label();
            SampleCountLabel = new Label();
            StatusStrip = new StatusStrip();
            PicStatusLabel = new ToolStripStatusLabel();
            RateStatusLabel = new ToolStripStatusLabel();
            FileStatusLabel = new ToolStripStatusLabel();
            StatusTimer = new System.Windows.Forms.Timer(components);
            ToolTip = new ToolTip(components);
            OpenFileDialog = new OpenFileDialog();
            SaveFileDialog = new SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)GraphPictureBox).BeginInit();
            StatusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // MenuStrip
            // 
            MenuStrip.ImageScalingSize = new Size(20, 20);
            MenuStrip.Location = new Point(0, 0);
            MenuStrip.Name = "MenuStrip";
            MenuStrip.Size = new Size(800, 24);
            MenuStrip.TabIndex = 0;
            MenuStrip.Text = "menuStrip1";
            // 
            // GraphicsContextMenu
            // 
            GraphicsContextMenu.ImageScalingSize = new Size(20, 20);
            GraphicsContextMenu.Name = "GraphicsContextMenu";
            GraphicsContextMenu.Size = new Size(61, 4);
            // 
            // SampleRateComboBox
            // 
            SampleRateComboBox.FormattingEnabled = true;
            SampleRateComboBox.Location = new Point(12, 26);
            SampleRateComboBox.Name = "SampleRateComboBox";
            SampleRateComboBox.Size = new Size(151, 28);
            SampleRateComboBox.TabIndex = 2;
            // 
            // GraphicsViewComboBox
            // 
            GraphicsViewComboBox.FormattingEnabled = true;
            GraphicsViewComboBox.Location = new Point(12, 60);
            GraphicsViewComboBox.Name = "GraphicsViewComboBox";
            GraphicsViewComboBox.Size = new Size(151, 28);
            GraphicsViewComboBox.TabIndex = 3;
            // 
            // SaveButton
            // 
            SaveButton.Location = new Point(683, 374);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new Size(105, 51);
            SaveButton.TabIndex = 4;
            SaveButton.Text = "Save";
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;
            // 
            // StopButton
            // 
            StopButton.Location = new Point(572, 374);
            StopButton.Name = "StopButton";
            StopButton.Size = new Size(105, 51);
            StopButton.TabIndex = 5;
            StopButton.Text = "Stop";
            StopButton.UseVisualStyleBackColor = true;
            StopButton.Click += StopButton_Click;
            // 
            // StartButton
            // 
            StartButton.Location = new Point(461, 374);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(105, 51);
            StartButton.TabIndex = 6;
            StartButton.Text = "Start";
            StartButton.UseVisualStyleBackColor = true;
            StartButton.Click += StartButton_Click;
            // 
            // GraphPictureBox
            // 
            GraphPictureBox.Location = new Point(169, 27);
            GraphPictureBox.Name = "GraphPictureBox";
            GraphPictureBox.Size = new Size(619, 341);
            GraphPictureBox.TabIndex = 7;
            GraphPictureBox.TabStop = false;
            // 
            // ADCValueLabel
            // 
            ADCValueLabel.AutoSize = true;
            ADCValueLabel.Location = new Point(12, 102);
            ADCValueLabel.Name = "ADCValueLabel";
            ADCValueLabel.Size = new Size(128, 20);
            ADCValueLabel.TabIndex = 8;
            ADCValueLabel.Text = "Lastest ADC Value";
            // 
            // SampleCountLabel
            // 
            SampleCountLabel.AutoSize = true;
            SampleCountLabel.Location = new Point(12, 132);
            SampleCountLabel.Name = "SampleCountLabel";
            SampleCountLabel.Size = new Size(143, 20);
            SampleCountLabel.TabIndex = 9;
            SampleCountLabel.Text = "Number Of Samples";
            // 
            // StatusStrip
            // 
            StatusStrip.ImageScalingSize = new Size(20, 20);
            StatusStrip.Items.AddRange(new ToolStripItem[] { PicStatusLabel, RateStatusLabel, FileStatusLabel });
            StatusStrip.Location = new Point(0, 424);
            StatusStrip.Name = "StatusStrip";
            StatusStrip.Size = new Size(800, 26);
            StatusStrip.TabIndex = 10;
            StatusStrip.Text = "statusStrip1";
            // 
            // PicStatusLabel
            // 
            PicStatusLabel.Name = "PicStatusLabel";
            PicStatusLabel.Size = new Size(116, 20);
            PicStatusLabel.Text = "COM Port Status";
            // 
            // RateStatusLabel
            // 
            RateStatusLabel.Name = "RateStatusLabel";
            RateStatusLabel.Size = new Size(125, 20);
            RateStatusLabel.Text = "Sampling Interval";
            // 
            // FileStatusLabel
            // 
            FileStatusLabel.Name = "FileStatusLabel";
            FileStatusLabel.Size = new Size(150, 20);
            FileStatusLabel.Text = "Current Log Filename";
            // 
            // OpenFileDialog
            // 
            OpenFileDialog.FileName = "openFileDialog1";
            // 
            // Datalogger
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(StatusStrip);
            Controls.Add(SampleCountLabel);
            Controls.Add(ADCValueLabel);
            Controls.Add(GraphPictureBox);
            Controls.Add(StartButton);
            Controls.Add(StopButton);
            Controls.Add(SaveButton);
            Controls.Add(GraphicsViewComboBox);
            Controls.Add(SampleRateComboBox);
            Controls.Add(MenuStrip);
            MainMenuStrip = MenuStrip;
            Name = "Datalogger";
            Text = "DataLoggerForm";
            ((System.ComponentModel.ISupportInitialize)GraphPictureBox).EndInit();
            StatusStrip.ResumeLayout(false);
            StatusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip MenuStrip;
        private ContextMenuStrip GraphicsContextMenu;
        private ComboBox SampleRateComboBox;
        private ComboBox GraphicsViewComboBox;
        private Button SaveButton;
        private Button StopButton;
        private Button StartButton;
        private PictureBox GraphPictureBox;
        private Label ADCValueLabel;
        private Label SampleCountLabel;
        private StatusStrip StatusStrip;
        private ToolStripStatusLabel PicStatusLabel;
        private ToolStripStatusLabel RateStatusLabel;
        private ToolStripStatusLabel FileStatusLabel;
        private System.Windows.Forms.Timer StatusTimer;
        private ToolTip ToolTip;
        private OpenFileDialog OpenFileDialog;
        private SaveFileDialog SaveFileDialog;
    }
}
