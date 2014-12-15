using System.Windows;
using System.Windows.Input;

namespace VideoReinforcer.Windows
{
    /// <summary>
    /// Interaction logic for CaptureShortcutWindow.xaml
    /// </summary>
    public partial class CaptureShortcutWindow : Window
    {
        /// <summary>
        /// Update button shortcut key function.
        /// </summary>
        /// <param name="shortcut">Updated shortcut key.</param>
        public delegate void SetShortcutFunction(Key shortcut);

        private SetShortcutFunction setShortcut = null;

        private CaptureShortcutWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="setShortcut">Delegate to invoke when OK button pressed.</param>
        public CaptureShortcutWindow(SetShortcutFunction setShortcut)
            : this()
        {
            this.setShortcut = setShortcut;
        }

        /// <summary>
        /// Close window and return no shortcut key.
        /// </summary>
        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.setShortcut != null)
            {
                this.setShortcut(Key.None);
            }
            Close();
        }

        /// <summary>
        /// Close window without returning a shortcut key.
        /// </summary>
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Close window and return pressed key.
        /// </summary>
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.setShortcut != null)
            {
                this.setShortcut(e.Key);
            }
            Close();
        }
    }
}
