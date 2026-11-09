namespace SmartX.App
{
    partial class Form1
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
            lblHeading = new Label();
            Part1 = new Panel();
            btnOpen = new Button();
            lblAbout = new Label();
            lblPart1 = new Label();
            containterPart2 = new Panel();
            btnLocked = new Button();
            label2 = new Label();
            panel1 = new Panel();
            button1 = new Button();
            lblPart3 = new Label();
            label1 = new Label();
            Part1.SuspendLayout();
            containterPart2.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // lblHeading
            // 
            lblHeading.AutoSize = true;
            lblHeading.Location = new Point(447, 22);
            lblHeading.Name = "lblHeading";
            lblHeading.Size = new Size(71, 20);
            lblHeading.TabIndex = 1;
            lblHeading.Text = "SMART-X";
            // 
            // Part1
            // 
            Part1.Controls.Add(btnOpen);
            Part1.Controls.Add(lblAbout);
            Part1.Controls.Add(lblPart1);
            Part1.Location = new Point(196, 74);
            Part1.Name = "Part1";
            Part1.Size = new Size(589, 118);
            Part1.TabIndex = 3;
            // 
            // btnOpen
            // 
            btnOpen.Location = new Point(16, 72);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(94, 29);
            btnOpen.TabIndex = 2;
            btnOpen.Text = "Open";
            btnOpen.UseVisualStyleBackColor = true;
            btnOpen.Click += btnOpen_Click;
            // 
            // lblAbout
            // 
            lblAbout.AutoSize = true;
            lblAbout.Location = new Point(13, 49);
            lblAbout.Name = "lblAbout";
            lblAbout.Size = new Size(431, 20);
            lblAbout.TabIndex = 1;
            lblAbout.Text = "Register sensors, ingest telemetry and monitor sensor behaviour";
            // 
            // lblPart1
            // 
            lblPart1.AutoSize = true;
            lblPart1.Location = new Point(3, 11);
            lblPart1.Name = "lblPart1";
            lblPart1.Size = new Size(244, 20);
            lblPart1.TabIndex = 0;
            lblPart1.Text = "Sensor Data Ingestion and Telemtry";
            // 
            // containterPart2
            // 
            containterPart2.Controls.Add(btnLocked);
            containterPart2.Controls.Add(label2);
            containterPart2.Location = new Point(199, 214);
            containterPart2.Name = "containterPart2";
            containterPart2.Size = new Size(589, 102);
            containterPart2.TabIndex = 4;
            // 
            // btnLocked
            // 
            btnLocked.Location = new Point(13, 53);
            btnLocked.Name = "btnLocked";
            btnLocked.Size = new Size(94, 29);
            btnLocked.TabIndex = 2;
            btnLocked.Text = "Open";
            btnLocked.UseVisualStyleBackColor = true;
            btnLocked.Click += btnLocked_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 11);
            label2.Name = "label2";
            label2.Size = new Size(281, 20);
            label2.TabIndex = 0;
            label2.Text = "Real-Time Command Stream and History";
            // 
            // panel1
            // 
            panel1.Controls.Add(button1);
            panel1.Controls.Add(lblPart3);
            panel1.Location = new Point(199, 338);
            panel1.Name = "panel1";
            panel1.Size = new Size(589, 109);
            panel1.TabIndex = 5;
            // 
            // button1
            // 
            button1.Location = new Point(13, 62);
            button1.Name = "button1";
            button1.Size = new Size(94, 29);
            button1.TabIndex = 2;
            button1.Text = "Open";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // lblPart3
            // 
            lblPart3.AutoSize = true;
            lblPart3.Location = new Point(3, 11);
            lblPart3.Name = "lblPart3";
            lblPart3.Size = new Size(256, 20);
            lblPart3.TabIndex = 0;
            lblPart3.Text = "Network Topology and Mesh Routing";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(341, 42);
            label1.Name = "label1";
            label1.Size = new Size(299, 20);
            label1.TabIndex = 6;
            label1.Text = "IoT Data Ingestion and Monitoring Platform";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(973, 515);
            Controls.Add(label1);
            Controls.Add(panel1);
            Controls.Add(containterPart2);
            Controls.Add(Part1);
            Controls.Add(lblHeading);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            Part1.ResumeLayout(false);
            Part1.PerformLayout();
            containterPart2.ResumeLayout(false);
            containterPart2.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label lblHeading;
        private Panel Part1;
        private Label lblPart1;
        private Button btnOpen;
        private Label lblAbout;
        private Panel containterPart2;
        private Button btnLocked;
        private Label label2;
        private Panel panel1;
        private Button button1;
        private Label lblPart3;
        private Label label1;
    }
}
