using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VideoReinforcer.Common;
using VideoReinforcer.Resources;
using VideoReinforcer.Settings;

namespace VideoReinforcer.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // globals
        private VideoReinforcerSettingsContainer settings = null;
        private List<ToggleButton> buttonList = new List<ToggleButton>();
        private List<VideoWindow> videoWindowList = new List<VideoWindow>();
        private bool isMuted = false;
        private bool hideCursor = false;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Process shortcut keys.
        /// </summary>
        public void Window_KeyUp(object sender, KeyEventArgs e)
        {
            for (int i = 0; i < settings.ButtonSettingsList.Count; i++)
            {
                if (e.Key == settings.ButtonSettingsList[i].Shortcut)
                {
                    ToggleVideo(i);
                    return;
                }
            }
            if (e.Key == settings.GeneralSettings.MuteShortcut)
            {
                ToggleSound();
                return;
            }
            if (e.Key == settings.GeneralSettings.HideCursorShortcut)
            {
                ToggleCursor();
                return;
            }
        }

        /// <summary>
        /// Loads settings from default file.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitialiseVideoWindowList();

            settings = new VideoReinforcerSettingsContainer();
            LoadSettingsFromFile();
        }

        /// <summary>
        /// Displays open file dialog and loads selected settings file.
        /// </summary>
        private void LoadMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = Strings.OpenSettingsFilter;
            openDialog.FilterIndex = 1;
            openDialog.Multiselect = false;
            if (openDialog.ShowDialog() ?? false)
            {

                LoadSettingsFromFile(openDialog.FileName);
            }
        }

        /// <summary>
        /// Displays save file dialog and saves current settings to file.
        /// </summary>
        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = Strings.SaveSettingsFilter;
            saveDialog.DefaultExt = Strings.SettingsExtension;
            if (saveDialog.ShowDialog() ?? false)
            {

                SaveSettingsToFile(saveDialog.FileName);
            }
        }

        /// <summary>
        /// Displays General Settings window.
        /// </summary>
        private void GeneralSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GeneralSettingsWindow window = new GeneralSettingsWindow(settings.GeneralSettings, UpdateGeneralSettings);
            window.ShowDialog();
        }

        /// <summary>
        /// Displays Button Settings window.
        /// </summary>
        private void AddButtonMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ButtonSettingsWindow window = new ButtonSettingsWindow(AddOrEditButtonSettings);
            window.ShowDialog();
        }

        /// <summary>
        /// Ensures video window is closed correctly.
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ClearVideoWindowList();
        }

        /// <summary>
        /// Save current settings to default file.
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            SaveSettingsToFile();
        }

        /// <summary>
        /// Display Button Settings window for selected button.
        /// </summary>
        private void EditButtonSettings(object sender, RoutedEventArgs e)
        {
            int index = GetIndexFromSource(e.Source);
            if (index >= 0)
            {
                ButtonSettingsWindow window = new ButtonSettingsWindow(AddOrEditButtonSettings, settings.ButtonSettingsList[index], index);
                window.ShowDialog();
            }
        }

        /// <summary>
        /// Move selected button to different position in list.
        /// </summary>
        private void MoveButtonSettings(object sender, RoutedEventArgs e)
        {
            int index = GetIndexFromSource(e.Source);
            if (index >= 0 && index != settings.ButtonSettingsList.Count - 1)
            {
                settings.ButtonSettingsList.Reverse(index, 2);

                BuildFullLayout();
            }
        }

        /// <summary>
        /// Delete selected button.
        /// </summary>
        private void DeleteButtonSettings(object sender, RoutedEventArgs e)
        {
            int index = GetIndexFromSource(e.Source);
            if (index >= 0)
            {
                settings.ButtonSettingsList.RemoveAt(index);

                DeleteButton(index);
            }
        }

        /// <summary>
        /// Process selected button click.
        /// </summary>
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            int index = GetIndexFromSource(e.Source);
            if (index >= 0)
            {             
                foreach (var button in buttonList.Where(b => b != e.Source))
                {
                    button.IsChecked = false;
                }
                ToggleVideo(index);
            }
        }

        /// <summary>
        /// Load settings from file.
        /// </summary>
        /// <param name="fileName">Location of settings.</param>
        /// <returns>True if successful, false otherwise.</returns>
        private bool LoadSettingsFromFile(string fileName = Constants.DEFAULT_SETTINGS_LOCATION)
        {
            string errorMessage = settings.LoadFromFile(fileName);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show(this, errorMessage, Strings.ErrorCaption);
                return false;
            }
            else
            {
                BuildFullLayout();
            }
            return true;
        }

        /// <summary>
        /// Save settings to file.
        /// </summary>
        /// <param name="fileName">Location to save settings.</param>
        /// <returns>True if successful, false otherise.</returns>
        private bool SaveSettingsToFile(string fileName = Constants.DEFAULT_SETTINGS_LOCATION)
        {
            string errorMessage = settings.SaveToFile(fileName);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show(this, errorMessage, Strings.ErrorCaption);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Update layout after general settings change.
        /// </summary>
        /// <param name="generalSettings">New general settings.</param>
        private void UpdateGeneralSettings(GeneralSettingsContainer generalSettings)
        {
            settings.GeneralSettings = generalSettings;

            UpdateButtonLayout();
        }

        /// <summary>
        /// Update layout after button settings change.
        /// </summary>
        /// <param name="buttonSettings">New button settings.</param>
        /// <param name="index">Index of existing button settings to edit, -1 if adding.</param>
        private void AddOrEditButtonSettings(ButtonSettingsContainer buttonSettings, int index = -1)
        {
            if (index >= 0)
            {
                settings.ButtonSettingsList[index] = buttonSettings;
            }
            else
            {
                settings.ButtonSettingsList.Add(buttonSettings);
                index = settings.ButtonSettingsList.Count - 1;
            }

            AddOrEditButton(index);
        }

        /// <summary>
        /// Gets button index from source control.
        /// </summary>
        /// <param name="source">Source control.</param>
        /// <returns>Index of button, -1 if not found.</returns>
        private int GetIndexFromSource(object source)
        {
            if (source is MenuItem)
            {
                MenuItem menuItem = (MenuItem)source;
                if (menuItem.Parent is MenuItem)
                {
                    MenuItem parentMenuItem = (MenuItem)menuItem.Parent;
                    return SettingsMenu.Items.IndexOf(parentMenuItem) - 2;
                }
            }
            else if (source is ToggleButton)
            {
                return buttonList.IndexOf((ToggleButton)source);
            }
            return -1;
        }

        /// <summary>
        /// Build full layout from current settings.
        /// </summary>
        private void BuildFullLayout()
        {
            ClearExistingLayout();

            for (int i = 0; i < settings.ButtonSettingsList.Count; i++)
            {
                AddOrEditButton(i, false);
            }

            UpdateButtonLayout();
        }

        /// <summary>
        /// Clear existing layout.
        /// </summary>
        private void ClearExistingLayout()
        {
            // clear button items
            while (SettingsMenu.Items.Count > 4)
            {
                SettingsMenu.Items.RemoveAt(2);
            }

            // clear buttons
            ButtonGrd.Children.Clear();
            buttonList.Clear();

            // clear videos
            foreach (VideoWindow videoWindow in videoWindowList)
            {
                videoWindow.ClearVideoList();
            }
        }

        /// <summary>
        /// Update grid to match current settings.
        /// </summary>
        private void UpdateGrid()
        {
            // build grid
            if (settings.ButtonSettingsList.Count <= 0)
            {
                return;
            }

            // columns
            int columnCount = Math.Min(settings.GeneralSettings.ButtonColumns, settings.ButtonSettingsList.Count);
            if (ButtonGrd.ColumnDefinitions.Count < columnCount)
            {
                while (ButtonGrd.ColumnDefinitions.Count < columnCount)
                {
                    ButtonGrd.ColumnDefinitions.Add(new ColumnDefinition());
                }
            }
            else if (ButtonGrd.ColumnDefinitions.Count > columnCount)
            {
                ButtonGrd.ColumnDefinitions.RemoveRange(columnCount, ButtonGrd.ColumnDefinitions.Count - columnCount);
            }

            // rows
            int rowCount = (settings.ButtonSettingsList.Count / columnCount) + 1;
            if (ButtonGrd.RowDefinitions.Count < rowCount)
            {
                while (ButtonGrd.RowDefinitions.Count < rowCount)
                {
                    ButtonGrd.RowDefinitions.Add(new RowDefinition());
                }
            }
            else if (ButtonGrd.RowDefinitions.Count > rowCount)
            {
                ButtonGrd.RowDefinitions.RemoveRange(rowCount, ButtonGrd.RowDefinitions.Count - rowCount);
            }
        }

        /// <summary>
        /// Update button layout from current settings.
        /// </summary>
        private void UpdateButtonLayout()
        {
            UpdateGrid();

            int columnCount = ButtonGrd.ColumnDefinitions.Count;

            for (int i = 0; i < buttonList.Count; i++)
            {
                buttonList[i].Width = settings.GeneralSettings.ButtonWidth;
                buttonList[i].Height = settings.GeneralSettings.ButtonHeight;

                Grid.SetRow(buttonList[i], i / columnCount);
                Grid.SetColumn(buttonList[i], i % columnCount);
            }
        }

        /// <summary>
        /// Add or edit button to layout.
        /// </summary>
        /// <param name="index">Index of button.</param>
        /// <param name="updateButtonLayout">Should button layout be updated after button has been added?  Defaults to true.</param>
        private void AddOrEditButton(int index, bool updateButtonLayout = true)
        {
            if (index < settings.ButtonSettingsList.Count)
            {
                ButtonSettingsContainer buttonSettings = settings.ButtonSettingsList[index];

                bool exists = (index < buttonList.Count);
                ToggleButton button;
                if (exists)
                {
                    button = buttonList[index];
                }
                else
                {
                    button = new ToggleButton();
                    button.Margin = new Thickness(4);
                    button.Click += ButtonClick;
                }
                if (buttonSettings.DisplayText)
                {
                    button.Content = buttonSettings.ButtonText;
                    button.Foreground = new SolidColorBrush(buttonSettings.TextColor);
                }
                bool useImage = false;
                if (!string.IsNullOrEmpty(buttonSettings.ImageLocation) && File.Exists(buttonSettings.ImageLocation))
                {
                    try
                    {
                        button.Background = new ImageBrush(new BitmapImage(new Uri(buttonSettings.ImageLocation)));
                        useImage = true;
                    }
                    catch { }
                }
                if (!useImage)
                {
                    button.Background = new SolidColorBrush(buttonSettings.ButtonColor);
                }
                if (!exists)
                {
                    buttonList.Add(button);
                    ButtonGrd.Children.Add(button);
                }
                AddOrEditMenuItem(index, buttonSettings.Name);
                AddOrEditVideo(index, buttonSettings.VideoLocation, buttonSettings.SpeakerBalance, buttonSettings.DisplayScreen);

                if (updateButtonLayout)
                {
                    UpdateButtonLayout();
                }
            }
        }

        /// <summary>
        /// Add or edit menu item to layout.
        /// </summary>
        /// <param name="index">Index of menu item.</param>
        /// <param name="text">Text to display on menu item.</param>
        private void AddOrEditMenuItem(int index, string text)
        {
            if (index < SettingsMenu.Items.Count - 4)
            {
                ((MenuItem)SettingsMenu.Items[index + 2]).Header = text;
            }
            else
            {
                MenuItem buttonMenuItem = new MenuItem();
                buttonMenuItem.Header = text;

                MenuItem editMenuItem = new MenuItem();
                editMenuItem.Header = Strings.EditText;
                editMenuItem.Click += EditButtonSettings;

                MenuItem moveMenuItem = new MenuItem();
                moveMenuItem.Header = Strings.MoveText;
                moveMenuItem.Click += MoveButtonSettings;

                MenuItem deleteMenuItem = new MenuItem();
                deleteMenuItem.Header = Strings.DeleteText;
                deleteMenuItem.Click += DeleteButtonSettings;

                buttonMenuItem.Items.Add(editMenuItem);
                buttonMenuItem.Items.Add(moveMenuItem);
                buttonMenuItem.Items.Add(deleteMenuItem);
                SettingsMenu.Items.Insert(SettingsMenu.Items.Count - 2, buttonMenuItem);
            }
        }

        /// <summary>
        /// Delete button from layout.
        /// </summary>
        /// <param name="index">Index of button.</param>
        /// <param name="updateButtonLayout">Should button layout be updated after button has been added?  Defaults to true.</param>
        private void DeleteButton(int index, bool updateButtonLayout = true)
        {
            if (index < buttonList.Count)
            {
                ToggleButton button = buttonList[index];
                SettingsMenu.Items.RemoveAt(index + 2);
                ButtonGrd.Children.Remove(button);
                buttonList.Remove(button);
                RemoveVideo(index);

                if (updateButtonLayout)
                {
                    UpdateButtonLayout();
                }
            }
        }

        /// <summary>
        /// Add or edit video.
        /// </summary>
        /// <param name="index">Index of video.</param>
        /// <param name="videoLocation">Location of video.</param>
        /// <param name="speakerBalance">Speaker balance.</param>
        /// <param name="displayScreen">Display screen.</param>
        private void AddOrEditVideo(int index, string videoLocation, double speakerBalance, int displayScreen)
        {
            foreach (VideoWindow videoWindow in videoWindowList)
            {
                videoWindow.AddOrEditVideo(index, videoLocation, speakerBalance, displayScreen);
            }
        }

        /// <summary>
        /// Close and remove video.
        /// </summary>
        /// <param name="index">Index of video.</param>
        private void RemoveVideo(int index)
        {
            foreach (VideoWindow videoWindow in videoWindowList)
            {
                videoWindow.RemoveVideo(index);
            }
        }

        /// <summary>
        /// Toggle video state.
        /// </summary>
        /// <param name="index">Index of video.</param>
        private void ToggleVideo(int index)
        {
            foreach (VideoWindow videoWindow in videoWindowList)
            {
                videoWindow.ToggleVideo(index);
            }
        }

        /// <summary>
        /// Toggle sound state.
        /// </summary>
        private void ToggleSound()
        {
            isMuted = !isMuted;
            foreach (VideoWindow videoWindow in videoWindowList)
            {
                videoWindow.ToggleSound(isMuted);
            }
        }

        /// <summary>
        /// Toggle cursor state.
        /// </summary>
        private void ToggleCursor()
        {
            hideCursor = !hideCursor;
            if (hideCursor)
            {
                Cursor = System.Windows.Input.Cursors.None;
            }
            else
            {
                Cursor = System.Windows.Input.Cursors.Arrow;
            }
            foreach (VideoWindow videoWindow in videoWindowList)
            {
                videoWindow.ToggleCursor(hideCursor);
            }
        }

        private void ClearVideoWindowList()
        {
            foreach (VideoWindow videoWindow in videoWindowList)
            {
                videoWindow.CanClose = true;
                videoWindow.Close();
            }
            videoWindowList.Clear();
        }

        private void InitialiseVideoWindowList()
        {
            for (int i = 1; i <= Constants.NUMBER_OF_WINDOWS; i++)
            {
                System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.PrimaryScreen;
                if (i <= System.Windows.Forms.Screen.AllScreens.Length)
                {
                    screen = System.Windows.Forms.Screen.AllScreens[i - 1];
                }

                videoWindowList.Add(new VideoWindow(this, screen, i));
            }
        }
    }
}
