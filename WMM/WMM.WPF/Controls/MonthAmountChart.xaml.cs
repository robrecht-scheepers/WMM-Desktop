using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WMM.WPF.Goals;

namespace WMM.WPF.Controls
{
    /// <summary>
    /// Interaction logic for MonthAmountChart.xaml
    /// </summary>
    public partial class MonthAmountChart : UserControl
    {
        private const int NumberOfYSegments = 4;

        private DateTime _monthMin;
        private DateTime _monthMax;
        private int _amountOfMonths;
        private double _monthSectionWidth;
        private double _amountMin;
        private double _amountMax;
        private double _canvasWidth;
        private double _canvasHeight;

        private readonly GoalStatusColorConverter _colorConverter;
        
        public MonthAmountChart()
        {
            InitializeComponent();
            _colorConverter = new GoalStatusColorConverter();
            Canvas.Loaded += (s, a) => Draw();
            Canvas.SizeChanged += (s, a) => Draw();
        }

        public static readonly DependencyProperty GoalYearInfoProperty = DependencyProperty.Register(
            "GoalYearInfo", typeof(GoalYearInfo), typeof(MonthAmountChart), new PropertyMetadata(default(GoalYearInfo), GoalYearInfoChanged));

        public GoalYearInfo GoalYearInfo
        {
            get => (GoalYearInfo) GetValue(GoalYearInfoProperty);
            set => SetValue(GoalYearInfoProperty, value);
        }
        
        private static void GoalYearInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MonthAmountChart)d).Draw();
        }

        private void Draw()
        {
            if(Canvas == null || Canvas.ActualHeight <1 || Canvas.ActualWidth < 1)
                return;

            Canvas.Children.Clear();
            DateLabelCanvas.Children.Clear();

            if(GoalYearInfo == null || !GoalYearInfo.MonthAmountPoints.Any())
                return;

            _canvasWidth = Canvas.ActualWidth;
            _canvasHeight = Canvas.ActualHeight;

            DrawMonthAxis();
            DrawAmountAxis();
            DrawBars();
            DrawLimitLine();
        }

        private void DrawMonthAxis()
        {
            _monthMin = GoalYearInfo.MonthAmountPoints.Select(x => x.Month).Min();
            _monthMax = GoalYearInfo.MonthAmountPoints.Select(x => x.Month).Max();

            DateLabelCanvas.Children.Clear();
            _amountOfMonths = (_monthMax.Year - _monthMin.Year)*12 + _monthMax.Month - _monthMin.Month + 1;
            _monthSectionWidth = _canvasWidth / _amountOfMonths;
            
            for (int i = 0; i <= _amountOfMonths; i++)
            {
                var x = i * _monthSectionWidth;
                var date = _monthMin.AddMonths(i).Date;

                Canvas.Children.Add(new Line
                {
                    Stroke = Brushes.DarkGray,
                    StrokeThickness = (i == 0 || i == _amountOfMonths ? 1 : 0.5),
                    X1 = x,
                    X2 = x,
                    Y1 = _canvasHeight + (i == 0 || i == _amountOfMonths ? 0 : 10),
                    Y2 = 0
                });

                var label = new TextBlock
                {
                    FontSize = 12,
                    Foreground = Brushes.Black,
                    Margin = new Thickness(0),
                    Width = _monthSectionWidth,
                    TextAlignment = TextAlignment.Center,
                    Text = date.ToString("MMM yy")
                };
                DateLabelCanvas.Children.Add(label);
                Canvas.SetTop(label, 6);
                Canvas.SetLeft(label, x);
            }
        }

        private void DrawAmountAxis()
        {
            var amounts = GoalYearInfo.MonthAmountPoints.Select(x => x.Amount).ToList();
            amounts.Add(GoalYearInfo.Limit);
            amounts.Add(GoalYearInfo.Average);
            
            var amountMin = amounts.Min();
            var amountMax = amounts.Max();

            double factor;
            if (amountMax - amountMin < 0.01)
            {
                factor = 10;
            }
            else
            {
                var orderOfMagnitude = Math.Round(Math.Log10(amountMax - amountMin), MidpointRounding.AwayFromZero) - 1;
                factor = Math.Pow(10, orderOfMagnitude);
            }

            _amountMin = Math.Floor(amountMin / factor) * factor;
            _amountMax = (Math.Floor(amountMax / factor) + 1) * factor;
            // increase the axis range, alternating on top and bottom, until it is dividable by
            // the number of segments, so that we have round numbers for each marker.
            // If one of the ends is 0, do not expand it
            bool alternator = false;
            while ((_amountMax - _amountMin) / 4 % factor > 0)
            {
                if (Math.Abs(_amountMin) < 0.001)
                    _amountMax += factor;
                else if (Math.Abs(_amountMax) < 0.001)
                    _amountMin -= factor;
                else if (alternator)
                    _amountMin -= factor;
                else
                    _amountMax += factor;

                alternator = !alternator;
            }

            var numberOfFactorsPerSegment = (int)((_amountMax - _amountMin) / (4*factor));

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

                if (i > 0)
                {
                    var amountOfSubSegments = numberOfFactorsPerSegment == 1 || numberOfFactorsPerSegment == 2
                        ? 4
                        : numberOfFactorsPerSegment;

                    for (int j = 1; j < amountOfSubSegments; j++)
                    {
                        Canvas.Children.Add(new Line
                        {
                            Stroke = Brushes.Gray,
                            StrokeThickness = 0.2,
                            X1 = 0,
                            X2 = _canvasWidth,
                            Y1 = (i - (j/(double)amountOfSubSegments)) * (_canvasHeight / NumberOfYSegments),
                            Y2 = (i - (j/(double)amountOfSubSegments)) * (_canvasHeight / NumberOfYSegments)
                        });
                    }
                }
            }
        }

        private void DrawBars()
        {
            int i = 0;
            foreach (var monthAmountPoint in GoalYearInfo.MonthAmountPoints.OrderBy(x => x.Month))
            {
                var diff = monthAmountPoint.Amount - GoalYearInfo.Limit;
                var limitY = (_amountMax - GoalYearInfo.Limit) * _canvasHeight / (_amountMax - _amountMin);

                var converterBrush = (SolidColorBrush) _colorConverter.Convert(monthAmountPoint.Status, typeof(SolidColorBrush),
                    null, CultureInfo.CurrentCulture) ?? Brushes.CornflowerBlue;
                var fill = new SolidColorBrush(converterBrush.Color) {Opacity = 0.5};

                var bar = new Rectangle
                {
                    Fill = fill,
                    Width = _monthSectionWidth/3,
                    Height = Math.Abs(diff) * _canvasHeight / (_amountMax - _amountMin) - 1 //-1 because we will move it 1 pixel up so it does not draw over the X axis
                };
                Canvas.Children.Add(bar);
                if (diff > 0)
                    Canvas.SetTop(bar, limitY - diff*_canvasHeight/(_amountMax - _amountMin));
                else
                    Canvas.SetTop(bar, limitY);

                Canvas.SetLeft(bar, (i + 1/3d)* _monthSectionWidth);
                i++;
            }
        }

        private void DrawLimitLine()
        {
            var y = (_amountMax - GoalYearInfo.Limit) * _canvasHeight / (_amountMax - _amountMin);
            Canvas.Children.Add(new Line
            {
                StrokeThickness = 1.5,
                Stroke = Brushes.Green,
                X1 = 1,
                Y1 = y,
                X2 = _canvasWidth,
                Y2 = y
            });
        }
    }
}
