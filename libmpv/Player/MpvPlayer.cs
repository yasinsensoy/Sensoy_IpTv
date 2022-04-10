using Mpv.NET.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Mpv.NET.Player
{
    public partial class MpvPlayer : IMpvPlayer, IDisposable
    {
        #region Properties
        public API.Mpv API => mpv;

        public string LibMpvPath { get; private set; }

        public string MediaTitle { get; private set; }

        public Track[] Videos
        {
            get
            {
                List<Track> l = new List<Track>(tracks.Where(t => t.Type == "video"));
                if (l.Count > 0)
                    l.Add(new Track(0, "Kapalı", "video", l.All(t => !t.Selected)));
                return l.OrderBy(t => t.Id).ToArray();
            }
        }

        public Track[] Audios
        {
            get
            {
                List<Track> l = new List<Track>(tracks.Where(t => t.Type == "audio"));
                if (l.Count > 0)
                    l.Add(new Track(0, "Kapalı", "audio", l.All(t => !t.Selected)));
                return l.OrderBy(t => t.Id).ToArray();
            }
        }

        public Track[] Subs
        {
            get
            {
                List<Track> l = new List<Track>(tracks.Where(t => t.Type == "sub"));
                if (l.Count > 0)
                    l.Add(new Track(0, "Kapalı", "sub", l.All(t => !t.Selected)));
                return l.OrderBy(t => t.Id).ToArray();
            }
        }

        public string Vid
        {
            get
            {
                lock (mpvLock)
                    return mpv.GetPropertyString("vid");
            }
            set
            {
                lock (mpvLock)
                    mpv.SetPropertyString("vid", value);
            }
        }

        public string Aid
        {
            get
            {
                lock (mpvLock)
                    return mpv.GetPropertyString("aid");
            }
            set
            {
                lock (mpvLock)
                    mpv.SetPropertyString("aid", value);
            }
        }

        public string Sid
        {
            get
            {
                lock (mpvLock)
                    return mpv.GetPropertyString("sid");
            }
            set
            {
                lock (mpvLock)
                    mpv.SetPropertyString("sid", value);
            }
        }

        public bool IsVideoShowing
        {
            get
            {
                lock (mpvLock)
                    return MpvPlayerHelper.YesNoToBool(mpv.GetPropertyString("vo-configured"));
            }
        }

        public bool IsMediaLoaded
        {
            get
            {
                lock (mpvLock)
                    return !MpvPlayerHelper.YesNoToBool(mpv.GetPropertyString("idle-active"));
            }
        }

        public bool IsPlaying
        {
            get
            {
                lock (mpvLock)
                    return !MpvPlayerHelper.YesNoToBool(mpv.GetPropertyString("core-idle"));
            }
        }

        public string Codec
        {
            get
            {
                lock (mpvLock)
                    return mpv.GetPropertyString("video-format");
            }
        }

        public Size Size
        {
            get
            {
                lock (mpvLock)
                    return new Size((int)mpv.GetPropertyLong("width"), (int)mpv.GetPropertyLong("height"));
            }
        }

        public MpvLogLevel LogLevel
        {
            get => logLevel;
            set
            {
                lock (mpvLock)
                    mpv.RequestLogMessages(value);
                logLevel = value;
            }
        }

        public KeepOpen KeepOpen
        {
            get
            {
                lock (mpvLock)
                    return mpv.GetPropertyString("keep-open").GetEnum<KeepOpen>();
            }
            set
            {
                lock (mpvLock)
                    mpv.SetPropertyString("keep-open", value.GetDisplayName());
            }
        }

        public bool EndReached
        {
            get
            {
                lock (mpvLock)
                    return MpvPlayerHelper.YesNoToBool(mpv.GetPropertyString("eof-reached"));
            }
        }

        public TimeSpan Duration
        {
            get
            {
                if (!IsMediaLoaded)
                    return TimeSpan.Zero;
                lock (mpvLock)
                    return TimeSpan.FromSeconds(mpv.GetPropertyDouble("duration"));
            }
        }

        public TimeSpan Position
        {
            get
            {
                if (!IsMediaLoaded)
                    return TimeSpan.Zero;
                lock (mpvLock)
                    return TimeSpan.FromSeconds(mpv.GetPropertyDouble("time-pos"));
            }
            set
            {
                lock (mpvLock)
                    mpv.SetPropertyDouble("time-pos", value.TotalSeconds);
            }
        }

        public int Volume
        {
            get
            {
                lock (mpvLock)
                    return (int)mpv.GetPropertyDouble("volume");
            }
            set
            {
                lock (mpvLock)
                    mpv.SetPropertyDouble("volume", value);
            }
        }

        public double Speed
        {
            get
            {
                lock (mpvLock)
                    return mpv.GetPropertyDouble("speed");
            }
            set
            {
                lock (mpvLock)
                    mpv.SetPropertyDouble("speed", value);
            }
        }

        public int SubPos
        {
            get
            {
                lock (mpvLock)
                    return (int)mpv.GetPropertyLong("sub-pos");
            }
        }
        #endregion

        #region Events
        public event EventHandler VideoUpdated;
        public event EventHandler MediaStoped;
        public event EventHandler MediaLoaded;
        public event EventHandler<MpvErrorEventArgs> MediaError;
        public event EventHandler MediaFinished;

        private EventHandler<MpvTimeEventArgs> positionChanged;
        private const int timePosUserData = 1;
        public event EventHandler<MpvTimeEventArgs> PositionChanged
        {
            add
            {
                lock (mpvLock)
                    mpv.ObserveProperty("time-pos", MpvFormat.Double, timePosUserData);
                positionChanged += value;
            }
            remove
            {
                lock (mpvLock)
                    mpv.UnobserveProperty(timePosUserData);
                positionChanged -= value;
            }
        }

        private EventHandler<MpvVolumeEventArgs> volumeChanged;
        private const int volumeUserData = 2;
        public event EventHandler<MpvVolumeEventArgs> VolumeChanged
        {
            add
            {
                lock (mpvLock)
                    mpv.ObserveProperty("volume", MpvFormat.Double, volumeUserData);
                volumeChanged += value;
            }
            remove
            {
                lock (mpvLock)
                    mpv.UnobserveProperty(volumeUserData);
                volumeChanged -= value;
            }
        }

        private EventHandler<MpvTimeEventArgs> durationChanged;
        private const int durationUserData = 3;
        public event EventHandler<MpvTimeEventArgs> DurationChanged
        {
            add
            {
                lock (mpvLock)
                    mpv.ObserveProperty("duration", MpvFormat.Double, durationUserData);
                durationChanged += value;
            }
            remove
            {
                lock (mpvLock)
                    mpv.UnobserveProperty(durationUserData);
                durationChanged -= value;
            }
        }

        private EventHandler mediaResumed;
        private const int mediaResumedUserData = 4;
        public event EventHandler MediaResumed
        {
            add
            {
                lock (mpvLock)
                    mpv.ObserveProperty("core-idle", MpvFormat.String, mediaResumedUserData);
                mediaResumed += value;
            }
            remove
            {
                lock (mpvLock)
                    mpv.UnobserveProperty(mediaResumedUserData);
                mediaResumed -= value;
            }
        }

        private EventHandler mediaPaused;
        private const int mediaPausedUserData = 5;
        public event EventHandler MediaPaused
        {
            add
            {
                lock (mpvLock)
                    mpv.ObserveProperty("pause", MpvFormat.String, mediaPausedUserData);
                mediaPaused += value;
            }
            remove
            {
                lock (mpvLock)
                    mpv.UnobserveProperty(mediaPausedUserData);
                mediaPaused -= value;
            }
        }

        private EventHandler<MpvBufferingEventArgs> mediaBuffering;
        private const int mediaBufferingUserData = 6;
        public event EventHandler<MpvBufferingEventArgs> MediaBuffering
        {
            add
            {
                lock (mpvLock)
                    mpv.ObserveProperty("cache-buffering-state", MpvFormat.Double, mediaBufferingUserData);
                mediaBuffering += value;
            }
            remove
            {
                lock (mpvLock)
                    mpv.UnobserveProperty(mediaBufferingUserData);
                mediaBuffering -= value;
            }
        }

        private EventHandler<MpvSeekingEventArgs> mediaSeeking;
        private const int mediaSeekingUserData = 7;
        public event EventHandler<MpvSeekingEventArgs> MediaSeeking
        {
            add
            {
                lock (mpvLock)
                    mpv.ObserveProperty("seeking", MpvFormat.String, mediaSeekingUserData);
                mediaSeeking += value;
            }
            remove
            {
                lock (mpvLock)
                    mpv.UnobserveProperty(mediaSeekingUserData);
                mediaSeeking -= value;
            }
        }

        private EventHandler<MpvSpeedEventArgs> speedChanged;
        private const int speedChangedUserData = 8;
        public event EventHandler<MpvSpeedEventArgs> SpeedChanged
        {
            add
            {
                lock (mpvLock)
                    mpv.ObserveProperty("speed", MpvFormat.Double, speedChangedUserData);
                speedChanged += value;
            }
            remove
            {
                lock (mpvLock)
                    mpv.UnobserveProperty(speedChangedUserData);
                speedChanged -= value;
            }
        }

        private EventHandler tracklistChanged;
        private const int tracklistChangedUserData = 9;
        public event EventHandler TrackListChanged
        {
            add
            {
                lock (mpvLock)
                    mpv.ObserveProperty("track-list", MpvFormat.String, tracklistChangedUserData);
                tracklistChanged += value;
            }
            remove
            {
                lock (mpvLock)
                    mpv.UnobserveProperty(tracklistChangedUserData);
                tracklistChanged -= value;
            }
        }

        private EventHandler<MpvDemuxerCacheStateEventArgs> demuxerCacheStateChanged;
        private const int demuxerCacheStateChangedUserData = 10;
        public event EventHandler<MpvDemuxerCacheStateEventArgs> DemuxerCacheStateChanged
        {
            add
            {
                lock (mpvLock)
                    mpv.ObserveProperty("demuxer-cache-state", MpvFormat.String, demuxerCacheStateChangedUserData);
                demuxerCacheStateChanged += value;
            }
            remove
            {
                lock (mpvLock)
                    mpv.UnobserveProperty(demuxerCacheStateChangedUserData);
                demuxerCacheStateChanged -= value;
            }
        }

        private EventHandler<MpvChapterListEventArgs> chapterListChanged;
        private const int chapterListChangedUserData = 11;
        public event EventHandler<MpvChapterListEventArgs> ChapterListChanged
        {
            add
            {
                lock (mpvLock)
                    mpv.ObserveProperty("chapter-list", MpvFormat.String, chapterListChangedUserData);
                chapterListChanged += value;
            }
            remove
            {
                lock (mpvLock)
                    mpv.UnobserveProperty(chapterListChangedUserData);
                chapterListChanged -= value;
            }
        }

        private EventHandler<MpvChapterEventArgs> chapterChanged;
        private const int chapterChangedUserData = 12;
        public event EventHandler<MpvChapterEventArgs> ChapterChanged
        {
            add
            {
                lock (mpvLock)
                    mpv.ObserveProperty("chapter", MpvFormat.Int64, chapterChangedUserData);
                chapterChanged += value;
            }
            remove
            {
                lock (mpvLock)
                    mpv.UnobserveProperty(chapterChangedUserData);
                chapterChanged -= value;
            }
        }
        #endregion

        #region Initialise
        private API.Mpv mpv;
        private readonly IntPtr hwnd;
        private MpvLogLevel logLevel = MpvLogLevel.None;
        private readonly string[] possibleLibMpvPaths = new string[] { "mpv-1.dll", @"lib\mpv-1.dll" };
        private readonly object mpvLock = new object();
        private List<Track> tracks = new List<Track>();

        public MpvPlayer() : this(IntPtr.Zero)
        {
        }

        public MpvPlayer(string libMpvPath) : this(IntPtr.Zero, libMpvPath)
        {
        }

        public MpvPlayer(IntPtr hwnd)
        {
            this.hwnd = hwnd;
            Initialise();
        }

        public MpvPlayer(IntPtr hwnd, string libMpvPath)
        {
            this.LibMpvPath = libMpvPath;
            this.hwnd = hwnd;
            Initialise();
        }

        private void Initialise()
        {
            if (!string.IsNullOrEmpty(LibMpvPath))
                InitialiseMpv(LibMpvPath);
            else
            {
                var foundPath = possibleLibMpvPaths.FirstOrDefault(File.Exists);
                if (foundPath != null)
                    InitialiseMpv(foundPath);
                else
                    throw new MpvPlayerException("Failed to find libmpv. Check your path.");
            }
        }

        private void InitialiseMpv(string libMpvPath)
        {
            mpv = new API.Mpv(libMpvPath, hwnd);
            mpv.LogMessage += MpvOnLogMessage;
            mpv.StartFile += MpvOnStartFile;
            mpv.FileLoaded += MpvOnFileLoaded;
            mpv.EndFile += MpvOnEndFile;
            mpv.VideoReconfig += MpvOnVideoReconfig;
            mpv.PropertyChange += MpvOnPropertyChange;
        }
        #endregion

        #region Methods
        public void Load(string path, params string[] options)
        {
            Guard.AgainstNullOrEmptyOrWhiteSpaceString(path, nameof(path));
            lock (mpvLock)
                mpv.Command(true, "loadfile", path, LoadMethod.Replace.GetDisplayName(), string.Join(",", options));
        }

        public void ShowText(string text, TextAlign align = TextAlign.TopLeft, string timeout = "1000")
        {
            lock (mpvLock)
                mpv.Command(false, InputMethod.ExpandProperties, "show-text", $"${{osd-ass-cc/0}}{{\\an{(int)align}}}{text}", timeout);
        }

        public void SetChapter(int chapter)
        {
            lock (mpvLock)
                mpv.Command(false, "set", "chapter", chapter);
        }

        public void SetSubPos(int pos)
        {
            lock (mpvLock)
                mpv.Command(false, InputMethod.Async, "set", "sub-pos", pos);
        }

        public void OverlayAdd(int id, int x, int y, Bitmap bmp, int offset = 0, string fmt = "bgra")
        {
            lock (mpvLock)
            {
                BitmapData bd = bmp.LockBits(new Rectangle(new Point(0, 0), bmp.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                mpv.Command(false, InputMethod.Async, "overlay-add", $"{id}", $"{x}", $"{y}", $"&{bd.Scan0}", $"{offset}", fmt, $"{bd.Width}", $"{bd.Height}", $"{bd.Stride}");
                bmp.UnlockBits(bd);
            }
        }

        public void AddMedia(string type, string url, TrackAddType flag = TrackAddType.Cached, string title = "", string lang = "")
        {
            lock (mpvLock)
                mpv.Command(true, $"{type}-add", url, flag.GetDisplayName(), title, lang);
        }

        public void RemoveMedia(string type, long id)
        {
            lock (mpvLock)
                mpv.Command(false, $"{type}-remove", $"{id}");
        }

        public void ReloadMedia(string type, long id)
        {
            lock (mpvLock)
                mpv.Command(false, $"{type}-reload", $"{id}");
        }

        public void OverlayRemove(int id)
        {
            lock (mpvLock)
                mpv.Command(false, "overlay-remove", $"{id}");
        }

        public void Seek(string target, params object[] args)
        {
            lock (mpvLock)
            {
                var prefix = args.Where(a => a is InputMethod).ToArray();
                var arg = args.Where(a => a is SeekMethod).Cast<SeekMethod>().Select(a => a.GetDisplayName()).ToArray();
                mpv.Command(false, prefix.Concat(new object[] { "seek", target }).Concat(arg).ToArray());
            }
        }

        public void Resume()
        {
            lock (mpvLock)
                mpv.SetPropertyString("pause", "no");
        }

        public void Pause()
        {
            lock (mpvLock)
                mpv.SetPropertyString("pause", "yes");
        }

        public void Stop()
        {
            lock (mpvLock)
                mpv.Command(false, "stop");
        }

        public void LoadConfig(string configFilePath)
        {
            lock (mpvLock)
                mpv.LoadConfigFile(configFilePath);
        }

        public void NextFrame()
        {
            lock (mpvLock)
                mpv.Command(false, "frame-step");
        }

        public void PreviousFrame()
        {
            lock (mpvLock)
                mpv.Command(false, "frame-back-step");
        }
        #endregion

        #region MpvOnEvents
        private void MpvOnVideoReconfig(object sender, EventArgs e)
        {
            VideoUpdated?.Invoke(this, new MpvBufferingEventArgs(0));
        }

        private void MpvOnStartFile(object sender, EventArgs e)
        {
            mediaBuffering?.Invoke(this, new MpvBufferingEventArgs(0));
        }

        private void MpvOnFileLoaded(object sender, EventArgs e)
        {
            MediaTitle = mpv.GetPropertyString("media-title");
            MediaLoaded?.Invoke(this, EventArgs.Empty);
        }

        private void MpvOnEndFile(object sender, MpvEndFileEventArgs e)
        {
            switch (e.EventEndFile.Reason)
            {
                case MpvEndFileReason.Stop:
                    MediaStoped?.Invoke(this, EventArgs.Empty);
                    break;
                case MpvEndFileReason.Error:
                    MediaError?.Invoke(this, new MpvErrorEventArgs(mpv.ErrorString(e.EventEndFile.Error)));
                    break;
                case MpvEndFileReason.EndOfFile:
                    MediaFinished?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }

        private void MpvOnLogMessage(object sender, MpvLogMessageEventArgs e)
        {
            var message = e.Message;
            var prefix = message.Prefix;
            var text = message.Text;
            var output = $"[{prefix}] {text}";
            Trace.Write(output);
        }

        private void MpvOnPropertyChange(object sender, MpvPropertyChangeEventArgs e)
        {
            if (e.EventProperty.Name != "time-pos" && e.EventProperty.Name != "duration" && e.EventProperty.Name != "demuxer-cache-state" && e.EventProperty.Name != "track-list")
                Console.WriteLine($"{e.EventProperty.Name} {(e.EventProperty.Format == MpvFormat.String ? e.EventProperty.DataString : (e.EventProperty.Format == MpvFormat.Int64 ? e.EventProperty.DataLong.ToString() : e.EventProperty.DataDouble.ToString()))}");
            switch (e.ReplyUserData)
            {
                case timePosUserData:
                    var pos = TimeSpan.FromSeconds(e.EventProperty.DataDouble);
                    positionChanged?.Invoke(this, new MpvTimeEventArgs(pos));
                    break;
                case volumeUserData:
                    var vol = e.EventProperty.DataDouble;
                    volumeChanged?.Invoke(this, new MpvVolumeEventArgs((int)vol));
                    break;
                case durationUserData:
                    var dur = TimeSpan.FromSeconds(e.EventProperty.DataDouble);
                    durationChanged?.Invoke(this, new MpvTimeEventArgs(dur));
                    break;
                case mediaResumedUserData:
                    var resume = !MpvPlayerHelper.YesNoToBool(e.EventProperty.DataString);
                    if (resume)
                        mediaResumed?.Invoke(this, EventArgs.Empty);
                    break;
                case mediaPausedUserData:
                    var paused = MpvPlayerHelper.YesNoToBool(e.EventProperty.DataString);
                    if (paused)
                        mediaPaused?.Invoke(this, EventArgs.Empty);
                    break;
                case mediaBufferingUserData:
                    var buffer = e.EventProperty.DataDouble;
                    mediaBuffering?.Invoke(this, new MpvBufferingEventArgs((int)buffer));
                    break;
                case mediaSeekingUserData:
                    var seeking = MpvPlayerHelper.YesNoToBool(e.EventProperty.DataString);
                    mediaSeeking?.Invoke(this, new MpvSeekingEventArgs(MpvPlayerHelper.YesNoToBool(e.EventProperty.DataString)));
                    break;
                case speedChangedUserData:
                    var speed = e.EventProperty.DataDouble;
                    speedChanged?.Invoke(this, new MpvSpeedEventArgs(speed));
                    break;
                case tracklistChangedUserData:
                    tracks = new List<Track>(JsonConvert.DeserializeObject<Track[]>(e.EventProperty.DataString));
                    tracklistChanged?.Invoke(this, EventArgs.Empty);
                    break;
                case demuxerCacheStateChangedUserData:
                    DemuxerCacheState state = JsonConvert.DeserializeObject<DemuxerCacheState>(e.EventProperty.DataString);
                    demuxerCacheStateChanged?.Invoke(this, new MpvDemuxerCacheStateEventArgs(state));
                    break;
                case chapterListChangedUserData:
                    ChapterList[] list = JsonConvert.DeserializeObject<ChapterList[]>(e.EventProperty.DataString);
                    chapterListChanged?.Invoke(this, new MpvChapterListEventArgs(list));
                    break;
                case chapterChangedUserData:
                    int id = (int)e.EventProperty.DataLong;
                    chapterChanged?.Invoke(this, new MpvChapterEventArgs(id));
                    break;
            }
        }
        #endregion

        public void Dispose() => mpv.Dispose();
    }
}
