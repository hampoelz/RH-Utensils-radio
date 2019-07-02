using MaterialDesignThemes.Wpf;
using Radio.Wpf.Utilities;
using Sample_NAudio;
using System.IO;
using System.Windows;

namespace Radio.Wpf
{
    public partial class App : Application
    {
        public static string Title;

        private static string file;

        public static string File
        {
            get => file;
            set
            {
                if (value.EndsWith(".radio"))
                {
                    Pages.Player.FileName.Text = Path.GetFileNameWithoutExtension(value);

                    var content = System.IO.File.ReadAllText(value);

                    (string title, string url) = Converter.Online.Youtube(content);

                    if (title == null || url == null)
                    {
                        value = content;
                        Title = null;
                    }
                    else
                    {
                        value = url;
                        Title = title;
                    }
                }
                else
                {
                    Title = null;
                }

                file = value;

                if (string.IsNullOrEmpty(file)) return;

                AudioPlayer.Instance.OpenFile(file);

                if (AudioPlayer.Instance.CanPlay)
                {
                    AudioPlayer.Instance.Play();
                    Pages.Player.PausePlayIcon.Kind = PackIconKind.PauseCircleOutline;

                    Pages.Player.StopButton.IsEnabled = true;
                }
            }
        }
    }
}