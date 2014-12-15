using System;
using System.Windows;
using System.Windows.Input;
using VideoReinforcer.Common;
using VideoReinforcer.Resources;
using VideoReinforcer.Settings;

namespace VideoReinforcer.Windows
{
    /// <summary>
    /// Interaction logic for GeneralSettingsWindow.xaml
    /// </summary>
    public partial class GeneralSettingsWindow : Window
    {
        /// <summary>
        /// Update general settings function.
        /// </summary>
        /// <param name="generalSettings">Updated general settings.</param>
        public delegate void UpdateGeneralSettingsFunction(GeneralSettingsContainer generalSettings);

        private GeneralSettingsContainer generalSettings = null;
        private UpdateGeneralSettingsFunction updateGeneralSettings = null;
        private Key muteShortcut = Key.None;
        private Key hideCursorShortcut = Key.None;

        private GeneralSettingsWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generalSettings">Current general settings.</param>
        /// <param name="updateGeneralSettings">Delegate to invoke when OK button pressed.</param>
        public GeneralSettingsWindow(GeneralSettingsContainer generalSettings, UpdateGeneralSettingsFunction updateGeneralSettings)
            : this()
        {
            this.generalSettings = generalSettings;
            this.updateGeneralSettings = updateGeneralSettings;
        }

        /// <summary>
        /// Populate window from existing general settings.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.generalSettings == null)
            {
                this.generalSettings = new GeneralSettingsContainer();
            }
            muteShortcut = this.generalSettings.MuteShortcut;
            hideCursorShortcut = this.generalSettings.HideCursorShortcut;

            WidthSlr.Value = this.generalSettings.ButtonWidth;
            HeightSlr.Value = this.generalSettings.ButtonHeight;
            ColumnsSlr.Value = this.generalSettings.ButtonColumns;
            MuteShortcutTxt.Text = this.generalSettings.MuteShortcut.ToString();
            HideCursorShortcutTxt.Text = this.generalSettings.HideCursorShortcut.ToString();

            WidthSlr_ValueChanged(sender, null);
            HeightSlr_ValueChanged(sender, null);
            ColumnsSlr_ValueChanged(sender, null);
        }

        /// <summary>
        /// Update text related to Width slider.
        /// </summary>
        private void WidthSlr_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WidthValueTxt != null)
            {
                WidthValueTxt.Content = ((int)WidthSlr.Value).ToString();
            }
        }

        /// <summary>
        /// Update text related to Height slider.
        /// </summary>
        private void HeightSlr_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (HeightValueTxt != null)
            {
                HeightValueTxt.Content = ((int)HeightSlr.Value).ToString();
            }
        }

        /// <summary>
        /// Update text related to Columns slider.
        /// </summary>
        private void ColumnsSlr_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ColumnsValueTxt != null)
            {
                ColumnsValueTxt.Content = ((int)ColumnsSlr.Value).ToString();
            }
        }

        /// <summary>
        /// Get new general settings from screen and validate.
        /// If valid, return new general settings to main screen and close window.
        /// If not valid, display error message.
        /// </summary>
        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            GeneralSettingsContainer newGeneralSettings = new GeneralSettingsContainer();
            newGeneralSettings.ButtonWidth = (int)WidthSlr.Value;
            newGeneralSettings.ButtonHeight = (int)HeightSlr.Value;
            newGeneralSettings.ButtonColumns = (int)ColumnsSlr.Value;
            newGeneralSettings.MuteShortcut = muteShortcut;
            newGeneralSettings.HideCursorShortcut = hideCursorShortcut;

            string errorMessage = "";
            if (!newGeneralSettings.IsValid(out errorMessage))
            {

                MessageBox.Show(this, errorMessage, Strings.ErrorCaption);
            }
            else
            {
                if (this.updateGeneralSettings != null)
                {
                    this.updateGeneralSettings(newGeneralSettings);
                }
                Close();
            }
        }

        /// <summary>
        /// Close window without saving.
        /// </summary>
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Display Capture Shortcut window.
        /// </summary>
        private void MuteShortcutBtn_Click(object sender, RoutedEventArgs e)
        {
            CaptureShortcutWindow window = new CaptureShortcutWindow(SetMuteShortcut);
            window.ShowDialog();
        }

        /// <summary>
        /// Set mute shortcut key.
        /// </summary>
        /// <param name="shortcut">Shortcut key.</param>
        private void SetMuteShortcut(Key shortcut)
        {
            this.muteShortcut = shortcut;
            MuteShortcutTxt.Text = shortcut.ToString();
        }

        /// <summary>
        /// Display Capture Shortcut window.
        /// </summary>
        private void HideCursorBtn_Click(object sender, RoutedEventArgs e)
        {
            CaptureShortcutWindow window = new CaptureShortcutWindow(SetHideCursorShortcut);
            window.ShowDialog();
        }

        /// <summary>
        /// Set hide cursor shortcut key.
        /// </summary>
        /// <param name="shortcut">Shortcut key.</param>
        private void SetHideCursorShortcut(Key shortcut)
        {
            this.hideCursorShortcut = shortcut;
            HideCursorShortcutTxt.Text = shortcut.ToString();
        }
    }
}
