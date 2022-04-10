using DevExpress.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Tile;
using DevExpress.XtraGrid.Views.Tile.ViewInfo;
using Mpv.NET.Player;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Sensoy_IpTv
{
    public partial class VideoPlayer : UserControl
    {
        private MpvPlayer mp;
        private bool muted = false, tamekran = false, vdegis = false, idegis = false, tek = false, iaktif = false, loading = false, cursor = true;
        private Control econtrol;
        private DockStyle edock;
        private Form fullScreenForm;
        private Color butonrenk;
        private object ctrl;
        private Point yer;
        private string adres;
        private int kses;
        private TimeFormat timeformat;
        private List<string> opt;
        private Dictionary<Bitmap, int> loadingbmp;
        private readonly Bitmap loadbmp = new Bitmap(@"C:\Users\Administrator\Desktop\YouTube_loading_symbol_3_(transparent).gif");

        private enum TimeFormat
        {
            Normal,
            Kalan,
            Ayrinti
        }

        public VideoPlayer() => InitializeComponent();

        #region Properties
        [Browsable(false)]
        public MpvPlayer Player => mp;

        [Browsable(false)]
        public DemuxerCacheState CacheState { get; set; }

        [Browsable(false)]
        public ChapterList[] Chapters { get; set; }

        [Browsable(false)]
        public int Chapter { get; set; }

        private bool Bar
        {
            get => layoutControl1.Visible;
            set
            {
                if (Bar == value)
                    return;
                if (value)
                    SureAyarla();
                layoutControl1.Visible = value;
            }
        }

        private Size VideoArea => new Size(video.Width, video.Height - (Bar ? layoutControl1.Height : 0) - (layoutControl2.Visible ? layoutControl2.Height : 0));

        private int SubPos => video.Height == 0 ? 100 : VideoArea.Height * 100 / video.Height;

        private int vol = 0;
        private int Volume
        {
            get => vol;
            set
            {
                if (mp != null)
                {
                    if ((value >= 100 && Volume == 100) || (value <= 0 && Volume == 0))
                        return;
                    if (value < 0)
                        mp.Volume = 0;
                    else if (value > 100)
                        mp.Volume = 100;
                    else
                        mp.Volume = value;
                }
            }
        }

        private double speed = 1;
        private double Speed
        {
            get => speed;
            set
            {
                if (mp != null)
                {
                    if ((value >= 3 && Speed == 3) || (value <= 0.25 && Speed == 0.25))
                        return;
                    if (value < 0.25)
                        mp.Speed = 0.25;
                    else if (value > 3)
                        mp.Speed = 3;
                    else
                        mp.Speed = value;
                }
            }
        }

        private TimeSpan position = TimeSpan.Zero;
        private TimeSpan Position
        {
            get => position;
            set
            {
                if (mp != null && Duration.TotalSeconds > 0)
                {
                    if (value < TimeSpan.Zero)
                        mp.Position = TimeSpan.Zero;
                    else if (value > Duration)
                        mp.Position = Duration;
                    else
                        mp.Position = value;
                }
            }
        }

        private TimeSpan Duration { get; set; } = TimeSpan.Zero;

        [Browsable(false)]
        private string GecenSure => Format(Position);

        [Browsable(false)]
        private string KalanSure => $"-{Format(mp != null ? Duration - Position : TimeSpan.Zero)}";

        [Browsable(false)]
        private string Sure => Format(Duration);

        private string Format(TimeSpan s) => s.ToString($"{(s.TotalHours < 1 ? "" : "hh\\:")}mm\\:ss{(timeformat == TimeFormat.Ayrinti ? "\\.fff" : "")}");

        public Color ButonRenk
        {
            get => butonrenk;
            set
            {
                playpause.ItemAppearance.Normal.FillColor = value;
                stop.ItemAppearance.Normal.FillColor = value;
                refresh.ItemAppearance.Normal.FillColor = value;
                volume.ItemAppearance.Normal.FillColor = value;
                fullscreen.ItemAppearance.Normal.FillColor = value;
                medya.ItemAppearance.Normal.FillColor = value;
                chapterback.ItemAppearance.Normal.FillColor = value;
                chapternext.ItemAppearance.Normal.FillColor = value;
                chapters.ItemAppearance.Normal.FillColor = value;
                butonrenk = value;
            }
        }
        #endregion

        #region Custom Events
        public event EventHandler<VideoBilgiYuklendiArgs> VideoBilgiYuklendi;
        public event EventHandler<DurumDegistiArgs> DurumDegisti;
        #endregion

        #region Komutlar
        public void MedyaYukle(string url, bool oynat = true, params string[] options)
        {
            if (mp == null)
            {
                DurumDegis("Hata", "Video player yüklenemedi.");
                return;
            }
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                adres = uri.OriginalString;
                try
                {
                    var list = options.Where(a => a != "").ToList();
                    opt = list.ToList();
                    list.RemoveAll(a => a.Contains("pause="));
                    list.Add($"pause={(oynat ? "no" : "yes")}");
                    mp.Load(uri.AbsoluteUri, list.ToArray());
                }
                catch (Exception ex)
                {
                    DurumDegis("Hata", ex.Message);
                }
            }
            else
                DurumDegis("Hata", "Hatalı url biçimi.");
        }

        private void SureAyarla()
        {
            if (mp != null)
            {
                ilerleme.InvokeIfRequired(i => i.Properties.Maximum = (int)Duration.TotalMilliseconds);
                layoutControl1.InvokeIfRequired(s => sure.Text = $"{GecenSure} | {(timeformat == TimeFormat.Kalan ? KalanSure : Sure)}{(Chapters != null && Chapters.Length > 0 ? $" - {Chapters[Chapter].Title}" : "")}");
                if (!idegis)
                    ilerleme.InvokeIfRequired(i => i.Position = (int)(i.Properties.Maximum * Position.TotalMilliseconds / Duration.TotalMilliseconds));
            }
        }

        private void Sifirla()
        {
            position = TimeSpan.Zero;
            Duration = TimeSpan.Zero;
            playpause.InvokeIfRequired(p => p.SvgImage = Properties.Resources.player_play);
            playpause.Tag = "Oynat";
            Chapters = null;
            listchapters.InvokeIfRequired(l => l.Visible = false);
            layoutControl1.InvokeIfRequired(l => layoutControlGroup5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never);
            SureAyarla();
            buffer.InvokeIfRequired(b => b.Visible = false);
            loading = false;
        }

        private Dictionary<Bitmap, int> GetFrames(Bitmap img, int speedpercent = 0)
        {
            if (speedpercent < 0)
                speedpercent = 0;
            else if (speedpercent > 100)
                speedpercent = 100;
            Dictionary<Bitmap, int> frames = new Dictionary<Bitmap, int>();
            FrameDimension dimension = new FrameDimension(img.FrameDimensionsList.First());
            int numberOfFrames = img.GetFrameCount(dimension);
            if (numberOfFrames > 1)
            {
                byte[] fdv = img.GetPropertyItem(0x5100).Value;
                for (int i = 0; i < numberOfFrames; i++)
                {
                    int framedelay = BitConverter.ToInt32(fdv, 4 * i) * 10;
                    if (framedelay == 0)
                        framedelay = 90;
                    img.SelectActiveFrame(dimension, i);
                    frames.Add((Bitmap)img.Clone(), framedelay - (framedelay * speedpercent / 100));
                }
            }
            else
                frames.Add((Bitmap)img.Clone(), 0);
            return frames;
        }

        private void DurumDegis(string durum, string hata = "") => this.InvokeIfRequired(a => DurumDegisti?.Invoke(this, new DurumDegistiArgs(durum, hata)));
        private void VideoBilgiYukle(Size size, string codec = "") => this.InvokeIfRequired(a => VideoBilgiYuklendi?.Invoke(this, new VideoBilgiYuklendiArgs(size, codec)));
        #endregion

        #region Kurulum
        private void Detach()
        {
            if (mp == null)
                return;
            mp.PositionChanged -= Mp_PositionChanged;
            mp.DurationChanged -= Mp_DurationChanged;
            mp.VolumeChanged -= Mp_VolumeChanged;
            mp.MediaBuffering -= Mp_MediaBuffering;
            mp.MediaSeeking -= Mp_MediaSeeking;
            mp.MediaPaused -= Mp_Paused;
            mp.MediaFinished -= Mp_MediaFinished;
            mp.MediaResumed -= Mp_Playing;
            mp.MediaStoped -= Mp_MediaStoped;
            mp.MediaError -= Mp_MediaError;
            mp.VideoUpdated -= Mp_VideoUpdated;
            mp.SpeedChanged -= Mp_SpeedChanged;
            mp.TrackListChanged -= Mp_TrackListChanged;
            mp.DemuxerCacheStateChanged -= Mp_DemuxerCacheStateChanged;
            mp.ChapterListChanged -= Mp_ChapterListChanged;
            mp.ChapterChanged -= Mp_ChapterChanged;
            loading = false;
            while (bwloading.IsBusy)
                Application.DoEvents();
            mp.Dispose();
        }

        private void VideoPlayer_Load(object sender, EventArgs e)
        {
            if (IsInDesignMode)
                return;
            try
            {
                if (Tema.TemaRenkler.TryGetValue("Paint High", out SvgColor item))
                    BackColor = item.Value;
                if (Tema.TemaRenkler.TryGetValue("Accent Paint", out SvgColor item1))
                    ButonRenk = item1.Value;
                ParentForm.FormClosing += (s, d) => { Detach(); };
                mp = new MpvPlayer(video.Handle, Path.Combine(Application.StartupPath, "mpv-1.dll"));
                //ThreadPool.QueueUserWorkItem((a) => { loadingbmp = GetFrames(loadbmp); });
                mp.API.SetPropertyString("cache-dir", "d:\\cache");
                mp.API.SetPropertyString("cache-on-disk", "yes");
                mp.API.SetPropertyString("demuxer-max-bytes", "1024000000");
                mp.API.SetPropertyString("demuxer-max-back-bytes", "1024000000");
                mp.API.SetPropertyString("hwdec", "auto");
                mp.API.SetPropertyString("sub-font-size", "42");
                mp.Volume = Kayit.ayar.PlayerVolume;
                mp.KeepOpen = KeepOpen.Yes;
                mp.PositionChanged += Mp_PositionChanged;
                mp.DurationChanged += Mp_DurationChanged;
                mp.VolumeChanged += Mp_VolumeChanged;
                mp.MediaBuffering += Mp_MediaBuffering;
                mp.MediaSeeking += Mp_MediaSeeking;
                mp.MediaPaused += Mp_Paused;
                mp.MediaFinished += Mp_MediaFinished;
                mp.MediaResumed += Mp_Playing;
                mp.MediaStoped += Mp_MediaStoped;
                mp.MediaError += Mp_MediaError;
                mp.VideoUpdated += Mp_VideoUpdated;
                mp.SpeedChanged += Mp_SpeedChanged;
                mp.TrackListChanged += Mp_TrackListChanged;
                mp.DemuxerCacheStateChanged += Mp_DemuxerCacheStateChanged;
                mp.ChapterListChanged += Mp_ChapterListChanged;
                mp.ChapterChanged += Mp_ChapterChanged;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ParentForm, ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsInDesignMode
        {
            get
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                    return true;
                Control ctrl = this;
                while (ctrl != null)
                {
                    if ((ctrl.Site != null) && ctrl.Site.DesignMode)
                        return true;
                    ctrl = ctrl.Parent;
                }
                return false;
            }
        }
        #endregion

        #region MP Events
        private void Mp_ChapterChanged(object sender, MpvChapterEventArgs e)
        {
            Chapter = e.Chapter < 0 ? 0 : e.Chapter;
            if (Chapter > 0)
            {
                chapterback.Tag = Chapters[Chapter - 1].Title;
                chapterback.InvokeIfRequired(c => c.Enabled = true);
            }
            else
                chapterback.InvokeIfRequired(c => c.Enabled = false);
            if (Chapter < Chapters.Length - 1)
            {
                chapternext.Tag = Chapters[Chapter + 1].Title;
                chapternext.InvokeIfRequired(c => c.Enabled = true);
            }
            else
                chapternext.InvokeIfRequired(c => c.Enabled = false);
        }

        private void Mp_ChapterListChanged(object sender, MpvChapterListEventArgs e)
        {
            Chapters = e.List;
            listchapters.InvokeIfRequired(l => l.DataSource = Chapters);
            layoutControl1.InvokeIfRequired(s => layoutControlGroup5.Visibility = Chapters.Length > 1 ? DevExpress.XtraLayout.Utils.LayoutVisibility.Always : DevExpress.XtraLayout.Utils.LayoutVisibility.Never);
        }

        private void Mp_DemuxerCacheStateChanged(object sender, MpvDemuxerCacheStateEventArgs e)
        {
            CacheState = e.State;
            ilerleme.InvokeIfRequired(i => i.Update());
        }

        private void Mp_TrackListChanged(object sender, EventArgs e)
        {
            int.TryParse(mp.Vid, out int vid);
            int.TryParse(mp.Aid, out int aid);
            int.TryParse(mp.Sid, out int sid);
            listvideo.InvokeIfRequired(l => l.DataSource = mp.Videos);
            listvideo.InvokeIfRequired(l => tileView1.MoveBy(vid));
            listaudio.InvokeIfRequired(l => l.DataSource = mp.Audios);
            listaudio.InvokeIfRequired(l => tileView2.MoveBy(aid));
            listsub.InvokeIfRequired(l => l.DataSource = mp.Subs);
            listsub.InvokeIfRequired(l => tileView3.MoveBy(sid));
        }

        private void Mp_MediaSeeking(object sender, MpvSeekingEventArgs e)
        {
            loading = e.Seeking;
            if (!bwloading.IsBusy)
                bwloading.RunWorkerAsync();
        }

        private void Mp_MediaBuffering(object sender, MpvBufferingEventArgs e)
        {
            loading = e.Buffer < 100;
            if (loading)
            {
                DurumDegis("Yükleniyor");
                if (!bwloading.IsBusy)
                    bwloading.RunWorkerAsync();
            }
            buffer.InvokeIfRequired(b => b.Visible = loading);
            buffer.InvokeIfRequired(b => b.Position = e.Buffer);
        }

        private void Mp_VideoUpdated(object sender, EventArgs e)
        {
            VideoBilgiYukle(mp.Size, mp.Codec);
        }

        private void Mp_DurationChanged(object sender, MpvTimeEventArgs e)
        {
            Duration = e.Time;
            if (!Bar)
                return;
            SureAyarla();
        }

        private void Mp_PositionChanged(object sender, MpvTimeEventArgs e)
        {
            position = e.Time;
            if (!Bar)
                return;
            SureAyarla();
        }

        private void Mp_Playing(object sender, EventArgs e)
        {
            playpause.InvokeIfRequired(p => p.SvgImage = Properties.Resources.player_pause);
            playpause.Tag = "Duraklat";
            DurumDegis("Oynatılıyor");
        }

        private void Mp_Paused(object sender, EventArgs e)
        {
            playpause.InvokeIfRequired(p => p.SvgImage = Properties.Resources.player_play);
            playpause.Tag = "Oynat";
            DurumDegis("Duraklatıldı");
        }

        private void Mp_MediaError(object sender, MpvErrorEventArgs e)
        {
            Sifirla();
            DurumDegis("Hata", e.Text);
        }

        private void Mp_MediaFinished(object sender, EventArgs e)
        {
            Sifirla();
            DurumDegis("Bitti");
        }

        private void Mp_MediaStoped(object sender, EventArgs e)
        {
            Sifirla();
            DurumDegis("Durduruldu");
        }

        private void Mp_VolumeChanged(object sender, MpvVolumeEventArgs e)
        {
            vol = e.Volume;
            if (Volume > 0 && Volume <= 50)
                volume.InvokeIfRequired(v => v.SvgImage = Properties.Resources.player_low_volume);
            else if (Volume > 50)
                volume.InvokeIfRequired(v => v.SvgImage = Properties.Resources.player_volume_adjustment);
            else
                volume.InvokeIfRequired(v => v.SvgImage = Properties.Resources.player_mute);
            if (Kayit.ayar.PlayerVolume != Volume)
            {
                Kayit.ayar.PlayerVolume = Volume;
                Kayit.Kaydet();
            }
            muted = Volume == 0;
            volume.Tag = muted ? "Sesi Aç" : "Sustur";
            mp.ShowText(muted ? "Sessiz" : $"Ses: {Volume}", TextAlign.TopRight);
            volumebar.InvokeIfRequired(v => v.Position = Volume);
        }

        private void Mp_SpeedChanged(object sender, MpvSpeedEventArgs e)
        {
            speed = e.Speed;
            mp.ShowText($"Hız: {Speed}", TextAlign.TopRight);
        }
        #endregion

        #region Fullscreen
        private void Fullscreen()
        {
            fullscreen.Tag = tamekran ? "Büyüt" : "Küçült";
            yer = Point.Empty;
            if (tamekran)
            {
                econtrol.Controls.Add(this);
                Dock = edock;
                fullScreenForm.FormClosed -= FullScreenForm_FormClosed;
                ParentForm.Show();
                fullScreenForm.Close();
            }
            else
            {
                econtrol = Parent;
                edock = Dock;
                Screen screen = Screen.FromControl(ParentForm);
                fullScreenForm = new Form
                {
                    FormBorderStyle = FormBorderStyle.None,
                    WindowState = FormWindowState.Maximized,
                    StartPosition = FormStartPosition.Manual,
                    Location = screen.Bounds.Location,
                    Text = ParentForm.Text,
                    Icon = ParentForm.Icon,
                    ForeColor = ParentForm.ForeColor,
                    BackColor = ParentForm.BackColor
                };
                fullScreenForm.FormClosed += FullScreenForm_FormClosed;
                ParentForm.Hide();
                fullScreenForm.Controls.Add(this);
                Dock = DockStyle.Fill;
                fullScreenForm.Show();
            }
            tamekran = !tamekran;
            fullscreen.SvgImage = tamekran ? Properties.Resources.player_minimize : Properties.Resources.player_maximize;
            BringToFront();
            video.Focus();
        }

        private void FullScreenForm_FormClosed(object sender, FormClosedEventArgs e) => econtrol.FindForm().Close();
        #endregion

        #region Controls
        #region video
        private void video_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Fullscreen();
        }

        private void video_MouseEnter(object sender, EventArgs e)
        {
            video.Focus();
            timer1.Stop();
            if (cursor)
                Bar = true;
        }

        private void video_MouseMove(object sender, MouseEventArgs e)
        {
            if (yer != e.Location)
            {
                yer = e.Location;
                if (tek)
                {
                    tek = false;
                    return;
                }
                ctrl = sender;
                Bar = true;
                if (!cursor)
                {
                    Cursor.Show();
                    cursor = true;
                }
                timer2.Stop();
                timer2.Start();
            }
        }

        private void video_MouseLeave(object sender, EventArgs e)
        {
            timer1.Start();
            if (tek || (sender != layoutControl1 && sender != buffer && sender != video))
                return;
            Bar = true;
            if (!cursor)
            {
                Cursor.Show();
                cursor = true;
            }
            timer2.Stop();
        }

        private void video_SizeChanged(object sender, EventArgs e)
        {
            int p = SubPos;
            if (mp != null)
                mp.SetSubPos(p);
        }

        private void video_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    playpause_MouseClick(playpause, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                    break;
                case Keys.F:
                    Fullscreen();
                    break;
                case Keys.M:
                    volume_MouseClick(volume, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                    break;
                case Keys.Right:
                    if (mp != null) mp.Seek($"+{(e.Control ? 60 : (e.Shift ? 3 : 10))}", InputMethod.OsdMsg, SeekMethod.Relative, SeekMethod.Exact);
                    break;
                case Keys.Left:
                    if (mp != null) mp.Seek($"-{(e.Control ? 60 : (e.Shift ? 3 : 10))}", InputMethod.OsdMsg, SeekMethod.Relative, SeekMethod.Exact);
                    break;
                case Keys.Up:
                    Volume += e.Control ? 10 : 5;
                    break;
                case Keys.Down:
                    Volume -= e.Control ? 10 : 5;
                    break;
                case Keys.B:
                    chapterback_MouseClick(chapterback, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                    break;
                case Keys.N:
                    chapternext_MouseClick(chapternext, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                    break;
                case Keys.T:
                    medya_MouseClick(medya, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                    break;
                case Keys.C:
                    chapters_MouseClick(chapters, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                    break;
                case Keys.Escape:
                    if (tamekran) Fullscreen();
                    break;
                case Keys.E:
                    if (mp != null) mp.NextFrame();
                    break;
                case Keys.Q:
                    if (mp != null) mp.PreviousFrame();
                    break;
                case Keys.S:
                    stop_MouseClick(stop, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                    break;
                case Keys.R:
                    refresh_MouseClick(refresh, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                    break;
                case Keys.P:
                    playpause_MouseClick(playpause, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                    break;
                case Keys.NumPad0:
                    if (mp != null) mp.Seek("0", InputMethod.OsdMsg, SeekMethod.AbsolutePercent);
                    break;
                case Keys.NumPad1:
                    if (mp != null) mp.Seek("10", InputMethod.OsdMsg, SeekMethod.AbsolutePercent);
                    break;
                case Keys.NumPad2:
                    if (mp != null) mp.Seek("20", InputMethod.OsdMsg, SeekMethod.AbsolutePercent);
                    break;
                case Keys.NumPad3:
                    if (mp != null) mp.Seek("30", InputMethod.OsdMsg, SeekMethod.AbsolutePercent);
                    break;
                case Keys.NumPad4:
                    if (mp != null) mp.Seek("40", InputMethod.OsdMsg, SeekMethod.AbsolutePercent);
                    break;
                case Keys.NumPad5:
                    if (mp != null) mp.Seek("50", InputMethod.OsdMsg, SeekMethod.AbsolutePercent);
                    break;
                case Keys.NumPad6:
                    if (mp != null) mp.Seek("60", InputMethod.OsdMsg, SeekMethod.AbsolutePercent);
                    break;
                case Keys.NumPad7:
                    if (mp != null) mp.Seek("70", InputMethod.OsdMsg, SeekMethod.AbsolutePercent);
                    break;
                case Keys.NumPad8:
                    if (mp != null) mp.Seek("80", InputMethod.OsdMsg, SeekMethod.AbsolutePercent);
                    break;
                case Keys.NumPad9:
                    if (mp != null) mp.Seek("90", InputMethod.OsdMsg, SeekMethod.AbsolutePercent);
                    break;
                case Keys.End:
                    if (mp != null) mp.Seek("100", InputMethod.OsdMsg, SeekMethod.AbsolutePercent);
                    break;
                case Keys.Home:
                    if (mp != null) mp.Seek("0", InputMethod.OsdMsg, SeekMethod.AbsolutePercent);
                    break;
                case Keys.Add:
                    Speed += 0.25;
                    break;
                case Keys.Subtract:
                    Speed -= 0.25;
                    break;
            }
            e.IsInputKey = true;
        }
        #endregion

        #region volume
        private void volume_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && mp != null)
            {
                if (!muted)
                    kses = Volume;
                Volume = muted ? (kses == 0 ? 50 : kses) : 0;
                if (muted)
                    kses = 0;
            }
        }

        private void volume_MouseEnter(object sender, EventArgs e)
        {
            timer3.Stop();
            timer1.Stop();
            volumebar.BringToFront();
            volumebar.Location = new Point(volume.Left, layoutControl1.Top - volumebar.Height);
            volumebar.Visible = true;
        }

        private void volume_MouseLeave(object sender, EventArgs e)
        {
            timer3.Start();
            timer1.Start();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta > 0)
                if (iaktif && mp != null)
                    mp.Seek("+5", SeekMethod.Relative, SeekMethod.KeyFrames);
                else
                    Volume += 5;
            else if (e.Delta < 0)
                if (iaktif && mp != null)
                    mp.Seek("-5", SeekMethod.Relative, SeekMethod.KeyFrames);
                else
                    Volume -= 5;
        }
        #endregion

        #region refresh
        private void refresh_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && mp != null && !string.IsNullOrEmpty(adres))
            {
                TimeSpan konum = mp.EndReached ? TimeSpan.Zero : Position;
                opt.RemoveAll(a => a.Contains("start="));
                opt.Add($"start={konum.TotalSeconds.ToString(CultureInfo.InvariantCulture)}");
                MedyaYukle(adres, true, opt.ToArray());
            }
        }
        #endregion

        #region ilerleme
        private void ilerleme_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                idegis = true;
                ilerleme.Position = (int)(e.X * (long)ilerleme.Properties.Maximum / ilerleme.Width);
            }
        }

        private void ilerleme_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                idegis = false;
                Position = TimeSpan.FromMilliseconds(e.X * mp.Duration.TotalMilliseconds / ilerleme.Width);
            }
        }

        private void ilerleme_MouseMove(object sender, MouseEventArgs e)
        {
            if (mp == null || volumebar.Visible)
                return;
            int x = e.X <= 0 ? 0 : (e.X >= ilerleme.Width ? ilerleme.Width : e.X);
            TimeSpan ts = TimeSpan.FromMilliseconds(x * Duration.TotalMilliseconds / ilerleme.Width);
            string c = "";
            if (Chapters != null)
            {
                double t = ts.TotalSeconds;
                if (Chapters.Length > 0 && t >= Duration.TotalSeconds)
                    c = Chapters.Last().Title;
                else
                {
                    for (int i = 0; i < Chapters.Length; i++)
                    {
                        double st = (i + 1) == Chapters.Length ? Duration.TotalSeconds : Chapters[i + 1].Time;
                        if (t >= Chapters[i].Time && t < st)
                        {
                            c = Chapters[i].Title;
                            break;
                        }
                    }
                }
            }
            toolTipController1.ShowHint(Format(ts) + (c == "" ? "" : $" - {c}"), ToolTipLocation.TopCenter, ilerleme.PointToScreen(new Point(x, -ilerleme.Top)));
            if (idegis)
                ilerleme.Position = (int)(x * (long)ilerleme.Properties.Maximum / ilerleme.Width);
        }

        private void ilerleme_MouseEnter(object sender, EventArgs e)
        {
            timer1.Stop();
            iaktif = true;
        }

        private void ilerleme_MouseLeave(object sender, EventArgs e)
        {
            timer1.Start();
            iaktif = false;
        }

        private void ilerleme_Paint(object sender, PaintEventArgs e)
        {
            if (CacheState != null && CacheState.SeekableRanges != null)
            {
                foreach (Ranges range in CacheState?.SeekableRanges)
                {
                    float h = ilerleme.Height * 0.2f;
                    float y = ilerleme.Height / 2.0f - h / 2;
                    float x1 = (float)(ilerleme.Width * TimeSpan.FromSeconds(range.Start).TotalMilliseconds / Duration.TotalMilliseconds);
                    float x2 = (float)(ilerleme.Width * TimeSpan.FromSeconds(range.End).TotalMilliseconds / Duration.TotalMilliseconds);
                    e.Graphics.FillRectangle(Brushes.Red, x1, y, x2 - x1, h);
                }
            }
        }
        #endregion

        #region volumebar
        private void volumebar_MouseEnter(object sender, EventArgs e)
        {
            timer3.Stop();
            timer1.Stop();
        }

        private void volumebar_MouseLeave(object sender, EventArgs e)
        {
            timer3.Start();
            timer1.Start();
        }

        private void volumebar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                vdegis = true;
                Volume = (volumebar.Height - e.Y) * 100 / volumebar.Height;
            }
        }

        private void volumebar_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                vdegis = false;
        }

        private void volumebar_MouseMove(object sender, MouseEventArgs e)
        {
            int y = e.Y <= 0 ? 0 : (e.Y >= volumebar.Height ? volumebar.Height : e.Y);
            toolTipController1.ShowHint(((volumebar.Height - y) * 100 / volumebar.Height).ToString(), DevExpress.Utils.ToolTipLocation.LeftCenter, volumebar.PointToScreen(new Point(0, y)));
            if (vdegis)
                Volume = (volumebar.Height - y) * 100 / volumebar.Height;
        }
        #endregion

        #region playpause
        private void playpause_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && mp != null && mp.IsMediaLoaded)
            {
                if (mp.IsPlaying)
                    mp.Pause();
                else
                {
                    if (mp.EndReached)
                        Position = TimeSpan.Zero;
                    mp.Resume();
                }
            }
        }
        #endregion

        #region medya
        private void medya_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                layoutControl2.BringToFront();
                layoutControl2.Visible = !layoutControl2.Visible;
            }
        }

        private void tileView1_ItemDoubleClick(object sender, TileViewItemClickEventArgs e)
        {
            if (mp == null || loading)
                return;
            TileView tw = (TileView)sender;
            Track t = (Track)tw.GetRow(tw.GetSelectedRows()[0]);
            if (t.Selected)
                return;
            string id = $"{t.Id}";
            bool isp = mp.IsPlaying;
            mp.Pause();
            if (t.Type == "video")
                mp.Vid = id;
            else if (t.Type == "audio")
                mp.Aid = id;
            else if (t.Type == "sub")
                mp.Sid = id;
            if (isp)
                mp.Resume();
        }

        private void btnadd_Click(object sender, EventArgs e)
        {
            if (mp == null || loading)
                return;
            XtraInputBoxArgs args = new XtraInputBoxArgs
            {
                Caption = "Medya Yükle",
                Prompt = "Medya yolu",
                Owner = ParentForm,
                DefaultButtonIndex = 0
            };
            ButtonEdit editor = new ButtonEdit();
            editor.Properties.ButtonClick += (a, b) =>
            {
                if (xtraOpenFileDialog1.ShowDialog(ParentForm) == DialogResult.OK)
                    editor.Text = xtraOpenFileDialog1.FileName;
            };
            args.Editor = editor;
            string result = (string)XtraInputBox.Show(args);
            if (string.IsNullOrEmpty(result))
                return;
            if (Uri.TryCreate(result, UriKind.Absolute, out Uri uri))
            {
                bool isp = mp.IsPlaying;
                try
                {
                    mp.Pause();
                    mp.AddMedia(((SimpleButton)sender).Tag.ToString(), uri.OriginalString);
                    if (isp)
                        mp.Resume();
                }
                catch (Exception ex)
                {
                    if (isp)
                        mp.Resume();
                    XtraMessageBox.Show(ParentForm, ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
                XtraMessageBox.Show(ParentForm, "Geçersiz url.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #region popupmenu1
        private void tileView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                popupMenu1.ShowPopup(MousePosition, sender);
        }

        private void popupMenu1_BeforePopup(object sender, CancelEventArgs e)
        {
            TileView tw = (TileView)popupMenu1.Activator;
            popupMenu1.Tag = tw;
            bool a = tw.SelectedRowsCount > 0 && !((Track)tw.GetRow(tw.GetSelectedRows()[0])).Selected;
            pmtracksec.Enabled = a;
            pmtracksil.Enabled = a && ((Track)tw.GetRow(tw.GetSelectedRows()[0])).External;
            pmtrackyenile.Enabled = tw.SelectedRowsCount > 0 && ((Track)tw.GetRow(tw.GetSelectedRows()[0])).External;
        }

        private void pmtrackekle_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            object o = null;
            if (popupMenu1.Tag == tileView1)
                o = btnvideoaudio;
            else if (popupMenu1.Tag == tileView2)
                o = btnaddaudio;
            else if (popupMenu1.Tag == tileView3)
                o = btnaddsub;
            btnadd_Click(o, null);
        }

        private void pmtracksec_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            tileView1_ItemDoubleClick(popupMenu1.Tag, null);
        }

        private void pmtracksil_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (mp == null || loading)
                return;
            TileView tw = (TileView)popupMenu1.Tag;
            Track t = (Track)tw.GetRow(tw.GetSelectedRows()[0]);
            mp.RemoveMedia(t.Type, t.Id);
        }

        private void pmtrackyenile_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (mp == null || loading)
                return;
            TileView tw = (TileView)popupMenu1.Tag;
            Track t = (Track)tw.GetRow(tw.GetSelectedRows()[0]);
            bool isp = mp.IsPlaying;
            mp.Pause();
            mp.ReloadMedia(t.Type, t.Id);
            if (isp)
                mp.Resume();
        }
        #endregion
        #endregion

        #region stop
        private void stop_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && mp != null)
                mp.Stop();
        }
        #endregion

        #region fullscreen
        private void fullscreen_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Fullscreen();
        }
        #endregion

        #region chapter
        private void chapternext_MouseClick(object sender, MouseEventArgs e)
        {
            if (chapternext.Enabled && e.Button == MouseButtons.Left && mp != null)
                mp.SetChapter(Chapter + 1);
        }

        private void chapterback_MouseClick(object sender, MouseEventArgs e)
        {
            if (chapterback.Enabled && e.Button == MouseButtons.Left && mp != null)
                mp.SetChapter(Chapter - 1);
        }

        private void chapters_MouseClick(object sender, MouseEventArgs e)
        {
            if (Chapters != null && Chapters.Length > 0 && e.Button == MouseButtons.Left)
            {
                listchapters.BringToFront();
                listchapters.Location = new Point(chapters.Left, layoutControl1.Top - listchapters.Height);
                listchapters.Visible = !listchapters.Visible;
                if (listchapters.Visible)
                {
                    tileView4.MoveFirst();
                    tileView4.MoveBy(Chapter);
                }
            }
        }

        private void tileView4_ItemClick(object sender, TileViewItemClickEventArgs e)
        {
            if (mp != null)
                mp.SetChapter(e.Item.RowHandle);
        }
        #endregion

        #region sure
        private void sure_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int i = (int)timeformat + 1;
                timeformat = (TimeFormat)(i >= Enum.GetValues(typeof(TimeFormat)).Length ? 0 : i);
                SureAyarla();
            }
        }
        #endregion

        #region tooltip
        private void toolTipController1_GetActiveObjectInfo(object sender, ToolTipControllerGetActiveObjectInfoEventArgs e)
        {
            if (e.SelectedControl is GridControl control)
            {
                TileViewHitInfo hi = ((TileView)control.MainView).CalcHitInfo(e.ControlMousePosition);
                if (hi.HitTest == TileControlHitTest.Item)
                {
                    var elemInfo = hi.ItemInfo.Elements[0];
                    var a = TextRenderer.MeasureText(elemInfo.Text, elemInfo.StringInfo.Font);
                    if (a.Width > elemInfo.ItemContentBounds.Width)
                        e.Info = new ToolTipControlInfo(elemInfo, elemInfo.Text);
                }
            }
            else if (e.SelectedControl != ilerleme && e.SelectedControl != volumebar)
            {
                e.Info = new ToolTipControlInfo(e.SelectedControl, e.SelectedControl.Tag?.ToString())
                {
                    ImmediateToolTip = true,
                    ToolTipLocation = ToolTipLocation.TopCenter,
                    ToolTipPosition = e.SelectedControl.PointToScreen(new Point(e.SelectedControl.Width / 2, -e.SelectedControl.Top - (e.SelectedControl == volume ? volumebar.Height : 0)))
                };
            }
        }

        private void toolTipController1_CalcSize(object sender, ToolTipControllerCalcSizeEventArgs e)
        {
            var a = TextRenderer.MeasureText(e.ToolTip, e.ShowInfo.Appearance.Font);
            e.Size = new Size(a.Width - e.ShowInfo.RoundRadius, a.Height);
        }
        #endregion

        #region bar
        private void layoutControl1_VisibleChanged(object sender, EventArgs e)
        {
            int p = SubPos;
            if (mp != null)
                mp.SetSubPos(p);
        }
        #endregion

        private void bwloading_DoWork(object sender, DoWorkEventArgs e)
        {
            if (mp == null)
                return;
            if (!buffer.Visible)
                Thread.Sleep(400);
            while (loading)
            {
                if (loadingbmp == null)
                    continue;
                foreach (var bmp in loadingbmp)
                {
                    if (video.Width == 0 || VideoArea.Height == 0)
                        continue;
                    Bitmap b = new Bitmap(video.Width, VideoArea.Height);
                    Graphics g = Graphics.FromImage(b);
                    g.Clear(Color.FromArgb(80, Color.Black));
                    double v1 = b.Width * 0.1;
                    double v2 = b.Height * 0.1;
                    int r = (int)Math.Max(v1, v2);
                    if (r > b.Width)
                        r = b.Width;
                    else if (r > b.Height)
                        r = b.Height;
                    int w = r;
                    int h = r;
                    g.DrawImage(bmp.Key, b.Width / 2 - w / 2, b.Height / 2 - h / 2, w, h);
                    mp.OverlayAdd(0, 0, 0, b);
                    if (!loading)
                        break;
                    Thread.Sleep(bmp.Value);
                }
            }
            mp.OverlayRemove(0);
        }
        #endregion

        #region Timers
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (layoutControl2.Visible || listchapters.Visible)
                return;
            Bar = false;
            volumebar.Visible = false;
            timer3.Stop();
            timer1.Stop();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (layoutControl2.Visible || listchapters.Visible)
                return;
            if (ctrl == layoutControl1 || ctrl == buffer)
                tek = true;
            Bar = false;
            Cursor.Hide();
            cursor = false;
            timer2.Stop();
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            volumebar.Visible = false;
            timer3.Stop();
        }
        #endregion
    }

    public class VideoBilgiYuklendiArgs
    {
        public Size Size { get; private set; }
        public string Codec { get; private set; }

        public VideoBilgiYuklendiArgs(Size size, string codec)
        {
            Size = size;
            Codec = codec;
        }

        public override string ToString() => Size.IsEmpty ? "" : $"{Size.Width}x{Size.Height}";
    }

    public class DurumDegistiArgs
    {
        public string Durum { get; private set; }
        public string Hata { get; private set; }

        public DurumDegistiArgs(string durum, string hata)
        {
            Durum = durum;
            Hata = hata;
        }
    }

    public static class ControlExtensions
    {
        public static T InvokeIfRequired<T>(this T source, Action<T> action) where T : Control
        {
            try
            {
                if (!source.InvokeRequired)
                    action(source);
                else
                    source.Invoke(new Action(() => action(source)));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on 'InvokeIfRequired': {0}", ex.Message);
            }
            return source;
        }
    }
}
