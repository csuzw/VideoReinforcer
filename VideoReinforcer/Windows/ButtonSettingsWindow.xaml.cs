using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;
using VideoReinforcer.Common;
using VideoReinforcer.Resources;
using VideoReinforcer.Settings;

namespace VideoReinforcer.Windows
{
    /// <summary>
    /// Interaction logic for ButtonSettingsWindow.xaml
    /// </summary>
    public partial class ButtonSettingsWindow : Window
    {
        /// <summary>
        /// Update button settings function.
        /// </summary>
        /// <param name="buttonSettings">Updated button settings.</param>
        /// <param name="index">Index of button settings, -1 if new button settings.</param>
        public delegate void UpdateButtonSettingsFunction(ButtonSettingsContainer buttonSettings, int index = -1);

        private ButtonSettingsContainer buttonSettings = null;
        private int index = -1;
        private UpdateButtonSettingsFunction updateButtonSettings = null;
        private Key shortcut = Key.None;

        private ButtonSettingsWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="updateButtonSettings">Delegate to invoke when OK button pressed.</param>
        /// <param name="buttonSettings">Existing button settings to edit, null if new button settings.</param>
        /// <param name="index">Index of button settings, -1 if new button settings.</param>
        public ButtonSettingsWindow(UpdateButtonSettingsFunction updateButtonSettings, ButtonSettingsContainer buttonSettings = null, int index = -1)
            : this()
        {
            this.updateButtonSettings = updateButtonSettings;
            this.buttonSettings = buttonSettings;
            this.index = index;
        }

        /// <summary>
        /// Populate window from existing button settings.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayScreenSlr.Maximum = Constants.NUMBER_OF_WINDOWS;

            if (this.buttonSettings == null)
            {
                this.buttonSettings = new ButtonSettingsContainer();
            }
            shortcut = this.buttonSettings.Shortcut;

            NameTxt.Text = this.buttonSettings.Name;
            ButtonColorCpr.SelectedColor = this.buttonSettings.ButtonColor;
            TextColorCpr.SelectedColor = this.buttonSettings.TextColor;
            ImageTxt.Text = this.buttonSettings.ImageLocation;
            ShortcutTxt.Text = this.buttonSettings.Shortcut.ToString();
            DisplayTextChk.IsChecked = this.buttonSettings.DisplayText;
            VideoTxt.Text = this.buttonSettings.VideoLocation;
            DisplayScreenSlr.Value = this.buttonSettings.DisplayScreen;
            SpeakerBalanceSlr.Value = this.buttonSettings.SpeakerBalance;

            DisplayScreenSlr_ValueChanged(sender, null);
            SpeakerBalanceSlr_ValueChanged(sender, null);
        }

        /// <summary>
        /// Display open file dialog for selecting an image.
        /// </summary>
        private void ImageBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = Strings.OpenImageFilter;
            openDialog.FilterIndex = 1;
            openDialog.Multiselect = false;
            if (openDialog.ShowDialog() ?? false)
            {
                ImageTxt.Text = openDialog.FileName;
            }
        }

        /// <summary>
        /// Display Capture Shortcut window.
        /// </summary>
        private void ShortcutBtn_Click(object sender, RoutedEventArgs e)
        {
            CaptureShortcutWindow window = new CaptureShortcutWindow(SetShortcut);
            window.ShowDialog();
        }

        /// <summary>
        /// Set shortcut key.
        /// </summary>
        /// <param name="shortcut">Shortcut key.</param>
        private void SetShortcut(Key shortcut)
        {
            this.shortcut = shortcut;
            ShortcutTxt.Text = shortcut.ToString();
        }

        /// <summary>
        /// Display open file dialog for selecting a video.
        /// </summary>
        private void VideoBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = Strings.OpenVideoFilter;
            openDialog.FilterIndex = 1;
            openDialog.Multiselect = false;
            if (openDialog.ShowDialog() ?? false)
            {
                VideoTxt.Text = openDialog.FileName;
            }
        }

        /// <summary>
        /// Get new button settings from screen and validate.
        /// If valid, return new button settings to main screen and close window.
        /// If not valid, display error message.
        /// </summary>
        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            ButtonSettingsContainer newButtonSettings = new ButtonSettingsContainer();
            newButtonSettings.Name = NameTxt.Text;
            newButtonSettings.ButtonColor = ButtonColorCpr.SelectedColor;
            newButtonSettings.TextColor = TextColorCpr.SelectedColor;
            newButtonSettings.ImageLocation = ImageTxt.Text;
            newButtonSettings.Shortcut = shortcut;
            newButtonSettings.DisplayText = DisplayTextChk.IsChecked ?? false;
            newButtonSettings.VideoLocation = VideoTxt.Text;
            newButtonSettings.DisplayScreen = (int)DisplayScreenSlr.Value;
            newButtonSettings.SpeakerBalance = SpeakerBalanceSlr.Value;

            string errorMessage = "";
            if (!newButtonSettings.IsValid(out errorMessage))
            {
                MessageBox.Show(this, errorMessage, Strings.ErrorCaption);
            }
            else
            {
                if (this.updateButtonSettings != null)
                {
                    this.updateButtonSettings(newButtonSettings, this.index);
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
        /// Update text related to Speaker Balance slider.
        /// </summary>
        private void SpeakerBalanceSlr_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SpeakerBalanceValueLbl != null)
            {
                SpeakerBalanceValueLbl.Content = string.Format("{0}%", Math.Round(SpeakerBalanceSlr.Value * 100, 2));
            }
        }

        private void DisplayScreenSlr_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DisplayScreenValueLbl != null)
            {
                DisplayScreenValueLbl.Content = ((int)DisplayScreenSlr.Value).ToString();
            }
        }
    }
}
