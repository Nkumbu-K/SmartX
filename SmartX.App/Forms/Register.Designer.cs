namespace SmartX.App.Forms
{
    partial class Register
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
            lblReg = new Label();
            lblMac = new Label();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            lblLocation = new Label();
            lblCategory = new Label();
            comboBox1 = new ComboBox();
            btnCancel = new Button();
            btnReg = new Button();
            SuspendLayout();
            // 
            // lblReg
            // 
            lblReg.AutoSize = true;
            lblReg.Location = new Point(320, 26);
            lblReg.Name = "lblReg";
            lblReg.Size = new Size(145, 20);
            lblReg.TabIndex = 0;
            lblReg.Text = "Register New Sensor";
            // 
            // lblMac
            // 
            lblMac.AutoSize = true;
            lblMac.Location = new Point(218, 124);
            lblMac.Name = "lblMac";
            lblMac.Size = new Size(98, 20);
            lblMac.TabIndex = 1;
            lblMac.Text = "MAC Address";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(339, 121);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(254, 27);
            textBox1.TabIndex = 2;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(339, 164);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(254, 27);
            textBox2.TabIndex = 4;
            // 
            // lblLocation
            // 
            lblLocation.AutoSize = true;
            lblLocation.Location = new Point(218, 167);
            lblLocation.Name = "lblLocation";
            lblLocation.Size = new Size(66, 20);
            lblLocation.TabIndex = 3;
            lblLocation.Text = "Location";
            // 
            // lblCategory
            // 
            lblCategory.AutoSize = true;
            lblCategory.Location = new Point(218, 211);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(117, 20);
            lblCategory.TabIndex = 5;
            lblCategory.Text = "Sensor Category";
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(339, 208);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(252, 28);
            comboBox1.TabIndex = 6;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(241, 377);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(94, 29);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnReg
            // 
            btnReg.Location = new Point(476, 377);
            btnReg.Name = "btnReg";
            btnReg.Size = new Size(151, 29);
            btnReg.TabIndex = 8;
            btnReg.Text = "Register Sensor";
            btnReg.UseVisualStyleBackColor = true;
            btnReg.Click += btnReg_Click;
            // 
            // Register
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnReg);
            Controls.Add(btnCancel);
            Controls.Add(comboBox1);
            Controls.Add(lblCategory);
            Controls.Add(textBox2);
            Controls.Add(lblLocation);
            Controls.Add(textBox1);
            Controls.Add(lblMac);
            Controls.Add(lblReg);
            Name = "Register";
            Text = "Register";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblReg;
        private Label lblMac;
        private TextBox textBox1;
        private TextBox textBox2;
        private Label lblLocation;
        private Label lblCategory;
        private ComboBox comboBox1;
        private Button btnCancel;
        private Button btnReg;
    }
}