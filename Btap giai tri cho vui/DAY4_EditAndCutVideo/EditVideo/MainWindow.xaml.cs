using Microsoft.Win32;
using System.Diagnostics;
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
namespace EditVideo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "MP4 files (*.mp4)|*.mp4"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = openFileDialog.FileName;
                GetVideoDuration(openFileDialog.FileName);
                //PlayVideo(openFileDialog.FileName, VideoPlayer);
            }
        }

        private void Browse_Click1(object sender, RoutedEventArgs e) => BrowseForSegment(VideoPlayer1, "segment1");
        private void Browse_Click2(object sender, RoutedEventArgs e) => BrowseForSegment(VideoPlayer2, "segment2");
        private void Browse_Click3(object sender, RoutedEventArgs e) => BrowseForSegment(VideoPlayer3, "segment3");

        private void BrowseForSegment(MediaElement player, string segmentName)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "MP4 files (*.mp4)|*.mp4"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string segmentPath = System.IO.Path.Combine(@"D:\PRN221\DAY4_EditAndCutVideo\CutVideo", $"{segmentName}.mp4");
                File.Copy(openFileDialog.FileName, segmentPath, true); // Copy file to the designated path
                //PlayVideo(segmentPath, player);
            }
        }

        private void PlayVideo(string filePath, MediaElement player)
        {
            player.Source = new Uri(filePath);
            player.LoadedBehavior = MediaState.Manual;
            player.UnloadedBehavior = MediaState.Stop;
            player.Play();
        }

        private void GetVideoDuration(string filePath)
        {
            var ffmpegPath = @"D:\PRN221\DAY4_EditAndCutVideo\ffmpeg.exe";
            var processInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-i \"{filePath}\"",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = processInfo };
            process.Start();
            string output = process.StandardError.ReadToEnd();
            process.WaitForExit();

            var duration = ParseDurationFromFFmpegOutput(output);
            DurationTextBox.Text = duration;
        }

        private string ParseDurationFromFFmpegOutput(string output)
        {
            var durationLine = "Duration:";
            var index = output.IndexOf(durationLine);
            if (index > -1)
            {
                var start = index + durationLine.Length;
                var end = output.IndexOf(",", start);
                return output.Substring(start, end - start).Trim();
            }
            return "N/A";
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            if (IsValidCutInput())
            {
                var inputFilePath = FilePathTextBox.Text;
                var outputFolder = @"D:\PRN221\DAY4_EditAndCutVideo\CutVideo";
                Directory.CreateDirectory(outputFolder);

                CutSegment(inputFilePath, outputFolder, "output_segment1.mp4", StartTime1TextBox.Text, EndTime1TextBox.Text);
                CutSegment(inputFilePath, outputFolder, "output_segment2.mp4", StartTime2TextBox.Text, EndTime2TextBox.Text);
                CutSegment(inputFilePath, outputFolder, "output_segment3.mp4", StartTime3TextBox.Text, EndTime3TextBox.Text);

                MessageBox.Show("Video has been cut into 3 segments and saved in the CutVideo folder.");
                PlaySegmentVideos(outputFolder);
            }
            else
            {
                MessageBox.Show("Please enter valid start and end times for all segments.");
            }
        }

        private bool IsValidCutInput()
        {
            return !string.IsNullOrEmpty(FilePathTextBox.Text) &&
                   !string.IsNullOrEmpty(StartTime1TextBox.Text) && !string.IsNullOrEmpty(EndTime1TextBox.Text) &&
                   !string.IsNullOrEmpty(StartTime2TextBox.Text) && !string.IsNullOrEmpty(EndTime2TextBox.Text) &&
                   !string.IsNullOrEmpty(StartTime3TextBox.Text) && !string.IsNullOrEmpty(EndTime3TextBox.Text);
        }

        private void CutSegment(string inputFilePath, string outputFolder, string outputFileName, string startTime, string endTime)
        {
            var ffmpegPath = @"D:\PRN221\DAY4_EditAndCutVideo\ffmpeg.exe";
            var outputFilePath = System.IO.Path.Combine(outputFolder, outputFileName);
            var processInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-i \"{inputFilePath}\" -ss {startTime} -to {endTime} -c copy \"{outputFilePath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processInfo })
            {
                process.Start();
                process.WaitForExit();
            }
        }

        private void PlaySegmentVideos(string outputFolder)
        {
            Segment1Player.Source = new Uri(System.IO.Path.Combine(outputFolder, "output_segment1.mp4"));
            //Segment1Player.Play();
            Segment2Player.Source = new Uri(System.IO.Path.Combine(outputFolder, "output_segment2.mp4"));
            //Segment2Player.Play();
            Segment3Player.Source = new Uri(System.IO.Path.Combine(outputFolder, "output_segment3.mp4"));
            //Segment3Player.Play();
        }

        private void Merge_Click(object sender, RoutedEventArgs e)
        {
            var thread = new Thread(MergeVideos);
            thread.Start();
        }

        private void MergeVideos()
        {
            var outputFolder = @"D:\PRN221\DAY4_EditAndCutVideo\CutVideo";
            var segmentPaths = new string[]
            {
                System.IO.Path.Combine(outputFolder, "output_segment1.mp4"),

                System.IO.Path.Combine(outputFolder, "output_segment2.mp4"),
                //Path.Combine(outputFolder, "segment2.mp4"),
                System.IO.Path.Combine(outputFolder, "output_segment3.mp4"),
                System.IO.Path.Combine(outputFolder, "segment1.mp4"),
                //Path.Combine(outputFolder, "segment3.mp4")
            };

            foreach (var segmentPath in segmentPaths)
            {
                if (!File.Exists(segmentPath))
                {
                    Dispatcher.Invoke(() => MessageBox.Show("Please cut all video segments before merging."));
                    return;
                }
            }

            var fileListPath = System.IO.Path.Combine(outputFolder, "filelist.txt");
            using (StreamWriter writer = new StreamWriter(fileListPath))
            {
                foreach (var segmentPath in segmentPaths)
                {
                    writer.WriteLine($"file '{segmentPath}'");
                }
            }

            var outputFilePath = System.IO.Path.Combine(outputFolder, "merged_video.mp4");

            var ffmpegPath = @"D:\PRN221\DAY4_EditAndCutVideo\ffmpeg.exe";
            var processInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-f concat -safe 0 -i \"{fileListPath}\" -c:v libx264 -crf 23 -pix_fmt yuv420p \"{outputFilePath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processInfo })
            {
                process.Start();
                process.WaitForExit();
            }

            if (File.Exists(outputFilePath))
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Videos have been merged successfully!");
                    Segment4Player.Source = new Uri(outputFilePath);
                    //Segment4Player.Play();
                });
            }
            else
            {
                Dispatcher.Invoke(() => MessageBox.Show("Failed to merge videos."));
            }
        }
    }
}