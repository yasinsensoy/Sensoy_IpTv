using System.ComponentModel.DataAnnotations;

namespace Mpv.NET.Player
{
    public enum LoadMethod
    {
        /// <summary>
        /// Stop playback of current media and start new one.
        /// </summary>
        [Display(Name = "replace")]
        Replace,

        /// <summary>
        /// Append media to playlist.
        /// </summary>
        [Display(Name = "append")]
        Append,

        /// <summary>
        /// Append media to playlist and play if nothing else is playing.
        /// </summary>
        [Display(Name = "append-play")]
        AppendPlay
    }
}