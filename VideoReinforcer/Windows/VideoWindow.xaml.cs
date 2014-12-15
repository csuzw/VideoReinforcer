using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

using VideoReinforcer.Common;
using VideoReinforcer.Resources;

namespace VideoReinforcer.Windows
{
    /// <summary>
    /// Interaction logic for VideoWindow.xaml
    /// </summary>
    public partial class VideoWindow : Window
    {
        /// <summary>
        /// If true, window can be closed, otherwise window will only be hidden.
        /// </summary>
        public bool CanClose { get; set; }

        private MainWindow mainWindow = null;
        private Screen screen = null;
        private int screenIndex = 0;
        private Dictionary<int, MediaElement> mediaElementDictionary = new Dictionary<int, MediaElement>();
        private MediaElement currentMediaElement = null;
        private bool isPlaying = false;
        private bool isMuted = false;
        private bool hideCursor = false;
        private bool hasBeenShown = false;

        /// <summary>
        /// Default constructor.
        /// </summary>
        private VideoWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mainWindow">Main window that this window was called from.</param>
        /// <param name="screen">The screen to display the window on.</param>
        /// <param name="screenIndex">Index of display screen.</param>
        public VideoWindow(MainWindow mainWindow, Screen screen, int screenIndex)
            : this()
        {
            this.mainWindow = mainWindow;
            this.screen = screen;
            this.screenIndex = screenIndex;

            this.Title = string.Format(Strings.VideoWindowTitle, screenIndex);
        }

        /// <summary>
        /// Closes and clears all loaded videos
        /// </summary>
        public void ClearVideoList()
        {
            ClearCurrentVideo();

            foreach (MediaElement mediaElement in this.mediaElementDictionary.Values)
            {
                mediaElement.Close();
            }

            this.mediaElementDictionary.Clear();
        }

        /// <summary>
        /// Adds or edits a video to list
        /// </summary>
        /// <param name="index">Index of video in list if editing, -1 if adding.</param>
        /// <param name="videoLocation">Location of video.</param>
        /// <param name="speakerBalance">Speaker balance.</param>
        /// <param name="displayScreen">Display screen.</param>
        public void AddOrEditVideo(int index, string videoLocation, double speakerBalance, int displayScreen)
        {
            ClearCurrentVideo();

            if (index >= 0 && this.mediaElementDictionary.ContainsKey(index))
            {
                if (displayScreen == this.screenIndex)
                {
                    this.mediaElementDictionary[index].Source = new Uri(videoLocation);
                    this.mediaElementDictionary[index].Balance = speakerBalance;
                }
                else
                {
                    RemoveVideo(index);
                }
            }
            else if (displayScreen == this.screenIndex)
            {
                this.mediaElementDictionary.Add(index, CreateMediaElement(videoLocation, speakerBalance));
            }
        }

        /// <summary>
        /// Close and remove a video from list
        /// </summary>
        /// <param name="index">Index of video in list</param>
        public void RemoveVideo(int index)
        {
            ClearCurrentVideo();

            if (index >= 0 && mediaElementDictionary.ContainsKey(index))
            {
                this.mediaElementDictionary[index].Close();
                this.mediaElementDictionary.Remove(index);
            }
        }

        /// <summary>
        /// Toggles video state.  Play if it is not currently playing, stop otherwise.
        /// </summary>
        /// <param name="index">Index of video in list</param>
        public void ToggleVideo(int index)
        {
            if (index >= 0 && this.mediaElementDictionary.ContainsKey(index))
            {
                if (this.currentMediaElement != this.mediaElementDictionary[index])
                {
                    ClearCurrentVideo();
                    this.currentMediaElement = this.mediaElementDictionary[index];
                }
                if (this.currentMediaElement.Source != null)
                {
                    if (this.isPlaying)
                    {
                        this.currentMediaElement.Visibility = Visibility.Hidden;
                        this.currentMediaElement.Stop();
                        this.isPlaying = false;
                    }
                    else
                    {
                        this.ShowWindow();
                        this.currentMediaElement.Visibility = Visibility.Visible;
                        this.currentMediaElement.IsMuted = isMuted;
                        this.currentMediaElement.Play();
                        this.isPlaying = true;
                    }
                }
            }
            else
            {
                ClearCurrentVideo();
            }
        }

