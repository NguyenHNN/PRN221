using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CsvHelper;
using System.Globalization;
using System.IO;

namespace DAY3_ThongKeDiemCacMonHoc
{
    public partial class MainWindow : Window
    {
        private List<DiemThi> _data;
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
                _data = csv.GetRecords<DiemThi>().ToList();
            }
        }




        private void btnQ2_Click_1(object sender, RoutedEventArgs e)
        {
            var topStudentInA = _data.Where(d => d.toan.HasValue && d.vat_li.HasValue && d.hoa_hoc.HasValue)
                                     .Select(d => new
                                     {
                                         sbd = d.sbd,
                                         TotalScore = d.toan.Value + d.vat_li.Value + d.hoa_hoc.Value
                                     })
                                     .OrderByDescending(d => d.TotalScore)
                                     .FirstOrDefault();

            if (topStudentInA != null)
            {
                MessageBox.Show($"Thủ khoa khối A: {topStudentInA.sbd} với tổng điểm {topStudentInA.TotalScore:F2}");
            }
        }

        private void btnQ3_Click_1(object sender, RoutedEventArgs e)
        {
            var subjectCounts = new Dictionary<string, int>
            {
                { "Toán", _data.Count(d => d.toan.HasValue) },
                { "Ngữ văn", _data.Count(d => d.ngu_van.HasValue) },
                { "Ngoại ngữ", _data.Count(d => d.ngoai_ngu.HasValue) },
                { "Vật lý", _data.Count(d => d.vat_li.HasValue) },
                { "Hóa học", _data.Count(d => d.hoa_hoc.HasValue) },
                { "Sinh học", _data.Count(d => d.sinh_hoc.HasValue) },
                { "Lịch sử", _data.Count(d => d.lich_su.HasValue) },
                { "Địa lý", _data.Count(d => d.dia_li.HasValue) },
                { "GDCD", _data.Count(d => d.gdcd.HasValue) }
            };

            MessageBox.Show("Số lượng thí sinh tham gia thi từng môn:\n" +
                            string.Join("\n", subjectCounts.Select(x => $"{x.Key}: {x.Value}")));
        }

        private void btnQ4_Click_1(object sender, RoutedEventArgs e)
        {

            var khtnCount = _data.Count(d => d.vat_li.HasValue || d.hoa_hoc.HasValue || d.sinh_hoc.HasValue  );


            var khxhCount = _data.Count(d => d.lich_su.HasValue || d.dia_li.HasValue || d.gdcd.HasValue  );


            MessageBox.Show($"Số lượng thí sinh tham gia thi KHTN: {khtnCount}\nSố lượng thí sinh tham gia thi KHXH: {khxhCount}");
        }

        private void btnQ1_Click_1(object sender, RoutedEventArgs e)
        {


            // Lọc dữ liệu chỉ lấy những học sinh có điểm Toán
            var filteredData = _data.Where(d => d.toan.HasValue).ToList();

            // Tính điểm trung bình môn Toán của từng tỉnh
            var provinceScores = filteredData
                .GroupBy(d => d.sbd.Substring(0, 2))  // Lấy 2 ký tự đầu làm mã tỉnh
                .Select(g => new
                {
                    ProvinceCode = g.Key,
                    AverageScore = g.Average(x => x.toan.Value)
                })
                .OrderByDescending(x => x.AverageScore)
                .ToList();

            // Lấy top 5 tỉnh có điểm trung bình cao nhất và thấp nhất
            var top5HighestProvinces = provinceScores.Take(5).ToList();
            var top5LowestProvinces = provinceScores.OrderBy(x => x.AverageScore).Take(5).ToList();

            // Sắp xếp theo mã tỉnh từ 01 đến 63
            var sortedByProvinceCode = provinceScores
                .OrderBy(x => int.Parse(x.ProvinceCode))
                .ToList();

            // Hiển thị top 5 tỉnh có điểm trung bình cao nhất
            string highestProvincesMessage = "Top 5 tỉnh có điểm trung bình cao nhất:\n";
            foreach (var province in top5HighestProvinces)
            {
                highestProvincesMessage += $"Mã tỉnh: {province.ProvinceCode}, Điểm trung bình: {province.AverageScore:F2}\n";
            }

            // Hiển thị top 5 tỉnh có điểm trung bình thấp nhất
            string lowestProvincesMessage = "Top 5 tỉnh có điểm trung bình thấp nhất:\n";
            foreach (var province in top5LowestProvinces)
            {
                lowestProvincesMessage += $"Mã tỉnh: {province.ProvinceCode}, Điểm trung bình: {province.AverageScore:F2}\n";
            }

            // Hiển thị thông tin về các tỉnh theo mã tỉnh từ 01 đến 63
            string sortedProvincesMessage = "Danh sách tỉnh từ 01 đến 63:\n";
            foreach (var province in sortedByProvinceCode)
            {
                sortedProvincesMessage += $"Mã tỉnh: {province.ProvinceCode}, Điểm trung bình: {province.AverageScore:F2}\n ";
            }

            // Hiển thị thông tin
            MessageBox.Show(highestProvincesMessage + "\n" + lowestProvincesMessage + "\n" + sortedProvincesMessage);

        }
    }
}