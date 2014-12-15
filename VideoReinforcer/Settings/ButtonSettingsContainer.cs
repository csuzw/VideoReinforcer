using System;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;

using VideoReinforcer.Common;
using VideoReinforcer.Resources;

namespace VideoReinforcer.Settings
{
    /// <summary>
    /// Defines button settings
    /// </summary>
    [Serializable]
    public class ButtonSettingsContainer
    {
        private const string BUTTON_TEXT = "{0} ({1})";

        /// <summary>
        /// Button name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Button colour.
        /// </summary>
        public Color ButtonColor { get; set; }

        /// <summary>
        /// Button text colour.
        /// </summary>
        public Color TextColor { get; set; }

        /// <summary>
        /// Button image location.
        /// </summary>
        public string ImageLocation { get; set; }

        /// <summary>
        /// Button shortcut key.
        /// </summary>
        public Key Shortcut { get; set; }

        /// <summary>
        /// Button video location.
        /// </summary>
        public string VideoLocation { get; set; }

        /// <summary>
        /// Should button text be displayed on button?
        /// </summary>
        public bool DisplayText { get; set; }

        /// <summary>
        /// Text to display on button.
        /// </summary>
        public string ButtonText
        {
            get
            {
                return string.Format(BUTTON_TEXT, this.Name, this.Shortcut);
            }
        }

        /// <summary>
        /// Which screen should video display on?
        /// </summary>
        public int DisplayScreen { get; set; }

        /// <summary>
        /// Speaker balance (-1 = 100% left, 1 = 100% right)
        /// </summary>
        public double SpeakerBalance { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ButtonSettingsContainer()
        {
            this.Name = "";
            this.ButtonColor = Colors.LightGray;
            this.TextColor = Colors.Black;
            this.ImageLocation = "";
            this.Shortcut = Key.None;
            this.VideoLocation = "";
            this.DisplayText = true;
            this.DisplayScreen = 1;
            this.SpeakerBalance = 0;
        }

        /// <summary>
        /// Are these settings valid?
        /// </summary>
        /// <param name="errorMessage">Error message if not valid, otherwise an empty string.</param>
        /// <returns>True if this object is valid.</returns>
        public bool IsValid(out string errorMessage)
        {
            errorMessage = "";
            if (string.IsNullOrEmpty(this.Name))
            {
                errorMessage += Strings.ErrorButtonName + Environment.NewLine;
            }
            if (string.IsNullOrEmpty(this.VideoLocation) || !File.Exists(this.VideoLocation))
            {
                errorMessage += Strings.ErrorButtonVideoLocation + Environment.NewLine;
            }
            return string.IsNullOrEmpty(errorMessage);
        }
    }
}
