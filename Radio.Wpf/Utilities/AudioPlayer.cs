using NAudio.Wave;
using WPFSoundVisualizationLib;
using System;
using System.ComponentModel;
using Sample_NAudio;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows;

namespace Radio.Wpf.Utilities
{
    public class AudioPlayer : INotifyPropertyChanged, ISpectrumPlayer, IDisposable
    {
        private static AudioPlayer instance;
        private readonly DispatcherTimer positionTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
        private readonly int fftDataSize = (int)FFTDataSize.FFT2048;

        private bool disposed;
        private bool canPlay;
        private bool canPause;
        private bool canStop;
        private bool isPlaying;
        private bool inChannelTimerUpdate;
        private double channelLength;
        private double channelPosition;
        private bool inChannelSet;

        private WaveOutEvent waveOutDevice;
        private WaveStream activeStream;
        private WaveChannel32 inputStream;
        private SampleAggregator sampleAggregator;

        private TagLib.File fileTag;

        public static string Path;
        //public static List<string> PlaylistFiles;

        #region Instance
        public static AudioPlayer Instance
        {
            get
            {
                return instance ?? (instance = new AudioPlayer());
            }
        }

        private AudioPlayer()
        {
            positionTimer.Interval = TimeSpan.FromMilliseconds(50);
            positionTimer.Tick += PositionTimer_Tick;
        }

        private void PositionTimer_Tick(object sender, EventArgs e)
        {
            inChannelTimerUpdate = true;
            ChannelPosition = ((double)ActiveStream.Position / (double)ActiveStream.Length) * ActiveStream.TotalTime.TotalSeconds;
            inChannelTimerUpdate = false;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    StopAndCloseStream();
                }

                disposed = true;
            }
        }
        #endregion

        #region ISpectrumPlayer
        public bool GetFFTData(float[] fftDataBuffer)
        {
            sampleAggregator.GetFFTResults(fftDataBuffer);
            return isPlaying;
        }

        public int GetFFTFrequencyIndex(int frequency)
        {
            double maxFrequency;
            if (ActiveStream != null)
                maxFrequency = ActiveStream.WaveFormat.SampleRate / 2.0d;
            else
                maxFrequency = 22050; // Assume a default 44.1 kHz sample rate.
            return (int)((frequency / maxFrequency) * (fftDataSize / 2));
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public TagLib.File FileTag
        {
            get { return fileTag; }
            set
            {
                TagLib.File oldValue = fileTag;
                fileTag = value;
                if (oldValue != fileTag)
                    NotifyPropertyChanged("FileTag");
            }
        }

        public WaveStream ActiveStream
        {
            get { return activeStream; }
            protected set
            {
                WaveStream oldValue = activeStream;
                activeStream = value;
                if (oldValue != activeStream)
                    NotifyPropertyChanged("ActiveStream");
            }
        }

        public double ChannelLength
        {
            get { return channelLength; }
            protected set
            {
                double oldValue = channelLength;
                channelLength = value;
                if (oldValue != channelLength)
                    NotifyPropertyChanged("ChannelLength");
            }
        }

        public double ChannelPosition
        {
            get { return channelPosition; }
            set
            {
                if (!inChannelSet)
                {
                    inChannelSet = true; // Avoid recursion
                    double oldValue = channelPosition;
                    double position = Math.Max(0, Math.Min(value, ChannelLength));
                    if (!inChannelTimerUpdate && ActiveStream != null)
                        ActiveStream.Position = (long)((position / ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);
                    channelPosition = position;
                    if (oldValue != ActiveStream.Position)
                        NotifyPropertyChanged("ChannelPosition");
                    inChannelSet = false;
                }
            }
        }

        private void StopAndCloseStream()
        {
            waveOutDevice?.Stop();
            if (activeStream != null)
            {
                ActiveStream.Close();
                ActiveStream = null;

                inputStream.Close();
                inputStream = null;
            }
            if (waveOutDevice != null)
            {
                waveOutDevice.Dispose();
                waveOutDevice = null;
            }
        }

        public void Stop()
        {
            waveOutDevice?.Stop();
            IsPlaying = false;
            CanStop = false;
            CanPlay = false;
            CanPause = false;

            App.File = "";
        }

        public void Pause()
        {
            if (IsPlaying && CanPause)
            {
                waveOutDevice.Pause();
                IsPlaying = false;
                CanPlay = true;
                CanPause = false;
            }
        }

        public void Play()
        {
            if (CanPlay)
            {
                waveOutDevice.Play();
                IsPlaying = true;
                CanPause = true;
                CanPlay = false;
                CanStop = true;
            }
        }

        public void OpenFile(string path)
        {
            StopAndCloseStream();

            if (ActiveStream != null)
            {
                ChannelPosition = 0;
            }

            try
            {
                waveOutDevice = new WaveOutEvent()
                {
                    DesiredLatency = 100
                };
                ActiveStream = new MediaFoundationReader(path);
                inputStream = new WaveChannel32(ActiveStream);
                sampleAggregator = new SampleAggregator(fftDataSize);
                inputStream.Sample += InputStream_Sample;
                waveOutDevice.Init(inputStream);

                ChannelLength = 0;
                ChannelLength = inputStream.TotalTime.TotalSeconds;
                if (System.IO.File.Exists(path)) FileTag = TagLib.File.Create(path);
                else FileTag = null;

                CanPlay = true;
            }
            catch
            {
                ActiveStream = null;
                CanPlay = false;
            }
        }

        public bool CanPlay
        {
            get { return canPlay; }
            protected set
            {
                bool oldValue = canPlay;
                canPlay = value;
                if (oldValue != canPlay)
                    NotifyPropertyChanged("CanPlay");
            }
        }

        public bool CanPause
        {
            get { return canPause; }
            protected set
            {
                bool oldValue = canPause;
                canPause = value;
                if (oldValue != canPause)
                    NotifyPropertyChanged("CanPause");
            }
        }

        public bool CanStop
        {
            get { return canStop; }
            protected set
            {
                bool oldValue = canStop;
                canStop = value;
                if (oldValue != canStop)
                    NotifyPropertyChanged("CanStop");
            }
        }

        public bool IsPlaying
        {
            get { return isPlaying; }
            protected set
            {
                bool oldValue = isPlaying;
                isPlaying = value;
                if (oldValue != isPlaying)
                    NotifyPropertyChanged("IsPlaying");
                positionTimer.IsEnabled = value;
            }
        }

        private void InputStream_Sample(object sender, SampleEventArgs e)
        {
            sampleAggregator.Add(e.Left, e.Right);
        }
    }
}
