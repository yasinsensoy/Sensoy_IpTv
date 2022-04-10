using DevExpress.XtraEditors;
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
    public partial class XtraForm2 : DevExpress.XtraEditors.XtraForm
    {
        public XtraForm2()
        {
            InitializeComponent();
        }

        private void XtraForm2_Load(object sender, EventArgs e)
        {
            videoPlayer1.MedyaYukle("https://d1m7jfoe9zdc1j.cloudfront.net/2e3d7aec3b74f7109b16_unlostv_42608578637_1624991435/chunked/index-dvr.m3u8");
        }
    }
}