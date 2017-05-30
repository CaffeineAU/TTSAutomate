using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TTSAutomate
{
    /// <summary>
    /// Interaction logic for PolygonWaveFormControl.xaml
    /// </summary>
    public partial class PolygonWaveFormControl : UserControl, INotifyPropertyChanged
    {
        private WaveForm waveForm = new WaveForm();

        public WaveForm WaveFormDisplay
        {
            get { return waveForm; }
            set
            {
                waveForm = value;
                OnPropertyChanged("WaveFormDisplay");
            }
        }


        Cursor defaultCursor;

        private double maxValue = 0;

        public double MaxValue
        {
            get { return maxValue; }
            set
            {
                maxValue = value;
                OnPropertyChanged("MaxValue");
                //System.Diagnostics.Debug.WriteLine("max {0}", MaxValue);
            }
        }

        private double minValue = 0;

        public double MinValue
        {
            get { return minValue; }
            set
            {
                minValue = value;
                OnPropertyChanged("MinValue");
            }
        }

        private TimeSpan cursorTime = new TimeSpan();

        public TimeSpan CursorTime
        {
            get { return cursorTime; }
            set
            {
                cursorTime = value;
                OnPropertyChanged("CursorTime");
            }
        }

        private String cursordB = "";

        public String CursordB
        {
            get { return cursordB; }
            set
            {
                cursordB = value;
                OnPropertyChanged("CursordB");
            }
        }


        public double XScale { get; set; }

        Line cursor;
        Label cursorPosition;
        Label selectionDuration;
        Rectangle selectionRect;
        double mouseX;

        private TimeSpan selectionStart = new TimeSpan();

        public TimeSpan SelectionStart
        {
            get { return selectionStart; }
            set
            {
                selectionStart = value;
                OnPropertyChanged("SelectionStart");
            }
        }

        private TimeSpan selectionDurationTS = new TimeSpan();

        public TimeSpan SelectionDuration
        {
            get { return selectionDurationTS; }
            set
            {
                selectionDurationTS = value;
                OnPropertyChanged("SelectionDuration");
            }
        }

        private TimeSpan selectionEnd = new TimeSpan();

        public TimeSpan SelectionEnd
        {
            get { return selectionEnd; }
            set
            {
                selectionEnd = value;
                OnPropertyChanged("SelectionEnd");
            }
        }

        private Brush gridBrush = new SolidColorBrush(Color.FromRgb(0, 16, 0));

        public Brush GridBrush
        {
            get { return gridBrush; }
            set { gridBrush = value; }
        }


        bool selecting = false;
        private bool movingStart = false;
        private bool movingEnd = false;

        public PolygonWaveFormControl()
        {
            this.SizeChanged += OnSizeChanged;
            InitializeComponent();
            XScale = 2;
            defaultCursor = Cursor;
            this.DataContext = this;
        }

        public void AddNewWaveForm(Color newColor, int samplerate, int bitspersample, int channels)
        {
            WaveFormDisplay = new WaveForm();
            WaveFormDisplay.yTranslate = 300;
            waveForm.yScale = 300;
            //WaveFormDisplay.WaveDisplayShape = new Polygon();
            WaveFormDisplay.Values = new Dictionary<int, Tuple<float, float>>();
            WaveFormDisplay.Stroke = this.Foreground;
            WaveFormDisplay.StrokeThickness = 1;
            WaveFormDisplay.Fill = new SolidColorBrush(newColor);
            WaveFormDisplay.SampleRate = samplerate;
            WaveFormDisplay.BitsPerSample = bitspersample;
            WaveFormDisplay.Channels = channels;
            Canvas.SetZIndex(WaveFormDisplay.WaveDisplayShape, 5);
            mainCanvas.Children.Add(WaveFormDisplay.WaveDisplayShape);
        }


        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {

            RedrawGrid();
        }

        public void ClearWaveForm()
        {
            //mainCanvas.Children.Remove(WaveFormDisplay.WaveDisplayShape);
            WaveFormDisplay.Reset();
            //mainCanvas.InvalidateVisual();
            //mainCanvas.UpdateLayout();
            RedrawGrid();
        }

        public void RedrawGrid()
        {
            // We will remove everything as we are going to rescale vertically
            
            mainCanvas.Children.Clear();
            cursor = null;
            mainCanvas.InvalidateVisual();

            for (int i = 25; i < mainCanvas.ActualWidth; i += 25)
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
                    Content = String.Format("{0} ms", XLocationToTimeSpan(i).TotalMilliseconds),
                    FontSize = 10,
                    Margin =
                    new Thickness(
                                    i - 5, ActualHeight - 14,
                                    i, ActualHeight - 40),
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
            LegendCanvas.Children.Add(new Line
            {
                Stroke = Brushes.Red,
                StrokeThickness = 0.25f,
                X1 = 0,
                X2 = ActualWidth,
                Y1 = SampleToYPosition(0),
                Y2 = SampleToYPosition(0)
            });


            WaveFormDisplay.renderPosition = 0;
            //WaveFormDisplay.ClearAllPoints();
            //WaveFormDisplay.ActualWidth = ActualWidth;
            //WaveFormDisplay.ActualHeight = ActualHeight;
            WaveFormDisplay.BlankZone = 10;
            //WaveFormDisplay.yTranslate = this.ActualHeight / 2;
            //WaveFormDisplay.yScale = this.ActualHeight / 2;
            WaveFormDisplay.xScale = XScale;
            mainCanvas.Children.Add(WaveFormDisplay.WaveDisplayShape);


        }

        private void DrawDBScaleLine(double db, bool flip)
        {
            //Console.WriteLine("DB {0} is {1}", db, NAudio.Utils.Decibels.DecibelsToLinear(db));
            LegendCanvas.Children.Add(new Line
            {
                Stroke = GridBrush,
                StrokeThickness = 0.25f,
                X1 = 0,
                X2 = ActualWidth,
                Y1 = SampleToYPosition(NAudio.Utils.Decibels.DecibelsToLinear(db) * (flip ? -1 : 1)),// *(ActualHeight/21) + ActualHeight / 2,
                Y2 = SampleToYPosition(NAudio.Utils.Decibels.DecibelsToLinear(db) * (flip ? -1 : 1))
            });
            LegendCanvas.Children.Add(new Label
            {
                Content = String.Format("{0} dB", db),
                FontSize = 10,
                Foreground = GridBrush,
                Margin =
                new Thickness(
                                -40, SampleToYPosition(NAudio.Utils.Decibels.DecibelsToLinear(db) * (flip ? -1 : 1)) - 10,
                                15, SampleToYPosition(NAudio.Utils.Decibels.DecibelsToLinear(db) * (flip ? -1 : 1)) - 10)
            });
            //LegendCanvas.Children.Add(new Label
            //{
            //    Content = String.Format("{0} dB", db),
            //    FontSize = 10,
            //    Foreground = GridBrush,
            //    Margin =
            //    new Thickness(
            //                    ActualWidth - 20, 20 - SampleToYPosition(NAudio.Utils.Decibels.DecibelsToLinear(db) * (flip ? -1 : 1)) - 10,
            //                    ActualWidth, 20 - SampleToYPosition(NAudio.Utils.Decibels.DecibelsToLinear(db) * (flip ? -1 : 1)) - 10)
            //});
        }

        private void mainCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            mouseX = Math.Min(mainCanvas.ActualWidth, Math.Max(0, e.GetPosition(mainCanvas).X));

            Point mousepos = e.GetPosition(mainCanvas);

            UpdateCursor(mousepos, selecting);
        }

        public void UpdateCursor(Point mousepos, bool drawSelection)
        {
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

            if ((Difference(mousepos.X, TimeSpanToXLocation(SelectionStart)) < 2) ||
                    Difference(mousepos.X, TimeSpanToXLocation(SelectionEnd)) < 2)
            {
                Cursor = System.Windows.Input.Cursors.SizeWE;
            }
            else
            {
                Cursor = defaultCursor;
            }

            int xPos = Convert.ToInt32(mouseX);
            if (xPos % 2 != 0)
            {
                xPos--;
            }
            MaxValue = WaveFormDisplay.Values.ContainsKey(xPos) ? -WaveFormDisplay.Values[xPos].Item2 : double.MinValue;
            MinValue = WaveFormDisplay.Values.ContainsKey(xPos) ? WaveFormDisplay.Values[xPos].Item1 : double.MinValue;
            Canvas.SetLeft(cursor, mouseX);
            cursorPosition.Content = String.Format("{0}ms", XLocationToTimeSpan(mouseX).TotalMilliseconds);
            CursorTime = XLocationToTimeSpan(mouseX);
            CursordB = String.Format("({0:F2}dB / {1:F2}dB)", double.IsNaN(NAudio.Utils.Decibels.LinearToDecibels(MaxValue))? double.NegativeInfinity : NAudio.Utils.Decibels.LinearToDecibels(MaxValue), double.IsNaN(NAudio.Utils.Decibels.LinearToDecibels(MinValue)) ? double.NegativeInfinity : NAudio.Utils.Decibels.LinearToDecibels(MinValue));

            Canvas.SetLeft(cursorPosition, mouseX);
            Canvas.SetTop(cursorPosition, 10);
            if (drawSelection)
            {
                if (selectionDuration == null)
                {
                    selectionDuration = new Label { Content = String.Format("{0}ms", XLocationToTimeSpan(Difference(TimeSpanToXLocation(SelectionStart), mouseX))), FontSize = 8, Width = 50, HorizontalContentAlignment = HorizontalAlignment.Center };
                    mainCanvas.Children.Add(selectionDuration);

                }
                //Console.WriteLine(mouseX);
                Canvas.SetLeft(selectionRect, mouseX > TimeSpanToXLocation(SelectionStart) ? TimeSpanToXLocation(SelectionStart) : mouseX);
                selectionRect.Width = Math.Abs(TimeSpanToXLocation(SelectionStart) - mouseX);

                selectionDuration.Content = String.Format("{0}ms", XLocationToTimeSpan(Difference(TimeSpanToXLocation(SelectionStart), mouseX)).TotalMilliseconds);
                SelectionDuration = XLocationToTimeSpan(Difference(TimeSpanToXLocation(SelectionStart), mouseX));
                SelectionEnd = XLocationToTimeSpan(mouseX);
                Canvas.SetLeft(selectionDuration, Math.Max(0, Math.Min(TimeSpanToXLocation(SelectionStart), mouseX) + (Difference(TimeSpanToXLocation(SelectionStart), mouseX) / 2) - selectionDuration.Width / 2));

                Canvas.SetZIndex(selectionDuration, 10);

            }
            if (movingStart)
            {
                SelectionStart = XLocationToTimeSpan(mouseX);
                SelectionDuration = XLocationToTimeSpan(Difference(TimeSpanToXLocation(SelectionEnd), mouseX));

                if (SelectionStart > selectionEnd) // We crossed the streams
                {
                    SwapStartAndEnd();
                    movingStart = false;
                    movingEnd = true;
                    return;
                }
                Canvas.SetLeft(selectionRect, mouseX);
                selectionRect.Width = Difference(TimeSpanToXLocation(SelectionEnd), mouseX);
            }
            if (movingEnd)
            {
                SelectionEnd = XLocationToTimeSpan(mouseX);
                SelectionDuration = XLocationToTimeSpan(Difference(TimeSpanToXLocation(SelectionStart), mouseX));

                if (SelectionStart > selectionEnd) // We crossed the streams
                {
                    SwapStartAndEnd();
                    movingStart = true;
                    movingEnd = false;
                    return;
                }

                selectionRect.Width = Difference(TimeSpanToXLocation(SelectionStart), mouseX);
            }
        }

        private void SwapStartAndEnd()
        {
            TimeSpan temp = selectionStart;
            SelectionStart = SelectionEnd;
            selectionEnd = temp;
        }

        private void mainCanvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TimeSpan mousepos = XLocationToTimeSpan(Math.Max(0, e.GetPosition(mainCanvas).X));
            selecting = false;
            if (movingStart)
            {
                SelectionStart = mousepos;
                movingStart = false;
            }
            else if (movingEnd)
            {
                SelectionEnd = mousepos;
                movingEnd = false;
            }
            else
            {
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
            }
            mainCanvas.ReleaseMouseCapture();
        }

        private void mainCanvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mouseX = e.GetPosition(mainCanvas).X;

            if (mainCanvas.Children.Contains(selectionRect))
            {
                if (Difference(mouseX, TimeSpanToXLocation(SelectionStart)) < 2) // we're over the start of the selection rectangle
                {
                    movingStart = true;
                }
                else if (Difference(mouseX, TimeSpanToXLocation(SelectionEnd)) < 2) // we're over the start of the selection rectangle
                {
                    movingEnd = true;
                }
                else
                {
                    mainCanvas.Children.Remove(selectionRect);
                }
            }

            if (!movingStart && !movingEnd)
            {
                SelectionStart = XLocationToTimeSpan(e.GetPosition(mainCanvas).X);
                selectionRect = new Rectangle { Width = 1, Height = ActualHeight - 120, Fill = new SolidColorBrush(Color.FromArgb(64, 128, 0, 0)), Stroke = Brushes.DarkRed };
                Canvas.SetZIndex(selectionRect, 1);
                Canvas.SetLeft(selectionRect, TimeSpanToXLocation(SelectionStart));
                selectionRect.Width = 0;
                Canvas.SetTop(selectionRect, 60);
                mainCanvas.Children.Add(selectionRect);
                selecting = true;

            }
            mainCanvas.CaptureMouse();
        }

        private double Difference(double first, double second)
        {
            return Math.Abs(first - second);

        }

        public void DrawSelectionRect()
        {
            mainCanvas.Children.Remove(selectionRect);
            mainCanvas.Children.Remove(selectionDuration);

            selectionRect = new Rectangle { Width = 1, Height = ActualHeight - 120, Fill = new SolidColorBrush(Color.FromArgb(64, 128, 0, 0)), Stroke = Brushes.DarkRed };
            mainCanvas.Children.Add(selectionRect);

            Canvas.SetZIndex(selectionRect, 1);
            Canvas.SetTop(selectionRect, 60);

            selectionDuration = new Label { Content = String.Format("{0}ms", XLocationToTimeSpan(Difference(TimeSpanToXLocation(SelectionStart), mouseX))), FontSize = 8, Width = 50, HorizontalContentAlignment = HorizontalAlignment.Center };
            mainCanvas.Children.Add(selectionDuration);
            //Console.WriteLine(mouseX);
            Canvas.SetLeft(selectionRect, TimeSpanToXLocation(SelectionStart));
            selectionRect.Width = Math.Abs(TimeSpanToXLocation(SelectionStart) - TimeSpanToXLocation(SelectionEnd));

            selectionDuration.Content = String.Format("{0}ms", XLocationToTimeSpan(Difference(TimeSpanToXLocation(SelectionStart), TimeSpanToXLocation(SelectionEnd))).TotalMilliseconds);
            SelectionDuration = XLocationToTimeSpan(Difference(TimeSpanToXLocation(SelectionStart), TimeSpanToXLocation(SelectionEnd)));
            Canvas.SetLeft(selectionDuration, Math.Max(0, Math.Min(TimeSpanToXLocation(SelectionStart), TimeSpanToXLocation(SelectionEnd)) + (Difference(TimeSpanToXLocation(SelectionStart), TimeSpanToXLocation(SelectionEnd)) / 2) - selectionDuration.Width / 2));

            Canvas.SetZIndex(selectionDuration, 10);

        }

        public TimeSpan XLocationToTimeSpan(double x)
        {
            // x is 1024 bytes

            double scale = 1024 * WaveFormDisplay.SampleRate * WaveFormDisplay.BitsPerSample / 256000;

            double millis = 1000 * x * scale / WaveFormDisplay.SampleRate / (WaveFormDisplay.BitsPerSample) * WaveFormDisplay.Channels;
            //System.Diagnostics.Debug.WriteLine("X was {0}, millis is {1}", x, millis);

            return TimeSpan.FromMilliseconds(millis);

        }

        private double TimeSpanToXLocation(TimeSpan time)
        {
            double scale = 1024 * WaveFormDisplay.SampleRate * WaveFormDisplay.BitsPerSample / 256000;
            return time.TotalMilliseconds / (1000 * scale / WaveFormDisplay.SampleRate / (WaveFormDisplay.BitsPerSample) * WaveFormDisplay.Channels);
        }

        private double SampleToYPosition(double value)
        {
            return (ActualHeight / 2) + value * (ActualHeight / 2);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(String name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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

        public Dictionary<int, Tuple<float, float>> Values { get; set; }


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
            //if (visiblePixels > 0)
            //{
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
            //}
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
            Values.Add(Convert.ToInt32(xPos), new Tuple<float, float>(topValue, bottomValue));
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
