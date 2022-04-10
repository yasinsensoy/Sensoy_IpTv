using Newtonsoft.Json;

namespace Mpv.NET.Player
{
    public class DemuxerCacheState
    {
        #region Properties
        /// <summary>
        /// BofCached indicates whether the seek range with the lowest timestamp points to the beginning of the stream (BOF). This implies you cannot seek before this position at all.
        /// If both BofCached and EofCached are true, and there's only 1 cache range, the entire stream is cached.
        /// </summary>
        [JsonProperty("bof-cached")]
        public bool BofCached { get; set; }

        /// <summary>
        /// EofCached indicates whether the seek range with the highest timestamp points to the end of the stream (EOF).
        /// If both BofCached and EofCached are true, and there's only 1 cache range, the entire stream is cached.
        /// </summary>
        [JsonProperty("eof-cached")]
        public bool EofCached { get; set; }

        /// <summary>
        /// FwBytes is the number of bytes of packets buffered in the range starting from the current decoding position. This is a rough estimate (may not account correctly for various overhead), and stops at the demuxer position (it ignores seek ranges after it).
        /// </summary>
        [JsonProperty("fw-bytes")]
        public long FwBytes { get; set; }

        /// <summary>
        /// FileCacheBytes is the number of bytes stored in the file cache. This includes all overhead, and possibly unused data (like pruned data). This member is missing if the file cache wasn't enabled with --cache-on-disk=yes.
        /// </summary>
        [JsonProperty("file-cache-bytes")]
        public long FileCacheBytes { get; set; }

        /// <summary>
        /// CacheEnd is demuxer-cache-time. Missing if unavailable.
        /// </summary>
        [JsonProperty("cache-end")]
        public double CacheEnd { get; set; }

        /// <summary>
        /// ReaderPts is the approximate timestamp of the start of the buffered range. Missing if unavailable.
        /// </summary>
        [JsonProperty("reader-pts")]
        public double ReaderPts { get; set; }

        /// <summary>
        /// CacheDuration is demuxer-cache-duration. Missing if unavailable.
        /// </summary>
        [JsonProperty("cache-duration")]
        public double CacheDuration { get; set; }

        /// <summary>
        /// RawInputRate is the estimated input rate of the network layer (or any other byte-oriented input layer) in bytes per second. May be inaccurate or missing.
        /// </summary>
        [JsonProperty("raw-input-rate")]
        public long RawInputRate { get; set; }

        /// <summary>
        /// Whether the reader thread has hit the end of the file.
        /// </summary>
        [JsonProperty("eof")]
        public bool Eof { get; set; }

        /// <summary>
        /// Whether the reader thread could not satisfy a decoder's request for a new packet.
        /// </summary>
        [JsonProperty("underrun")]
        public bool UnderRun { get; set; }

        /// <summary>
        /// Whether the thread is currently not reading.
        /// </summary>
        [JsonProperty("idle")]
        public bool Idle { get; set; }

        /// <summary>
        /// Sum of packet bytes(plus some overhead estimation) of the entire packet queue, including cached seekable ranges.
        /// </summary>
        [JsonProperty("total-bytes")]
        public long TotalBytes { get; set; }

        [JsonProperty("seekable-ranges")]
        public Ranges[] SeekableRanges { get; set; }
        #endregion

        public DemuxerCacheState()
        {
        }
    }

    public class Ranges
    {
        [JsonProperty("start")]
        public double Start { get; set; }

        [JsonProperty("end")]
        public double End { get; set; }

        public Ranges()
        {
        }
    }
}