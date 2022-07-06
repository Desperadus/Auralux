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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;

namespace Auralux
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer Timer = new DispatcherTimer();
        MediaPlayer player = new MediaPlayer();
        int rychlost = 1; 
        int pticku = 0; //kolikrat se spustil engine (tickl)
        double rychlostrotace = 0.03; //pricita se k radianum
        Hrac hrac;
        int velikost;
        int procent;
        Bot[] boti;
        Planeta[] planety;
        Planeta vybrano; //planeta vybrana klikem
        public MainWindow()
        {
            InitializeComponent();
            Timer.Tick += Engine;
            Timer.Interval = TimeSpan.FromMilliseconds(20);
            Start();


        }

        private void Klik(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Ellipse) //kliknuti na nejakou elipsu
            {
                var souradnicekliku = e.GetPosition(this);
                int sourx = (int)(souradnicekliku.X);
                int soury = (int)(souradnicekliku.Y);
                

                foreach (Planeta planeta in planety)
                {
                    if (planeta is null) break;
                    //Trace.WriteLine($"eukleides={Math.Sqrt((sourx - planeta.px) * (sourx - planeta.px) + (soury - planeta.py) * (soury - planeta.py))} majitel planety={planeta.majitel} px a py={planeta.px} {planeta.py}  sourx a y = {sourx} {soury}");
                    if (planeta.majitel == 1 && Math.Sqrt((sourx - planeta.px) * (sourx - planeta.px) + (soury - planeta.py) * (soury - planeta.py)) < velikost/2) //klikl na svoji planetu
                    {
                        if (vybrano == planeta)
                        {
                            procent -= 33;
                        }
                        if (vybrano is null) //novy vyber
                        {
                            vybrano = planeta;
                            procent = 100;
                        }
                        
                        if (vybrano != planeta)
                        {

                            int kolik = (int)(vybrano.pjednotek * ((double)procent / 100));
                            for (int i = 0; i < kolik ; i++)
                            {
                                //Trace.WriteLine($"pjednotek {vybrano.pjednotek} podminka={(int)(vybrano.pjednotek * (double)(procent / 100)) - 2}");
                                vybrano.pjednotek--;
                                vybrano.jednotky[vybrano.pjednotek].PoslatNaCestu(sourx, soury);
                            }
                            vybrano = null;
                        }
                        
                    }
                }
                

            }
        }

        private void Unklik(object sender, MouseButtonEventArgs e)
        {

        }

        private void Engine(object sender, EventArgs e)
        {
            pticku++;
            PohybJednotek();
            if (pticku%30 == 0)
            {
                hrac.TvorbaJednotek(); //tvori jedntoky na planetach hrace
                foreach (Bot bot in boti)
                {
                    bot.TvorbaJednotek();
                }

            }
        }

        private void Start()
        {
            this.planety = new Planeta[20];   //Vytvoreni planet
            int pocetplanet = 4;
            int pocetbotu = 2;
            this.boti = new Bot[pocetbotu];
            for (int i = 0; i < pocetbotu; i++)
            {
                boti[i] = new Bot(myCanvas, planety, i+2);
            }


            velikost = 60;
            planety[0] = new Planeta(myCanvas, 590, 380, velikost, 1, 1);
            planety[1] = new Planeta(myCanvas, 300, 180, velikost, 1, 1);
            planety[2] = new Planeta(myCanvas, 880, 180, velikost, 2, 1);
            planety[3] = new Planeta(myCanvas, 590, 680, velikost, 3, 1);

            this.hrac = new Hrac(myCanvas, planety,1);
            Timer.Start();
        }
        private void PohybJednotek()
        {

            foreach (Jednotka unit in hrac.jednotky) //Pohyb jendotek hrace
            {
                
                if (unit == null) break;
                if (unit.obiha == true)
                {
                    unit.radian += rychlostrotace;
                    unit.px = (unit.stredx + Math.Cos(unit.radian) * (velikost - 7) - velikost / 2 - 5);
                    unit.py = (unit.stredy + Math.Sin(unit.radian) * (velikost - 7) - velikost / 2 - 5);
                    Canvas.SetTop(unit.gjednotka, unit.py);
                    Canvas.SetLeft(unit.gjednotka, unit.px);
                }
                else if (unit.naceste == true) //Jednotka je na ceste
                {
                    double eukleides = Math.Sqrt((unit.px - unit.kamx) * (unit.px - unit.kamx) + (unit.py - unit.kamy) * (unit.py - unit.kamy));
                    unit.px = (unit.px + ((unit.kamx - unit.px) / eukleides) *1.5);
                    unit.py = (unit.py + ((unit.kamy-unit.py) / eukleides)*1.5);
                    Canvas.SetTop(unit.gjednotka, unit.py);
                    Canvas.SetLeft(unit.gjednotka, unit.px);
                }



            }
            foreach (Bot bot in boti) //Pohyb jendotek botu
            {
                foreach (Jednotka unit in bot.jednotky)
                {
                    if (unit == null) break;
                    if (unit.obiha == true)
                    {
                        unit.radian += rychlostrotace;
                        unit.px = (unit.stredx + Math.Cos(unit.radian) * (velikost - 7) - velikost / 2 - 5);
                        unit.py = (unit.stredy + Math.Sin(unit.radian) * (velikost - 7) - velikost / 2 - 5);
                        Canvas.SetTop(unit.gjednotka, unit.py);
                        Canvas.SetLeft(unit.gjednotka, unit.px);
                    }
                }
            }
        }
    }
    public abstract class Player
    {
        Planeta[] planety;
        public Jednotka[] jednotky;
        Canvas myCanvas;
        public int celkemunits;
        int id;
        public Player(Canvas myCanvas, Planeta[] planety, int id)
        {
            this.myCanvas = myCanvas;
            this.planety = planety;
            this.id = id;
            jednotky = new Jednotka[1000];
            celkemunits = 0;
        }

        public void TvorbaJednotek()
        {
            foreach (Planeta planeta in planety)
            {
                if (planeta == null) break;
                if (planeta.majitel == id)
                {
                    for (int i = 0; i < planeta.level; i++)
                    {
                        jednotky[celkemunits] = new Jednotka(myCanvas, planeta, planeta.majitel, planeta.px + planeta.velikost / 2 + 15, planeta.py + planeta.velikost / 2 + 15, planeta.px + planeta.velikost / 2, planeta.py + planeta.velikost / 2);
                        planeta.VytvorJednotku(jednotky[celkemunits]);
                        celkemunits++;
                    }
                }
            }
        }
    }
    public class Hrac:Player
    {
        public Hrac(Canvas myCanvas, Planeta[] planety, int id) : base(myCanvas, planety, id)
        {

        }
    }
    public class Bot : Player
    {
        public Bot(Canvas myCanvas, Planeta[] planety, int id) : base(myCanvas, planety, id)
        {

        }
    }



    public class Planeta
    {
        public int majitel;
        public int zdravi;
        public int pjednotek;
        public int velikost;
        public int level;
        int[] kontrolor = new int[2];
        public int px;
        public int py;
        Canvas myCanvas;
        public Jednotka[] jednotky = new Jednotka[200];
        public Planeta(Canvas myCanvas, int px, int py, int velikost, int majitel, int level)
        {
            this.px = px;
            this.py = py;
            this.majitel = majitel;
            this.myCanvas = myCanvas;
            this.velikost = velikost;
            this.level = level;
            zdravi = 100;
            pjednotek = 0;
            kontrolor[0] = 0;
            kontrolor[1] = 0;

            Ellipse gplaneta = new Ellipse
            {
                Tag = "planeta",
                Height = velikost,
                Width = velikost,
                Fill = NavratBarvy.Navrat(majitel)
            };
            Canvas.SetTop(gplaneta, py - velikost / 2);
            Canvas.SetLeft(gplaneta, px - velikost / 2);
            myCanvas.Children.Add(gplaneta);


        }
        
        public void VytvorJednotku(Jednotka jednotka)
        {
            if (pjednotek < 199)
            {
                jednotky[pjednotek] = jednotka;
                pjednotek++;
            }
        }

        public void utok(int utocnik)
        {
            if (majitel == 0) //planeta nema majitele
            {
                if (kontrolor[0] == 0) //nikdo ji nekontroluje
                {
                    kontrolor[0] = utocnik;
                    kontrolor[1] = 1;
                }
                if (kontrolor[0] == utocnik) //planetu kontroluje ten kod posila jednotky
                {
                    if (kontrolor[1] >= 33) //uspesne zabral planetu
                    {
                        majitel = utocnik;
                        zdravi = 33;
                        pjednotek = 1;
                    }
                    else //pricteni jednotky
                    {
                        kontrolor[1] += 1;
                    }
                }
                if (kontrolor[0] != utocnik) //nekdo cizi utoci
                {
                    if (kontrolor[1] <= 1) //ztrata kontroly
                    {
                        kontrolor[0] = 0;
                        kontrolor[1] = 0;
                    }
                    else
                    {
                        kontrolor[1] -= 1;
                    }
                }
            }
            if (majitel == utocnik) //majitel pridava jednotky
            {
                pjednotek++;
                if (pjednotek > 0)
                {
                    zdravi = 33;
                }
            }
            if (majitel != utocnik && majitel !=0) //na planetu, ktera ma uz majitele utoci nekdo cizi
            {
                pjednotek--;
                if (pjednotek>0) 
                {
                    zdravi = 33;
                }

                if (pjednotek <= 0) //vsechny obrane jednotky umrely
                {
                    zdravi = 33 + pjednotek;
                }
            }
        }
    }
    


    public class Jednotka
    {
        public bool naceste;
        public int kamx; //kam pohybuje
        public int kamy;
        public bool obiha;
        public double px;
        public double py;
        public int stredx;
        public int stredy;
        public double radian;
        public Polygon gjednotka;

        public Jednotka(Canvas myCanvas, Planeta planeta, int majitel, int px, int py, int stredx, int stredy)
        {

            this.px = px;
            this.py = py;
            this.stredx = stredx;
            this.stredy = stredy;
            this.obiha = true;
            var rand = new Random(); //pro nahodny radian
            this.radian = (double)rand.Next(10);

            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point(0, 0));
            myPointCollection.Add(new Point(0, 1));
            myPointCollection.Add(new Point(1, 1));

            this.gjednotka = new Polygon();
            gjednotka.Points = myPointCollection;
            gjednotka.Fill = NavratBarvy.Navrat(majitel);
            gjednotka.Stretch = Stretch.Fill;
            gjednotka.Width = 10;
            gjednotka.Height = 10;
            Canvas.SetTop(gjednotka, stredy - planeta.velikost/2);
            Canvas.SetLeft(gjednotka, stredx - planeta.velikost / 2);
            myCanvas.Children.Add(gjednotka);

        }
        public void PoslatNaCestu(int kamx, int kamy)
        {
            naceste = true;
            obiha = false;
            this.kamx = kamx;
            this.kamy = kamy;
        }
    }

    class NavratBarvy
    {
        public static SolidColorBrush Navrat(int majitel)
        {
            switch (majitel)
            {
                case 0:
                    return new SolidColorBrush(Colors.Gray);
                case 1:
                    return new SolidColorBrush(Colors.Blue); //hrac
                case 2:
                    return new SolidColorBrush(Colors.Green);
                case 3:
                    return new SolidColorBrush(Colors.Orange);
                case 4:
                    return new SolidColorBrush(Colors.Purple);
                default:
                    return new SolidColorBrush(Colors.Red);
            }
        }
    }

}