        /// <summary>
        /// Toggle sound state.
        /// </summary>
        public void ToggleSound(bool isMuted)
        {
            this.isMuted = isMuted;
            if (this.currentMediaElement != null)
            {
                this.currentMediaElement.IsMuted = this.isMuted;
            }
        }

        /// <summary>
        /// Toggle cursor state.
        /// </summary>
        public void ToggleCursor(bool hideCursor)
        {
            this.hideCursor = hideCursor;
            if (hideCursor)
            {
                Cursor = System.Windows.Input.Cursors.None;
            }
            else
            {
                Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        /// <summary> 
        /// Return window state to normal from maximised when Escape is pressed.
        /// </summary>
        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized && e.Key == Key.Escape)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                mainWindow.Window_KeyUp(sender, e);
            }
        }

        /// <summary>
        /// Used to change border style when window state changes.
        /// </summary>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowStyle = WindowStyle.None;
            }
            else
            {
                this.WindowStyle = WindowStyle.SingleBorderWindow;
            }
        }

        /// <summary>
        /// Prevents this window being closed whilst main application is still open.
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ClearCurrentVideo();
            if (this.CanClose)
            {
                ClearVideoList();
            }
            else
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        /// <summary>
        /// Display error message and close video on failure.
        /// </summary>
        private void MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            System.Windows.MessageBox.Show(this, string.Format(Strings.ErrorMediaFailure, e.ErrorException.Message), Strings.ErrorCaption);
            ClearCurrentVideo();
        }

        /// <summary>
        /// Display error message and close video on failure.
        /// </summary>
        private void MediaEnded(object sender, RoutedEventArgs e)
        {
            if (e.Source is MediaElement)
            {
                MediaElement mediaElement = (MediaElement)e.Source;
                mediaElement.Position = new TimeSpan(0);
                mediaElement.Play();
            }
        }

        /// <summary>
        /// Stops and clears the currently selected video.
        /// </summary>
        private void ClearCurrentVideo()
        {
            if (this.currentMediaElement != null)
            {
                this.currentMediaElement.Visibility = Visibility.Hidden;
                if (this.isPlaying)
                {
                    this.currentMediaElement.Stop();
                    this.isPlaying = false;
                }
            }
        }

        /// <summary>
        /// Creates new video element.
        /// </summary>
        /// <param name="videoLocation">Location of video.</param>
        /// <param name="speakerBalance">Speaker balance.</param>
        /// <returns></returns>
        private MediaElement CreateMediaElement(string videoLocation, double speakerBalance)
        {
            MediaElement mediaElement = new MediaElement();
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.MediaFailed += MediaFailed;
            mediaElement.MediaEnded += MediaEnded;
            if (!string.IsNullOrEmpty(videoLocation) && File.Exists(videoLocation))
            {
                try
                {
                    mediaElement.Source = new Uri(videoLocation);
                }
                catch { }
            }
            mediaElement.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            mediaElement.VerticalAlignment = VerticalAlignment.Stretch;
            mediaElement.Visibility = Visibility.Hidden;
            mediaElement.Balance = speakerBalance;
            VideoGrd.Children.Add(mediaElement);
            Grid.SetRow(mediaElement, 0);
            Grid.SetColumn(mediaElement, 0);

            return mediaElement;
        }

        /// <summary>
        /// Show window.
        /// </summary>
        private void ShowWindow()
        {
            if (!this.IsActive && !this.IsVisible)
            {
                this.Show();

                if (!this.hasBeenShown && this.screen != null)
                {
                    this.Top = screen.WorkingArea.Top;
                    this.Left = screen.WorkingArea.Left;
                    this.WindowState = WindowState.Maximized;
                    this.hasBeenShown = true;
                }
            }
        }
    }
}
