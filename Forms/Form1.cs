using DevExpress.XtraEditors;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Sensoy_IpTv
{
    public partial class Form1 : DevExpress.XtraBars.ToolbarForm.ToolbarForm
    {
        public Form1()
        {
            InitializeComponent();
        }
        private string size, durum, codec;
        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            videoPlayer1.Player.API.Command(false, "load-script", "ytdl_hook.lua");
            videoPlayer1.Player.API.Command(false, "load-script", "twitch-chat.lua");
            //videoPlayer1.Player.LogLevel = Mpv.NET.API.MpvLogLevel.Debug;
        }

        private void barButtonItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) => videoPlayer1.MedyaYukle(@"http://ftnh1881.xyz:8080/series/yasinsensoy/BdV4CwmwwA/69420.mp4");

        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) => videoPlayer1.MedyaYukle(@"Y:\Filmler\Yabancı\[2020] Bad Boys Her Zaman Çılgın [TR].mkv");

        private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) => videoPlayer1.MedyaYukle(@"http://ftnh1881.xyz:8080/live/yasinsensoy/BdV4CwmwwA/6366.m3u8");

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string[] p = textBox1.Text.Split(' ');
                videoPlayer1.Player.API.Command(true, p);
                label1.Text = "Başarılı";
            }
            catch (Exception ex)
            {
                label1.Text = ex.Message;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                string[] p = textBox1.Text.Split(' ');
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        videoPlayer1.Player.API.SetPropertyString(p[0], p[1], true);
                        break;
                    case 1:
                        videoPlayer1.Player.API.SetPropertyLong(p[0], Convert.ToInt64(p[1]), true);
                        break;
                    case 2:
                        videoPlayer1.Player.API.SetPropertyDouble(p[0], Convert.ToDouble(p[1]), true);
                        break;
                }
                label1.Text = "Başarılı";
            }
            catch (Exception ex)
            {
                label1.Text = ex.Message;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string[] p = textBox1.Text.Split(' ');
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        textBox2.Text = videoPlayer1.Player.API.GetPropertyString(p[0], true);
                        break;
                    case 1:
                        textBox2.Text = videoPlayer1.Player.API.GetPropertyLong(p[0], true).ToString();
                        break;
                    case 2:
                        textBox2.Text = videoPlayer1.Player.API.GetPropertyDouble(p[0], true).ToString();
                        break;
                }
                label1.Text = "Başarılı";
            }
            catch (Exception ex)
            {
                textBox2.Text = "";
                label1.Text = ex.Message;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string[] plist = videoPlayer1.Player.API.GetPropertyString("property-list").Split(',');
            string cevap = "";
            foreach (var item in plist)
                cevap += $"{item}\r\n{videoPlayer1.Player.API.GetPropertyString(item)}\r\n\r\n";
            File.WriteAllText(@"D:\property-list.txt", cevap, Encoding.UTF8);
        }

        private void repositoryItemButtonEdit1_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (e.Button.Kind == DevExpress.XtraEditors.Controls.ButtonPredefines.OK)
                videoPlayer1.MedyaYukle(barEditItem1.EditValue.ToString(), true, barEditItem4.EditValue.ToString());
            else if (xtraOpenFileDialog1.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                barEditItem1.EditValue = xtraOpenFileDialog1.FileName;
                repositoryItemButtonEdit1_ButtonClick(repositoryItemButtonEdit1, new DevExpress.XtraEditors.Controls.ButtonPressedEventArgs(repositoryItemButtonEdit1.Buttons[0]));
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string oku = videoPlayer1.Player.API.GetPropertyString("demuxer-cache-state");
            if (oku == null)
                return;
            var r = JObject.Parse(oku);
            var t = r.Value<JToken>("seekable-ranges");
            var d = string.Join("\r\n", t.Children().Select(a => string.Join(" --> ", a.Values().Select(b => $"{Format(TimeSpan.FromSeconds(b.Value<double>()))}"))));
            label2.Text = $"Cache Size: {BoyutDuzenle(r.Value<long>("file-cache-bytes"))}\r\n" +
                $"Forward Byte: {BoyutDuzenle(r.Value<long>("fw-bytes"))}\r\n" +
                $"Backward Byte: {BoyutDuzenle(r.Value<long>("total-bytes") - r.Value<long>("fw-bytes"))}\r\n" +
                $"Speed: {BoyutDuzenle(r.Value<long>("raw-input-rate"))}\r\n" + d;
        }
        private string Format(TimeSpan s) => s.ToString($"{(s.TotalHours < 1 ? "" : "hh\\:")}mm\\:ss");
        private string BoyutDuzenle(long size, int decimals = 3)
        {
            string[] sizes = { "Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            double formattedSize = size;
            int sizeIndex = 0;
            while (formattedSize >= 1024 && sizeIndex < sizes.Length)
            {
                formattedSize /= 1024;
                sizeIndex += 1;
            }
            return string.Format("{0} {1}", Math.Round(formattedSize, decimals).ToString("N3"), sizes[sizeIndex]);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;
        }

        private void barButtonItem5_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Program.frm = new Giris();
            Program.frm.Show();
        }

        private void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Form2 frm = new Form2();
            frm.Show();
        }

        private void barButtonItem6_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            XtraForm1 frm1 = new XtraForm1();
            XtraForm2 frm2 = new XtraForm2();
            frm1.Show();
            frm2.Show();
        }

        private void videoPlayer1_VideoBilgiYuklendi(object sender, VideoBilgiYuklendiArgs e)
        {
            Text = $"{codec = e.Codec} {size = e.ToString()} {durum}";
            string mt = videoPlayer1.Player.API.GetPropertyString("media-title");
            string fn = videoPlayer1.Player.API.GetPropertyString("filename");
            barStaticItem1.Caption = mt == "" ? fn : mt;
        }

        private void videoPlayer1_DurumDegisti(object sender, DurumDegistiArgs e)
        {
            if (e.Durum == "Hata")
                XtraMessageBox.Show(e.Hata);
            Text = $"{codec} {size} {durum = e.Durum}";
        }
    }
}
