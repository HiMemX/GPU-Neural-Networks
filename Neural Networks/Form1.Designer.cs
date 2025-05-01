namespace Neural_Networks
{
    partial class Form1
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
            this.glControlPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // glControlPanel
            // 
            this.glControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControlPanel.Location = new System.Drawing.Point(0, 0);
            this.glControlPanel.Name = "glControlPanel";
            this.glControlPanel.Size = new System.Drawing.Size(892, 499);
            this.glControlPanel.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(892, 499);
            this.Controls.Add(this.glControlPanel);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel glControlPanel;
    }
}

