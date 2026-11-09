namespace SmartX.App.Forms
{
    partial class RealTimeDashboard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            grpLiveTelemetry = new GroupBox();
            btnStop = new Button();
            dgvTelemetry = new DataGridView();
            colTimestamp = new DataGridViewTextBoxColumn();
            colDeviceId = new DataGridViewTextBoxColumn();
            colSensorType = new DataGridViewTextBoxColumn();
            colReading = new DataGridViewTextBoxColumn();
            colStatus = new DataGridViewTextBoxColumn();
            colPriority = new DataGridViewTextBoxColumn();
            btnStart = new Button();
            grpCommands = new GroupBox();
            btnUndo = new Button();
            bnSend = new Button();
            cmbCommands = new ComboBox();
            cmbDevices = new ComboBox();
            grpHistory = new GroupBox();
            lstHistory = new ListBox();
            grpLiveTelemetry.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTelemetry).BeginInit();
            grpCommands.SuspendLayout();
            grpHistory.SuspendLayout();
            SuspendLayout();
            // 
            // grpLiveTelemetry
            // 
            grpLiveTelemetry.Controls.Add(btnStop);
            grpLiveTelemetry.Controls.Add(dgvTelemetry);
            grpLiveTelemetry.Controls.Add(btnStart);
            grpLiveTelemetry.Location = new Point(61, 27);
            grpLiveTelemetry.Name = "grpLiveTelemetry";
            grpLiveTelemetry.Size = new Size(886, 198);
            grpLiveTelemetry.TabIndex = 0;
            grpLiveTelemetry.TabStop = false;
            grpLiveTelemetry.Text = "Live Telemetry";
            // 
            // btnStop
            // 
            btnStop.Location = new Point(507, 155);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(94, 29);
            btnStop.TabIndex = 2;
            btnStop.Text = "Stop";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // dgvTelemetry
            // 
            dgvTelemetry.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTelemetry.Columns.AddRange(new DataGridViewColumn[] { colTimestamp, colDeviceId, colSensorType, colReading, colStatus, colPriority });
            dgvTelemetry.Location = new Point(16, 26);
            dgvTelemetry.Name = "dgvTelemetry";
            dgvTelemetry.RowHeadersWidth = 51;
            dgvTelemetry.Size = new Size(860, 123);
            dgvTelemetry.TabIndex = 3;
            // 
            // colTimestamp
            // 
            colTimestamp.HeaderText = "Timestamp";
            colTimestamp.MinimumWidth = 6;
            colTimestamp.Name = "colTimestamp";
            colTimestamp.Width = 125;
            // 
            // colDeviceId
            // 
            colDeviceId.HeaderText = "Device ID";
            colDeviceId.MinimumWidth = 6;
            colDeviceId.Name = "colDeviceId";
            colDeviceId.Width = 125;
            // 
            // colSensorType
            // 
            colSensorType.HeaderText = "Sensor Type";
            colSensorType.MinimumWidth = 6;
            colSensorType.Name = "colSensorType";
            colSensorType.Width = 125;
            // 
            // colReading
            // 
            colReading.HeaderText = "Reading";
            colReading.MinimumWidth = 6;
            colReading.Name = "colReading";
            colReading.Width = 125;
            // 
            // colStatus
            // 
            colStatus.HeaderText = "Status";
            colStatus.MinimumWidth = 6;
            colStatus.Name = "colStatus";
            colStatus.Width = 125;
            // 
            // colPriority
            // 
            colPriority.HeaderText = "Priority";
            colPriority.MinimumWidth = 6;
            colPriority.Name = "colPriority";
            colPriority.Width = 125;
            // 
            // btnStart
            // 
            btnStart.Location = new Point(126, 155);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(94, 29);
            btnStart.TabIndex = 1;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click_1;
            // 
            // grpCommands
            // 
            grpCommands.Controls.Add(btnUndo);
            grpCommands.Controls.Add(bnSend);
            grpCommands.Controls.Add(cmbCommands);
            grpCommands.Controls.Add(cmbDevices);
            grpCommands.Location = new Point(61, 244);
            grpCommands.Name = "grpCommands";
            grpCommands.Size = new Size(886, 125);
            grpCommands.TabIndex = 1;
            grpCommands.TabStop = false;
            grpCommands.Text = "Device Commands";
            // 
            // btnUndo
            // 
            btnUndo.Location = new Point(594, 82);
            btnUndo.Name = "btnUndo";
            btnUndo.Size = new Size(94, 29);
            btnUndo.TabIndex = 3;
            btnUndo.Text = "Undo";
            btnUndo.UseVisualStyleBackColor = true;
            btnUndo.Click += btnUndo_Click;
            // 
            // bnSend
            // 
            bnSend.Location = new Point(151, 82);
            bnSend.Name = "bnSend";
            bnSend.Size = new Size(94, 29);
            bnSend.TabIndex = 2;
            bnSend.Text = "Send";
            bnSend.UseVisualStyleBackColor = true;
            bnSend.Click += bnSend_Click;
            // 
            // cmbCommands
            // 
            cmbCommands.FormattingEnabled = true;
            cmbCommands.Location = new Point(561, 35);
            cmbCommands.Name = "cmbCommands";
            cmbCommands.Size = new Size(151, 28);
            cmbCommands.TabIndex = 1;
            // 
            // cmbDevices
            // 
            cmbDevices.FormattingEnabled = true;
            cmbDevices.Location = new Point(126, 35);
            cmbDevices.Name = "cmbDevices";
            cmbDevices.Size = new Size(151, 28);
            cmbDevices.TabIndex = 0;
            // 
            // grpHistory
            // 
            grpHistory.Controls.Add(lstHistory);
            grpHistory.Location = new Point(61, 382);
            grpHistory.Name = "grpHistory";
            grpHistory.Size = new Size(886, 127);
            grpHistory.TabIndex = 2;
            grpHistory.TabStop = false;
            grpHistory.Text = "Command History";
            // 
            // lstHistory
            // 
            lstHistory.FormattingEnabled = true;
            lstHistory.Location = new Point(28, 26);
            lstHistory.Name = "lstHistory";
            lstHistory.Size = new Size(627, 84);
            lstHistory.TabIndex = 0;
            // 
            // RealTimeDashboard
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1027, 556);
            Controls.Add(grpHistory);
            Controls.Add(grpCommands);
            Controls.Add(grpLiveTelemetry);
            Name = "RealTimeDashboard";
            Text = "RealTimeDashboard";
            grpLiveTelemetry.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvTelemetry).EndInit();
            grpCommands.ResumeLayout(false);
            grpHistory.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpLiveTelemetry;
        private Button btnStart;
        private Button btnStop;
        private DataGridView dgvTelemetry;
        private GroupBox grpCommands;
        private Button btnUndo;
        private Button bnSend;
        private ComboBox cmbCommands;
        private ComboBox cmbDevices;
        private GroupBox grpHistory;
        private ListBox lstHistory;
        private DataGridViewTextBoxColumn colTimestamp;
        private DataGridViewTextBoxColumn colDeviceId;
        private DataGridViewTextBoxColumn colSensorType;
        private DataGridViewTextBoxColumn colReading;
        private DataGridViewTextBoxColumn colStatus;
        private DataGridViewTextBoxColumn colPriority;
    }
}