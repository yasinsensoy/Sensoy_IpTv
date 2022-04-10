using Newtonsoft.Json;
using System;

namespace Mpv.NET.Player
{
    public class Track
    {
        #region Properties
        /// <summary>
        /// The ID as it's used for -sid/--aid/--vid. This is unique within tracks of the same type (sub/audio/video), but otherwise not.
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// String describing the media type. One of audio, video, sub.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Track ID as used in the source file. Not always available. (It is missing if the format has no native ID, if the track is a pseudo-track that does not exist in this way in the actual file, or if the format is handled by libavformat, and the format was not whitelisted as having track IDs.)
        /// </summary>
        [JsonProperty("src-id")]
        public long SrcId { get; set; }

        /// <summary>
        /// Track title as it is stored in the file. Not always available.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Track language as identified by the file. Not always available.
        /// </summary>
        [JsonProperty("lang")]
        public string Lang
        {
            get => lang;
            set
            {
                lang = value;
                Languages.Language l = string.IsNullOrEmpty(value) ? default : Languages.Language.FromPart2(value);
                LangName = l == null ? value : l.Name;
            }
        }
        private string lang;

        /// <summary>
        /// yes/true if this is a video track that consists of a single picture, no/false or unavailable otherwise. This is used for video tracks that are really attached pictures in audio files.
        /// </summary>
        [JsonProperty("albumart")]
        public bool Albumart { get; set; }

        /// <summary>
        /// yes/true if the track has the default flag set in the file, no/false or unavailable otherwise.
        /// </summary>
        [JsonProperty("default")]
        public bool Default { get; set; }

        /// <summary>
        /// yes/true if the track has the forced flag set in the file, no/false or unavailable otherwise.
        /// </summary>
        [JsonProperty("forced")]
        public bool Forced { get; set; }

        /// <summary>
        /// yes/true if the track is currently decoded, no/false or unavailable otherwise.
        /// </summary>
        [JsonProperty("selected")]
        public bool Selected { get; set; }

        /// <summary>
        /// It indicates the selection order of tracks for the same type. If a track is not selected, or is selected by the --lavfi-complex, it is not available. For subtitle tracks, 0 represents the sid, and 1 represents the secondary-sid.
        /// </summary>
        [JsonProperty("main-selection")]
        public long MainSelection { get; set; }

        /// <summary>
        /// yes/true if the track is an external file, no/false or unavailable otherwise. This is set for separate subtitle files.
        /// </summary>
        [JsonProperty("external")]
        public bool External { get; set; }

        /// <summary>
        /// The filename if the track is from an external file, unavailable otherwise.
        /// </summary>
        [JsonProperty("external-filename")]
        public string ExternalFilename { get; set; }

        /// <summary>
        /// The codec name used by this track, for example h264. Unavailable in some rare cases.
        /// </summary>
        [JsonProperty("codec")]
        public string Codec { get; set; }

        /// <summary>
        /// The stream index as usually used by the FFmpeg utilities. Note that this can be potentially wrong if a demuxer other than libavformat (--demuxer=lavf) is used. For mkv files, the index will usually match even if the default (builtin) demuxer is used, but there is no hard guarantee.
        /// </summary>
        [JsonProperty("ff-index")]
        public long FFIndex { get; set; }

        /// <summary>
        /// If this track is being decoded, the human-readable decoder name.
        /// </summary>
        [JsonProperty("decoder-desc")]
        public string DecoderDesc { get; set; }

        /// <summary>
        /// Video width hint as indicated by the container. (Not always accurate.)
        /// </summary>
        [JsonProperty("demux-w")]
        public long DemuxW { get; set; }

        /// <summary>
        /// Video height hint as indicated by the container. (Not always accurate.)
        /// </summary>
        [JsonProperty("demux-h")]
        public long DemuxH { get; set; }

