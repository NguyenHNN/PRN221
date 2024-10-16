using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DAY2_Webcam
{
    public partial class MainWindow : Window
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;

        public MainWindow()
        {
            InitializeComponent();

            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
            {
                MessageBox.Show("No video devices found.");
                return;
            }

            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            using (Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone())
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Bmp);
                    ms.Seek(0, SeekOrigin.Begin);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = ms;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    Dispatcher.Invoke(() =>
                    {
                        picturebox.Source = bitmapImage;  // Ensure 'picturebox' matches XAML
                    });
                }
            }
        }

        private void Button_StartClick(object sender, RoutedEventArgs e)
        {
            videoSource.Start();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
        }

        private void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            if (picturebox.Source != null)
            {
                BitmapSource bitmapSource = (BitmapSource)picturebox.Source;
                WriteableBitmap writeableBitmap = new WriteableBitmap(bitmapSource);

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string ipAddress = GetLocalIPAddress();
                string dimensions = $"{bitmapSource.PixelWidth}x{bitmapSource.PixelHeight}";
                BitmapSource annotatedBitmap = AddTextToBitmap(writeableBitmap, $"{timestamp} | IP: {ipAddress} | {dimensions}");

                string timestampForFilename = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"capturedImage_{timestampForFilename}.jpg";
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), fileName);

                SaveBitmapToFile(annotatedBitmap, path);

                MessageBox.Show("Image saved to " + path);
            }
            else
            {
                MessageBox.Show("No image to save. Please capture an image first.");
            }
        }

        private BitmapSource AddTextToBitmap(BitmapSource bitmap, string text)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                context.DrawImage(bitmap, new Rect(0, 0, width, height));

                FormattedText formattedText = new FormattedText(
                    text,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    12,
                    System.Windows.Media.Brushes.White);

                context.DrawRectangle(System.Windows.Media.Brushes.Black, null, new Rect(width - formattedText.Width - 10, height - formattedText.Height - 10, formattedText.Width + 5, formattedText.Height + 5));
                context.DrawText(formattedText, new System.Windows.Point(width - formattedText.Width - 5, height - formattedText.Height - 5));
            }

            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(visual);

            return rtb;
        }

        private void SaveBitmapToFile(BitmapSource bitmap, string path)
        {
            BitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

        private string GetLocalIPAddress()
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "Local IP Not Found!";
        }
    }
}
