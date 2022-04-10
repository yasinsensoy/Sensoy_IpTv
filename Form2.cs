using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sensoy_IpTv
{
    public partial class Form2 : DevExpress.XtraEditors.XtraForm
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            videoPlayer1.MedyaYukle("https://dgeft87wbj63p.cloudfront.net/ea769858fcf5e79fd1a3_elraenn_42485112716_1624986985/chunked/index-dvr.m3u8");
            videoPlayer2.MedyaYukle("https://d1m7jfoe9zdc1j.cloudfront.net/2e3d7aec3b74f7109b16_unlostv_42608578637_1624991435/chunked/index-dvr.m3u8");
        }
    }
}
