using System.ComponentModel.DataAnnotations;

namespace Mpv.NET.Player
{
    public enum TrackAddType
    {
        /// <summary>
        /// Select the subtitle immediately (default).
        /// </summary>
        [Display(Name = "select")]
        Select,

        /// <summary>
        /// Don't select the subtitle. (Or in some special situations, let the default stream selection mechanism decide.)
        /// </summary>
        [Display(Name = "auto")]
        Auto,

        /// <summary>
        /// Select the subtitle. If a subtitle with the same filename was already added, that one is selected, instead of loading a duplicate entry. (In this case, title/language are ignored, and if the was changed since it was loaded, these changes won't be reflected.)
        /// </summary>
        [Display(Name = "cached")]
        Cached
    }
}