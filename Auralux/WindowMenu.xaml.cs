using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Auralux
{
    /// <summary>
    /// Interaction logic for WindowMenu.xaml
    /// </summary>
    public partial class WindowMenu : Window
    {

        ImageBrush background = new ImageBrush();
        public WindowMenu()
        {
            InitializeComponent();

            background.ImageSource = new BitmapImage(new Uri(@"C:\Users\tomasjelinek\source\repos\Auralux\Auralux\mlhovina.jpeg"));
            myCanvas.Background = background;
        }

        private void Kliknuti(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {

        }
    }
}
