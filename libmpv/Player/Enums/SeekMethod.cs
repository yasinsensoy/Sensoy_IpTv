using System.ComponentModel.DataAnnotations;

namespace Mpv.NET.Player
{
    public enum SeekMethod
    {
        /// <summary>
        /// Seek relative to current position (a negative value seeks backwards).
        /// </summary>
        [Display(Name = "relative")]
        Relative,

        /// <summary>
        /// Seek to a given time (a negative value starts from the end of the file).
        /// </summary>
        [Display(Name = "absolute")]
        Absolute,

        /// <summary>
        /// Seek to a given percent position.
        /// </summary>
        [Display(Name = "absolute-percent")]
        AbsolutePercent,

        /// <summary>
        /// Seek relative to current position in percent.
        /// </summary>
        [Display(Name = "relative-percent")]
        RelativePercent,

        /// <summary>
        /// Always restart playback at keyframe boundaries (fast).
        /// </summary>
        [Display(Name = "keyframes")]
        KeyFrames,

        /// <summary>
        /// Always do exact/hr/precise seeks (slow).
        /// </summary>
        [Display(Name = "exact")]
        Exact
    }
}