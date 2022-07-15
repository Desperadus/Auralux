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

        private void Button_Click(object sender, RoutedEventArgs e) //jak hrat
        {
            MessageBox.Show("Vaším cílem hry je obsadit všechny planety v daném levelu. Na začátku začnete s jednou nebo více planetami - ty Vaše jsou modré. Postupně se jim budou tvořit jednotky, které je obíhají, planeta má maximálně 100 * její level jednotek. Své jednotky můžete odeslat na svoji vlastní planetu či zaútočit na nepřátelskou nebo se pokusit obsadit novou. Neobsazené planety jsou šedé, nepřátelské jsou barevné. Jakmile vaše planeta nemá jednotky a je pod útokem začne jí ubývat zdraví, pokud přijde o všechno, ztrácíte její kontrolu. Obsadit novou planetu stojí 20 jednotek. Stejně jednotek stojí upgrade, který planetě zvýší zdraví o 100 a urychlí vznik jednotek, lze jej provést pouze když je planeta plně zdravá.\nOvládání:\nVybrat všechny jednotky na planetě - levý klik, další klik sníží výběr na 2/3, další na 1/3 a další zruší výběr\nOdeslat jednotky - klik na nějakou planetu (musíte mít vybrané jednotky)\nLéčit - pravý klik na planetu\nUpgrade - scroll kolečkem myši nahoru o 1 na dané planetě\nZrychlit hru - tlačítko +\nZpomalit hru - tlačítko -");

        }

        private void Button_Click_1(object sender, RoutedEventArgs e) //credits
        {
            MessageBox.Show("Tato hra byla udělána jako zápočtový program pro předmět Programovaní 2 na Matematicko-fyzikální fakultě UK v roce 2022.\nAutor: Tomáš Jelínek");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var mainwin = new MainWindow(1);
            mainwin.Show();
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
