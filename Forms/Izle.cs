using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Tile.ViewInfo;
using DevExpress.XtraLayout.Utils;
using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Sensoy_IpTv
{
    public partial class Izle : DevExpress.XtraBars.ToolbarForm.ToolbarForm
    {
        public bool focustileview = false;
        private readonly IpTv iptv;
        private Kategori kategori;
        private TimeZoneInfo tz;
        private readonly List<Baslik> baslik = new List<Baslik>(), pbaslik = new List<Baslik>();

        private dynamic GetItem => ((Liste)tileView1.GetRow(tileView1.GetSelectedRows()[0])).Klasor;

        public Izle(IpTv _iptv)
        {
            InitializeComponent();
            iptv = _iptv;
        }

        private void Izle_Load(object sender, EventArgs e)
        {
            Text = $"Şensoy İpTv ({iptv.hesap.Ad})";
            Rectangle size = Screen.GetWorkingArea(this);
            Size = new Size(Convert.ToInt32(size.Width * 0.6), Convert.ToInt32(size.Height * 0.9));
            CenterToScreen();
            Tema.TemaDuzenle(ref TemaSec);
            TimeZoneConverter.TZConvert.TryGetTimeZoneInfo(iptv.Bolge, out tz);
            if (tz != null)
                iptv.SunucuTarih = TimeZoneInfo.ConvertTime(DateTime.Now, tz);
            lblaktifmax.Text = $"{iptv.AktifKullaniciSayisi}/{iptv.MaxKullaniciSayisi}";
            lblacilis.Text = iptv.BaslamaTarih == DateTime.MinValue ? "" : iptv.BaslamaTarih.ToString();
            lblkapanis.Text = iptv.BitisTarih == DateTime.MinValue ? "Yok" : iptv.BitisTarih.ToString();
            lblsunucusaat.Text = iptv.SunucuTarih == DateTime.MinValue ? "" : iptv.SunucuTarih.ToString();
            lblkalan.Text = iptv.KalanSure;
            lblbolge.Text = iptv.Bolge;
            lblturler.Text = string.Join(",", iptv.DesteklenenTurler);
            if (tz != null && iptv.SunucuTarih != DateTime.MinValue)
                timer1.Start();
            bwkategoriyukle.RunWorkerAsync();
        }

        #region Kategori Yükle
        private void bwkategoriyukle_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            IOverlaySplashScreenHandle handle = null;
            if (e.Argument == null)
            {
                Invoke((MethodInvoker)delegate
                {
                    Tema.DurumDegis(true);
                    handle = SplashScreenManager.ShowOverlayForm(listkategoriler);
                    lbldurum.Caption = $"Kategoriler alınıyor...";
                });
                kategori = Komutlar.Kategoriler(iptv.hesap);
            }
            else if (e.Argument is int index)
            {
                dynamic oge = ((Liste)tileView1.GetRow(index)).Klasor;
                if (oge is Medya)
                {
                    if (oge != lblbaslik.Tag)
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            lblvideoboyut.Caption = "";
                            kategori.FocusIndex = index;
                            if (oge.Tip != Tipler.bolum)
                                pbaslik.Clear();
                            else if (pbaslik.Count == 3)
                                pbaslik.Remove(pbaslik.Last());
                            pbaslik.Add(new Baslik(oge.Ad, kategori));
                            baslik.Clear();
                            baslik.AddRange(pbaslik);
                            BaslikGuncelle();
                            videoPlayer1.MedyaYukle(oge.Link);
                            lblbaslik.Tag = oge;
                        });
                    }
                    return;
                }
                Invoke((MethodInvoker)delegate
                {
                    Tema.DurumDegis(true);
                    handle = SplashScreenManager.ShowOverlayForm(listkategoriler);
                    kategori.FocusIndex = index;
                    lbldurum.Caption = $"Veriler alınıyor... ({oge.Ad})";
                });
                if (oge is Dizi)
                {
                    pbaslik.Clear();
                    pbaslik.Add(new Baslik(oge.Ad, kategori));
                }
                else if (oge is Sezon)
                {
                    if (pbaslik.Count == 2)
                        pbaslik.Remove(pbaslik.Last());
                    pbaslik.Add(new Baslik(oge.Ad, kategori));
                }
                if (oge.AltOgeler.Count == 0)
                {
                    if (oge is Klasor)
                    {
                        if (oge.Tip == Tipler.dizik)
                            oge.AltOgeler = Komutlar.GetDiziler(iptv.hesap, oge.Id);
                        else if (oge.Tip == Tipler.canlik)
                            oge.AltOgeler = Komutlar.GetKanallar(iptv.hesap, tz, oge.Id, iptv.DesteklenenTurler.FirstOrDefault(t => t.ToLower() == "m3u8") == null ? "ts" : "m3u8");
                        else if (oge.Tip == Tipler.filmk)
                            oge.AltOgeler = Komutlar.GetFilmler(iptv.hesap, tz, oge.Id);
                    }
                    else if (oge is Dizi)
                        oge.AltOgeler = Komutlar.GetSezonlar(iptv.hesap, tz, oge.Id);
                }
                Kategori kat = new Kategori
                {
                    Onceki = kategori,
                    Ogeler = oge.AltOgeler
                };
                kat.Baslik.AddRange(kategori.Baslik);
                kat.Baslik.Add(new Baslik(oge.Ad, kategori));
                kategori = kat;
            }
            Invoke((MethodInvoker)delegate
            {
                if (e.Argument is string)
                {
                    Tema.DurumDegis(true);
                    handle = SplashScreenManager.ShowOverlayForm(listkategoriler);
                    lbldurum.Caption = $"Kategoriler alınıyor...";
                }
                Yukle();
                Tema.DurumDegis(false);
            });
            SplashScreenManager.CloseOverlayForm(handle);
        }

        private void Ac()
        {
            if (bwkategoriyukle.IsBusy)
                XtraMessageBox.Show("Aktif bir işlem devam ediyor. Lütfen bitmesini bekleyin.", "Meşgul", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            else if (tileView1.SelectedRowsCount == 0)
                XtraMessageBox.Show("Seçili öğe yok.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            else
                bwkategoriyukle.RunWorkerAsync(tileView1.GetSelectedRows()[0]);
        }

        private void Yukle()
        {
            layoutControlItem3.Enabled = kategori.Onceki != null;
            List<Liste> liste = new List<Liste>();
            foreach (dynamic klasor in kategori.Ogeler)
            {
                string alt;
                if (klasor is Medya)
                    alt = klasor.EklenmeTarih != DateTime.MinValue && iptv.SunucuTarih != DateTime.MinValue ? $"{Komutlar.KalanSureDuzenle(iptv.SunucuTarih - klasor.EklenmeTarih, 1)} önce eklendi" : "";
                else
                    alt = klasor is Medya ? "" : klasor.Ogesayisi;
                liste.Add(new Liste(klasor.Ad, alt, klasor));
            }
            listkategoriler.DataSource = liste;
            if (kategori.Ogeler.Count > 0)
                tileView1.MoveBy(kategori.FocusIndex);
            else
            {
                afis.Visible = false;
                lblbilgi.Text = "Veri yok";
            }
            lbldurum.Caption = $"{kategori.Ogeler.Count} öğe yüklendi";
            tileView1.Focus();
            BaslikGuncelle();
        }

        public void BaslikGuncelle()
        {
            if (kategori != null)
                lblkategori.Text = string.Join($"<color={Tema.AnaRenk}> ➜ </color>", kategori.Baslik.Select(a => $"<href={a.Ad}><b><color={Tema.FontRenk}>{a.Ad}</color></b></href>"));
            string b = string.Join($"<color={Tema.AnaRenk}> ➜ </color>", baslik.Select(a => $"<href={a.Ad}><b><color={Tema.FontRenk}>{a.Ad}</color></b></href>"));
            lblbaslik.Text = b == "" ? " " : b;
        }

        private void lblkategori_HyperlinkClick(object sender, DevExpress.Utils.HyperlinkClickEventArgs e)
        {
            if (e.MouseArgs.Button == MouseButtons.Left)
            {
                kategori = kategori.Baslik.Find(a => a.Ad == e.Link).Kategori;
                bwkategoriyukle.RunWorkerAsync("Yükle");
            }
        }

        private void lblbaslik_HyperlinkClick(object sender, DevExpress.Utils.HyperlinkClickEventArgs e)
        {
            if (e.MouseArgs.Button == MouseButtons.Left)
            {
                kategori = baslik.Find(a => a.Ad == e.Link).Kategori;
                bwkategoriyukle.RunWorkerAsync("Yükle");
            }
        }
        #endregion

        #region Tasarım
        private void videoPlayer1_VideoBilgiYuklendi(object sender, VideoBilgiYuklendiArgs e)
        {
            lblvideoboyut.Caption = e.ToString();
            lblcodec.Caption = e.Codec;
        }

        private void videoPlayer1_DurumDegisti(object sender, DurumDegistiArgs e)
        {
            lblstatus.Caption = e.Durum;
            lblstatus.Hint = e.Hata;
            if (e.Durum == "Hata")
                lblbaslik.Tag = null;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            iptv.SunucuTarih = iptv.SunucuTarih.AddSeconds(1);
            lblsunucusaat.Text = iptv.SunucuTarih.ToString();
            lblkalan.Text = iptv.KalanSure;
        }

        private void btngeri_Click(object sender, EventArgs e)
        {
            if (bwkategoriyukle.IsBusy)
            {
                XtraMessageBox.Show("Aktif bir işlem devam ediyor. Lütfen bitmesini bekleyin.", "Meşgul", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            kategori = kategori.Onceki;
            bwkategoriyukle.RunWorkerAsync("Yükle");
        }

        private void toolTipController1_GetActiveObjectInfo(object sender, DevExpress.Utils.ToolTipControllerGetActiveObjectInfoEventArgs e)
        {
            if (e.SelectedControl == listkategoriler)
            {
                TileViewHitInfo hi = tileView1.CalcHitInfo(e.ControlMousePosition);
                if (hi.HitTest == TileControlHitTest.Item)
                {
                    var elemInfo = hi.ItemInfo.Elements[0];
                    var a = TextRenderer.MeasureText(elemInfo.Text, elemInfo.StringInfo.Font);
                    if (a.Width > elemInfo.ItemContentBounds.Width)
                        e.Info = new DevExpress.Utils.ToolTipControlInfo(elemInfo, elemInfo.Text);
                }
            }
        }

        private void tileView1_ItemDoubleClick(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemClickEventArgs e)
        {
            Ac();
        }

        private void tileView1_GotFocus(object sender, EventArgs e)
        {
            Tema.TileViewTema(ref tileView1, true);
        }

        private void tileView1_LostFocus(object sender, EventArgs e)
        {
            Tema.TileViewTema(ref tileView1, false);
        }

        private void listkategoriler_ProcessGridKey(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Control && e.KeyCode == Keys.A)
                tileView1.SelectAll();
            else if (e.KeyCode == Keys.Back)
                btngeri.PerformClick();
            else if (e.KeyCode == Keys.Enter && tileView1.SelectedRowsCount == 1)
                Ac();
            tileView1.BeginSelection();
            switch (e.KeyCode)
            {
                case Keys.Down: tileView1.MoveNext(); break;
                case Keys.Up: tileView1.MovePrev(); break;
                case Keys.Home: tileView1.MoveFirst(); break;
                case Keys.End: tileView1.MoveLast(); break;
                case Keys.PageUp: tileView1.MovePrevPage(); break;
                case Keys.PageDown: tileView1.MoveNextPage(); break;
            }
            tileView1.EndSelection();
        }

        private void tileView1_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            afis.Image = null;
            if (tileView1.SelectedRowsCount == 1)
            {
                LayoutVisibility a = LayoutVisibility.Never;
                dynamic item = GetItem;
                string bilgi = $"<b>Ad:</b> {item.Ad}";
                Size size = new Size(150, 0);
                if (!(item is Klasor))
                {
                    afis.Properties.SizeMode = PictureSizeMode.Stretch;
                    if (item is Medya)
                    {
                        if (item.Tip == Tipler.canli)
                        {
                            size.Width = 100;
                            afis.Properties.SizeMode = PictureSizeMode.Squeeze;
                        }
                        bilgi += item.Sure != "" ? $"\r\n<b>Süre:</b> {item.Sure}" : "";
                        bilgi += item.EklenmeTarih != DateTime.MinValue ? $"\r\n<b>Eklenme Tarih:</b> {item.EklenmeTarih}" : "";
                    }
                    bilgi += item.Tarih != DateTime.MinValue ? $"\r\n<b>Tarih:</b> {item.Tarih.ToShortDateString()}" : "";
                    bilgi += item.Ozet != "" ? $"\r\n<b>Özet:</b> {item.Ozet}" : "";
                    if (Uri.TryCreate(item.Resim, UriKind.Absolute, out Uri uri))
                    {
                        afis.LoadAsync(uri.OriginalString);
                        a = LayoutVisibility.Always;
                    }
                }
                lblbilgi.Text = bilgi;
                size.Height = size.Width == 150 && !(item is Klasor) ? 200 : 60;
                layoutControlItem5.MaxSize = layoutControlItem5.MinSize = size;
                layoutControlItem6.MaxSize = layoutControlItem6.MinSize = new Size(0, size.Height);
                layoutControlItem5.Visibility = a;
            }
            else
            {
                layoutControlItem5.Visibility = LayoutVisibility.Never;
                lblbilgi.Text = "Veri yok.";
            }
        }
        #endregion

        #region Hesaplar Popup Menu
        private void pmbtnac_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Ac();
        }

        private void pmbtnid_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Clipboard.SetText(GetItem.Id);
        }

        private void pmbtnlink_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Clipboard.SetText(GetItem.Link);
        }

        private void popupMenu1_BeforePopup(object sender, System.ComponentModel.CancelEventArgs e)
        {
            dynamic item = null;
            if (tileView1.SelectedRowsCount == 1)
                item = GetItem;
            pmbtnac.Enabled = pmbtnid.Enabled = tileView1.SelectedRowsCount == 1;
            pmbtnid.Visibility = tileView1.SelectedRowsCount == 1 && item.Id != "" ? BarItemVisibility.Always : BarItemVisibility.Never;
            pmbtnlink.Enabled = tileView1.SelectedRowsCount == 1 && item is Medya && item.Link != "";
            pmbtnlink.Visibility = tileView1.SelectedRowsCount == 1 && item is Medya ? BarItemVisibility.Always : BarItemVisibility.Never;
        }

        private void toolTipController1_CalcSize(object sender, DevExpress.Utils.ToolTipControllerCalcSizeEventArgs e)
        {
            var a = TextRenderer.MeasureText(e.ToolTip, e.ShowInfo.Appearance.Font);
            e.Size = new Size(a.Width - e.ShowInfo.RoundRadius, a.Height);
        }

        private void tileView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                popupMenu1.ShowPopup(MousePosition);
        }
        #endregion
    }

    #region Sınıflar

    public enum Tipler { ana, canlik, filmk, dizik, dizi, sezon, canli, film, bolum }

    public class Kategori
    {
        public Kategori Onceki { get; set; }
        public int FocusIndex { get; set; }
        public List<Baslik> Baslik { get; set; } = new List<Baslik>();
        public List<object> Ogeler { get; set; } = new List<object>();
    }

    public class Baslik
    {
        public string Ad { get; set; }
        public Kategori Kategori { get; set; }

        public Baslik(string ad, Kategori kategori)
        {
            Ad = ad;
            Kategori = kategori;
        }
    }

    public class Liste
    {
        public string Ad { get; set; }
        public string OgeSayisi { get; set; }
        public object Klasor { get; set; }

        public Liste(string ad, string ogesayisi, object klasor)
        {
            Ad = ad;
            OgeSayisi = ogesayisi;
            Klasor = klasor;
        }
    }

    public class Klasor
    {
        public string Ad { get; set; }
        public string Id { get; set; }
        public Tipler Tip { get; set; }
        public string Ogesayisi => AltOgeler.Count > 0 ? AltOgeler.Count.ToString() + TipGetir() : "";
        public List<object> AltOgeler { get; set; } = new List<object>();

        public Klasor(string ad, Tipler tip, string id = "")
        {
            Ad = ad;
            Id = id;
            Tip = tip;
        }

        private string TipGetir()
        {
            switch (Tip)
            {
                case Tipler.ana:
                    return " Kategori";
                case Tipler.canlik:
                    return " Kanal";
                case Tipler.filmk:
                    return " Film";
                case Tipler.dizik:
                    return " Dizi";
                default:
                    return "";
            }
        }
    }

    public class Dizi
    {
        public string Ad { get; set; }
        public string Id { get; set; }
        public string Resim { get; set; }
        public string Ozet { get; set; }
        public DateTime Tarih { get; set; }
        public string Ogesayisi => AltOgeler.Count > 0 ? AltOgeler.Count.ToString() + " Sezon" : "";
        public List<object> AltOgeler { get; set; } = new List<object>();

        public Dizi(string ad, string id, string resim, string ozet, DateTime tarih)
        {
            Ad = ad;
            Id = id;
            Resim = resim;
            Ozet = ozet;
            Tarih = tarih;
        }
    }

    public class Sezon
    {
        public string Ad { get; set; }
        public string Resim { get; set; }
        public string Ozet { get; set; }
        public DateTime Tarih { get; set; }
        public string Ogesayisi => AltOgeler.Count > 0 ? AltOgeler.Count.ToString() + " Bölüm" : "";
        public List<object> AltOgeler { get; set; } = new List<object>();

        public Sezon(string ad, string resim, string ozet, DateTime tarih)
        {
            Ad = ad;
            Resim = resim;
            Ozet = ozet;
            Tarih = tarih;
        }
    }

    public class Medya
    {
        public string Ad { get; set; }
        public string Id { get; set; }
        public string Sure { get; set; }
        public string Uzantı { get; set; }
        public string Resim { get; set; }
        public Tipler Tip { get; set; }
        public DateTime EklenmeTarih { get; set; }
        public DateTime Tarih { get; set; }
        public string Ozet { get; set; }
        public Hesap Hesap { get; set; }
        public string Link => $"{Hesap.SunucuUrl}{(Tip == Tipler.canli ? "" : (Tip == Tipler.film ? "/movie" : "/series"))}/{Hesap.KullaniciAd}/{Hesap.Sifre}/{Id}{(Tip == Tipler.canli ? "" : $".{Uzantı}")}";

        public Medya(string ad, string id, string sure, string uzantı, string resim, Tipler tip, DateTime etarih, DateTime tarih, string ozet, Hesap hesap)
        {
            Ad = ad;
            Id = id;
            Sure = sure;
            Uzantı = uzantı;
            Resim = resim;
            Tip = tip;
            EklenmeTarih = etarih;
            Tarih = tarih;
            Ozet = ozet;
            Hesap = hesap;
        }

        public void Yukle(Medya medya)
        {
            DateTime tarih = DateTime.MinValue;
            var info = (dynamic)Komutlar.GetFilm(medya.Hesap, medya.Id);
            string sure = info == null || info.duration_secs == null ? "" : Komutlar.KalanSureDuzenle(TimeSpan.FromSeconds(Convert.ToDouble(info.duration_secs.Value)), 3);
            string ozet = info == null || info.plot == null ? "" : info.plot.Value.ToString();
            if (info != null && info.releasedate != null)
                DateTime.TryParse(info.releasedate.Value.ToString(), out tarih);
            medya.Sure = sure;
            medya.Ozet = ozet;
            medya.Tarih = tarih;
        }
    }
    #endregion "Sınıflar"
}