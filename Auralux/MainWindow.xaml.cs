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
        int pticku = 0; //kolikrat se spustil engine (ticknul)
        double rychlostrotace = 0.03; //pricita se k radianum
        Hrac hrac;
        int velikost;
        int procent; //kolik procent jednotek je vybrano
        int kolik; //kolik jednotek je vybrano po kliknuti na planetu
        Bot[] boti;
        Planeta[] planety;
        Planeta vybrano; //planeta vybrana klikem
        List<Jednotka> odpadky; //jednotky na smazani
        public MainWindow()
        {
            InitializeComponent();
            Timer.Tick += Engine;
            Timer.Interval = TimeSpan.FromMilliseconds(20);
            Start();


        }

        private void Klik(object sender, MouseButtonEventArgs e)
        {
            Trace.WriteLine(e.OriginalSource);
            if (e.OriginalSource is TextBlock || e.OriginalSource is Ellipse || e.OriginalSource is Border) //kliknuti na nejakou planetku
            {
                var souradnicekliku = e.GetPosition(this);
                int sourx = (int)(souradnicekliku.X);
                int soury = (int)(souradnicekliku.Y);


                foreach (Planeta planeta in planety) //zjisti na jakou se kliklo
                {
                    if (planeta is null) break;
                    //Trace.WriteLine($"eukleides={Math.Sqrt((sourx - planeta.px) * (sourx - planeta.px) + (soury - planeta.py) * (soury - planeta.py))} majitel planety={planeta.majitel} px a py={planeta.px} {planeta.py}  sourx a y = {sourx} {soury}");
                    if (Math.Sqrt((sourx - planeta.px) * (sourx - planeta.px) + (soury - planeta.py) * (soury - planeta.py)) < velikost / 2) //klikl na planetu
                    {
                        if (vybrano == planeta) //kliklo se na vybranou planetu
                        {
                            procent -= 33;
                            kolik = (int)(vybrano.pjednotek * ((double)procent / 100));
                            vybrano.InfoVypisPocetVyberu(kolik);
                            if (procent == 1)
                            {

                                vybrano.OdeberseGraficky();
                                vybrano.InfoSmazat();
                                vybrano = null;
                                return;
                            }
                        }
                        if (vybrano is null && planeta.majitel == 1) //novy vyber
                        {
                            vybrano = planeta;
                            procent = 100;
                            kolik = (int)(vybrano.pjednotek * ((double)procent / 100));
                            vybrano.VyberseGraficky(); //udela vyberovy kruh
                            vybrano.InfoVypisPocetVyberu(kolik);
                        }

                        if (vybrano != planeta && vybrano is not null) //poslani jednotek na nejakou jinou planetu  
                        {
                            kolik = Math.Min(kolik, 199 - planeta.pjednotek); //aby to neslo pres 200 nebo proste hranici
                            for (int i = 0; i < kolik; i++)
                            {
                                vybrano.pjednotek--;
                                vybrano.jednotky[vybrano.pjednotek].PoslatNaCestu(sourx, soury, planeta);
                            }
                            vybrano.OdeberseGraficky();
                            vybrano.InfoSmazat();
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
            LoopPresPlanety();

            if (pticku % 30 == 0)
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
            int pocetbotu = 4;
            this.boti = new Bot[pocetbotu];
            for (int i = 0; i < pocetbotu; i++)
            {
                boti[i] = new Bot(myCanvas, planety, i + 2);
            }


            velikost = 60;
            planety[0] = new Planeta(myCanvas, 590, 380, velikost, 1, 1);
            planety[1] = new Planeta(myCanvas, 300, 180, velikost, 1, 1);
            planety[2] = new Planeta(myCanvas, 880, 180, velikost, 2, 1);
            planety[3] = new Planeta(myCanvas, 590, 680, velikost, 3, 1);

            this.hrac = new Hrac(myCanvas, planety, 1);

            Timer.Start();
        }
        private void PohybJednotek() //pohyb po obezne draze, pohyb mezi a chovani pri dosahu cile
        {

            List<Jednotka> odpadky = new List<Jednotka>();
            foreach (Jednotka unit in hrac.jednotky) //Pohyb jendotek hrace
            {
                if (unit == null) break;
                unit.PosunLetu(velikost, rychlostrotace, boti, odpadky, hrac);
            }
            if (odpadky is not null || odpadky.Count != 0) //vysype kos (mazani instanci mrtvych jednotek aby nepretekla pamet)
            {
                //Trace.WriteLine("Sype kos");
                foreach (Jednotka mrtvajednotka in odpadky)
                {
                    hrac.jednotky.Remove(mrtvajednotka);
                }
            }



            foreach (Bot bot in boti) //Pohyb jendotek botu
            {
                foreach (Jednotka unit in bot.jednotky)
                {
                    if (unit == null) break;
                    unit.PosunLetu(velikost, rychlostrotace, boti, odpadky, hrac);
                }
            }
        }

        private void LoopPresPlanety()
        {
            foreach (Planeta planeta in planety)
            {
                if (planeta == null) return;
                if (planeta.majitel > 1)
                {
                    planeta.InfoVypispjednotek();
                }
            }
        }
    }
    public abstract class Player
    {
        Planeta[] planety;
        public List<Jednotka> jednotky;
        Canvas myCanvas;
        public int celkemunits;
        int id;
        public Player(Canvas myCanvas, Planeta[] planety, int id)
        {
            this.myCanvas = myCanvas;
            this.planety = planety;
            this.id = id;
            //jednotky = new Jednotka[1000];
            jednotky = new List<Jednotka>();
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
                        jednotky.Add(new Jednotka(myCanvas, planeta, planeta.majitel, planeta.px + planeta.velikost / 2 + 15, planeta.py + planeta.velikost / 2 + 15, planeta.px + planeta.velikost / 2, planeta.py + planeta.velikost / 2, id));
                        planeta.VytvorJednotku(jednotky.Last());
                        celkemunits++;
                    }
                }
            }
        }
    }
    public class Hrac : Player
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
        public bool vybrana; //planeta je vybrana kliknutim
        int[] kontrolor = new int[2]; //kdo ji kontorluje, 0=id, 1=procenta
        public int px;
        public int py;
        public Label info; //info o statusu planety
        public Ellipse gplaneta; // graficka instance planety
        private Ellipse vyberovykruh; //okruh kdyz je vybrana planeta
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
            kontrolor[0] = 0; //id kontrolora
            kontrolor[1] = 0; //mira kontroly v (procentech?)

            gplaneta = new Ellipse
            {
                Tag = "planeta",
                Height = velikost,
                Width = velikost,
                Fill = NavratBarvy.Navrat(majitel)
            };
            Canvas.SetTop(gplaneta, py - velikost / 2);
            Canvas.SetLeft(gplaneta, px - velikost / 2);
            myCanvas.Children.Add(gplaneta);

            info = new Label
            {
                Tag = "txt",
                Content = "neco",
                Foreground = NavratBarvy.Navrat(majitel)
            };
            Canvas.SetTop(info, py - velikost / 3);
            Canvas.SetLeft(info, px - velikost / 2);
            myCanvas.Children.Add(info);

        }

        public void VytvorJednotku(Jednotka jednotka)
        {
            if (pjednotek < level * 100)
            {
                jednotky[pjednotek] = jednotka;
                pjednotek++;
            }
        }

        public void Utok(int utocnik)
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
            if (majitel != utocnik && majitel != 0) //na planetu, ktera ma uz majitele utoci nekdo cizi
            {
                pjednotek--;
                if (pjednotek > 0)
                {
                    zdravi = 33;
                }

                if (pjednotek <= 0) //vsechny obrane jednotky umrely
                {
                    zdravi = 33 + pjednotek;
                }
            }
        }

        public Jednotka ZabijJednotku()
        {
            Jednotka cozabijim = jednotky[pjednotek - 1];
            cozabijim.SmazZCanvasu();
            jednotky[pjednotek - 1] = null;
            pjednotek--;
            return cozabijim;
        }

        public void VyberseGraficky() //vytvori graficky kruh aby se planeta oznacila
        {
            vyberovykruh = new Ellipse
            {
                Tag = "vyberovykruh",
                Height = velikost,
                Width = velikost,
                Fill = NavratBarvy.Navrat(majitel),
                Stroke = NavratBarvy.Navrat(12),
                StrokeThickness = 5
            };
            Canvas.SetTop(vyberovykruh, py - velikost / 2);
            Canvas.SetLeft(vyberovykruh, px - velikost / 2);
            myCanvas.Children.Add(vyberovykruh);

            info.Foreground = NavratBarvy.Navrat(majitel);
            Canvas.SetZIndex(info, 1);
        }
        public void OdeberseGraficky()//odebere kruh 
        {
            myCanvas.Children.Remove(vyberovykruh);
        }

        public void InfoVypisPocetVyberu(int kolik) //vypise kolik je vybrano jednotek
        {
            info.Foreground = NavratBarvy.Navrat(666);
            info.Content = $"Vybrano:\n    {kolik}";
        }

        public void InfoVypispjednotek()
        {
            info.Foreground = NavratBarvy.Navrat(666);
            info.Content = $"  {pjednotek} \n Jednotek";
        }

        public void InfoSmazat() //odstrani zobrazene info at uz je jakykoliv
        {
            info.Content = "x";
            info.Foreground = NavratBarvy.Navrat(majitel);
        }
    }



    public class Jednotka
    {
        public bool naceste;
        public int kamx; //kam pohybuje
        public int kamy;
        public int id;
        public bool obiha;
        public double px;
        public double py;
        public int stredx;
        public int stredy;
        public double radian;
        public Polygon gjednotka;
        public Planeta planetakamletim;
        private Canvas myCanvas;

        public Jednotka(Canvas myCanvas, Planeta planeta, int majitel, int px, int py, int stredx, int stredy, int id)
        {

            this.px = px;
            this.py = py;
            this.stredx = stredx;
            this.stredy = stredy;
            this.id = id;
            this.obiha = true;
            var rand = new Random(); //pro nahodny radian
            this.radian = (double)rand.Next(10);
            this.myCanvas = myCanvas;

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
            Canvas.SetTop(gjednotka, stredy - planeta.velikost / 2);
            Canvas.SetLeft(gjednotka, stredx - planeta.velikost / 2);
            myCanvas.Children.Add(gjednotka);

        }
        public void PoslatNaCestu(int kamx, int kamy, Planeta planetakamletim)
        {
            naceste = true;
            obiha = false;
            this.kamx = kamx;
            this.kamy = kamy;
            this.planetakamletim = planetakamletim;
        }

        public void SmazZCanvasu() //smaze ji z platna
        {
            myCanvas.Children.Remove(gjednotka);
        }

        public void PosunLetu(int velikost, double rychlostrotace, Bot[] boti, List<Jednotka> odpadky, Hrac hrac)
        {
            if (obiha == true) //jednotka obiha
            {
                radian += rychlostrotace;
                px = (stredx + Math.Cos(radian) * (velikost - 7) - velikost / 2 - 5);
                py = (stredy + Math.Sin(radian) * (velikost - 7) - velikost / 2 - 5);
                Canvas.SetTop(gjednotka, py);
                Canvas.SetLeft(gjednotka, px);
            }
            else if (naceste == true) //Jednotka je na ceste
            {
                double eukleides = Math.Sqrt((px - kamx) * (px - kamx) + (py - kamy) * (py - kamy));
                px = (px + ((kamx - px) / eukleides) * 1.5);
                py = (py + ((kamy - py) / eukleides) * 1.5);
                Canvas.SetTop(gjednotka, py);
                Canvas.SetLeft(gjednotka, px);
                SrazkaSPlanetou(boti, odpadky, hrac);
                //dopsat
            }
        }

        public void SrazkaSPlanetou(Bot[] boti, List<Jednotka> odpadky, Hrac hrac)
        {
            double vzdalenostodplanety = Math.Sqrt((px - planetakamletim.px) * (px - planetakamletim.px) + (py - planetakamletim.py) * (py - planetakamletim.py));

            if (vzdalenostodplanety < planetakamletim.velikost + 15 && planetakamletim.majitel == id) //jednotka se dostala do orbitu cilove vlastni planety
            {
                planetakamletim.jednotky[planetakamletim.pjednotek] = this; //ERROR?
                planetakamletim.pjednotek++;
                obiha = true;
                naceste = false;
                //unit.px = unit.planetakamletim.px + unit.planetakamletim.velikost / 2 + 15;
                //unit.py = unit.planetakamletim.py + unit.planetakamletim.velikost / 2 + 15;
                stredx = planetakamletim.px + planetakamletim.velikost / 2;
                stredy = planetakamletim.py + planetakamletim.velikost / 2;

            }

            if (vzdalenostodplanety < planetakamletim.velikost + 15 && planetakamletim.majitel != id && planetakamletim.majitel != 0) //jednotka se dostala do orbitu nepratelske planety
            {
                if (planetakamletim.pjednotek > 0)
                {
                    //planetakamletim.jednotky[planetakamletim.pjednotek - 1].SmazZCanvasu(); //Killnu jednotku grafiky na planete kam se utoci
                    boti[planetakamletim.majitel - 2].jednotky.Remove(planetakamletim.ZabijJednotku()); //(planetakamletim.majitel - 2) je pozice bota v poli na jehoz planetu jednotka leti //smaze instanci //zmenit pro boty nestaci ctrl+v

                    SmazZCanvasu(); //Killnu jednotku co utocila
                                    
                    odpadky.Add(this);
                    px = 0;
                    py = 0;
                    hrac.celkemunits--;

                }
                if (planetakamletim.pjednotek == 0)
                {
                    planetakamletim.majitel = 0;
                    planetakamletim.gplaneta.Fill = NavratBarvy.Navrat(0);
                    planetakamletim.InfoSmazat();
                }

            }
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
                case 11:
                    return new SolidColorBrush(Colors.Black);
                case 12:
                    return new SolidColorBrush(Colors.GreenYellow);
                default:
                    return new SolidColorBrush(Colors.Red);
            }
        }
    }
}

