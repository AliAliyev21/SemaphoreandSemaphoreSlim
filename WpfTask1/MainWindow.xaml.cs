using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace WpfTask1
{
    //https://www.youtube.com/watch?v=LNYm40RmRzs
    //https://www.youtube.com/watch?v=5jyEhiKmbDo
    //https://www.youtube.com/watch?v=hhTZuWhYSkQ
    //https://www.youtube.com/watch?v=GGXE44ucqCE
    
    public partial class MainWindow : Window
    {
        private ProgressBar[] progressBars;
        private SemaphoreSlim semaphore = new SemaphoreSlim(3);

        public MainWindow()
        {
            InitializeComponent();
            InitializeProgressBars();
        }

        private void InitializeProgressBars()
        {
            progressBars = new ProgressBar[] { progressBar1, progressBar2, progressBar3 };
        }

        private async void downloadBtn_Click(object sender, RoutedEventArgs e)
        {
            string videoLink = linkTxt.Text;
            var youtube = new YoutubeClient();
            var manifest = await youtube.Videos.Streams.GetManifestAsync(videoLink);

            var streamInfo = manifest.GetMuxedStreams().GetWithHighestVideoQuality();

            ProgressBar availableProgressBar = FindAvailableProgressBar();
            if (availableProgressBar == null)
            {
                MessageBox.Show("There are no available progress bars to use");
                return;
            }

            await semaphore.WaitAsync();
            await Task.Run(async () =>
            {
                var progressIndicator = new Progress<double>(progress => UpdateProgressBar(availableProgressBar, progress));
                try
                {
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string filePath = Path.Combine(desktopPath, $"video{Array.IndexOf(progressBars, availableProgressBar) + 1}.mp4");
                    await youtube.Videos.Streams.DownloadAsync(streamInfo, filePath, progressIndicator);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "File Access Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            MessageBox.Show("Video indirme tamamlandı.");
        }

        private ProgressBar FindAvailableProgressBar()
        {
            foreach (var progressBar in progressBars)
            {
                if (progressBar.Value == 0)
                {
                    return progressBar;
                }
            }
            return null;
        }

        private void UpdateProgressBar(ProgressBar progressBar, double progress)
        {
            Dispatcher.Invoke(() =>
            {
                progressBar.Value = progress * 100;
            });
        }
    }
}
