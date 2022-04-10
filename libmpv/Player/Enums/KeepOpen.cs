using System.ComponentModel.DataAnnotations;

namespace Mpv.NET.Player
{
    public enum KeepOpen
    {
        /// <summary>
        /// When the current media ends, play the next media or stop.
        /// </summary>
        [Display(Name = "no")]
        No,

        /// <summary>
        /// Do not unload media when it reaches the end and it is the last entry in the playlist.
        /// </summary>
        [Display(Name = "yes")]
        Yes,

        /// <summary>
        /// Similar to "Yes" but it will not advance to the next entry in the playlist.
        /// </summary>
        [Display(Name = "always")]
        Always
    }
}