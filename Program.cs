using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Sensoy_IpTv
{
    static class Program
    {
        public static Giris frm;

        //public static object decrypt_msl_payload(string payload)
        //{
        //    var payloads = Regex.Split(payload.Split(new string[] { "}}" }, StringSplitOptions.RemoveEmptyEntries)[1],@",""signature"":""[0-9A-Za-z/+=]+""}");
        //    payloads = (from x in payloads
        //                select (x + "}")).ToArray();
        //    var payload_chunks = payloads;
        //    var chunks = new List<object>();
        //    foreach (var chunk in payload_chunks)
        //    {
        //        var payloadchunk = (dynamic)JObject.Parse(chunk);
        //        var encryption_envelope = payloadchunk["payload"].Value;
        //        var cipher = new AesCryptoServiceProvider().CreateDecryptor(;
        //        cipher.
        //        //var plaintext = cipher.decrypt(base64.b64decode(json.loads(base64.b64decode(encryption_envelope).decode("utf8"))["ciphertext"]));
        //        //plaintext = json.loads(Padding.unpad(plaintext, 16).decode("utf8"));
        //        //var data = plaintext["data"];
        //        //data = base64.b64decode(data).decode("utf8");
        //        //chunks.Add(data);
        //    }
        //    //var decrypted_payload = json.loads("".join(chunks));
        //    return "";
        //}

        [STAThread]
        static void Main()
        {
            //decrypt_msl_payload(File.ReadAllText(@"C:\Users\Administrator\Desktop\data.json", Encoding.UTF8));
            //string url = "https://www.netflix.com/nq/cadmium/pbo_manifests/%5E1.0.0/router?reqAttempt=1&reqPriority=10&reqName=manifest&clienttype=akira&uiversion=vbe1263cd&browsername=edgeoss&browserversion=90.0.0&osname=windows&osversion=10.0";
            //CookieCollection cookie = new CookieCollection();
            //cookie.Add(new Cookie("NetflixId", "v%3D2%26ct%3DBQAOAAEBEGhe4U_oCKcGsK2G0bxqnoGB0D3yM2Vh4N0pxMlTSwqqkF9bxSEr0nW3tlsJ0a4UqYvfYkSHoyaoWaN79pXD4YTfkq5mZ51-O65ZMdkvYYRbtE35xTkTteYFqU97RGHLKsKn1XJYmNrIqdSYjxMTntz_oiqj7MUCYH0Y0x7uVCn-bU0uRn9t_m8Mlw8ONOHmXzXoM5YLmtI4PDvRYovNNKq8lGWRSE9LN3gX2o-6eHJ6lOObF8CB0D6iYIS3etsLGLTfCCyl-qoN4ySjsa7K8sNrWg7x-Pv-dDapmrDY_bX4pK7M6UX-aa5R07LXokHwp_dzB4wYiilAo0Y8qsjYO5w_sB8X-q3dHgqP0sRNaCLt3V5Zvtc965-0j1jwvYftncNePHhh1458KeHrv86Hyqgjsx1IjWnJ6l3VKcg5K58H6lJ2HoeElYK-nKuNZzRZYdGhNP2v3EyaEu7FFS6sQgqcOX3G67RW_Jt5cu0ST5NMcUc4CCJZ8uRxHRmKSS6_W8g0982DSdReSALw0hp2alQl0_X_dCS2Rj6WQiskTTsSDLPpjYjD0ZNzPyJ8JhHTi-0D3_J7xYTxE006YATiSwNMvx_DaX_gDt3ehf469kTnO39GkoIRo4HiZark8eW8HZB8%26bt%3Ddbl%26ch%3DAQEAEAABABT1WaMM1_sniV3S6KFTEqJrVLpLh4yx-HI.%26mac%3DAQEAEAABABT7iYnYiXemBv_NCHqb749LSWv6VYLcQnw.", "/", ".netflix.com"));
            //string post = File.ReadAllText(@"C:\Users\Administrator\Desktop\post.json", Encoding.UTF8);
            //string kaynak = Komutlar.KaynakKoduAl(url,"POST",post, "text/plain", cookie);
            #region Kurulum
            Kayit.Yukle();
            WindowsFormsSettings.LoadApplicationSettings();
            Assembly asm = typeof(DevExpress.UserSkins.CustomSkin).Assembly;
            SkinManager.Default.RegisterAssembly(asm);
            UserLookAndFeel.Default.StyleChanged += Default_StyleChanged;
            UserLookAndFeel.Default.SetSkinStyle("My Basic", Kayit.ayar.PaletteName);
            Mutex mutex = new Mutex(true, "sensoyiptv", out bool kontrol);
            if (!kontrol)
            {
                XtraMessageBox.Show("Program zaten çalışıyor.");
                return;
            }
            //WindowsFormsSettings.AllowSkinEditorAttach = DefaultBoolean.True;
            //WindowsFormsSettings.AnimationMode = AnimationMode.DisableAll;
            WindowsFormsSettings.FocusRectStyle = DevExpress.Utils.Paint.DXDashStyle.None;
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            #endregion
            bool test = true;
            if (test)
            {
                Form form = new Form();
                Button btn = new Button();
                btn.Click += (s, e) =>
                {
                    Form1 form1 = new Form1();
                    form1.Show();
                };
                form.Controls.Add(btn);
                Application.Run(new Form1());
            }
            else
            {
                frm = new Giris();
                Application.Run(frm);
            }
            GC.KeepAlive(mutex);
        }

        static void Default_StyleChanged(object sender, EventArgs e)
        {
            var commonSkin = CommonSkins.GetSkin(UserLookAndFeel.Default);
            var def = commonSkin.SvgPalettes[Skin.DefaultSkinPaletteName].Colors.ToDictionary(r => r.Name);
            Tema.TemaRenkler = def;
            if (def != null)
            {
                if (frm != null && frm.frm != null && frm.frm.Visible)
                {
                    if (def.TryGetValue("Accent Paint", out SvgColor item4) && def.TryGetValue("Brush", out SvgColor item3))
                    {
                        frm.frm.videoPlayer1.ButonRenk = item4.Value;
                        string anarenk = Tema.AnaRenk;
                        string fontrenk = Tema.FontRenk;
                        string ahtmlcode = ColorTranslator.ToHtml(item4.Value);
                        string fhtmlcode = ColorTranslator.ToHtml(item3.Value);
                        if (anarenk != ahtmlcode || fontrenk != fhtmlcode)
                        {
                            anarenk = ahtmlcode;
                            fontrenk = fhtmlcode;
                            frm.frm.BaslikGuncelle();
                        }
                    }
                    if (def.TryGetValue("Paint High", out SvgColor item2))
                    {
                        frm.frm.xtraScrollableControl1.BackColor = item2.Value;
                        frm.frm.tableLayoutPanel1.BackColor = item2.Value;
                        frm.frm.videoPlayer1.BackColor = item2.Value;
                    }
                }
                if (def.TryGetValue("Pencere", out SvgColor item))
                {
                    if (Tema.Mesgul)
                        item.Value = Tema.MesgulRenk;
                    else
                        item.Reset();
                }
            }
            Tema.Kaydet(UserLookAndFeel.Default.ActiveSvgPaletteName);
        }
    }
}