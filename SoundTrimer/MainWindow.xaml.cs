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
    public partial class MainWindow : Window
    {
        public MusicPlayer Player { get; set; }

        private bool _isDrag;

        private DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            DisableControls();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            lblStatus.Content = String.Format("{0} / {1}", Player.CurrentTime.ToString(@"mm\:ss"), Player.TotalTime.ToString(@"mm\:ss"));

            if (!_isDrag)
                sliProgress.Value = Player.CurrentTime.TotalSeconds;
            
        }
        
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                if (System.IO.Path.GetExtension(openFileDialog.FileName).ToLower() == "mp3")
                    loadFile(openFileDialog.FileName);
                else
                    MessageBox.Show("Invalid audio file!", "Error");
            }
        }

        private async void loadFile(string filePath)
        {
            Player = new MusicPlayer(filePath);

            lblTitle.Content = Player.SongTitle;

            sliProgress.Minimum = 0;
            sliProgress.Maximum = Player.TotalTime.TotalSeconds;

            rangeSlider.Minimum = 0;
            rangeSlider.UpperSlider.Value = Player.TotalTime.TotalSeconds;
            rangeSlider.Maximum = Player.TotalTime.TotalSeconds;

            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += timer_Tick;

            EnableControls();

            var processedBitmap = await Task.Run(() => {
                var bmp = RenderWave(filePath);
                return bmp;
            });
            
            selectionRectangle.Visibility = Visibility.Visible;
            rangeSlider.Visibility = Visibility.Visible;
            aimg.Source = processedBitmap;
        }

        public void EnableControls()
        {
            btnPause.IsEnabled = true;
            btnPlay.IsEnabled = true;
            btnStop.IsEnabled = true;
            btnTrim.IsEnabled = true;
            sliProgress.IsEnabled = true;
            rangeSlider.IsEnabled = true;
        }

        public void DisableControls()
        {
            btnPause.IsEnabled = false;
            btnPlay.IsEnabled = false;
            btnStop.IsEnabled = false;
            btnTrim.IsEnabled = false;
            sliProgress.IsEnabled = false;
            rangeSlider.IsEnabled = false;
        }

        private BitmapImage RenderWave(string filePath)
        {
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
            settings.BottomHeight = 0;
            settings.Width = 500;
            settings.DecibelScale = false;

            var img = rnd.Render(filePath, new RmsPeakProvider(200), settings);

            var bmp = (Bitmap)img;

            return ToBitmapImage(bmp);
        }

        public BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png); // Was .Bmp, but this did not show a transparent background.

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            Player.Play();
            timer.Start();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            Player.Pause();
            timer.Stop();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            Player.Stop();
        }

        private void sliProgress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            var sl = sender as Slider;

            var val = sl.Value;


            Player.CurrentTime = TimeSpan.FromSeconds((int)sliProgress.Value);

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

        private void rangeSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            var sl = sender as RangeSlider;

            var seconds = sl.LowerSlider.Value * sl.Maximum / 10;

            Player.CurrentTime = TimeSpan.FromSeconds((int)seconds);
            sliProgress.Value = seconds;

            AdjustRangeRectangle();
        }

        private void AdjustRangeRectangle()
        {
            var startPercent = rangeSlider.LowerSlider.Value / 10;
            var endPercent = rangeSlider.UpperSlider.Value / 10;

            selectionRectangle.Width = 450 * (endPercent - startPercent);
            selectionRectangle.Margin = new Thickness(450 * startPercent, 0, 450 * (1-endPercent), 0);
        }

        private void rangeSlider_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            AdjustRangeRectangle();
        }

        private void btnTrim_Click(object sender, RoutedEventArgs e)
        {
            var currTime = Player.CurrentTime;

            Player.Stop();

            SaveFileDialog dlg = new SaveFileDialog
            {
                FileName = "trimed_" + Player.SongTitle, // Default file name
                DefaultExt = ".mp3", // Default file extension
                Filter = "Text documents (.mp3)|*.mp3" // Filter files by extension
            };

            // Show save file dialog box
            var result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;

                var start = rangeSlider.LowerSlider.Value * rangeSlider.Maximum / 10;

                var s = TimeSpan.FromSeconds((int)start);

                var end = rangeSlider.UpperSlider.Value * rangeSlider.Maximum / 10;

                var en = TimeSpan.FromSeconds((int)end);

                Player.TrimMp3(filename, s, en);
            }

            Player.CurrentTime = currTime;

            Player.Play();
        }
    }
}
