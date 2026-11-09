namespace SmartX.App.Forms
{
    partial class Sensors
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
            lblSearch = new Label();
            textBox1 = new TextBox();
            panel1 = new Panel();
            lblStatusHeading = new Label();
            lblCategoryHeading = new Label();
            lblLocationHeading = new Label();
            lblMacheading = new Label();
            btnViewSensors = new Button();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(76, 26);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(53, 20);
            lblSearch.TabIndex = 0;
            lblSearch.Text = "Search";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(147, 28);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(216, 27);
            textBox1.TabIndex = 1;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // panel1
            // 
            panel1.Controls.Add(lblStatusHeading);
            panel1.Controls.Add(lblCategoryHeading);
            panel1.Controls.Add(lblLocationHeading);
            panel1.Controls.Add(lblMacheading);
            panel1.Location = new Point(72, 92);
            panel1.Name = "panel1";
            panel1.Size = new Size(660, 267);
            panel1.TabIndex = 2;
            // 
            // lblStatusHeading
            // 
            lblStatusHeading.AutoSize = true;
            lblStatusHeading.Location = new Point(516, 10);
            lblStatusHeading.Name = "lblStatusHeading";
            lblStatusHeading.Size = new Size(49, 20);
            lblStatusHeading.TabIndex = 3;
            lblStatusHeading.Text = "Status";
            // 
            // lblCategoryHeading
            // 
            lblCategoryHeading.AutoSize = true;
            lblCategoryHeading.Location = new Point(352, 10);
            lblCategoryHeading.Name = "lblCategoryHeading";
            lblCategoryHeading.Size = new Size(69, 20);
            lblCategoryHeading.TabIndex = 2;
            lblCategoryHeading.Text = "Category";
            // 
            // lblLocationHeading
            // 
            lblLocationHeading.AutoSize = true;
            lblLocationHeading.Location = new Point(199, 10);
            lblLocationHeading.Name = "lblLocationHeading";
            lblLocationHeading.Size = new Size(66, 20);
            lblLocationHeading.TabIndex = 1;
            lblLocationHeading.Text = "Location";
            // 
            // lblMacheading
            // 
            lblMacheading.AutoSize = true;
            lblMacheading.Location = new Point(4, 10);
            lblMacheading.Name = "lblMacheading";
            lblMacheading.Size = new Size(98, 20);
            lblMacheading.TabIndex = 0;
            lblMacheading.Text = "MAC Address";
            // 
            // btnViewSensors
            // 
            btnViewSensors.Location = new Point(296, 374);
            btnViewSensors.Name = "btnViewSensors";
            btnViewSensors.Size = new Size(197, 29);
            btnViewSensors.TabIndex = 3;
            btnViewSensors.Text = "View Selected Sensor";
            btnViewSensors.UseVisualStyleBackColor = true;
            btnViewSensors.Click += btnViewSensors_Click;
            // 
            // Sensors
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnViewSensors);
            Controls.Add(panel1);
            Controls.Add(textBox1);
            Controls.Add(lblSearch);
            Name = "Sensors";
            Text = "Sensors";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblSearch;
        private TextBox textBox1;
        private Panel panel1;
        private Label lblStatusHeading;
        private Label lblCategoryHeading;
        private Label lblLocationHeading;
        private Label lblMacheading;
        private Button btnViewSensors;
    }
}