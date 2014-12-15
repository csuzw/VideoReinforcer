using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VideoReinforcer.Controls.ColorPicker {
    /// <summary>
    /// Code derived/copied from: SilverlightContrib (http://silverlightcontrib.codeplex.com/)
    /// SilverlightContribLicense.txt should be included with this application 
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl {

        /// <summary>
        /// Event fired when the selected color changes.  This event occurs when the 
        /// left-mouse button is lifted after clicking.
        /// </summary>
        public event SelectedColorChangedHandler SelectedColorChanged;

        /// <summary>
        /// Event fired when the selected color is changing.  This event occurs when the 
        /// left-mouse button is pressed and the user is moving the mouse.
        /// </summary>
        public event SelectedColorChangingHandler SelectedColorChanging;

        private readonly ColorSpace m_colorSpace;
        private bool m_hueMonitorMouseCaptured;
        private bool m_sampleMouseCaptured;
        private double m_huePos = 0;
        private double m_sampleX = 0;
        private double m_sampleY = 0;

        private ScaleTransform m_scale = new ScaleTransform();

        /// <summary>
        /// Create a new instance of the ColorPicker control.
        /// </summary>
        public ColorPicker() {
            InitializeComponent();

            DefaultStyleKey = typeof(ColorPicker);
            m_colorSpace = new ColorSpace();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {

            RootElement.RenderTransform = m_scale;

            ColorPickerElement.Visibility = Visibility.Collapsed;

            HeaderElement.MouseLeftButtonUp += stackPanelHeaderElement_MouseLeftButtonUp;

            HueMonitor.MouseLeftButtonDown += rectHueMonitor_MouseLeftButtonDown;
            HueMonitor.MouseLeftButtonUp += rectHueMonitor_MouseLeftButtonUp;
            HueMonitor.MouseMove += rectHueMonitor_MouseMove;

            ColorSample.MouseLeftButtonDown += rectSampleMonitor_MouseLeftButtonDown;
            ColorSample.MouseLeftButtonUp += rectSampleMonitor_MouseLeftButtonUp;
            ColorSample.MouseMove += rectSampleMonitor_MouseMove;

            UpdateVisuals();
        }

        private void stackPanelHeaderElement_MouseLeftButtonUp(object sender, MouseEventArgs e) {

            if (ColorPickerElement.Visibility == Visibility.Visible) {
                ColorPickerElement.Visibility = Visibility.Collapsed;
            }
            else {
                ColorPickerElement.Visibility = Visibility.Visible;
            }
        }

        private void rectHueMonitor_MouseLeftButtonDown(object sender, MouseEventArgs e) {

            m_hueMonitorMouseCaptured = HueMonitor.CaptureMouse();
            DragSliders(0, e.GetPosition((UIElement)sender).Y);
        }

        private void rectHueMonitor_MouseLeftButtonUp(object sender, MouseEventArgs e) {
            HueMonitor.ReleaseMouseCapture();
            m_hueMonitorMouseCaptured = false;
            SetValue(SelectedColorProperty, GetColor());
        }

        private void rectHueMonitor_MouseMove(object sender, MouseEventArgs e) {
            DragSliders(0, e.GetPosition((UIElement)sender).Y);
        }

        private void rectSampleMonitor_MouseLeftButtonDown(object sender, MouseEventArgs e) {
            m_sampleMouseCaptured = ColorSample.CaptureMouse();
            Point pos = e.GetPosition((UIElement)sender);
            DragSliders(pos.X, pos.Y);
        }

        private void rectSampleMonitor_MouseLeftButtonUp(object sender, MouseEventArgs e) {
            ColorSample.ReleaseMouseCapture();
            m_sampleMouseCaptured = false;
            SetValue(SelectedColorProperty, GetColor());
        }

        private void rectSampleMonitor_MouseMove(object sender, MouseEventArgs e) {
            if (!m_sampleMouseCaptured)
                return;

            Point pos = e.GetPosition((UIElement)sender);
            DragSliders(pos.X, pos.Y);
        }


        private Color GetColor() {
            double yComponent = 1 - (m_sampleY / ColorSample.Height);
            double xComponent = m_sampleX / ColorSample.Width;
            double hueComponent = (m_huePos / HueMonitor.Height) * 360;

            return m_colorSpace.ConvertHsvToRgb(hueComponent, xComponent, yComponent);
        }

        private void UpdateSatValSelection() {
            if (ColorSample == null)
                return;

            SampleSelector.SetValue(Canvas.LeftProperty, m_sampleX - (SampleSelector.Height / 2));
            SampleSelector.SetValue(Canvas.TopProperty, m_sampleY - (SampleSelector.Height / 2));

            Color currColor = GetColor();
            SelectedColorView.Fill = new SolidColorBrush(currColor);
            HexValue.Text = m_colorSpace.GetHexCode(currColor);

            FireSelectedColorChangingEvent(currColor);
        }

        private void UpdateHueSelection() {
            if (HueMonitor == null)
                return;
            double huePos = m_huePos / HueMonitor.Height * 255;
            Color c = m_colorSpace.GetColorFromPosition(huePos);
            ColorSample.Fill = new SolidColorBrush(c);

            HueSelector.SetValue(Canvas.TopProperty, m_huePos - (HueSelector.Height / 2));

            Color currColor = GetColor();

            SelectedColorView.Fill = new SolidColorBrush(currColor);
            HexValue.Text = m_colorSpace.GetHexCode(currColor);

            FireSelectedColorChangingEvent(currColor);
        }

        private void UpdateVisuals() {
            if (HueMonitor == null)
                return;

            Color c = this.SelectedColor;
            ColorSpace cs = new ColorSpace();
            HSV hsv = cs.ConvertRgbToHsv(c);

            m_huePos = (hsv.Hue / 360 * HueMonitor.Height);
            m_sampleY = -1 * (hsv.Value - 1) * ColorSample.Height;
            m_sampleX = hsv.Saturation * ColorSample.Width;
            if (!double.IsNaN(m_huePos))
                UpdateHueSelection();
            UpdateSatValSelection();
        }

        private void DragSliders(double x, double y) {
            if (m_hueMonitorMouseCaptured) {
                if (y < 0)
                    m_huePos = 0;
                else if (y > HueMonitor.Height)
                    m_huePos = HueMonitor.Height;
                else
                    m_huePos = y;
                UpdateHueSelection();
            }
            else if (m_sampleMouseCaptured) {
                if (x < 0)
                    m_sampleX = 0;
                else if (x > ColorSample.Width)
                    m_sampleX = ColorSample.Width;
                else
                    m_sampleX = x;

                if (y < 0)
                    m_sampleY = 0;
                else if (y > ColorSample.Height)
                    m_sampleY = ColorSample.Height;
                else
                    m_sampleY = y;

                UpdateSatValSelection();
            }
        }

        ///// <summary>
        ///// Called by the layout system during a layout pass.
        ///// </summary>
        ///// <param name="availableSize">The size available to the child elements.</param>
        ///// <returns>The size set by the child elements.</returns>
        //protected override Size MeasureOverride(Size availableSize) {
        //    Size size = new Size();
        //    if (RootElement != null) {
        //        RootElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        //        Size desiredSize = RootElement.DesiredSize;

        //        Size scale = ComputeScaleFactor(desiredSize, m_defaultSize);

        //        size.Width = scale.Width * desiredSize.Width;
        //        size.Height = scale.Height * desiredSize.Height;
        //    }

        //    return size;
        //}

        ///// <summary>
        ///// Called by the layout system during a layout pass.
        ///// </summary>
        ///// <param name="finalSize">The size determined to be availble to the child elements.</param>
        ///// <returns>The final size used by the child elements.</returns>
        //protected override Size ArrangeOverride(Size finalSize) {
        //    if (RootElement != null) {
        //        // Determine the scale factor given the final size
        //        Size desiredSize = RootElement.DesiredSize;
        //        Size scale = ComputeScaleFactor(finalSize, desiredSize);

        //        m_scale.ScaleX = scale.Width;
        //        m_scale.ScaleY = scale.Height;

        //        // Position the ChildElement to fill the ChildElement
        //        Rect originalPosition = new Rect(0, 0, desiredSize.Width, desiredSize.Height);
        //        RootElement.Arrange(originalPosition);

        //        // Determine the final size used by the Viewbox
        //        finalSize.Width = scale.Width * desiredSize.Width;
        //        finalSize.Height = scale.Height * desiredSize.Height;
        //    }
        //    return finalSize;
        //}


        //private Size ComputeScaleFactor(Size availableSize, Size contentSize) {
        //    double scaleX = 1.0;
        //    double scaleY = 1.0;

        //    bool isConstrainedWidth = !double.IsPositiveInfinity(availableSize.Width);
        //    bool isConstrainedHeight = !double.IsPositiveInfinity(availableSize.Height);

        //    // Don't scale if we shouldn't stretch or the scaleX and scaleY are both infinity.
        //    if (isConstrainedWidth || isConstrainedHeight) {
        //        // Compute the individual scaleX and scaleY scale factors
        //        scaleX = contentSize.Width == 0 ? 0.0 : (availableSize.Width / contentSize.Width);
        //        scaleY = contentSize.Height == 0 ? 0.0 : (availableSize.Height / contentSize.Height);

        //        // Make the scale factors uniform by setting them both equal to
        //        // the larger or smaller (depending on infinite lengths and the
        //        // Stretch value)
        //        if (!isConstrainedWidth) {
        //            scaleX = scaleY;
        //        }
        //        else if (!isConstrainedHeight) {
        //            scaleY = scaleX;
        //        }
        //    }

        //    return new Size(scaleX, scaleY);
        //}

        private void FireSelectedColorChangingEvent(Color selectedColor) {
            if (SelectedColorChanging != null) {
                SelectedColorEventArgs args = new SelectedColorEventArgs(selectedColor);
                SelectedColorChanging(this, args);
            }
        }

        #region SelectedColor Dependency Property
        /// <summary>
        /// Gets or sets the currently selected color in the Color Picker.
        /// </summary>
        public Color SelectedColor {
            get { return (Color)GetValue(SelectedColorProperty); }
            set {
                SetValue(SelectedColorProperty, value);
                this.UpdateVisuals();
            }
        }

        /// <summary>
        /// SelectedColor Dependency Property.
        /// </summary>
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(
                "SelectedColor",
                typeof(Color),
                typeof(ColorPicker),
                new PropertyMetadata(new PropertyChangedCallback(SelectedColorPropertyChanged)));

        private static void SelectedColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ColorPicker p = d as ColorPicker;
            if (p != null && p.SelectedColorChanged != null) {
                SelectedColorEventArgs args = new SelectedColorEventArgs((Color)e.NewValue);
                p.SelectedColorChanged(p, args);
            }
        }


        #endregion
    }
}
