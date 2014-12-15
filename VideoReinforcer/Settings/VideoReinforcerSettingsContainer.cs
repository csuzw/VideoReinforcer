using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using VideoReinforcer.Common;
using VideoReinforcer.Resources;

namespace VideoReinforcer.Settings
{
    /// <summary>
    /// Defines all application settings and provides functions for loading and saving to file
    /// </summary>
    [Serializable]
    public class VideoReinforcerSettingsContainer
    {
        /// <summary>
        /// General settings.
        /// </summary>
        public GeneralSettingsContainer GeneralSettings { get; set; }

        /// <summary>
        /// List of button settings.
        /// </summary>
        public List<ButtonSettingsContainer> ButtonSettingsList { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public VideoReinforcerSettingsContainer()
        {
            this.GeneralSettings = new GeneralSettingsContainer();
            this.ButtonSettingsList = new List<ButtonSettingsContainer>();
        }

        /// <summary>
        /// Are these settings valid?
        /// </summary>
        /// <returns>True if this object is valid.</returns>
        public bool IsValid()
        {
            string generalSettingsError = null;
            string buttonSettingsError = null;
            return this.GeneralSettings != null && this.GeneralSettings.IsValid(out generalSettingsError) && this.ButtonSettingsList != null && this.ButtonSettingsList.TrueForAll(delegate(ButtonSettingsContainer item) { return item.IsValid(out buttonSettingsError); });
        }

        /// <summary>
        /// Load settings from file
        /// </summary>
        /// <param name="fileName">Location to load settings from.  Uses default location if not supplied.</param>
        /// <returns>Error message if any occur, otherwise null.</returns>
        public string LoadFromFile(string fileName = Constants.DEFAULT_SETTINGS_LOCATION)
        {
            if (File.Exists(fileName))
            {

                try
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(VideoReinforcerSettingsContainer));
                    Stream fileStream = File.Open(fileName, FileMode.Open);
                    VideoReinforcerSettingsContainer fileSettings = (VideoReinforcerSettingsContainer)xmlSerializer.Deserialize(fileStream);
                    fileStream.Close();
                    if (fileSettings.IsValid())
                    {
                        this.GeneralSettings = fileSettings.GeneralSettings;
                        this.ButtonSettingsList = fileSettings.ButtonSettingsList;
                    }
                    else
                    {
                        return Strings.ErrorInvalidFile;
                    }
                }
                catch
                {
                    return Strings.ErrorOpeningFile;
                }
            }
            return null;
        }

        /// <summary>
        /// Saves settings to file
        /// </summary>
        /// <param name="fileName">Location to save settings to.  Uses default location if not supplied.</param>
        /// <returns>Error message if any occur, otherwise null.</returns>
        public string SaveToFile(string fileName = Constants.DEFAULT_SETTINGS_LOCATION)
        {
            try
            {
                if (!this.IsValid())
                {
                    return Strings.ErrorInvalidSettings;
                }
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(VideoReinforcerSettingsContainer));
                Stream fileStream = File.Open(fileName, FileMode.Create);
                xmlSerializer.Serialize(fileStream, this);
                fileStream.Close();
            }
            catch
            {
                return Strings.ErrorSavingFile;
            }
            return null;
        }
    }
}
