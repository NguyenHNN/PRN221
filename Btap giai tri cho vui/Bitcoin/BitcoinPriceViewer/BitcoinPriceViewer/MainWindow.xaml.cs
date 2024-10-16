using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using ServiceStack.Redis;
using System.Windows;
using System.Windows.Threading;

namespace BitcoinPriceViewer
{
    public partial class MainWindow : Window
    {
        private readonly RedisClient _redisClient;
        private readonly string _redisListName = "bitcoin-prices";
        private readonly DispatcherTimer _timer;
        private DateTime _startTime;
        private decimal _startPrice;
        private decimal _minPrice = decimal.MaxValue;
        private DateTime _minTime;
        private decimal _maxPrice = decimal.MinValue;
        private DateTime _maxTime;
        private decimal _previousPrice = 0; // Biến lưu trữ giá trước đó

        public SeriesCollection SeriesCollection { get; set; }
        public List<string> TimeLabels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // Kết nối Redis
            _redisClient = new RedisClient("127.0.0.1", 6379);

            // Khởi tạo timer
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Khởi tạo biểu đồ
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Giá Bitcoin",
                    Values = new ChartValues<decimal>()
                }
            };

            TimeLabels = new List<string>();
            YFormatter = value => value.ToString("C2");

            BitcoinChart.Series = SeriesCollection;
            DataContext = this;

            BitcoinChart.LegendLocation = LegendLocation.None;

            // Lấy giá trị khởi điểm
            _startTime = DateTime.Now;
            _startPrice = GetLatestPrice();
            UpdatePriceInfo();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdatePriceInfo();
        }

        private decimal GetLatestPrice()
        {
            string latestPriceStr = _redisClient.GetItemFromList(_redisListName, -1);
            return decimal.TryParse(latestPriceStr, out decimal price) ? price : 0;
        }

        private void UpdatePriceInfo()
        {
            // Lấy 50 giá trị gần đây nhất
            List<string> recentPricesStr = _redisClient.GetRangeFromList(_redisListName, -300, -1);
            List<decimal> recentPrices = recentPricesStr.Select(s => decimal.TryParse(s, out decimal p) ? p : 0).ToList();

            // Cập nhật giá trị min/max
            foreach (decimal price in recentPrices)
            {
                if (price < _minPrice)
                {
                    _minPrice = price;
                    _minTime = DateTime.Now;
                }
                if (price > _maxPrice)
                {
                    _maxPrice = price;
                    _maxTime = DateTime.Now;
                }
            }

            // Cập nhật UI
            Dispatcher.Invoke(() =>
            {
                CurrentDateTimeTextBlock.Text = $"{DateTime.Now:HH:mm:ss dd/MM/yyyy}";
                StartTimeTextBlock.Text = $"{_startTime:HH:mm:ss dd/MM/yyyy}";
                StartPriceTextBlock.Text = $"{_startPrice:F2}";
                MinTimeTextBlock.Text = $"{_minTime:HH:mm:ss dd/MM/yyyy}";
                MinPriceTextBlock.Text = $"{_minPrice:F2}";
                MaxTimeTextBlock.Text = $"{_maxTime:HH:mm:ss dd/MM/yyyy}";
                MaxPriceTextBlock.Text = $"{_maxPrice:F2}";

                // Xóa tất cả các series cũ
                SeriesCollection.Clear();

                // Thêm từng đoạn LineSeries
                for (int i = 1; i < recentPrices.Count; i++)
                {
                    var previousPrice = recentPrices[i - 1];
                    var currentPrice = recentPrices[i];

                    // Tạo một LineSeries mới cho từng đoạn giữa hai điểm
                    var lineSegment = new LineSeries
                    {
                        Values = new ChartValues<ObservablePoint>
                        {
                             new ObservablePoint(i - 1, (double)previousPrice),
                                new ObservablePoint(i, (double)currentPrice)
                        },
                        StrokeThickness = 2,
                        PointGeometry = null, // Loại bỏ các điểm tròn trên mỗi đoạn
                        Fill = System.Windows.Media.Brushes.Transparent,
                        Stroke = currentPrice > previousPrice ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red
                    };

                    // Thêm lineSegment vào SeriesCollection
                    SeriesCollection.Add(lineSegment);
                }

                // Cập nhật lại biểu đồ
                BitcoinChart.Series = SeriesCollection;

                // Cập nhật thời gian cho biểu đồ
                TimeLabels.Clear();
                TimeLabels.AddRange(Enumerable.Range(1, recentPrices.Count).Select(i => i.ToString()));
                BitcoinChart.AxisX[0].Labels = TimeLabels;
            });
        }
    }
}
