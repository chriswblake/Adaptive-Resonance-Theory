namespace Ch3_ART1_Clustering
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
            this.tbResults = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tbResults
            // 
            this.tbResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbResults.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbResults.Location = new System.Drawing.Point(0, 0);
            this.tbResults.Multiline = true;
            this.tbResults.Name = "tbResults";
            this.tbResults.Size = new System.Drawing.Size(974, 927);
            this.tbResults.TabIndex = 0;
            this.tbResults.TabStop = false;
            this.tbResults.WordWrap = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(974, 927);
            this.Controls.Add(this.tbResults);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbResults;
    }
}

