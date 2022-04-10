using Newtonsoft.Json;
using System;

namespace Mpv.NET.Player
{
    public class ChapterList
    {
        #region Properties
        /// <summary>
        /// Chapter title as stored in the file. Not always available.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Chapter start time in seconds as float.
        /// </summary>
        [JsonProperty("time")]
        public double Time
        {
            get => time;
            set
            {
                time = value;
                Timespan = TimeSpan.FromSeconds(value);
            }
        }
        private double time;
        #endregion

        [JsonIgnore]
        public TimeSpan Timespan { get; set; }

        [JsonIgnore]
        public string Info => Timespan.ToString($"{(Timespan.TotalHours < 1 ? "" : "hh\\:")}mm\\:ss");

        public ChapterList()
        {
        }
    }
}