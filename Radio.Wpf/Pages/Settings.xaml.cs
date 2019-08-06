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
        public static ToggleButton TestProperty = new ToggleButton();
        public static ToggleButton ThemeProperty = new ToggleButton();

        public Settings()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            TestControl.Children.Add(TestProperty);
            TestProperty.Checked += TestProperty_Checked;
            TestProperty.Unchecked += TestProperty_Unchecked;

            ThemeControl.Children.Add(ThemeProperty);
            ThemeProperty.Checked += ThemeProperty_Checked;
            ThemeProperty.Unchecked += ThemeProperty_Unchecked;
        }

        private static void ThemeProperty_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingsHelper.ChangeValue("theme", "dark");
        }

        private static void ThemeProperty_Checked(object sender, RoutedEventArgs e)
        {
            SettingsHelper.ChangeValue("theme", "light");
        }

        private static void TestProperty_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingsHelper.ChangeValue("test", false.ToString());
        }

        private static void TestProperty_Checked(object sender, RoutedEventArgs e)
        {
            SettingsHelper.ChangeValue("test", true.ToString());
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