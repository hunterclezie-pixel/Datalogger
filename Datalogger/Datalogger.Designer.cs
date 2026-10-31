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
            FileMenuStrip = new ToolStripMenuItem();
            StartStripMenuItem = new ToolStripMenuItem();
            StopStripMenuItem = new ToolStripMenuItem();
            SaveStripMenuItem = new ToolStripMenuItem();
            ExitStripMenuItem = new ToolStripMenuItem();
            OpenStripMenuItem = new ToolStripMenuItem();
            AboutStripMenuItem = new ToolStripMenuItem();
            GraphicsContextMenu = new ContextMenuStrip(components);
            StartContextMenu = new ToolStripMenuItem();
            StopContextMenu = new ToolStripMenuItem();
            SaveContextMenu = new ToolStripMenuItem();
            OpenContextMenu = new ToolStripMenuItem();
            ExitContextMenu = new ToolStripMenuItem();
            SampleRateComboBox = new ComboBox();
            GraphViewComboBox = new ComboBox();
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
            SampleTimer = new System.Windows.Forms.Timer(components);
            ToolTip = new ToolTip(components);
            OpenFileDialog = new OpenFileDialog();
            SaveFileDialog = new SaveFileDialog();
            ExitButton = new Button();
            MenuStrip.SuspendLayout();
            GraphicsContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)GraphPictureBox).BeginInit();
            StatusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // MenuStrip
            // 
            MenuStrip.ImageScalingSize = new Size(20, 20);
            MenuStrip.Items.AddRange(new ToolStripItem[] { FileMenuStrip, AboutStripMenuItem });
            MenuStrip.Location = new Point(0, 0);
            MenuStrip.Name = "MenuStrip";
            MenuStrip.Size = new Size(800, 28);
            MenuStrip.TabIndex = 0;
            MenuStrip.Text = "menuStrip1";
            // 
            // FileMenuStrip
            // 
            FileMenuStrip.DropDownItems.AddRange(new ToolStripItem[] { StartStripMenuItem, StopStripMenuItem, SaveStripMenuItem, ExitStripMenuItem, OpenStripMenuItem });
            FileMenuStrip.Name = "FileMenuStrip";
            FileMenuStrip.Size = new Size(46, 24);
            FileMenuStrip.Text = "File";
            // 
            // StartStripMenuItem
            // 
            StartStripMenuItem.Name = "StartStripMenuItem";
            StartStripMenuItem.Size = new Size(224, 26);
            StartStripMenuItem.Text = "Start";
            // 
            // StopStripMenuItem
            // 
            StopStripMenuItem.Name = "StopStripMenuItem";
            StopStripMenuItem.Size = new Size(224, 26);
            StopStripMenuItem.Text = "Stop";
            // 
            // SaveStripMenuItem
            // 
            SaveStripMenuItem.Name = "SaveStripMenuItem";
            SaveStripMenuItem.Size = new Size(224, 26);
            SaveStripMenuItem.Text = "Save";
            // 
            // ExitStripMenuItem
            // 
            ExitStripMenuItem.Name = "ExitStripMenuItem";
            ExitStripMenuItem.Size = new Size(224, 26);
            ExitStripMenuItem.Text = "Exit";
            // 
            // OpenStripMenuItem
            // 
            OpenStripMenuItem.Name = "OpenStripMenuItem";
            OpenStripMenuItem.Size = new Size(224, 26);
            OpenStripMenuItem.Text = "Open";
            // 
            // AboutStripMenuItem
            // 
            AboutStripMenuItem.Name = "AboutStripMenuItem";
            AboutStripMenuItem.Size = new Size(64, 24);
            AboutStripMenuItem.Text = "About";
            AboutStripMenuItem.Click += AboutStripMenuItem_Click;
            // 
            // GraphicsContextMenu
            // 
            GraphicsContextMenu.ImageScalingSize = new Size(20, 20);
            GraphicsContextMenu.Items.AddRange(new ToolStripItem[] { StartContextMenu, StopContextMenu, SaveContextMenu, OpenContextMenu, ExitContextMenu });
            GraphicsContextMenu.Name = "GraphicsContextMenu";
            GraphicsContextMenu.Size = new Size(115, 124);
            // 
            // StartContextMenu
            // 
            StartContextMenu.Name = "StartContextMenu";
            StartContextMenu.Size = new Size(114, 24);
            StartContextMenu.Text = "Start";
            // 
            // StopContextMenu
            // 
            StopContextMenu.Name = "StopContextMenu";
            StopContextMenu.Size = new Size(114, 24);
            StopContextMenu.Text = "Stop";
            // 
            // SaveContextMenu
            // 
            SaveContextMenu.Name = "SaveContextMenu";
            SaveContextMenu.Size = new Size(114, 24);
            SaveContextMenu.Text = "Save";
            // 
            // OpenContextMenu
            // 
            OpenContextMenu.Name = "OpenContextMenu";
            OpenContextMenu.Size = new Size(114, 24);
            OpenContextMenu.Text = "Open";
            OpenContextMenu.Click += OpenContextMenu_Click;
            // 
            // ExitContextMenu
            // 
            ExitContextMenu.Name = "ExitContextMenu";
            ExitContextMenu.Size = new Size(114, 24);
            ExitContextMenu.Text = "Exit";
            // 
            // SampleRateComboBox
            // 
            SampleRateComboBox.FormattingEnabled = true;
            SampleRateComboBox.Location = new Point(12, 31);
            SampleRateComboBox.Name = "SampleRateComboBox";
            SampleRateComboBox.Size = new Size(151, 28);
            SampleRateComboBox.TabIndex = 5;
            ToolTip.SetToolTip(SampleRateComboBox, "Select sampling rate");
            // 
            // GraphViewComboBox
            // 
            GraphViewComboBox.FormattingEnabled = true;
            GraphViewComboBox.Location = new Point(12, 65);
            GraphViewComboBox.Name = "GraphViewComboBox";
            GraphViewComboBox.Size = new Size(151, 28);
            GraphViewComboBox.TabIndex = 6;
            ToolTip.SetToolTip(GraphViewComboBox, "Select graph view");
            // 
            // SaveButton
            // 
            SaveButton.Location = new Point(572, 370);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new Size(105, 51);
            SaveButton.TabIndex = 3;
            SaveButton.Text = "Save";
            ToolTip.SetToolTip(SaveButton, "Save collected data");
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;
            // 
            // StopButton
            // 
            StopButton.Location = new Point(461, 370);
            StopButton.Name = "StopButton";
            StopButton.Size = new Size(105, 51);
            StopButton.TabIndex = 2;
            StopButton.Text = "Stop";
            ToolTip.SetToolTip(StopButton, "Stop collecting ADC data");
            StopButton.UseVisualStyleBackColor = true;
            StopButton.Click += StopButton_Click;
            // 
            // StartButton
            // 
            StartButton.Location = new Point(350, 370);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(105, 51);
            StartButton.TabIndex = 1;
            StartButton.Text = "Start";
            ToolTip.SetToolTip(StartButton, "Start collecting ADC data");
            StartButton.UseVisualStyleBackColor = true;
            StartButton.Click += StartButton_Click;
            // 
            // GraphPictureBox
            // 
            GraphPictureBox.Location = new Point(169, 31);
            GraphPictureBox.Name = "GraphPictureBox";
            GraphPictureBox.Size = new Size(619, 333);
            GraphPictureBox.TabIndex = 7;
            GraphPictureBox.TabStop = false;
            ToolTip.SetToolTip(GraphPictureBox, "Displays recorded ADC values");
            // 
            // ADCValueLabel
            // 
            ADCValueLabel.AutoSize = true;
            ADCValueLabel.Location = new Point(12, 107);
            ADCValueLabel.Name = "ADCValueLabel";
            ADCValueLabel.Size = new Size(128, 20);
            ADCValueLabel.TabIndex = 7;
            ADCValueLabel.Text = "Lastest ADC Value";
            // 
            // SampleCountLabel
            // 
            SampleCountLabel.AutoSize = true;
            SampleCountLabel.Location = new Point(12, 137);
            SampleCountLabel.Name = "SampleCountLabel";
            SampleCountLabel.Size = new Size(143, 20);
            SampleCountLabel.TabIndex = 8;
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
            // SampleTimer
            // 
            SampleTimer.Tick += SampleTimer_Tick;
            // 
            // OpenFileDialog
            // 
            OpenFileDialog.FileName = "openFileDialog1";
            // 
            // ExitButton
            // 
            ExitButton.Location = new Point(683, 370);
            ExitButton.Name = "ExitButton";
            ExitButton.Size = new Size(105, 51);
            ExitButton.TabIndex = 4;
            ExitButton.Text = "Exit";
            ExitButton.UseVisualStyleBackColor = true;
            ExitButton.Click += ExitButton_Click;
            // 
            // Datalogger
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(ExitButton);
            Controls.Add(StatusStrip);
            Controls.Add(SampleCountLabel);
            Controls.Add(ADCValueLabel);
            Controls.Add(GraphPictureBox);
            Controls.Add(StartButton);
            Controls.Add(StopButton);
            Controls.Add(SaveButton);
            Controls.Add(GraphViewComboBox);
            Controls.Add(SampleRateComboBox);
            Controls.Add(MenuStrip);
            MainMenuStrip = MenuStrip;
            Name = "Datalogger";
            Text = "DataLoggerForm";
            MenuStrip.ResumeLayout(false);
            MenuStrip.PerformLayout();
            GraphicsContextMenu.ResumeLayout(false);
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
        private ComboBox GraphViewComboBox;
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
        private System.Windows.Forms.Timer SampleTimer;
        private ToolTip ToolTip;
        private OpenFileDialog OpenFileDialog;
        private SaveFileDialog SaveFileDialog;
        private Button ExitButton;
        private ToolStripMenuItem FileMenuStrip;
        private ToolStripMenuItem AboutStripMenuItem;
        private ToolStripMenuItem StartContextMenu;
        private ToolStripMenuItem StopContextMenu;
        private ToolStripMenuItem SaveContextMenu;
        private ToolStripMenuItem OpenContextMenu;
        private ToolStripMenuItem ExitContextMenu;
        private ToolStripMenuItem StartStripMenuItem;
        private ToolStripMenuItem StopStripMenuItem;
        private ToolStripMenuItem SaveStripMenuItem;
        private ToolStripMenuItem ExitStripMenuItem;
        private ToolStripMenuItem OpenStripMenuItem;
    }
}
