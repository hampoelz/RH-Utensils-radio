using System;
using System.Windows.Input;

namespace Radio.Wpf.Utilities
{
    internal static class KeyDownEvent
    {
        public static void Handle(Key key)
        {
            var args = Environment.GetCommandLineArgs();

            for (var arg = 1; arg != args.Length; ++arg)
                if (args[arg] == "-page")
                    switch (args[arg + 1])
                    {
                        case "Player":
                            switch (key)
                            {
                                case Key.Space when AudioPlayer.Instance.IsPlaying && AudioPlayer.Instance.CanPause:
                                    AudioPlayer.Instance.Pause();
                                    break;
                                case Key.Space:
                                {
                                    if (!AudioPlayer.Instance.IsPlaying && AudioPlayer.Instance.CanPlay)
                                        AudioPlayer.Instance.Play();
                                    break;
                                }

                                case Key.Right:
                                    AudioPlayer.Instance.ChannelPosition += 10;
                                    break;
                                case Key.Left:
                                    AudioPlayer.Instance.ChannelPosition -= 10;
                                    break;
                                case Key.None:
                                    break;
                                case Key.Cancel:
                                    break;
                                case Key.Back:
                                    break;
                                case Key.Tab:
                                    break;
                                case Key.LineFeed:
                                    break;
                                case Key.Clear:
                                    break;
                                case Key.Return:
                                    break;
                                case Key.Pause:
                                    break;
                                case Key.Capital:
                                    break;
                                case Key.KanaMode:
                                    break;
                                case Key.JunjaMode:
                                    break;
                                case Key.FinalMode:
                                    break;
                                case Key.HanjaMode:
                                    break;
                                case Key.Escape:
                                    break;
                                case Key.ImeConvert:
                                    break;
                                case Key.ImeNonConvert:
                                    break;
                                case Key.ImeAccept:
                                    break;
                                case Key.ImeModeChange:
                                    break;
                                case Key.Prior:
                                    break;
                                case Key.Next:
                                    break;
                                case Key.End:
                                    break;
                                case Key.Home:
                                    break;
                                case Key.Up:
                                    break;
                                case Key.Down:
                                    break;
                                case Key.Select:
                                    break;
                                case Key.Print:
                                    break;
                                case Key.Execute:
                                    break;
                                case Key.Snapshot:
                                    break;
                                case Key.Insert:
                                    break;
                                case Key.Delete:
                                    break;
                                case Key.Help:
                                    break;
                                case Key.D0:
                                    break;
                                case Key.D1:
                                    break;
                                case Key.D2:
                                    break;
                                case Key.D3:
                                    break;
                                case Key.D4:
                                    break;
                                case Key.D5:
                                    break;
                                case Key.D6:
                                    break;
                                case Key.D7:
                                    break;
                                case Key.D8:
                                    break;
                                case Key.D9:
                                    break;
                                case Key.A:
                                    break;
                                case Key.B:
                                    break;
                                case Key.C:
                                    break;
                                case Key.D:
                                    break;
                                case Key.E:
                                    break;
                                case Key.F:
                                    break;
                                case Key.G:
                                    break;
                                case Key.H:
                                    break;
                                case Key.I:
                                    break;
                                case Key.J:
                                    break;
                                case Key.K:
                                    break;
                                case Key.L:
                                    break;
                                case Key.M:
                                    break;
                                case Key.N:
                                    break;
                                case Key.O:
                                    break;
                                case Key.P:
                                    break;
                                case Key.Q:
                                    break;
                                case Key.R:
                                    break;
                                case Key.S:
                                    break;
                                case Key.T:
                                    break;
                                case Key.U:
                                    break;
                                case Key.V:
                                    break;
                                case Key.W:
                                    break;
                                case Key.X:
                                    break;
                                case Key.Y:
                                    break;
                                case Key.Z:
                                    break;
                                case Key.LWin:
                                    break;
                                case Key.RWin:
                                    break;
                                case Key.Apps:
                                    break;
                                case Key.Sleep:
                                    break;
                                case Key.NumPad0:
                                    break;
                                case Key.NumPad1:
                                    break;
                                case Key.NumPad2:
                                    break;
                                case Key.NumPad3:
                                    break;
                                case Key.NumPad4:
                                    break;
                                case Key.NumPad5:
                                    break;
                                case Key.NumPad6:
                                    break;
                                case Key.NumPad7:
                                    break;
                                case Key.NumPad8:
                                    break;
                                case Key.NumPad9:
                                    break;
                                case Key.Multiply:
                                    break;
                                case Key.Add:
                                    break;
                                case Key.Separator:
                                    break;
                                case Key.Subtract:
                                    break;
                                case Key.Decimal:
                                    break;
                                case Key.Divide:
                                    break;
                                case Key.F1:
                                    break;
                                case Key.F2:
                                    break;
                                case Key.F3:
                                    break;
                                case Key.F4:
                                    break;
                                case Key.F5:
                                    break;
                                case Key.F6:
                                    break;
                                case Key.F7:
                                    break;
                                case Key.F8:
                                    break;
                                case Key.F9:
                                    break;
                                case Key.F10:
                                    break;
                                case Key.F11:
                                    break;
                                case Key.F12:
                                    break;
                                case Key.F13:
                                    break;
                                case Key.F14:
                                    break;
                                case Key.F15:
                                    break;
                                case Key.F16:
                                    break;
                                case Key.F17:
                                    break;
                                case Key.F18:
                                    break;
                                case Key.F19:
                                    break;
                                case Key.F20:
                                    break;
                                case Key.F21:
                                    break;
                                case Key.F22:
                                    break;
                                case Key.F23:
                                    break;
                                case Key.F24:
                                    break;
                                case Key.NumLock:
                                    break;
                                case Key.Scroll:
                                    break;
                                case Key.LeftShift:
                                    break;
                                case Key.RightShift:
                                    break;
                                case Key.LeftCtrl:
                                    break;
                                case Key.RightCtrl:
                                    break;
                                case Key.LeftAlt:
                                    break;
                                case Key.RightAlt:
                                    break;
                                case Key.BrowserBack:
                                    break;
                                case Key.BrowserForward:
                                    break;
                                case Key.BrowserRefresh:
                                    break;
                                case Key.BrowserStop:
                                    break;
                                case Key.BrowserSearch:
                                    break;
                                case Key.BrowserFavorites:
                                    break;
                                case Key.BrowserHome:
                                    break;
                                case Key.VolumeMute:
                                    break;
                                case Key.VolumeDown:
                                    break;
                                case Key.VolumeUp:
                                    break;
                                case Key.MediaNextTrack:
                                    break;
                                case Key.MediaPreviousTrack:
                                    break;
                                case Key.MediaStop:
                                    break;
                                case Key.MediaPlayPause:
                                    break;
                                case Key.LaunchMail:
                                    break;
                                case Key.SelectMedia:
                                    break;
                                case Key.LaunchApplication1:
                                    break;
                                case Key.LaunchApplication2:
                                    break;
                                case Key.Oem1:
                                    break;
                                case Key.OemPlus:
                                    break;
                                case Key.OemComma:
                                    break;
                                case Key.OemMinus:
                                    break;
                                case Key.OemPeriod:
                                    break;
                                case Key.Oem2:
                                    break;
                                case Key.Oem3:
                                    break;
                                case Key.AbntC1:
                                    break;
                                case Key.AbntC2:
                                    break;
                                case Key.Oem4:
                                    break;
                                case Key.Oem5:
                                    break;
                                case Key.Oem6:
                                    break;
                                case Key.Oem7:
                                    break;
                                case Key.Oem8:
                                    break;
                                case Key.Oem102:
                                    break;
                                case Key.ImeProcessed:
                                    break;
                                case Key.System:
                                    break;
                                case Key.OemAttn:
                                    break;
                                case Key.OemFinish:
                                    break;
                                case Key.OemCopy:
                                    break;
                                case Key.OemAuto:
                                    break;
                                case Key.OemEnlw:
                                    break;
                                case Key.OemBackTab:
                                    break;
                                case Key.Attn:
                                    break;
                                case Key.CrSel:
                                    break;
                                case Key.ExSel:
                                    break;
                                case Key.EraseEof:
                                    break;
                                case Key.Play:
                                    break;
                                case Key.Zoom:
                                    break;
                                case Key.NoName:
                                    break;
                                case Key.Pa1:
                                    break;
                                case Key.OemClear:
                                    break;
                                case Key.DeadCharProcessed:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(key), key, null);
                            }

                            return;

                        //other cases
                    }
        }
    }
}