using System;
using System.Windows.Input;

using VideoReinforcer.Common;
using VideoReinforcer.Resources;

namespace VideoReinforcer.Settings
{
    /// <summary>
    /// Defines general application settings
    /// </summary>
    [Serializable]
    public class GeneralSettingsContainer
    {
        /// <summary>
        /// Button height in pixels.
        /// </summary>
        public int ButtonHeight { get; set; }

        /// <summary>
        /// Button width in pixels.
        /// </summary>
        public int ButtonWidth { get; set; }

        /// <summary>
        /// Number of columns of buttons.
        /// </summary>
        public int ButtonColumns { get; set; }

        /// <summary>
        /// Mute shortcut key.
        /// </summary>
        public Key MuteShortcut { get; set; }

        /// <summary>
        /// Hide cursor shortcut key.
        /// </summary>
        public Key HideCursorShortcut { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GeneralSettingsContainer()
        {
            this.ButtonHeight = 120;
            this.ButtonWidth = 120;
            this.ButtonColumns = 2;
            this.MuteShortcut = Key.None;
            this.HideCursorShortcut = Key.None;
        }

        /// <summary>
        /// Are these settings valid?
        /// </summary>
        /// <param name="errorMessage">Error message if not valid, otherwise an empty string.</param>
        /// <returns>True if this object is valid.</returns>
        public bool IsValid(out string errorMessage)
        {
            errorMessage = "";
            if (this.ButtonWidth <= 0)
            {
                errorMessage += Strings.ErrorButtonWidth + Environment.NewLine;
            }
            if (this.ButtonHeight <= 0)
            {
                errorMessage += Strings.ErrorButtonHeight + Environment.NewLine;
            }
            if (this.ButtonColumns <= 0)
            {
                errorMessage += Strings.ErrorButtonColumns + Environment.NewLine;
            }
            return string.IsNullOrEmpty(errorMessage);
        }
    }
}
