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

        public PolygonWaveFormControl()
        {
            this.SizeChanged += OnSizeChanged;
            InitializeComponent();
        }

        public void AddNewWaveForm(Color newColor)
        {
            WaveForm waveForm = new WaveForm();

            waveForm.Stroke = this.Foreground;
            waveForm.StrokeThickness = 1;
            waveForm.Fill = new SolidColorBrush(newColor);
            waveForms.Add(waveForm);
            mainCanvas.Children.Add(waveForm.WaveDisplayShape);

        }


        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // We will remove everything as we are going to rescale vertically
            foreach (var item in waveForms)
            {
                item.renderPosition = 0;
                item.ClearAllPoints();
                item.ActualWidth = ActualWidth;
                item.BlankZone = 10;
                item.yTranslate = this.ActualHeight / 2;
                item.yScale = this.ActualHeight / 2;
            }
        }


       /// <summary>
        /// Clears the waveform and repositions on the left
        /// </summary>
    }

    public class WaveForm
    {
        public StringBuilder sb = new StringBuilder();

        public double ActualWidth { get; set; }

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
