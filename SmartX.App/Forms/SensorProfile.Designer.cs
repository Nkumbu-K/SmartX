namespace SmartX.App.Forms
{
    partial class SensorProfile
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
            panel1 = new Panel();
            textBox6 = new TextBox();
            textBox5 = new TextBox();
            textBox4 = new TextBox();
            textBox3 = new TextBox();
            textBox2 = new TextBox();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            lblSenforInfo = new Label();
            label6 = new Label();
            panel2 = new Panel();
            textBox1 = new TextBox();
            label7 = new Label();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(textBox6);
            panel1.Controls.Add(textBox5);
            panel1.Controls.Add(textBox4);
            panel1.Controls.Add(textBox3);
            panel1.Controls.Add(textBox2);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(lblSenforInfo);
            panel1.Location = new Point(155, 12);
            panel1.Name = "panel1";
            panel1.Size = new Size(489, 236);
            panel1.TabIndex = 2;
            // 
            // textBox6
            // 
            textBox6.Location = new Point(184, 185);
            textBox6.Name = "textBox6";
            textBox6.Size = new Size(211, 27);
            textBox6.TabIndex = 10;
            // 
            // textBox5
            // 
            textBox5.Location = new Point(138, 152);
            textBox5.Name = "textBox5";
            textBox5.Size = new Size(211, 27);
            textBox5.TabIndex = 9;
            // 
            // textBox4
            // 
            textBox4.Location = new Point(138, 119);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(211, 27);
            textBox4.TabIndex = 8;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(138, 86);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(211, 27);
            textBox3.TabIndex = 7;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(138, 53);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(211, 27);
            textBox2.TabIndex = 6;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(31, 181);
            label5.Name = "label5";
            label5.Size = new Size(147, 20);
            label5.TabIndex = 5;
            label5.Text = "Last Communication:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(31, 147);
            label4.Name = "label4";
            label4.Size = new Size(52, 20);
            label4.TabIndex = 4;
            label4.Text = "Status:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(31, 115);
            label3.Name = "label3";
            label3.Size = new Size(76, 20);
            label3.TabIndex = 3;
            label3.Text = "Category: ";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(31, 83);
            label2.Name = "label2";
            label2.Size = new Size(69, 20);
            label2.TabIndex = 2;
            label2.Text = "Location:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(27, 51);
            label1.Name = "label1";
            label1.Size = new Size(101, 20);
            label1.TabIndex = 1;
            label1.Text = "Mac Address: ";
            // 
            // lblSenforInfo
            // 
            lblSenforInfo.AutoSize = true;
            lblSenforInfo.Location = new Point(61, 15);
            lblSenforInfo.Name = "lblSenforInfo";
            lblSenforInfo.Size = new Size(135, 20);
            lblSenforInfo.TabIndex = 0;
            lblSenforInfo.Text = "Sensor Information";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(25, 305);
            label6.Name = "label6";
            label6.Size = new Size(92, 20);
            label6.TabIndex = 3;
            label6.Text = "Attachments";
            // 
            // panel2
            // 
            panel2.Controls.Add(textBox1);
            panel2.Controls.Add(label7);
            panel2.Location = new Point(155, 291);
            panel2.Name = "panel2";
            panel2.Size = new Size(489, 52);
            panel2.TabIndex = 4;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(149, 11);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(212, 27);
            textBox1.TabIndex = 1;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(24, 14);
            label7.Name = "label7";
            label7.Size = new Size(83, 20);
            label7.TabIndex = 0;
            label7.Text = "File Name: ";
            // 
            // button1
            // 
            button1.Location = new Point(145, 385);
            button1.Name = "button1";
            button1.Size = new Size(94, 29);
            button1.TabIndex = 5;
            button1.Text = "Upload";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(331, 385);
            button2.Name = "button2";
            button2.Size = new Size(130, 29);
            button2.TabIndex = 6;
            button2.Text = "View Telemetry";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Location = new Point(534, 385);
            button3.Name = "button3";
            button3.Size = new Size(94, 29);
            button3.TabIndex = 7;
            button3.Text = "Back";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // SensorProfile
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(panel2);
            Controls.Add(label6);
            Controls.Add(panel1);
            Name = "SensorProfile";
            Text = "SensorProfile";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Panel panel1;
        private TextBox textBox6;
        private TextBox textBox4;
        private TextBox textBox3;
        private TextBox textBox2;
        private Label label5;
        private Label label3;
        private Label label2;
        private Label label1;
        private Label lblSenforInfo;
        private Label label6;
        private Panel panel2;
        private TextBox textBox1;
        private Label label7;
        private Button button1;
        private Button button2;
        private Button button3;
        private TextBox textBox5;
        private Label label4;
    }
}