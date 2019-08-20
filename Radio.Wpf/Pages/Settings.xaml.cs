using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MaterialDesignThemes.Wpf.Transitions;
using Radio.Wpf.Utilities;

namespace Radio.Wpf.Pages
{
    public partial class Settings
    {
        public static ToggleButton Theme;
        public static ToggleButton VolumePosition;

        private bool _loaded;

        public Settings()
        {
            InitializeComponent();

            Theme = _theme;
            VolumePosition = _volumePosition;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _loaded = true;
        }

        private void Theme_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_loaded) return;

            SettingsHelper.ChangeValue("theme", "dark");
        }

        private void Theme_Checked(object sender, RoutedEventArgs e)
        {
            if (!_loaded) return;

            SettingsHelper.ChangeValue("theme", "light");
        }

        private void VolumePosition_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_loaded) return;

            SettingsHelper.ChangeValue("volumePosition", "left");
        }

        private void VolumePosition_Checked(object sender, RoutedEventArgs e)
        {
            if (!_loaded) return;

            SettingsHelper.ChangeValue("volumePosition", "right");
        }

        private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var content = "";

            for (var item = 0; item < IcSettings.Items.Count; item++)
            {
                var uiElement = (UIElement) IcSettings.ItemContainerGenerator.ContainerFromIndex(item);
                if (!(uiElement is TransitioningContent TC)) continue;

                if (!(VisualTreeHelper.GetOffset(TC).Y <= e.VerticalOffset)) continue;
                var uiGrid = (UIElement) TC.Content;
                if (!(uiGrid is Grid grid)) continue;

                for (var children = 0; children < grid.Children.Count; children++)
                {
                    var uiTextBox = grid.Children[children];

                    if (!(uiTextBox is TextBlock TB)) continue;

                    content = " / " + TB.Text;
                }
            }

            if (TbNavigation.Text == Title + content) return;

            var da2 = new DoubleAnimation(0, TimeSpan.FromSeconds(0.2));
            TbNavigation.BeginAnimation(OpacityProperty, da2);

            await Task.Delay(TimeSpan.FromSeconds(0.4));

            TbNavigation.Text = Title + content;

            var da1 = new DoubleAnimation(1, TimeSpan.FromSeconds(0.2));
            TbNavigation.BeginAnimation(OpacityProperty, da1);
        }

        private void Privacy_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://hampoelz.net/impressum/");
        }
    }
}