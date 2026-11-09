namespace SmartX.App.Forms
{
    partial class Telemetry
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
            btnRegister = new Button();
            btnViewSensor = new Button();
            panel1 = new Panel();
            lblStatus = new Label();
            lblReading = new Label();
            lblCategory = new Label();
            lblSensor = new Label();
            lblHead = new Label();
            panel2 = new Panel();
            lblTitles = new Label();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // btnRegister
            // 
            btnRegister.Location = new Point(178, 371);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(121, 29);
            btnRegister.TabIndex = 0;
            btnRegister.Text = "Register Sensor";
            btnRegister.UseVisualStyleBackColor = true;
            btnRegister.Click += btnRegister_Click;
            // 
            // btnViewSensor
            // 
            btnViewSensor.Location = new Point(438, 371);
            btnViewSensor.Name = "btnViewSensor";
            btnViewSensor.Size = new Size(138, 29);
            btnViewSensor.TabIndex = 1;
            btnViewSensor.Text = "View Sensors";
            btnViewSensor.UseVisualStyleBackColor = true;
            btnViewSensor.Click += btnViewSensor_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(lblStatus);
            panel1.Controls.Add(lblReading);
            panel1.Controls.Add(lblCategory);
            panel1.Controls.Add(lblSensor);
            panel1.Controls.Add(lblHead);
            panel1.Location = new Point(90, 27);
            panel1.Name = "panel1";
            panel1.Size = new Size(576, 171);
            panel1.TabIndex = 2;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(482, 39);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(49, 20);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "Status";
            // 
            // lblReading
            // 
            lblReading.AutoSize = true;
            lblReading.Location = new Point(315, 39);
            lblReading.Name = "lblReading";
            lblReading.Size = new Size(64, 20);
            lblReading.TabIndex = 3;
            lblReading.Text = "Reading";
            // 
            // lblCategory
            // 
            lblCategory.AutoSize = true;
            lblCategory.Location = new Point(140, 39);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(69, 20);
            lblCategory.TabIndex = 2;
            lblCategory.Text = "Category";
            // 
            // lblSensor
            // 
            lblSensor.AutoSize = true;
            lblSensor.Location = new Point(9, 38);
            lblSensor.Name = "lblSensor";
            lblSensor.Size = new Size(53, 20);
            lblSensor.TabIndex = 1;
            lblSensor.Text = "Sensor";
            // 
            // lblHead
            // 
            lblHead.AutoSize = true;
            lblHead.Location = new Point(30, 6);
            lblHead.Name = "lblHead";
            lblHead.Size = new Size(145, 20);
            lblHead.TabIndex = 0;
            lblHead.Text = "SENSOR TELEMETRY";
            // 
            // panel2
            // 
            panel2.Controls.Add(lblTitles);
            panel2.Location = new Point(89, 217);
            panel2.Name = "panel2";
            panel2.Size = new Size(577, 125);
            panel2.TabIndex = 3;
            // 
            // lblTitles
            // 
            lblTitles.AutoSize = true;
            lblTitles.Location = new Point(38, 9);
            lblTitles.Name = "lblTitles";
            lblTitles.Size = new Size(118, 20);
            lblTitles.TabIndex = 0;
            lblTitles.Text = "RECENT EVENTS";
            // 
            // Telemetry
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(btnViewSensor);
            Controls.Add(btnRegister);
            Name = "Telemetry";
            Text = "Telemetry";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button btnRegister;
        private Button btnViewSensor;
        private Panel panel1;
        private Label lblStatus;
        private Label lblReading;
        private Label lblCategory;
        private Label lblSensor;
        private Label lblHead;
        private Panel panel2;
        private Label lblTitles;
    }
}