using Microsoft.Win32;
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
    public class MusicPlayer
    {
        public string SongTitle { get; set; }

        public TimeSpan CurrentTime {
            get { return mp3Reader.CurrentTime; }
            set {
                mp3Reader.CurrentTime = value;
            }
        }

        public TimeSpan TotalTime
        {
            get { return mp3Reader.TotalTime; }
        }

        private WaveOutEvent waveOut;

        private Mp3FileReader mp3Reader;

        public MusicPlayer(string filePath)
        {
            waveOut = new WaveOutEvent();
            mp3Reader = new Mp3FileReader(filePath);
            waveOut.Init(mp3Reader);


            SongTitle = System.IO.Path.GetFileName(filePath);
        }

        public void Play()
        {
            waveOut.Play();
        }

        public void Pause()
        {
            waveOut.Pause();
        }

        public void Stop()
        {
            waveOut.Stop();
            mp3Reader.CurrentTime = TimeSpan.FromSeconds(0);
        }

        public void TrimMp3(string outputPath, TimeSpan? begin, TimeSpan? end)
        {            
            using (var writer = File.Create(outputPath))
            {
                Mp3Frame frame;
                while ((frame = mp3Reader.ReadNextFrame()) != null)
                    if (mp3Reader.CurrentTime >= begin || !begin.HasValue)
                    {
                        if (mp3Reader.CurrentTime <= end || !end.HasValue)
                            writer.Write(frame.RawData, 0, frame.RawData.Length);
                        else break;
                    }
            }

            mp3Reader.CurrentTime = TimeSpan.FromSeconds(0);
        }
    }
}
