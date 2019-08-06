using System;
using System.ComponentModel;
using System.Windows.Threading;
using NAudio.Wave;
using Sample_NAudio;
using TagLib;
using WPFSoundVisualizationLib;

namespace Radio.Wpf.Utilities
{
    public class AudioPlayer : ISpectrumPlayer, IDisposable
    {
        public enum FileResult
        {
            File = 1,
            Stream = 2
        }

        private static AudioPlayer instance;
        private readonly int fftDataSize = (int) FFTDataSize.FFT2048;
        private readonly DispatcherTimer positionTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
        private WaveStream activeStream;

        public bool CanOpen;
        private bool canPause;
        private bool canPlay;
        private bool canStop;
        private double channelLength;
        private double channelPosition;

        private bool disposed;

        private File fileTag;
        private bool inChannelSet;
        private bool inChannelTimerUpdate;
        private WaveChannel32 inputStream;
        private bool isPlaying;
        private SampleAggregator sampleAggregator;

        private WaveOutEvent waveOutDevice;

        public string Path { get; private set; }
        public FileResult? PathType { get; private set; }

        public File FileTag
        {
            get => fileTag;
            set
            {
                var oldValue = fileTag;
                fileTag = value;
                if (oldValue != fileTag)
                    NotifyPropertyChanged("FileTag");
            }
        }

        public WaveStream ActiveStream
        {
            get => activeStream;
            protected set
            {
                var oldValue = activeStream;
                activeStream = value;
                if (oldValue != activeStream)
                    NotifyPropertyChanged("ActiveStream");
            }
        }

        public double ChannelLength
        {
            get => channelLength;
            protected set
            {
                var oldValue = channelLength;
                channelLength = value;
                if (Math.Abs(oldValue - channelLength) > 1)
                    NotifyPropertyChanged("ChannelLength");
            }
        }

        public double ChannelPosition
        {
            get => channelPosition;
            set
            {
                if (inChannelSet) return;
                inChannelSet = true; // Avoid recursion
                var oldValue = channelPosition;
                var position = Math.Max(0, Math.Min(value, ChannelLength));
                if (!inChannelTimerUpdate && ActiveStream != null)
                    ActiveStream.Position =
                        (long) (position / ActiveStream.TotalTime.TotalSeconds * ActiveStream.Length);
                channelPosition = position;
                if (ActiveStream != null && Math.Abs(oldValue - ActiveStream.Position) > 1)
                    NotifyPropertyChanged("ChannelPosition");
                inChannelSet = false;
            }
        }

        public bool CanPlay
        {
            get => canPlay;
            protected set
            {
                var oldValue = canPlay;
                canPlay = value;
                if (oldValue != canPlay)
                    NotifyPropertyChanged("CanPlay");
            }
        }

        public bool CanPause
        {
            get => canPause;
            protected set
            {
                var oldValue = canPause;
                canPause = value;
                if (oldValue != canPause)
                    NotifyPropertyChanged("CanPause");
            }
        }

        public bool CanStop
        {
            get => canStop;
            protected set
            {
                var oldValue = canStop;
                canStop = value;
                if (oldValue != canStop)
                    NotifyPropertyChanged("CanStop");
            }
        }

        public bool IsPlaying
        {
            get => isPlaying;
            protected set
            {
                var oldValue = isPlaying;
                isPlaying = value;
                if (oldValue != isPlaying)
                    NotifyPropertyChanged("IsPlaying");
                positionTimer.IsEnabled = value;
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

            if (waveOutDevice == null) return;
            waveOutDevice.Dispose();
            waveOutDevice = null;
        }

        public void Stop()
        {
            if (!Instance.CanStop) return;

            waveOutDevice?.Stop();
            IsPlaying = false;
            CanStop = false;
            CanPlay = false;
            CanPause = false;

            Path = null;

            NotifyPropertyChanged("Stop");
        }

        public void Pause()
        {
            if (!IsPlaying || !CanPause) return;

            waveOutDevice.Pause();
            IsPlaying = false;
            CanPlay = true;
            CanPause = false;

            NotifyPropertyChanged("Pause");
        }

        public void Play()
        {
            if (!CanPlay) return;

            waveOutDevice.Play();
            IsPlaying = true;
            CanPause = true;
            CanPlay = false;
            CanStop = true;

            NotifyPropertyChanged("Play");
        }

        public void OpenFile(string path)
        {
            if (!CanOpen) return;

            StopAndCloseStream();

            if (ActiveStream != null) ChannelPosition = 0;

            try
            {
                PathType = null;

                if (System.IO.File.Exists(path))
                {
                    if (path.EndsWith(".radio")) path = System.IO.File.ReadAllText(path);
                    else PathType = FileResult.File;
                }

                if (Uri.TryCreate(path, UriKind.Absolute, out var uriResult) &&
                    (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    path = uriResult.ToString();

                    PathType = FileResult.Stream;
                }

                if (PathType == null)
                {
                    ActiveStream = null;
                    CanPlay = false;
                    Path = null;
                    return;
                }

                Path = path;

                waveOutDevice = new WaveOutEvent
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

                NotifyPropertyChanged("Content");

                switch (PathType)
                {
                    case FileResult.File:
                        FileTag = File.Create(path);
                        break;

                    case FileResult.Stream:
                        FileTag = null;
                        break;
                }

                CanPlay = true;

                Instance.Play();
            }
            catch
            {
                ActiveStream = null;
                CanPlay = false;
                Path = null;
            }
        }

        private void InputStream_Sample(object sender, SampleEventArgs e)
        {
            sampleAggregator.Add(e.Left, e.Right);
        }

        #region Instance

        public static AudioPlayer Instance => instance ?? (instance = new AudioPlayer());

        private AudioPlayer()
        {
            positionTimer.Interval = TimeSpan.FromMilliseconds(50);
            positionTimer.Tick += PositionTimer_Tick;
        }

        private void PositionTimer_Tick(object sender, EventArgs e)
        {
            inChannelTimerUpdate = true;
            ChannelPosition = ActiveStream.Position / (double) ActiveStream.Length *
                              ActiveStream.TotalTime.TotalSeconds;
            inChannelTimerUpdate = false;
        }

        #endregion Instance

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
                if (disposing) StopAndCloseStream();

                disposed = true;
            }
        }

        #endregion IDisposable

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
            return (int) (frequency / maxFrequency * (fftDataSize / 2));
        }

        #endregion ISpectrumPlayer

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        #endregion INotifyPropertyChanged
    }
}