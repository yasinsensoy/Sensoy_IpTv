
namespace Sensoy_IpTv
{
    partial class Form2
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.videoPlayer1 = new Sensoy_IpTv.VideoPlayer();
            this.videoPlayer2 = new Sensoy_IpTv.VideoPlayer();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.videoPlayer1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.videoPlayer2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 463);
            this.tableLayoutPanel1.TabIndex = 0;
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
            this.videoPlayer1.Margin = new System.Windows.Forms.Padding(0);
            this.videoPlayer1.Name = "videoPlayer1";
            this.videoPlayer1.Size = new System.Drawing.Size(400, 495);
            this.videoPlayer1.TabIndex = 0;
            // 
            // videoPlayer2
            // 
            this.videoPlayer2.AllowDrop = true;
            this.videoPlayer2.ButonRenk = System.Drawing.Color.Empty;
            this.videoPlayer2.CacheState = null;
            this.videoPlayer2.Chapter = 0;
            this.videoPlayer2.Chapters = null;
            this.videoPlayer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.videoPlayer2.Location = new System.Drawing.Point(400, 0);
            this.videoPlayer2.Margin = new System.Windows.Forms.Padding(0);
            this.videoPlayer2.Name = "videoPlayer2";
            this.videoPlayer2.Size = new System.Drawing.Size(400, 495);
            this.videoPlayer2.TabIndex = 1;
            // 
            // Form2
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(800, 463);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form2";
            this.Text = "Form2";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form2_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private VideoPlayer videoPlayer1;
        private VideoPlayer videoPlayer2;
    }
}