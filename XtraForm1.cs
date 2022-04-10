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
    public partial class XtraForm1 : DevExpress.XtraEditors.XtraForm
    {
        public XtraForm1()
        {
            InitializeComponent();
        }

        private void XtraForm1_Load(object sender, EventArgs e)
        {
            videoPlayer1.MedyaYukle("https://dgeft87wbj63p.cloudfront.net/ea769858fcf5e79fd1a3_elraenn_42485112716_1624986985/chunked/index-dvr.m3u8");
        }
    }
}