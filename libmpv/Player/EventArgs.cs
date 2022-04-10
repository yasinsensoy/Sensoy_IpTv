using System;

namespace Mpv.NET.Player
{
    public class MpvErrorEventArgs : EventArgs
    {
        public string Text { get; private set; }

        public MpvErrorEventArgs(string text) => Text = text;
    }

    public class MpvBufferingEventArgs : EventArgs
    {
        public int Buffer { get; private set; }

        public MpvBufferingEventArgs(int buffer) => Buffer = buffer;
    }

    public class MpvSeekingEventArgs : EventArgs
    {
        public bool Seeking { get; private set; }

        public MpvSeekingEventArgs(bool seeking) => Seeking = seeking;
    }

    public class MpvDemuxerCacheStateEventArgs : EventArgs
    {
        public DemuxerCacheState State { get; private set; }

        public MpvDemuxerCacheStateEventArgs(DemuxerCacheState state) => State = state;
    }

    public class MpvChapterListEventArgs : EventArgs
    {
        public ChapterList[] List { get; private set; }

        public MpvChapterListEventArgs(ChapterList[] list) => List = list;
    }

    public class MpvChapterEventArgs : EventArgs
    {
        public int Chapter { get; private set; }

        public MpvChapterEventArgs(int chapter) => Chapter = chapter;
    }

    public class MpvTimeEventArgs : EventArgs
    {
        public TimeSpan Time { get; private set; }

        public MpvTimeEventArgs(TimeSpan time) => Time = time;
    }

    public class MpvVolumeEventArgs : EventArgs
    {
        public int Volume { get; private set; }

        public MpvVolumeEventArgs(int volume) => Volume = volume;
    }

    public class MpvSpeedEventArgs : EventArgs
    {
        public double Speed { get; private set; }

        public MpvSpeedEventArgs(double speed) => Speed = speed;
    }
}