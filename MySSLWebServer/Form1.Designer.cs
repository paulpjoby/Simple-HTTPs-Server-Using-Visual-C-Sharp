namespace MySSLWebServer
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
            Start_Server = new Button();
            Stop_Server = new Button();
            textBox1 = new TextBox();
            SuspendLayout();
            // 
            // Start_Server
            // 
            Start_Server.Location = new Point(12, 12);
            Start_Server.Name = "Start_Server";
            Start_Server.Size = new Size(94, 48);
            Start_Server.TabIndex = 0;
            Start_Server.Text = "Start Server";
            Start_Server.UseVisualStyleBackColor = true;
            Start_Server.Click += Start_Server_Click;
            // 
            // Stop_Server
            // 
            Stop_Server.Enabled = false;
            Stop_Server.Location = new Point(1227, 12);
            Stop_Server.Name = "Stop_Server";
            Stop_Server.Size = new Size(94, 48);
            Stop_Server.TabIndex = 1;
            Stop_Server.Text = "Stop Server";
            Stop_Server.UseVisualStyleBackColor = true;
            Stop_Server.Click += Stop_Server_Click;
            // 
            // textBox1
            // 
            textBox1.Font = new Font("Segoe UI", 24F, FontStyle.Regular, GraphicsUnit.Point);
            textBox1.Location = new Point(12, 78);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(1309, 687);
            textBox1.TabIndex = 2;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1333, 788);
            Controls.Add(textBox1);
            Controls.Add(Stop_Server);
            Controls.Add(Start_Server);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button Start_Server;
        private Button Stop_Server;
        private TextBox textBox1;
    }
}