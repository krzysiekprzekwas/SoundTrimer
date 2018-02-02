﻿using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WaveFormRendererLib;
using System.Drawing.Imaging;

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

            var rnd = new WaveFormRenderer();


            var topSpacerColor = System.Drawing.Color.FromArgb(64, 83, 22, 3);
            var soundCloudOrangeTransparentBlocks = new SoundCloudBlockWaveFormSettings(System.Drawing.Color.FromArgb(196, 197, 53, 0), topSpacerColor, System.Drawing.Color.FromArgb(196, 79, 26, 0),
                System.Drawing.Color.FromArgb(64, 79, 79, 79))
            {
                Name = "SoundCloud Orange Transparent Blocks",
                PixelsPerPeak = 2,
                SpacerPixels = 1,
                TopSpacerGradientStartColor = topSpacerColor,
                BackgroundColor = System.Drawing.Color.Transparent
            };

            WaveFormRendererSettings settings = soundCloudOrangeTransparentBlocks;
            settings.TopHeight = 50;
            settings.BottomHeight = 20;
            settings.Width = 200;
            settings.DecibelScale = false;

            var img = rnd.Render(filePath, settings);

            var bitmapImage = new BitmapImage();

            using (var ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();

                
            }

            aimg.Source = bitmapImage;
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
