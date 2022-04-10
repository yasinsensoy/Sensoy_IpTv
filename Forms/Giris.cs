using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Tile.ViewInfo;
using DevExpress.XtraLayout.Utils;
using DevExpress.XtraSplashScreen;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sensoy_IpTv
{
    public partial class Giris : DevExpress.XtraBars.ToolbarForm.ToolbarForm
    {
        public Izle frm;
        private readonly object _lock = new object();
        private IpTv iptv;
        private IpTv diptv;
        private readonly BindingList<IpTv> hesaplar = new BindingList<IpTv>();

        private IpTv GetItem => (IpTv)tileView1.GetRow(tileView1.GetSelectedRows()[0]);

        public Giris()
        {
            InitializeComponent();
        }

        private void Giris_Load(object sender, EventArgs e)
        {
            toolbarFormManager1.ForceInitialize();
            Tema.TemaDuzenle(ref TemaSec);
            bwlisteyukle.RunWorkerAsync();
        }

        #region Tasarım
        private void layoutControl1_Resize(object sender, EventArgs e)
        {
            Size = new Size(layoutControl1.Width, layoutControl1.Bottom + barDockControl2.Height);
        }

        private void toolbarFormControl1_SizeChanged(object sender, EventArgs e)
        {
            layoutControl1.Top = toolbarFormControl1.Height;
        }

        private void tileView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
                tileView1.SelectAll();
            else if (e.KeyCode == Keys.Enter)
                Baglan();
            else if (e.KeyCode == Keys.Delete && tileView1.SelectedRowsCount > 0)
                pmbtnsil.PerformClick();
        }

        private void tileView1_ItemDoubleClick(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemClickEventArgs e)
        {
            Baglan();
        }

        private void tileView1_GotFocus(object sender, EventArgs e)
        {
            Tema.TileViewTema(ref tileView1, true);
        }

        private void tileView1_LostFocus(object sender, EventArgs e)
        {
            Tema.TileViewTema(ref tileView1, false);
        }

        private void tileView1_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            lbladet.Caption = $"{tileView1.SelectedRowsCount}/{tileView1.DataRowCount}";
        }

        private void toolTipController1_GetActiveObjectInfo(object sender, DevExpress.Utils.ToolTipControllerGetActiveObjectInfoEventArgs e)
        {
            if (e.SelectedControl == listhesaplar)
            {
                TileViewHitInfo hi = tileView1.CalcHitInfo(e.ControlMousePosition);
                if (hi.HitTest == TileControlHitTest.Item)
                {
                    TileItemElementViewInfo elemInfo;
                    if (hi.ItemInfo.Elements[1].Text == "Hata")
                        elemInfo = hi.ItemInfo.Elements[2];
                    else
                        elemInfo = hi.ItemInfo.Elements[0];
                    var a = TextRenderer.MeasureText(elemInfo.Text, elemInfo.StringInfo.Font);
                    if (a.Width > elemInfo.ItemContentBounds.Width)
                        e.Info = new DevExpress.Utils.ToolTipControlInfo(elemInfo, elemInfo.Text);
                }
            }
        }

        private void radioGroup1_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool a = radioGroup1.SelectedIndex == 0;
            if (!a)
                layoutControl1.AutoSizeMode = AutoSizeMode.GrowOnly;
            layoutControlItem6.Visibility = (LayoutVisibility)radioGroup1.SelectedIndex;
            if (a)
                layoutControl1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            layoutControlItem2.ContentVisible = a;
            txthesapad.Text = "";
            txtsurl.Text = "";
            txtkad.Text = "";
            txtsifre.Text = "";
            txtkad.Properties.AdvancedModeOptions.Label = a ? "Kullanıcı Adı" : "Mac Adresi";
            txtkad.Properties.MaskSettings.MaskManagerType = a ? null : typeof(DevExpress.Data.Mask.RegExpMaskManager);
            txtkad.Properties.MaskSettings.MaskExpression = a ? "" : "([0-9A-Fa-f]{2}\\:){5}[0-9A-Fa-f]{2}";
            txthesapad.Focus();
        }
        #endregion

        #region Hesaplar
        private void Baglan()
        {
            if (bwbaglan.IsBusy || bwekleduzenle.IsBusy || bwlisteyukle.IsBusy)
            {
                XtraMessageBox.Show("Aktif bir işlem devam ediyor. Lütfen bitmesini bekleyin.", "Meşgul", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (tileView1.SelectedRowsCount == 0)
            {
                XtraMessageBox.Show("Seçili item yok.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            btniptal.PerformClick();
            bwbaglan.RunWorkerAsync(GetItem.hesap);
        }

        private void bwbaglan_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            IOverlaySplashScreenHandle handle = null;
            Hesap hesap = (Hesap)e.Argument;
            iptv = null;
            Invoke((MethodInvoker)delegate
            {
                Tema.DurumDegis(true);
                handle = SplashScreenManager.ShowOverlayForm(this);
                lbldurum.Caption = $"Sunucuya bağlanılıyor... ({hesap.HesapAd})";
            });
            object cevap = Komutlar.Giris(hesap);
            if (cevap is string)
                Invoke((Action)(() => XtraMessageBox.Show(cevap.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error)));
            else
                iptv = (IpTv)cevap;
            Invoke((MethodInvoker)delegate
            {
                Tema.DurumDegis(false);
                lbldurum.Caption = $"Sunucuya bağlandı. ({hesap.HesapAd})";
            });
            SplashScreenManager.CloseOverlayForm(handle);
        }

        private void bwbaglan_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (iptv != null)
            {
                Hide();
                frm = new Izle(iptv);
                frm.FormClosed += Frm_FormClosed;
                frm.Show();
            }
        }

        private void Frm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Show();
            frm = null;
        }

        private void bwlisteyukle_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            IOverlaySplashScreenHandle handle = null;
            Invoke((MethodInvoker)delegate
            {
                Tema.DurumDegis(true);
                lbldurum.Caption = "Hesaplar yükleniyor...";
                handle = SplashScreenManager.ShowOverlayForm(listhesaplar);
                listhesaplar.DataSource = hesaplar;
            });
            Parallel.ForEach(Kayit.ayar.Hesaplar, hesap =>
            {
                object cevap = Komutlar.Giris(hesap);
                lock (_lock)
                {
                    Invoke((MethodInvoker)delegate
                    {
                        if (cevap is string)
                            hesaplar.Add(new IpTv(hesap, "Hata", cevap.ToString()));
                        else
                            hesaplar.Add((IpTv)cevap);
                    });
                }
            });
            Invoke((MethodInvoker)delegate
            {
                Tema.DurumDegis(false);
                lbldurum.Caption = "Hesaplar yüklendi.";
                tileView1.MoveBy(0);
                tileView1.Focus();
            });
            SplashScreenManager.CloseOverlayForm(handle);
        }
        #endregion

        #region Hesaplar Popup Menu
        private void pmbtnbaglan_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Baglan();
        }

        private void pmbtnduzenle_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (bwbaglan.IsBusy || bwekleduzenle.IsBusy || bwlisteyukle.IsBusy)
            {
                XtraMessageBox.Show("Aktif bir işlem devam ediyor. Lütfen bitmesini bekleyin.", "Meşgul", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (layoutControlItem7.ContentVisible)
                diptv.Durum = btniptal.Tag.ToString();
            diptv = GetItem;
            btniptal.Tag = diptv.Durum;
            diptv.Durum = "Düzenleniyor";
            radioGroup1.SelectedIndex = 0;
            radioGroup1.Properties.Items[radioGroup1.SelectedIndex == 0 ? 1 : 0].Enabled = false;
            txthesapad.Text = diptv.hesap.HesapAd;
            txtsurl.Text = diptv.hesap.SunucuUrl;
            txtkad.Text = diptv.hesap.KullaniciAd;
            txtsifre.Text = diptv.hesap.Sifre;
            btnekleduzenle.Text = "Düzenle";
            btnekleduzenle.Enabled = false;
            layoutControlItem7.ContentVisible = true;
            tileView1.RefreshData();
        }

        private void pmbtnsil_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (bwbaglan.IsBusy || bwekleduzenle.IsBusy || bwlisteyukle.IsBusy)
            {
                XtraMessageBox.Show("Aktif bir işlem devam ediyor. Lütfen bitmesini bekleyin.", "Meşgul", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            DialogResult cevap = XtraMessageBox.Show("Seçilen hesapları silmek istediğinize emin misiniz?", "Siliniyor", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (cevap != DialogResult.Yes)
                return;
            foreach (int index in tileView1.GetSelectedRows())
            {
                IpTv item = (IpTv)tileView1.GetRow(index);
                Kayit.ayar.Hesaplar.Remove(item.hesap);
            }
            tileView1.DeleteSelectedRows();
            Kayit.Kaydet();
        }

        private void pmbtnm3uurl_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            IpTv ip = GetItem;
            string link = $"{ip.hesap.SunucuUrl}/get.php?username={ip.hesap.KullaniciAd}&password={ip.hesap.Sifre}&type=m3u_plus&output={(ip.DesteklenenTurler.FirstOrDefault(t => t.ToLower() == "m3u8") == null ? "ts" : "m3u8")}";
            Clipboard.SetText(link);
            //using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
            //{
            //    string veri = wc.DownloadString(link);
            //    veri = veri.Replace("\r", "");
            //    string[] b = veri.Split(new string[] { "#EXTINF" }, StringSplitOptions.RemoveEmptyEntries);
            //    List<string[]> medyalar = new List<string[]>();
            //    foreach (string item in b)
            //    {
            //        if (item.StartsWith("#EXTM3U"))
            //            continue;
            //        string[] a = item.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            //        medyalar.Add(a);
            //    }
            //}
            XtraMessageBox.Show($"M3U bağlantısı kopyalandı.\n{link}", "Kopyalandı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void popupMenu1_BeforePopup(object sender, System.ComponentModel.CancelEventArgs e)
        {
            pmbtnsil.Caption = tileView1.SelectedRowsCount == 1 ? "Sil" : "Seçilenleri Sil";
            pmbtnm3uurl.Enabled = pmbtnduzenle.Enabled = pmbtnbaglan.Enabled = tileView1.SelectedRowsCount == 1;
            pmbtnsil.Enabled = tileView1.SelectedRowsCount > 0;
        }

        private void tileView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                popupMenu1.ShowPopup(MousePosition);
        }
        #endregion

        #region Hesap Ekle Düzenle
        private void btnm3u_Click(object sender, EventArgs e)
        {
            if (bwbaglan.IsBusy || bwekleduzenle.IsBusy || bwlisteyukle.IsBusy)
            {
                XtraMessageBox.Show("Aktif bir işlem devam ediyor. Lütfen bitmesini bekleyin.", "Meşgul", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (Uri.TryCreate(Clipboard.GetText(), UriKind.Absolute, out Uri uri))
            {
                string kad = Komutlar.TagYakala(uri.AbsoluteUri, "username=", "&");
                string sifre = Komutlar.TagYakala(uri.AbsoluteUri, "password=", "&");
                if (kad == "")
                    XtraMessageBox.Show("Yanlış url. Lütfen geçerli bir M3U url kopyalayın.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    txthesapad.Text = uri.Host;
                    txtsurl.Text = $"{uri.Scheme}://{uri.Authority}";
                    txtkad.Text = kad;
                    txtsifre.Text = sifre;
                }
            }
            else
                XtraMessageBox.Show("Hatalı url. Lütfen geçerli bir M3U url kopyalayın.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btniptal_Click(object sender, EventArgs e)
        {
            if (bwbaglan.IsBusy || bwekleduzenle.IsBusy || bwlisteyukle.IsBusy)
            {
                XtraMessageBox.Show("Aktif bir işlem devam ediyor. Lütfen bitmesini bekleyin.", "Meşgul", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            btnekleduzenle.Text = "Ekle";
            btnekleduzenle.Enabled = true;
            layoutControlItem7.ContentVisible = false;
            radioGroup1.Properties.Items[radioGroup1.SelectedIndex == 0 ? 1 : 0].Enabled = true;
            diptv.Durum = btniptal.Tag.ToString();
            tileView1.RefreshData();
        }

        private void btnekleduzenle_Click(object sender, EventArgs e)
        {
            if (bwbaglan.IsBusy || bwekleduzenle.IsBusy || bwlisteyukle.IsBusy)
            {
                XtraMessageBox.Show("Aktif bir işlem devam ediyor. Lütfen bitmesini bekleyin.", "Meşgul", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (string.IsNullOrEmpty(txthesapad.Text))
            {
                XtraMessageBox.Show("Hesap adı boş olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txthesapad.Focus();
            }
            else if (string.IsNullOrEmpty(txtsurl.Text))
            {
                XtraMessageBox.Show("Sunucu url boş olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtsurl.Focus();
            }
            else if (!Uri.TryCreate(txtsurl.Text, UriKind.Absolute, out Uri suri))
            {
                XtraMessageBox.Show("Sunucu url biçimi hatalı.\nÖrnek: http://sunucu:port", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtsurl.Focus();
            }
            else if (string.IsNullOrEmpty(txtkad.Text))
            {
                XtraMessageBox.Show("Kullanıcı adı boş olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtkad.Focus();
            }
            else if (Kayit.ayar.Hesaplar.FirstOrDefault(h => h.Ad == $"{txthesapad.Text} [{txtkad.Text}]" && (btnekleduzenle.Text == "Ekle" || (btnekleduzenle.Text == "Düzenle" && h.HesapAd != diptv.hesap.HesapAd))) != null)
            {
                XtraMessageBox.Show("Aynı hesap adı daha önce kullanılmış.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txthesapad.Focus();
            }
            else if (Kayit.ayar.Hesaplar.FirstOrDefault(h => h.SunucuUrl == $"{suri.Scheme}://{suri.Authority}" && h.KullaniciAd == txtkad.Text && h.Sifre == txtsifre.Text && (btnekleduzenle.Text == "Ekle" || (btnekleduzenle.Text == "Düzenle" && h.HesapAd != diptv.hesap.HesapAd))) != null)
            {
                XtraMessageBox.Show("Aynı hesap daha önce eklenmiş.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtsurl.Focus();
            }
            else
            {
                txtsurl.Text = $"{suri.Scheme}://{suri.Authority}";
                bwekleduzenle.RunWorkerAsync();
            }
        }

        private void Duzenle(object sender, EventArgs e)
        {
            if (btnekleduzenle.Text == "Düzenle")
                btnekleduzenle.Enabled = diptv.hesap.HesapAd != txthesapad.Text || diptv.hesap.SunucuUrl != txtsurl.Text || diptv.hesap.KullaniciAd != txtkad.Text || diptv.hesap.Sifre != txtsifre.Text;
            if (sender == txtsurl && txthesapad.Text == "" && Uri.TryCreate(txtsurl.Text, UriKind.Absolute, out Uri suri))
                txthesapad.Text = suri.Host;
        }

        private void bwekleduzenle_DoWork(object sender, DoWorkEventArgs e)
        {
            IOverlaySplashScreenHandle handle = null;
            Invoke((MethodInvoker)delegate
            {
                Tema.DurumDegis(true);
                handle = SplashScreenManager.ShowOverlayForm(this);
                lbldurum.Caption = $"Sunucuya bağlanılıyor... ({txtsurl.Text})";
            });
            Hesap hesap = new Hesap(txthesapad.Text, txtsurl.Text, txtkad.Text, txtsifre.Text);
        tekrar:
            object cevap = Komutlar.Giris(hesap);
            if (cevap is string)
            {
                DialogResult m = DialogResult.None;
                Invoke((Action)(() => m = XtraMessageBox.Show(cevap.ToString(), "Hata", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error)));
                if (m == DialogResult.Retry)
                    goto tekrar;
            }
            else
            {
                IpTv iptv = (IpTv)cevap;
                if (btnekleduzenle.Text == "Düzenle")
                {
                    diptv.hesap.Yukle(hesap);
                    diptv.Yukle(iptv);
                    Invoke((MethodInvoker)delegate
                    {
                        btnekleduzenle.Text = "Ekle";
                        btnekleduzenle.Enabled = true;
                        radioGroup1.Properties.Items[radioGroup1.SelectedIndex == 0 ? 1 : 0].Enabled = true;
                        layoutControlItem7.ContentVisible = false;
                        lbldurum.Caption = $"Hesap düzenlendi. ({diptv.hesap.HesapAd})";
                        tileView1.RefreshData();
                    });
                }
                else if (btnekleduzenle.Text == "Ekle")
                {
                    Kayit.ayar.Hesaplar.Add(hesap);
                    Invoke((MethodInvoker)delegate
                    {
                        hesaplar.Add(iptv);
                        lbldurum.Caption = $"Hesap eklendi. ({hesap.HesapAd})";
                        tileView1.MoveLast();
                    });
                }
                Kayit.Kaydet();
            }
            Invoke((MethodInvoker)delegate
            {
                if (cevap is string)
                    lbldurum.Caption = "";
                Tema.DurumDegis(false);
            });
            SplashScreenManager.CloseOverlayForm(handle);
        }
        #endregion

        private void toolTipController1_CalcSize(object sender, DevExpress.Utils.ToolTipControllerCalcSizeEventArgs e)
        {
            var a = TextRenderer.MeasureText(e.ToolTip, e.ShowInfo.Appearance.Font);
            e.Size = new Size(a.Width - e.ShowInfo.RoundRadius, a.Height);
        }
    }
}