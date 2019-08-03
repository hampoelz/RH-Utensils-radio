using Radio.Wpf.Utilities;
using System;
using System.Windows;
using System.Windows.Input;

namespace Radio.Wpf
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MessageHelper.ReceiveDataMessages();

            string[] args = Environment.GetCommandLineArgs();

            for (var arg = 1; arg != args.Length; ++arg)
            {
                switch (args[arg])
                {
                    case "-page":
                        Index.Navigate(new Uri("Pages/" + args[arg + 1] + ".xaml", UriKind.Relative));
                        continue;

                        //other cases
                }
            }
        }

        private void MetroWindow_KeyDown(object sender, KeyEventArgs e)
        {
            Utilities.KeyDownEvent.Handle(e.Key);

            e.Handled = true;
        }
    }
}