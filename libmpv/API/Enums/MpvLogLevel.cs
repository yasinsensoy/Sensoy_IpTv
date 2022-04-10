using System.ComponentModel.DataAnnotations;

namespace Mpv.NET.API
{
    public enum MpvLogLevel
    {
        /// <summary>
        /// disable absolutely all messages
        /// </summary>
        [Display(Name = "no")]
        None = 0,

        /// <summary>
        /// critical/aborting errors
        /// </summary>
        [Display(Name = "fatal")]
        Fatal = 10,

        /// <summary>
        /// simple errors
        /// </summary>
        [Display(Name = "error")]
        Error = 20,

        /// <summary>
        /// possible problems
        /// </summary>
        [Display(Name = "warn")]
        Warning = 30,

        /// <summary>
        /// informational message
        /// </summary>
        [Display(Name = "info")]
        Info = 40,

        /// <summary>
        /// noisy informational message
        /// </summary>
        [Display(Name = "v")]
        V = 50,

        /// <summary>
        /// very noisy technical information
        /// </summary>
        [Display(Name = "debug")]
        Debug = 60,

        /// <summary>
        /// extremely noisy
        /// </summary>
        [Display(Name = "trace")]
        Trace = 70

    }
}