using Mpv.NET.API;
using System;
using System.Drawing;

namespace Mpv.NET.Player
{
    public interface IMpvPlayer
    {
        API.Mpv API { get; }

        Track[] Videos { get; }

        Track[] Audios { get; }

        Track[] Subs { get; }

        string Vid { get; set; }

        string Aid { get; set; }

        string Sid { get; set; }

        string LibMpvPath { get; }

        string MediaTitle { get; }

        bool IsVideoShowing { get; }

        bool IsMediaLoaded { get; }

        bool IsPlaying { get; }

        string Codec { get; }

        Size Size { get; }

        MpvLogLevel LogLevel { get; set; }

        KeepOpen KeepOpen { get; set; }

        bool EndReached { get; }

        TimeSpan Duration { get; }

        TimeSpan Position { get; }

        int Volume { get; set; }

        double Speed { get; set; }

        int SubPos { get; }

        event EventHandler VideoUpdated;
        event EventHandler MediaResumed;
        event EventHandler MediaPaused;
        event EventHandler MediaStoped;
        event EventHandler<MpvBufferingEventArgs> MediaBuffering;
        event EventHandler MediaLoaded;
        event EventHandler MediaFinished;
        event EventHandler<MpvErrorEventArgs> MediaError;
        event EventHandler<MpvSeekingEventArgs> MediaSeeking;
        event EventHandler<MpvTimeEventArgs> PositionChanged;
        event EventHandler<MpvTimeEventArgs> DurationChanged;
        event EventHandler<MpvVolumeEventArgs> VolumeChanged;
        event EventHandler<MpvSpeedEventArgs> SpeedChanged;
        event EventHandler TrackListChanged;
        event EventHandler<MpvDemuxerCacheStateEventArgs> DemuxerCacheStateChanged;
        event EventHandler<MpvChapterListEventArgs> ChapterListChanged;
        event EventHandler<MpvChapterEventArgs> ChapterChanged;

        void Load(string path, params string[] options);

        void ShowText(string text, TextAlign align = TextAlign.TopLeft, string timeout = "1000");

        void OverlayAdd(int id, int x, int y, Bitmap bmp, int offset = 0, string fmt = "bgra");

        void AddMedia(string type, string url, TrackAddType flag = TrackAddType.Cached, string title = "", string lang = "");

        void RemoveMedia(string type, long id);

        void ReloadMedia(string type, long id);

        void Seek(string target, params object[] args);

        void Resume();

        void Pause();

        void Stop();

        void NextFrame();

        void PreviousFrame();
    }
}