        /// <summary>
        /// Number of audio channels as indicated by the container. (Not always accurate - in particular, the track could be decoded as a different number of channels.)
        /// </summary>
        [JsonProperty("demux-channel-count")]
        public long DemuxChannelCount { get; set; }

        /// <summary>
        /// Channel layout as indicated by the container. (Not always accurate.)
        /// </summary>
        [JsonProperty("demux-channels")]
        public string DemuxChannels { get; set; }

        /// <summary>
        /// Audio sample rate as indicated by the container. (Not always accurate.)
        /// </summary>
        [JsonProperty("demux-samplerate")]
        public long DemuxSamplerate { get; set; }

        /// <summary>
        /// Video FPS as indicated by the container. (Not always accurate.)
        /// </summary>
        [JsonProperty("demux-fps")]
        public double DemuxFps { get; set; }

        /// <summary>
        /// Audio average bitrate, in bits per second. (Not always accurate.)
        /// </summary>
        [JsonProperty("demux-bitrate")]
        public long DemuxBitrate { get; set; }

        /// <summary>
        /// Video clockwise rotation metadata, in degrees.
        /// </summary>
        [JsonProperty("demux-rotation")]
        public long DemuxRotation { get; set; }

        /// <summary>
        /// Pixel aspect ratio.
        /// </summary>
        [JsonProperty("demux-par")]
        public double DemuxPar { get; set; }

        /// <summary>
        /// Deprecated alias for DemuxChannelCount.
        /// </summary>
        [JsonProperty("audio-channels")]
        public long AudioChannels { get; set; }

        /// <summary>
        /// Per-track replaygain values. Only available for audio tracks with corresponding information stored in the source file.
        /// </summary>
        [JsonProperty("replaygain-track-peak")]
        public double ReplaygainTrackPeak { get; set; }

        /// <summary>
        /// Per-track replaygain values. Only available for audio tracks with corresponding information stored in the source file.
        /// </summary>
        [JsonProperty("replaygain-track-gain")]
        public double ReplaygainTrackGain { get; set; }

        /// <summary>
        /// Per-album replaygain values. If the file has per-track but no per-album information, the per-album values will be copied from the per-track values currently. It's possible that future mpv versions will make these properties unavailable instead in this case.
        /// </summary>
        [JsonProperty("replaygain-album-peak")]
        public double ReplaygainAlbumPeak { get; set; }

        /// <summary>
        /// Per-album replaygain values. If the file has per-track but no per-album information, the per-album values will be copied from the per-track values currently. It's possible that future mpv versions will make these properties unavailable instead in this case.
        /// </summary>
        [JsonProperty("replaygain-album-gain")]
        public double ReplaygainAlbumGain { get; set; }
        #endregion

        [JsonIgnore]
        public string Baslik => $"{(string.IsNullOrEmpty(Title) ? $"Kayıt {Id}" : Title)}{(string.IsNullOrEmpty(LangName) ? "" : $" [{LangName}]")}";

        [JsonIgnore]
        public string LangName { get; set; }

        [JsonIgnore]
        public string Info
        {
            get
            {
                string i = Codec ?? "";
                if (Type == "video")
                {
                    i += DemuxW == 0 || DemuxH == 0 ? "" : $"{(i == "" ? "" : " ")}{DemuxW}x{DemuxH}";
                    i += DemuxFps == 0 ? "" : $"{(i == "" ? "" : " ")}{Math.Round(DemuxFps, 3)} fps";
                }
                else if (Type == "audio")
                {
                    i += DemuxChannelCount == 0 ? "" : $"{(i == "" ? "" : " ")}{DemuxChannelCount}ch";
                    i += DemuxSamplerate == 0 ? "" : $"{(i == "" ? "" : " ")}{DemuxSamplerate}hz";
                }
                return i;
            }
        }

        public Track(long id, string title, string type, bool selected)
        {
            Id = id;
            Title = title;
            Type = type;
            Selected = selected;
        }
    }
}
