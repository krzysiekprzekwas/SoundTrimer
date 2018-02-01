using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
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
        private WaveOutEvent waveOut = new WaveOutEvent();

        private Mp3FileReader mp3Reader;

        private DispatcherTimer timer = new DispatcherTimer();

        private bool _isDrag = false;

        public MainWindow()
        {
            InitializeComponent();
            
        }

        void timer_Tick(object sender, EventArgs e)
        {
            lblStatus.Content = String.Format("{0} / {1}", mp3Reader.CurrentTime.ToString(@"mm\:ss"), mp3Reader.TotalTime.ToString(@"mm\:ss"));

            if (!_isDrag)
                sliProgress.Value = mp3Reader.CurrentTime.TotalSeconds;
            
        }
        
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                loadFile(openFileDialog.FileName);
            }
        }

        private void loadFile(string filePath)
        {
            mp3Reader = new Mp3FileReader(filePath);
            waveOut.Init(mp3Reader);
            lblTitle.Content = System.IO.Path.GetFileName(filePath);
            sliProgress.Minimum = 0;
            sliProgress.Maximum = mp3Reader.TotalTime.TotalSeconds;
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += timer_Tick;

            btnPause.IsEnabled = true;
            btnPlay.IsEnabled = true;
            btnStop.IsEnabled = true;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            waveOut.Play();
            timer.Start();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            waveOut.Pause();
            timer.Stop();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            waveOut.Stop();
            mp3Reader.Seek(0, SeekOrigin.Begin);
        }

        private void sliProgress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

            var sl = sender as Slider;

            var val = sl.Value;


            mp3Reader.CurrentTime = TimeSpan.FromSeconds((int)sliProgress.Value);

            _isDrag = false;
        }

        private void sliProgress_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _isDrag = true;
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                
                loadFile(files[0]);
            }
        }
    }
}
