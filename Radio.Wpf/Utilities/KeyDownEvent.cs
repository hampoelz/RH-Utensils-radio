using MaterialDesignThemes.Wpf;
using System;
using System.Windows.Input;

namespace Radio.Wpf.Utilities
{
    internal static class KeyDownEvent
    {
        public static void Handle(Key key)
        {
            string[] args = Environment.GetCommandLineArgs();

            for (var arg = 1; arg != args.Length; ++arg)
            {
                if (args[arg] == "-page")
                {
                    switch (args[arg + 1])
                    {
                        case "Player":
                            if (key == Key.Space)
                            {
                                if (AudioPlayer.Instance.IsPlaying && AudioPlayer.Instance.CanPause)
                                {
                                    Pages.Player.PausePlayIcon.Kind = PackIconKind.PlayCircleOutline;

                                    AudioPlayer.Instance.Pause();
                                }
                                else if (!AudioPlayer.Instance.IsPlaying && AudioPlayer.Instance.CanPlay)
                                {
                                    Pages.Player.PausePlayIcon.Kind = PackIconKind.PauseCircleOutline;

                                    AudioPlayer.Instance.Play();
                                }
                            }
                            else if (key == Key.Right)
                            {
                                AudioPlayer.Instance.ChannelPosition += 10;
                            }
                            else if (key == Key.Left)
                            {
                                AudioPlayer.Instance.ChannelPosition -= 10;
                            }
                            return;

                            //other cases
                    }
                }
            }
        }
    }
}