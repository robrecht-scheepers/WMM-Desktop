using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WMM.WPF.Goals;

namespace WMM.WPF.Controls
{
    /// <summary>
    /// Interaction logic for DateAmountChart.xaml
    /// </summary>
    public partial class DateAmountChart : UserControl
    {
        private const int NumberOfYSegments = 4;

        private DateTime _xMin;
        private DateTime _xMax;
        private int _amountOfDays;
        private double _dateSectionWidth;
        private double _yMin;
        private double _yMax;
        private double _canvasWidth;
        private double _canvasHeight;


        public DateAmountChart()
        {
            //Series = new List<DateAmountSeries>();
            InitializeComponent();
            Canvas.Loaded += (s, a) => Draw();
            Canvas.SizeChanged += (s, a) => Draw();
        }

        public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register(
            "Series", typeof(List<DateAmountSeries>), typeof(DateAmountChart), new PropertyMetadata(default(List<DateAmountSeries>), SeriesChangedCallback));

        private static void SeriesChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DateAmountChart)d).Draw();
        }

        public List<DateAmountSeries> Series
        {
            get { return (List<DateAmountSeries>)GetValue(SeriesProperty); }
            set { SetValue(SeriesProperty, value); }
        }

        private void Draw()
        {
            if(Canvas == null)
                return;

            Canvas.Children.Clear();

            if (Series == null || !Series.Any())
                return;

            _canvasWidth = Canvas.ActualWidth;
            _canvasHeight = Canvas.ActualHeight;

            SetChartRange();
            SetMarkers();
            DrawAmountGridLines();
            DrawPoints();
        }

        private void SetChartRange()
        {
            // TODO: add rounding margins 
            _xMin = Series.SelectMany(x => x.Points).Select(x => x.Date).Min();
            _xMax = Series.SelectMany(x => x.Points).Select(x => x.Date).Max();
            _yMin = Series.SelectMany(x => x.Points).Select(x => x.Amount).Min();
            _yMax = Series.SelectMany(x => x.Points).Select(x => x.Amount).Max();
        }

        private void SetMarkers()
        {
            var weekendStart = 0d;
            DateLabelCanvas.Children.Clear();
            _amountOfDays = _xMax.Subtract(_xMin).Days + 1;
            _dateSectionWidth = _canvasWidth / _amountOfDays;

            for (int i = 0; i <= _amountOfDays; i++)
            {
                var x = i * _dateSectionWidth;
                var date = _xMin.AddDays(i).Date;
                var weekday = date.DayOfWeek;

                if (weekday == DayOfWeek.Saturday)
                {
                    weekendStart = x;
                }
                else if (weekday == DayOfWeek.Monday || (weekday == DayOfWeek.Sunday && i == _amountOfDays))
                {
                    var weekend = new Rectangle
                    {
                        Fill = Brushes.LightGray,
                        Width = x - weekendStart,
                        Height = _canvasHeight
                    };
                    Canvas.Children.Add(weekend);
                    Canvas.SetTop(weekend, 0);
                    Canvas.SetLeft(weekend, weekendStart);
                }

                Canvas.Children.Add(new Line
                {
                    Stroke = weekday == DayOfWeek.Sunday ? Brushes.DarkGray : Brushes.LightGray,
                    StrokeThickness = 1,
                    X1 = x,
                    X2 = x,
                    Y1 = 0,
                    Y2 = _canvasHeight
                });

                if (i == _amountOfDays)
                    break;

                var label = new TextBlock
                {
                    FontSize = 12,
                    Width = _dateSectionWidth,
                    TextAlignment = TextAlignment.Center,
                    Text = date.Day.ToString()
                };
                DateLabelCanvas.Children.Add(label);
                Canvas.SetTop(label, 6);
                Canvas.SetLeft(label, x);
            }

            var stepY = (_yMax - _yMin) / NumberOfYSegments;
            MarkerY0.Text = _yMin.ToString("C");
            MarkerY1.Text = (_yMin + stepY).ToString("C");
            MarkerY2.Text = (_yMin + 2 * stepY).ToString("C");
            MarkerY3.Text = (_yMin + 3 * stepY).ToString("C");
            MarkerY4.Text = _yMax.ToString("C");
        }

        private void DrawAmountGridLines()
        {
            for (int i = 0; i <= NumberOfYSegments; i++)
            {
                Canvas.Children.Add(new Line
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    X1 = -5,
                    X2 = _canvasWidth,
                    Y1 = i * (_canvasHeight / NumberOfYSegments),
                    Y2 = i * (_canvasHeight / NumberOfYSegments)
                });
            }
        }

        private void DrawPoints()
        {
            foreach (var series in Series)
            {
                var brush = series.Brush;
                Point previousDrawPoint = default(Point);
                var firstPoint = true;

                foreach (var point in series.Points)
                {
                    var drawPoint = CalculateDrawPoint(point);
                    var pointShape = new Ellipse
                    {
                        Fill = brush,
                        Width = 5,
                        Height = 5,
                        ToolTip = $"{point.Date:d}\r\n{point.Amount:C}"
                    };
                    Canvas.Children.Add(pointShape);
                    Canvas.SetLeft(pointShape, drawPoint.X - pointShape.Width / 2);
                    Canvas.SetTop(pointShape, drawPoint.Y - pointShape.Height / 2);
                    if (!firstPoint)
                    {
                        Canvas.Children.Add(new Line
                        {
                            Stroke = brush,
                            StrokeThickness = 1,
                            X1 = previousDrawPoint.X,
                            Y1 = previousDrawPoint.Y,
                            X2 = drawPoint.X,
                            Y2 = drawPoint.Y
                        });
                    }

                    previousDrawPoint = drawPoint;
                    firstPoint = false;
                }
            }
        }

        private Point CalculateDrawPoint(DateAmountPoint point)
        {
            return new Point(
                (point.Date.Subtract(_xMin).Days) * _dateSectionWidth + _dateSectionWidth / 2,
                (_yMax - point.Amount) * _canvasHeight / (_yMax - _yMin)
                );
        }
    }
}
