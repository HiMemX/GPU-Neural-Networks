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
            this.debugTextBox = new System.Windows.Forms.TextBox();
            this.drawingPanel = new System.Windows.Forms.Panel();
            this.canvasResetButton = new System.Windows.Forms.Button();
            this.startTrainingButton = new System.Windows.Forms.Button();
            this.randomTestButton = new System.Windows.Forms.Button();
            this.drawingPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // glControlPanel
            // 
            this.glControlPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.glControlPanel.Location = new System.Drawing.Point(0, 0);
            this.glControlPanel.Margin = new System.Windows.Forms.Padding(2);
            this.glControlPanel.Name = "glControlPanel";
            this.glControlPanel.Size = new System.Drawing.Size(33, 585);
            this.glControlPanel.TabIndex = 0;
            // 
            // debugTextBox
            // 
            this.debugTextBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.debugTextBox.Location = new System.Drawing.Point(592, 0);
            this.debugTextBox.Multiline = true;
            this.debugTextBox.Name = "debugTextBox";
            this.debugTextBox.Size = new System.Drawing.Size(233, 585);
            this.debugTextBox.TabIndex = 1;
            // 
            // drawingPanel
            // 
            this.drawingPanel.Controls.Add(this.randomTestButton);
            this.drawingPanel.Controls.Add(this.canvasResetButton);
            this.drawingPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.drawingPanel.Location = new System.Drawing.Point(33, 0);
            this.drawingPanel.Name = "drawingPanel";
            this.drawingPanel.Size = new System.Drawing.Size(560, 585);
            this.drawingPanel.TabIndex = 2;
            this.drawingPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.drawingPanel_Paint);
            this.drawingPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.drawingPanel_MouseDown);
            this.drawingPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.drawingPanel_MouseMove);
            // 
            // canvasResetButton
            // 
            this.canvasResetButton.Location = new System.Drawing.Point(478, 550);
            this.canvasResetButton.Name = "canvasResetButton";
            this.canvasResetButton.Size = new System.Drawing.Size(75, 23);
            this.canvasResetButton.TabIndex = 3;
            this.canvasResetButton.Text = "Reset";
            this.canvasResetButton.UseVisualStyleBackColor = true;
            this.canvasResetButton.Click += new System.EventHandler(this.canvasResetButton_Click);
            // 
            // startTrainingButton
            // 
            this.startTrainingButton.Location = new System.Drawing.Point(728, 550);
            this.startTrainingButton.Name = "startTrainingButton";
            this.startTrainingButton.Size = new System.Drawing.Size(85, 23);
            this.startTrainingButton.TabIndex = 4;
            this.startTrainingButton.Text = "Start Training";
            this.startTrainingButton.UseVisualStyleBackColor = true;
            this.startTrainingButton.Click += new System.EventHandler(this.startTrainingButton_Click);
            // 
            // randomTestButton
            // 
            this.randomTestButton.Location = new System.Drawing.Point(382, 550);
            this.randomTestButton.Name = "randomTestButton";
            this.randomTestButton.Size = new System.Drawing.Size(90, 23);
            this.randomTestButton.TabIndex = 4;
            this.randomTestButton.Text = "Random Test";
            this.randomTestButton.UseVisualStyleBackColor = true;
            this.randomTestButton.Click += new System.EventHandler(this.randomTestButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(825, 585);
            this.Controls.Add(this.startTrainingButton);
            this.Controls.Add(this.drawingPanel);
            this.Controls.Add(this.debugTextBox);
            this.Controls.Add(this.glControlPanel);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.drawingPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel glControlPanel;
        private System.Windows.Forms.TextBox debugTextBox;
        private System.Windows.Forms.Panel drawingPanel;
        private System.Windows.Forms.Button canvasResetButton;
        private System.Windows.Forms.Button startTrainingButton;
        private System.Windows.Forms.Button randomTestButton;
    }
}

