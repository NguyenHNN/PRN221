using CsvHelper;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
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

namespace DAY3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Model> diemThiList_data;

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            string filePath = @"C:\Users\PC\Documents\Zalo Received Files\diem_thi_thpt_2024.csv";
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Tệp không tồn tại tại đường dẫn: " + filePath);
                return;
            }

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                diemThiList_data = csv.GetRecords<Model>().ToList();
            }
        }

        public List<Model> GetTop5ToanTren(List<Model> data, bool topHighest)
        {
            return topHighest
                ? data.Where(d => d.toan.HasValue).OrderByDescending(d => d.toan.Value).Take(5).ToList()
                : data.Where(d => d.toan.HasValue).OrderByDescending(d => d.toan.Value).Take(5).ToList();
        }

        public List<Model> GetTop5ToanDuoi(List<Model> data, bool topHighest)
        {
            return topHighest
                ? data.Where(d => d.toan.HasValue).OrderBy(d => d.toan.Value).Take(5).ToList()
                : data.Where(d => d.toan.HasValue).OrderBy(d => d.toan.Value).Take(5).ToList();
        }

        public Model GetTopKhoaA(List<Model> data)
        {
            return data.Where(d => d.toan.HasValue && d.vat_li.HasValue && d.hoa_hoc.HasValue)
                       .OrderByDescending(d => d.toan.Value + d.vat_li.Value + d.hoa_hoc.Value)
                       .FirstOrDefault();
        }

        public int CountMon(List<Model> data, string mon)
        {
            return mon switch
            {
                "toan" => data.Count(d => d.toan.HasValue),
                "ngu_van" => data.Count(d => d.ngu_van.HasValue),
                "ngoai_ngu" => data.Count(d => d.ngoai_ngu.HasValue),
                "vat_li" => data.Count(d => d.vat_li.HasValue),
                "hoa_hoc" => data.Count(d => d.hoa_hoc.HasValue),
                "sinh_hoc" => data.Count(d => d.sinh_hoc.HasValue),
                "lich_su" => data.Count(d => d.lich_su.HasValue),
                "dia_li" => data.Count(d => d.dia_li.HasValue),
                "gdcd" => data.Count(d => d.gdcd.HasValue),
                _ => 0
            };
        }

        public int CountKhoaHocTuNhien(List<Model> data)
        {
            return data.Count(d => d.vat_li.HasValue || d.hoa_hoc.HasValue || d.toan.HasValue);
        }

        public int CountKhoaHocXaHoi(List<Model> data)
        {
            return data.Count(d => d.lich_su.HasValue || d.dia_li.HasValue || d.gdcd.HasValue);
        }

        private void btnTop5Tren_Click(object sender, RoutedEventArgs e)
        {
            var top5Toan = GetTop5ToanTren(diemThiList_data, true); // Lấy top 5 cao nhất
            txtResult.Text = string.Join("\n", top5Toan.Select(d => $"{d.sbd}: {d.toan}"));
        }

        private void btnTop5Duoi_Click(object sender, RoutedEventArgs e)
        {
            var top5Toan = GetTop5ToanDuoi(diemThiList_data, true); // Lấy top 5 thấp nhất
            txtResult.Text = string.Join("\n", top5Toan.Select(d => $"{d.sbd}: {d.toan}"));
        }

        private void btnThukhoaA_Click(object sender, RoutedEventArgs e)
        {
            var thukhoa = GetTopKhoaA(diemThiList_data);
            txtResult.Text = $"{thukhoa.sbd}: {thukhoa.toan + thukhoa.vat_li + thukhoa.hoa_hoc}";
        }

        private void btnCount_mon_Click(object sender, RoutedEventArgs e)
        {
            var countToan = CountMon(diemThiList_data, "toan");
            var countNguVan = CountMon(diemThiList_data, "ngu_van");
            var countNgoaiNgu = CountMon(diemThiList_data, "ngoai_ngu");
            var countVatLi = CountMon(diemThiList_data, "vat_li");
            var countHoaHoc = CountMon(diemThiList_data, "hoa_hoc");
            var countSinhHoc = CountMon(diemThiList_data, "sinh_hoc");
            var countLichSu = CountMon(diemThiList_data, "lich_su");
            var countDiaLi = CountMon(diemThiList_data, "dia_li");
            var countGDCD = CountMon(diemThiList_data, "gdcd");

            txtResult.Text = $"Số thí sinh thi Toán: {countToan}\n" +
                             $"Số thí sinh thi Ngữ Văn: {countNguVan}\n" +
                             $"Số thí sinh thi Ngoại Ngữ: {countNgoaiNgu}\n" +
                             $"Số thí sinh thi Vật Lí: {countVatLi}\n" +
                             $"Số thí sinh thi Hóa Học: {countHoaHoc}\n" +
                             $"Số thí sinh thi Sinh Học: {countSinhHoc}\n" +
                             $"Số thí sinh thi Lịch Sử: {countLichSu}\n" +
                             $"Số thí sinh thi Địa Lí: {countDiaLi}\n" +
                             $"Số thí sinh thi Giáo Dục Công Dân: {countGDCD}\n";
        }

        private void btnCount_khtn_khxh_Click(object sender, RoutedEventArgs e)
        {
            var countKHTN = CountKhoaHocTuNhien(diemThiList_data);
            var countKHXH = CountKhoaHocXaHoi(diemThiList_data);


            txtResult.Text = $"Số thí sinh thi Khoa học tự nhiên: {countKHTN}\n" +
                             $"Số thí sinh thi Khoa học xã hội: {countKHXH}";
        }
    }
}