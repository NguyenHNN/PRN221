using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EditVideo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string videoFilePath;
        private TimeSpan videoDuration;
        private string additionalVideoFilePath = @"D:\PRN221\DAY4_EditVideo\CutVideo"; // Update to the path where additional videos are stored

        public MainWindow()
        {
            InitializeComponent();
            LoadAdditionalVideos();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "MP4 files (*.mp4)|*.mp4"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                videoFilePath = openFileDialog.FileName;
                VideoPathTextBox.Text = videoFilePath;
                GetVideoDuration(videoFilePath);
            }
        }

        private void GetVideoDuration(string filePath)
        {
            var ffmpegPath = @"D:\PRN221\DAY4_EditVideo\ffmpeg.exe"; // Update to the path of ffmpeg.exe
            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-i \"{filePath}\"",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                using (var reader = process.StandardError)
                {
                    string result = reader.ReadToEnd();
                    string duration = ParseDuration(result);
                    VideoDurationLabel.Content = duration;
                    videoDuration = TimeSpan.Parse(duration);
                    StartTimeSlider.Maximum = videoDuration.TotalSeconds;
                    EndTimeSlider.Maximum = videoDuration.TotalSeconds;
                }
            }
        }

        private string ParseDuration(string ffmpegOutput)
        {
            var durationLine = ffmpegOutput.Split('\n').FirstOrDefault(l => l.Contains("Duration"));
            if (durationLine != null)
            {
                var timePart = durationLine.Split(',')[0].Replace("Duration: ", "").Trim();
                return timePart;
            }
            return "00:00:00"; // Default if not found
        }

        private void StartTimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            StartTimeTextBox.Text = TimeSpan.FromSeconds(StartTimeSlider.Value).ToString(@"hh\:mm\:ss");
        }

        private void EndTimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            EndTimeTextBox.Text = TimeSpan.FromSeconds(EndTimeSlider.Value).ToString(@"hh\:mm\:ss");
        }

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(videoFilePath))
            {
                MessageBox.Show("Please select a video first!");
                return;
            }

            if (EndTimeSlider.Value <= StartTimeSlider.Value)
            {
                MessageBox.Show("End time must be greater than start time!", "Invalid Time Range", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string startTime = TimeSpan.FromSeconds(StartTimeSlider.Value).ToString(@"hh\:mm\:ss");
            string endTime = TimeSpan.FromSeconds(EndTimeSlider.Value).ToString(@"hh\:mm\:ss");

            string outputFilePath = Path.Combine(Path.GetDirectoryName(videoFilePath), $"cut_{DateTime.Now:yyyyMMddHHmmssfff}_{Path.GetFileName(videoFilePath)}");

            var ffmpegPath = @"D:\PRN221\DAY4_EditVideo\ffmpeg.exe"; // Update to the path of ffmpeg.exe
            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $" -i \"{videoFilePath}\" -ss {startTime} -to {endTime} -c copy \"{outputFilePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit();
            }

            // Display cut video paths
            DisplayCutVideoPaths(outputFilePath);
        }

        private void DisplayCutVideoPaths(string cutFilePath)
        {
            if (string.IsNullOrEmpty(CutVideo1TextBox.Text))
            {
                CutVideo1TextBox.Text = cutFilePath;
            }
            else if (string.IsNullOrEmpty(CutVideo2TextBox.Text))
            {
                CutVideo2TextBox.Text = cutFilePath;
            }
            else if (string.IsNullOrEmpty(CutVideo3TextBox.Text))
            {
                CutVideo3TextBox.Text = cutFilePath;
            }
            else
            {
                MessageBox.Show("Maximum of 3 video parts cut.");
            }
        }

        private void LoadAdditionalVideos()
        {
            if (Directory.Exists(additionalVideoFilePath))
            {
                var videoFiles = Directory.GetFiles(additionalVideoFilePath, "*.mp4", SearchOption.TopDirectoryOnly).ToList();
                var videoFileNames = videoFiles.Select(Path.GetFileName).ToArray();

                AdditionalVideoComboBox1.ItemsSource = videoFileNames;
                AdditionalVideoComboBox2.ItemsSource = videoFileNames;
                AdditionalVideoComboBox3.ItemsSource = videoFileNames;
            }
        }

        private string AppendVideo(string videoPath, string additionalVideoPath)
        {
            if (string.IsNullOrEmpty(videoPath)) return null;

            string tempFilePath = Path.Combine(Path.GetDirectoryName(videoPath), $"temp_{Path.GetFileName(videoPath)}");
            var ffmpegPath = @"D:\PRN221\DAY4_EditVideo\ffmpeg.exe"; // Update to the path of ffmpeg.exe

            string arguments;

            if (string.IsNullOrEmpty(additionalVideoPath))
            {
                arguments = $"-i \"{videoPath}\" -c copy \"{tempFilePath}\"";
            }
            else
            {
                string concatFilePath = Path.GetTempFileName();

                File.WriteAllLines(concatFilePath, new[]
                {
                    $"file '{videoPath}'",
                    $"file '{additionalVideoPath}'"
                });

                arguments = $"-f concat -safe 0 -i \"{concatFilePath}\" -c copy \"{tempFilePath}\"";
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(startInfo))
            {
                string errorOutput = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg error: {errorOutput}");
                }
            }

            return tempFilePath;
        }

        private async void MergeButton_Click(object sender, RoutedEventArgs e)
        {
            var videoParts = new[]
            {
                new { Path = CutVideo1TextBox.Text, AdditionalVideo = AdditionalVideoComboBox1 },
                new { Path = CutVideo2TextBox.Text, AdditionalVideo = AdditionalVideoComboBox2 },
                new { Path = CutVideo3TextBox.Text, AdditionalVideo = AdditionalVideoComboBox3 }
            };

            var tempOutputs = new ConcurrentBag<string>();
            var tasks = new List<Task>();

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            foreach (var video in videoParts)
            {
                if (!string.IsNullOrEmpty(video.Path))
                {
                    string additionalVideoFile = GetAdditionalVideoPath(video.AdditionalVideo);
                    tasks.Add(Task.Run(() =>
                    {
                        string tempOutput = AppendVideo(video.Path, additionalVideoFile);
                        tempOutputs.Add(tempOutput);
                    }));
                }
            }
            await Task.WhenAll(tasks);
            stopwatch.Stop();
            MessageBox.Show($"Time taken to append videos: {stopwatch.ElapsedMilliseconds} ms");

            if (tempOutputs.Any())
            {
                stopwatch.Restart();
                string finalOutputPath = Path.Combine(Path.GetDirectoryName(videoFilePath), $"final_output_{DateTime.Now:yyyyMMddHHmmss}.mp4");
                ConcatenateVideos(tempOutputs.ToArray(), finalOutputPath);
                stopwatch.Stop();
                MessageBox.Show($"Time taken to concatenate videos: {stopwatch.ElapsedMilliseconds} ms");
                MessageBox.Show("Merging complete. Final video saved to " + finalOutputPath);
            }
            else
            {
                MessageBox.Show("No videos selected for merging.");
            }

            stopwatch.Restart();
            CleanUpFiles(tempOutputs.ToArray());
            stopwatch.Stop();
            MessageBox.Show($"Time taken to clean up files: {stopwatch.ElapsedMilliseconds} ms");
        }

        private string GetAdditionalVideoPath(ComboBox comboBox)
        {
            string selectedVideoFileName = comboBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedVideoFileName))
            {
                return Path.Combine(additionalVideoFilePath, selectedVideoFileName);
            }
            return null;
        }

        private void ConcatenateVideos(string[] videoFiles, string outputFilePath)
        {
            string concatFilePath = Path.GetTempFileName();

            File.WriteAllLines(concatFilePath, videoFiles.Select(v => $"file '{v}'"));

            var ffmpegPath = @"D:\PRN221\DAY4_EditVideo\ffmpeg.exe"; // Update to the path of ffmpeg.exe
            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-f concat -safe 0 -i \"{concatFilePath}\" -c copy \"{outputFilePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(startInfo))
            {
                string errorOutput = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg error: {errorOutput}");
                }
            }
        }

        private void CleanUpFiles(string[] filePaths)
        {
            foreach (var filePath in filePaths)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error cleaning up file {filePath}: {ex.Message}");
                }
            }
        }
    }
}
