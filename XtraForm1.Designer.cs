
namespace Sensoy_IpTv
{
    partial class XtraForm1
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
            this.videoPlayer1 = new Sensoy_IpTv.VideoPlayer();
            this.SuspendLayout();
            // 
            // videoPlayer1
            // 
            this.videoPlayer1.AllowDrop = true;
            this.videoPlayer1.ButonRenk = System.Drawing.Color.Empty;
            this.videoPlayer1.CacheState = null;
            this.videoPlayer1.Chapter = 0;
            this.videoPlayer1.Chapters = null;
            this.videoPlayer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.videoPlayer1.Location = new System.Drawing.Point(0, 0);
            this.videoPlayer1.Name = "videoPlayer1";
            this.videoPlayer1.Size = new System.Drawing.Size(298, 268);
            this.videoPlayer1.TabIndex = 0;
            // 
            // XtraForm1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(298, 268);
            this.Controls.Add(this.videoPlayer1);
            this.Name = "XtraForm1";
            this.Text = "XtraForm1";
            this.Load += new System.EventHandler(this.XtraForm1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private VideoPlayer videoPlayer1;
    }
}