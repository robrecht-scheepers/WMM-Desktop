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

        private DateTime _dateMin;
        private DateTime _dateMax;
        private int _amountOfDays;
        private double _dateSectionWidth;
        private double _amountMin;
        private double _amountMax;
        private double _canvasWidth;
        private double _canvasHeight;


        public DateAmountChart()
        {
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
            get => (List<DateAmountSeries>)GetValue(SeriesProperty);
            set => SetValue(SeriesProperty, value);
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

            DrawDateAxis();
            DrawAmountAxis();
            DrawPoints();
        }

        private void DrawDateAxis()
        {
            _dateMin = Series.SelectMany(x => x.Points).Select(x => x.Date).Min();
            _dateMax = Series.SelectMany(x => x.Points).Select(x => x.Date).Max();

            var weekendStart = 0d;
            DateLabelCanvas.Children.Clear();
            _amountOfDays = _dateMax.Subtract(_dateMin).Days + 1;
            _dateSectionWidth = _canvasWidth / _amountOfDays;

            for (int i = 0; i <= _amountOfDays; i++)
            {
                var x = i * _dateSectionWidth;
                var date = _dateMin.AddDays(i).Date;
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
                    Stroke = weekday == DayOfWeek.Sunday ? Brushes.Black : Brushes.LightGray,
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
        }

        private void DrawAmountAxis()
        {
            var amountMin = Series.SelectMany(x => x.Points).Select(x => x.Amount).Min();
            var amountMax = Series.SelectMany(x => x.Points).Select(x => x.Amount).Max();

            var orderOfMagnitude = Math.Round(Math.Log10(amountMax - amountMin),MidpointRounding.AwayFromZero) - 1;
            var factor = Math.Pow(10, orderOfMagnitude);

            _amountMin = Math.Floor(amountMin / factor) * factor;
            _amountMax = (Math.Floor(amountMax / factor) + 1) * factor;
            // increase the axis range, alternating on top and bottom, until it is dividable by
            // the number of segments, so that we have round numbers for each marker
            bool alternator = false; 
            while ((_amountMax - _amountMin) / 4 % factor > 0)
            {
                if (alternator)
                    _amountMin -= factor;
                else
                    _amountMax += factor;

                alternator = !alternator;
            }

            var step = (_amountMax - _amountMin) / NumberOfYSegments;
            MarkerY0.Text = _amountMin.ToString("C");
            MarkerY1.Text = (_amountMin + step).ToString("C");
            MarkerY2.Text = (_amountMin + 2 * step).ToString("C");
            MarkerY3.Text = (_amountMin + 3 * step).ToString("C");
            MarkerY4.Text = _amountMax.ToString("C");

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
                (point.Date.Subtract(_dateMin).Days) * _dateSectionWidth + _dateSectionWidth / 2,
                (_amountMax - point.Amount) * _canvasHeight / (_amountMax - _amountMin)
                );
        }
    }
}
