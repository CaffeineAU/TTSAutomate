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
        public WaveForm waveForm = new WaveForm();

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

        private Brush gridBrush = new SolidColorBrush(Color.FromRgb(0,16,0));

        public Brush GridBrush
        {
            get { return gridBrush; }
            set { gridBrush = value; }
        }


        bool selecting = false;

        public PolygonWaveFormControl()
        {
            this.SizeChanged += OnSizeChanged;
            InitializeComponent();
            XScale = 2;
        }

        public void AddNewWaveForm(Color newColor, int samplerate, int bitspersample, int channels)
        {
            waveForm.Stroke = this.Foreground;
            waveForm.StrokeThickness = 1;
            waveForm.Fill = new SolidColorBrush(newColor);
            waveForm.SampleRate = samplerate;
            waveForm.BitsPerSample = bitspersample;
            waveForm.Channels = channels;
            Canvas.SetZIndex(waveForm.WaveDisplayShape, 5);
            mainCanvas.Children.Add(waveForm.WaveDisplayShape);
        }


        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {

            DrawGrid();
        }

        private void DrawGrid()
        {
             // We will remove everything as we are going to rescale vertically
           mainCanvas.Children.Clear();

            for (int i = 25; i < ActualWidth; i += 25)
            {
                mainCanvas.Children.Add(new Line
                {
                    Stroke = GridBrush,
                    StrokeThickness = 0.25f,
                    X1 = i,
                    X2 = i,
                    Y1 = 25,
                    Y2 = ActualHeight - 5
                });
                mainCanvas.Children.Add(new Label
                {
                    Content = String.Format("{0}ms", XLocationToTimeSpan(i).TotalMilliseconds),
                    FontSize = 10,
                    Margin =
                    new Thickness(
                                    i - 5, ActualHeight,
                                    i, ActualHeight),
                    RenderTransform = new RotateTransform(-90)
                });

            }

            double[] dBs = { -2, -4, -6, -9, -12, -15, -18, -24 };

            //for (double i = -9; i < 9; i += NAudio.Utils.Decibels.DecibelsToLinear(3))
            foreach (var db in dBs)
            {
                DrawDBScaleLine(db, true);
                DrawDBScaleLine(db, false);

            }
            mainCanvas.Children.Add(new Line
            {
                Stroke = GridBrush,
                StrokeThickness = 0.25f,
                X1 = 0,
                X2 = ActualWidth,
                Y1 = SampleToYPosition(0),// *(ActualHeight/21) + ActualHeight / 2,
                Y2 = SampleToYPosition(0)
            });


            waveForm.renderPosition = 0;
            //waveForm.ClearAllPoints();
            waveForm.ActualWidth = ActualWidth;
            waveForm.ActualHeight = ActualHeight;
            waveForm.BlankZone = 10;
            waveForm.yTranslate = this.ActualHeight / 2;
            waveForm.yScale = this.ActualHeight / 2;
            waveForm.xScale = XScale;
                mainCanvas.Children.Add(waveForm.WaveDisplayShape);

            
        }

        private void DrawDBScaleLine(double db, bool flip)
        {
            //Console.WriteLine("DB {0} is {1}", db, NAudio.Utils.Decibels.DecibelsToLinear(db));
            mainCanvas.Children.Add(new Line
            {
                Stroke = GridBrush,
                StrokeThickness = 0.25f,
                X1 = 0,
                X2 = ActualWidth,
                Y1 = SampleToYPosition(NAudio.Utils.Decibels.DecibelsToLinear(db) * (flip ? -1 : 1)),// *(ActualHeight/21) + ActualHeight / 2,
                Y2 = SampleToYPosition(NAudio.Utils.Decibels.DecibelsToLinear(db) * (flip ? -1 : 1))
            });
            mainCanvas.Children.Add(new Label
            {
                Content = String.Format("{0} dB", db),
                FontSize = 10,
                Foreground = GridBrush,
                Margin =
                new Thickness(
                                -40, SampleToYPosition(NAudio.Utils.Decibels.DecibelsToLinear(db) * (flip ? -1 : 1)),
                                15, SampleToYPosition(NAudio.Utils.Decibels.DecibelsToLinear(db) * (flip ? -1 : 1)))
            });
            mainCanvas.Children.Add(new Label
            {
                Content = String.Format("{0} dB", db),
                FontSize = 10,
                Foreground = GridBrush,
                Margin =
                new Thickness(
                                ActualWidth, SampleToYPosition(NAudio.Utils.Decibels.DecibelsToLinear(db) * (flip ? -1 : 1)),
                                15, SampleToYPosition(NAudio.Utils.Decibels.DecibelsToLinear(db) * (flip ? -1 : 1)))
            });
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
                //Console.WriteLine(mouseX);
                Canvas.SetLeft(selectionRect, mouseX > TimeSpanToXLocation(SelectionStart) ? TimeSpanToXLocation(SelectionStart) : mouseX);
                selectionRect.Width = Math.Abs(TimeSpanToXLocation(SelectionStart) - mouseX);

                selectionDuration.Content = String.Format("{0}ms", XLocationToTimeSpan(Math.Abs(TimeSpanToXLocation(SelectionStart) - mouseX)).TotalMilliseconds);
                Canvas.SetLeft(selectionDuration, Math.Max(0, Math.Min(TimeSpanToXLocation(SelectionStart), mouseX) + (Math.Abs(TimeSpanToXLocation(SelectionStart) - mouseX) / 2) - selectionDuration.Width / 2));

                Canvas.SetZIndex(selectionDuration, 10);

            }
        }

        private void mainCanvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TimeSpan mousepos = XLocationToTimeSpan(Math.Max(0, e.GetPosition(mainCanvas).X));
            selecting = false;
            if (mousepos < SelectionStart)
            {
                SelectionEnd = SelectionStart;
                SelectionStart = mousepos;
            }
            else
            {
                SelectionEnd = mousepos;
            }
            if (SelectionEnd == SelectionStart)
            {
                mainCanvas.Children.Remove(selectionRect);
                mainCanvas.Children.Remove(selectionDuration);
                selectionDuration = null;
            }
            mainCanvas.ReleaseMouseCapture();
        }

        private void mainCanvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (mainCanvas.Children.Contains(selectionRect))
            {
                mainCanvas.Children.Remove(selectionRect);
            }

            SelectionStart = XLocationToTimeSpan(e.GetPosition(mainCanvas).X);
            selectionRect = new Rectangle { Width = 1, Height = ActualHeight - 4, Fill = new SolidColorBrush(Color.FromArgb(64,128,0,0)), Stroke=Brushes.DarkRed};
            Canvas.SetZIndex(selectionRect, 1);
            Canvas.SetLeft(selectionRect, TimeSpanToXLocation(SelectionStart));
            selectionRect.Width = 0;
            Canvas.SetTop(selectionRect, 2);
            mainCanvas.Children.Add(selectionRect);
            selecting = true;
            mainCanvas.CaptureMouse();
        }

        private TimeSpan XLocationToTimeSpan(double x)
        {
            // x is 1024 bytes

            double scale = 1024 * waveForm.SampleRate * waveForm.BitsPerSample / 256000;

            double millis = 1000 * x * scale / waveForm.SampleRate / (waveForm.BitsPerSample) * waveForm.Channels;
            //System.Diagnostics.Debug.WriteLine("X was {0}, millis is {1}", x, millis);

            return TimeSpan.FromMilliseconds(millis);

        }

        private double TimeSpanToXLocation(TimeSpan time)
        {
            double scale = 1024 * waveForm.SampleRate * waveForm.BitsPerSample / 256000 ;
            return time.TotalMilliseconds / (1000 * scale / waveForm.SampleRate / (waveForm.BitsPerSample) * waveForm.Channels);
        }

        private double SampleToYPosition(double value)
        {
            return (ActualHeight/2) + value * (ActualHeight / 2);
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


        private int sampleRate = 16000;

        public int SampleRate
        {
            get { return sampleRate; }
            set
            {
                sampleRate = value;
            }
        }

        private int bitsPerSample = 16;

        public int BitsPerSample
        {
            get { return bitsPerSample; }
            set
            {
                bitsPerSample = value;
            }
        }

        private int channels = 1;

        public int Channels
        {
            get { return channels; }
            set
            {
                channels = value;
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

                //if (renderPosition > visiblePixels)
                //{
                //    renderPosition = 0;
                //}
                //int erasePosition = (renderPosition + BlankZone) % visiblePixels;
                //if (erasePosition < Points)
                //{
                //    double yPos = SampleToYPosition(0);
                //    WaveDisplayShape.Points[erasePosition] = new Point(erasePosition * xScale, yPos);
                //    WaveDisplayShape.Points[BottomPointIndex(erasePosition)] = new Point(erasePosition * xScale, yPos);
                //}
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
            //System.Diagnostics.Debug.WriteLine("Added point at x:{0}", xPos);
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
