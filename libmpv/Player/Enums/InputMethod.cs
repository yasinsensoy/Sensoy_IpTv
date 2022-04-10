using System.ComponentModel.DataAnnotations;

namespace Mpv.NET.Player
{
    public enum InputMethod
    {
        /// <summary>
        /// Use the default behavior for this command. This is the default for input.conf commands. Some libmpv/scripting/IPC APIs do not use this as default, but use no-osd instead.
        /// </summary>
        [Display(Name = "osd-auto")]
        OsdAuto,

        /// <summary>
        /// Do not use any OSD for this command.
        /// </summary>
        [Display(Name = "no-osd")]
        NoOsd,

        /// <summary>
        /// If possible, show a bar with this command. Seek commands will show the progress bar, property changing commands may show the newly set value.
        /// </summary>
        [Display(Name = "osd-bar")]
        OsdBar,

        /// <summary>
        /// If possible, show an OSD message with this command. Seek command show the current playback time, property changing commands show the newly set value as text.
        /// </summary>
        [Display(Name = "osd-msg")]
        OsdMsg,

        /// <summary>
        /// Combine osd-bar and osd-msg.
        /// </summary>
        [Display(Name = "osd-msg-bar")]
        OsdMsgBar,

        /// <summary>
        /// Do not expand properties in string arguments. (Like "${property-name}".) This is the default for some libmpv/scripting/IPC APIs.
        /// </summary>
        [Display(Name = "raw")]
        Raw,

        /// <summary>
        /// All string arguments are expanded as described in Property Expansion. This is the default for input.conf commands.
        /// </summary>
        [Display(Name = "expand-properties")]
        ExpandProperties,

        /// <summary>
        /// For some commands, keeping a key pressed doesn't run the command repeatedly. This prefix forces enabling key repeat in any case.
        /// </summary>
        [Display(Name = "repeatable")]
        Repeatable,

        /// <summary>
        /// Allow asynchronous execution (if possible). Note that only a few commands will support this (usually this is explicitly documented). Some commands are asynchronous by default (or rather, their effects might manifest after completion of the command). The semantics of this flag might change in the future. Set it only if you don't rely on the effects of this command being fully realized when it returns. See Synchronous vs. Asynchronous.
        /// </summary>
        [Display(Name = "async")]
        Async,

        /// <summary>
        /// Allow synchronous execution (if possible). Normally, all commands are synchronous by default, but some are asynchronous by default for compatibility with older behavior.
        /// </summary>
        [Display(Name = "sync")]
        Sync
    }
}