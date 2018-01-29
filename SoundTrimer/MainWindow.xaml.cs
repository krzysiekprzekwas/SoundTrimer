using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

namespace SoundTrimer
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();

        private DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
        }

        private void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            lblStatus.Content = String.Format("{0} / {1}", mediaPlayer.Position.ToString(@"mm\:ss"), mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));

            sliProgress.Minimum = 0;
            sliProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;

        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.Source != null)
            {
                lblStatus.Content = String.Format("{0} / {1}", mediaPlayer.Position.ToString(@"mm\:ss"), mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                sliProgress.Value = mediaPlayer.Position.TotalSeconds;
            }
            else
                lblStatus.Content = "00:00/00:00";
        }
        
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                mediaPlayer.Open(new Uri(openFileDialog.FileName));
                lblTitle.Content = System.IO.Path.GetFileName(openFileDialog.FileName);
            }
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += timer_Tick;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
            timer.Start();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            timer.Stop();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
        }
    }
}
