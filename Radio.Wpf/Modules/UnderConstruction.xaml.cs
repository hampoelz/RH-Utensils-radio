using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Radio.Wpf.Utilities;

namespace Radio.Wpf.Modules
{
    public partial class UnderConstruction
    {
        public static Image Image = new Image();

        public UnderConstruction()
        {
            InitializeComponent();

            Image = _Image;

            if (Settings.Theme == "light")
                Image.Source = new BitmapImage(new Uri("/Radio;component/Assets/under-construction-dark.png",
                    UriKind.Relative));
        }
    }
}