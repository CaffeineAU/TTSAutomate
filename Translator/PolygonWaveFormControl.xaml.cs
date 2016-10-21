using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TTSAutomate
{
    /// <summary>
    /// Interaction logic for PolygonWaveFormControl.xaml
    /// </summary>
    public partial class PolygonWaveFormControl : UserControl
    {
        public List<WaveForm> waveForms = new List<WaveForm>();

        public double XScale { get; set; }

        Line cursor;
        Label cursorPosition;
        Label selectionDuration;
        Rectangle selectionRect;

        private TimeSpan selectionStart = new TimeSpan();

        public TimeSpan SelectionStart
        {
            get { return selectionStart; }
            set { selectionStart = value; }
        }

        private TimeSpan selectionEnd = new TimeSpan();

        public TimeSpan SelectionEnd
        {
            get { return selectionEnd; }
            set { selectionEnd = value; }
        }



        bool selecting = false;

        public PolygonWaveFormControl()
        {
            this.SizeChanged += OnSizeChanged;
            InitializeComponent();
            XScale = 2;
        }

        public void AddNewWaveForm(Color newColor, TimeSpan duration)
        {
            WaveForm waveForm = new WaveForm();

            waveForm.Stroke = this.Foreground;
            waveForm.StrokeThickness = 1;
            waveForm.Duration = duration;
            waveForm.Fill = new SolidColorBrush(newColor);
            waveForms.Add(waveForm);
            Canvas.SetZIndex(waveForm.WaveDisplayShape, 2);
            mainCanvas.Children.Add(waveForm.WaveDisplayShape);

        }


        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // We will remove everything as we are going to rescale vertically

            for (int i = 25; i < ActualWidth; i += 25)
            {
                mainCanvas.Children.Add(new Line
                {
                    Stroke = Brushes.DarkRed,
                    StrokeThickness = 0.3f,
                    X1 = i,
                    X2 = i,
                    Y1 = 25,
                    Y2 = ActualHeight - 5
                });
                mainCanvas.Children.Add(new Label
                {
                    Content = String.Format("{0}ms", i * 8 / XScale),
                    FontSize = 8,
                    Margin =
                    new Thickness(
                                    i - 5, ActualHeight,
                                    i, ActualHeight),
                    RenderTransform = new RotateTransform(-90)
                });

            }

            for (int i = -10; i <= 10; i ++)
            {
                mainCanvas.Children.Add(new Line
                {
                    Stroke = Brushes.DarkRed,
                    StrokeThickness = 0.3f,
                    X1 = 0,
                    X2 = ActualWidth,
                    Y1 = i*(ActualHeight/21) + ActualHeight / 2,
                    Y2 = i * (ActualHeight / 21) + ActualHeight / 2
                });
                //mainCanvas.Children.Add(new Label
                //{
                //    Content = String.Format("{0}ms", i * 8 / XScale),
                //    FontSize = 8,
                //    Margin =
                //    new Thickness(
                //                    i - 5, ActualHeight,
                //                    i, ActualHeight),
                //    RenderTransform = new RotateTransform(-90)
                //});

            }


            foreach (var item in waveForms)
            {
                item.renderPosition = 0;
                item.ClearAllPoints();
                item.ActualWidth = ActualWidth;
                item.ActualHeight = ActualHeight;
                item.BlankZone = 10;
                item.yTranslate = this.ActualHeight / 2;
                item.yScale = this.ActualHeight / 2;
                item.xScale = XScale;
            }
        }

        private void mainCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            double mouseX = Math.Min(mainCanvas.ActualWidth, Math.Max(0, e.GetPosition(mainCanvas).X));
            if (cursor == null)
            {
                cursor = new Line
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.3f,
                    X1 = -1,
                    X2 = -1,
                    Y1 = 5,
                    Y2 = ActualHeight - 5,
                };
                Canvas.SetZIndex(cursor, 10);
                mainCanvas.Children.Add(cursor);
                cursorPosition = new Label { Content = String.Format("{0}ms", 0), FontSize = 8 };

                Canvas.SetZIndex(cursorPosition, 10);
                mainCanvas.Children.Add(cursorPosition);

            }
            Canvas.SetLeft(cursor, mouseX);
            cursorPosition.Content = String.Format("{0}ms",  XLocationToTimeSpan(mouseX).TotalMilliseconds);

            Canvas.SetLeft(cursorPosition, mouseX);
            Canvas.SetTop(cursorPosition, 10);
            if (selecting)
            {
                if (selectionDuration == null)
                {
                    selectionDuration = new Label { Content = String.Format("{0}ms", XLocationToTimeSpan(Math.Abs(TimeSpanToXLocation(SelectionStart) - mouseX))), FontSize = 8, Width=50, HorizontalContentAlignment= HorizontalAlignment.Center };
                    mainCanvas.Children.Add(selectionDuration);

                }
                Console.WriteLine(mouseX);
                Canvas.SetLeft(selectionRect, mouseX > TimeSpanToXLocation(SelectionStart) ? TimeSpanToXLocation(SelectionStart) : mouseX);
                selectionRect.Width = Math.Abs(TimeSpanToXLocation(SelectionStart) - mouseX);

                selectionDuration.Content = String.Format("{0}ms", XLocationToTimeSpan(Math.Abs(TimeSpanToXLocation(SelectionStart) - mouseX)).TotalMilliseconds);
                Canvas.SetLeft(selectionDuration, Math.Max(0, Math.Min(TimeSpanToXLocation(SelectionStart), mouseX) + (Math.Abs(TimeSpanToXLocation(SelectionStart) - mouseX) / 2) - selectionDuration.Width / 2));

                Canvas.SetZIndex(cursorPosition, 10);

            }
        }

        private void mainCanvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            selecting = false;
            SelectionEnd = XLocationToTimeSpan(Math.Max(0,e.GetPosition(mainCanvas).X));
            mainCanvas.ReleaseMouseCapture();
        }

        private void mainCanvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (mainCanvas.Children.Contains(selectionRect))
            {
                mainCanvas.Children.Remove(selectionRect);
            }

            SelectionStart = XLocationToTimeSpan(e.GetPosition(mainCanvas).X);
            selectionRect = new Rectangle { Width = 1, Height = ActualHeight - 4, Fill = new SolidColorBrush(Color.FromArgb(64, 0, 128, 0)), Stroke=Brushes.DarkGreen };
            Canvas.SetZIndex(selectionRect, 20);
            Canvas.SetLeft(selectionRect, TimeSpanToXLocation(SelectionStart));
            selectionRect.Width = 0;
            Canvas.SetTop(selectionRect, 2);
            mainCanvas.Children.Add(selectionRect);
            selecting = true;
            mainCanvas.CaptureMouse();
        }

        private TimeSpan XLocationToTimeSpan(double x)
        {
            return TimeSpan.FromMilliseconds(x / 25 * 100);
        }

        private double TimeSpanToXLocation(TimeSpan time)
        {
            return time.TotalMilliseconds / 100 * 25;
        }

        /// <summary>
        /// Clears the waveform and repositions on the left
        /// </summary>
    }

    public class WaveForm
    {
        public StringBuilder sb = new StringBuilder();

        private TimeSpan duration;

        public TimeSpan Duration
        {
            get { return duration; }
            set
            {
                duration = value;
            }
        }

        public double ActualWidth { get; set; }

        public double ActualHeight { get; set; }

        public int BlankZone { get; set; }

        private double ytranslate = 40;

        public double yTranslate
        {
            get { return ytranslate; }
            set { ytranslate = value; }
        }

        private double yscale = 40;

        public double yScale
        {
            get { return yscale; }
            set { yscale = value; }
        }

        private double xscale = 2;

        public double xScale
        {
            get { return xscale; }
            set { xscale = value; }
        }

        public Brush Stroke
        {
            get { return WaveDisplayShape.Stroke; }
            set { WaveDisplayShape.Stroke = value; }
        }

        public Brush Fill
        {
            get { return WaveDisplayShape.Fill; }
            set { WaveDisplayShape.Fill = value; }
        }

        public double StrokeThickness
        {
            get { return WaveDisplayShape.StrokeThickness; }
            set { WaveDisplayShape.StrokeThickness = value; }
        }

        public Polygon WaveDisplayShape = new Polygon();
        public int renderPosition { get; set; }
        public int Points
        {
            get { return WaveDisplayShape.Points.Count / 2; }
        }

        public void AddValue(float maxValue, float minValue)
        {
            int visiblePixels = (int)(ActualWidth / xScale);
            if (visiblePixels > 0)
            {
                CreatePoint(maxValue, minValue);

                if (renderPosition > visiblePixels)
                {
                    renderPosition = 0;
                }
                int erasePosition = (renderPosition + BlankZone) % visiblePixels;
                if (erasePosition < Points)
                {
                    double yPos = SampleToYPosition(0);
                    WaveDisplayShape.Points[erasePosition] = new Point(erasePosition * xScale, yPos);
                    WaveDisplayShape.Points[BottomPointIndex(erasePosition)] = new Point(erasePosition * xScale, yPos);
                }
            }
        }

        public void Reset()
        {
            renderPosition = 0;
            ClearAllPoints();
        }


        public void CreatePoint(float topValue, float bottomValue)
        {
            double topYPos = SampleToYPosition(topValue);
            double bottomYPos = SampleToYPosition(bottomValue);
            double xPos = renderPosition * xScale;
            if (renderPosition >= Points)
            {
                int insertPos = Points;
                WaveDisplayShape.Points.Insert(insertPos, new Point(xPos, topYPos));
                WaveDisplayShape.Points.Insert(insertPos + 1, new Point(xPos, bottomYPos));
            }
            else
            {
                WaveDisplayShape.Points[renderPosition] = new Point(xPos, topYPos);
                WaveDisplayShape.Points[BottomPointIndex(renderPosition)] = new Point(xPos, bottomYPos);
            }
            renderPosition++;
            sb.AppendFormat("{0}\t{1}\t{2}\r\n", xPos, topYPos, bottomYPos);
        }
        private int BottomPointIndex(int position)
        {
            return WaveDisplayShape.Points.Count - position - 1;
        }

        private double SampleToYPosition(float value)
        {
            return yTranslate + value * yScale;
        }

        public void ClearAllPoints()
        {
            WaveDisplayShape.Points.Clear();

        }



    }
}
