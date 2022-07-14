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
        int pticku = 0; //kolikrat se spustil engine (ticknul)
        int rychlost = 1;
        int lvl = 1;
        Hrac hrac;
        int velikost; //obecna zakladni velikost
        int procent; //kolik procent jednotek je vybrano
        int kolik; //kolik jednotek je vybrano po kliknuti na planetu
        Bot[] boti;
        Planeta[] planety;
        Planeta vybrano; //planeta vybrana klikem
        Random rand;
        public MainWindow()
        {
            InitializeComponent();
            Timer.Tick += Engine;
            Timer.Interval = TimeSpan.FromMilliseconds(15);
            System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency;
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
                    if (Math.Sqrt((sourx - planeta.px) * (sourx - planeta.px) + (soury - planeta.py) * (soury - planeta.py)) < planeta.velikost / 2) //klikl na planetu
                    {
                        if (vybrano == planeta) //kliklo se na vybranou planetu
                        {
                            procent -= 33;
                            kolik = (int)(vybrano.pjednotek * ((double)procent / 100));
                            vybrano.InfoVypisPocetVyberu(kolik);
                            if (procent == 1)
                            {

                                vybrano.OdeberseGraficky();
                                vybrano.vybrana = false;
                                vybrano = null;
                                return;
                            }
                        }
                        if (vybrano is null && planeta.majitel == 1) //novy vyber
                        {
                            vybrano = planeta;
                            vybrano.vybrana = true;
                            procent = 100;
                            kolik = (int)(vybrano.pjednotek * ((double)procent / 100));
                            vybrano.VyberseGraficky(); //udela vyberovy kruh
                            vybrano.InfoVypisPocetVyberu(kolik);
                            return;
                        }

                        if (vybrano != planeta && vybrano is not null) //poslani jednotek na nejakou jinou planetu  
                        {
                            if (vybrano.majitel == planeta.majitel) kolik = Math.Min(kolik, planeta.level*100 - (planeta.pjednotek+planeta.jednotekcobude)); //aby to neslo pres 100/200 nebo proste hranici max kapacity
                            vybrano.OdesliJednotky(kolik, planeta);
                            vybrano.OdeberseGraficky();
                            vybrano = null;
                            return;
                            //
                        }
                    }
                }


            }
        }

        private void Unklik(object sender, MouseButtonEventArgs e)
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
                    if (Math.Sqrt((sourx - planeta.px) * (sourx - planeta.px) + (soury - planeta.py) * (soury - planeta.py)) < planeta.velikost / 2) //klikl na planetu
                    {
                        planeta.Lecit();
                    }
                }


            }

        }

        private void Upgrade(object sender, MouseWheelEventArgs e)
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
                    if (Math.Sqrt((sourx - planeta.px) * (sourx - planeta.px) + (soury - planeta.py) * (soury - planeta.py)) < planeta.velikost / 2) //klikl na planetu
                    {
                        planeta.Upgrade();
                    }
                }

            }
        }

        private void ZmacklaKlavesa(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Add) 
            {
                rychlost = (rychlost + 1) % 4;
                if (rychlost == 0) rychlost = 1;
            }
            else Trace.WriteLine(e.Key);
        }

        private void Engine(object sender, EventArgs e)
        {
            pticku++;
            PohybJednotek();
            LoopPresPlanety();

            if (pticku % (72/rychlost) == 0)
            {
                hrac.TvorbaJednotek(); //tvori jedntoky na planetach hrace
                foreach (Bot bot in boti)
                {
                    bot.TvorbaJednotek();
                    
                }

            }
            if (pticku % (432 / rychlost) == 0) //spusti ai
            {
                
                foreach (Planeta planeta in planety)
                {
                    if (planeta is null) break;
                    Zamichej(planeta.sousedniplanety);
                }
                
                foreach (Bot bot in boti)
                {
                    if (bot is null) break;
                    Spoctiohrozeni();
                    bot.AI();
                }

            }
        }

        private void Start()
        {
            rand = new Random();

            if (lvl == 0)
            {
                planety = new Planeta[20];   //Vytvoreni planet
                int pocetplanet = 4;
                int pocetbotu = 2;
                hrac = new Hrac(myCanvas, planety, 1);
                boti = new Bot[pocetbotu];
                for (int i = 0; i < pocetbotu; i++)
                {
                    boti[i] = new Bot(myCanvas, planety, i + 2);
                }


                velikost = 60;
                planety[0] = new Planeta(myCanvas, 590, 380, velikost, 0, 1, 2, null);
                planety[1] = new Planeta(myCanvas, 300, 180, velikost, 1, 1, 2, hrac);
                planety[2] = new Planeta(myCanvas, 880, 180, velikost, 2, 1, 2, boti[0]);
                planety[3] = new Planeta(myCanvas, 590, 680, velikost, 3, 1, 2, boti[1]);

                planety[0].sousedniplanety = new[] { planety[1], planety[2], planety[3] };
                planety[1].sousedniplanety = new[] { planety[0], planety[2], planety[3] };
                planety[2].sousedniplanety = new[] { planety[1], planety[0], planety[3] };
                planety[3].sousedniplanety = new[] { planety[1], planety[2], planety[0] };

                /*
                planety[4] = new Planeta(myCanvas, 390, 680, velikost, 3, 2, 2, boti[1]);
                planety[5] = new Planeta(myCanvas, 190, 680, velikost, 3, 2, 2, boti[1]);
                planety[6] = new Planeta(myCanvas, 790, 680, velikost, 3, 2, 2, boti[1]);
                planety[7] = new Planeta(myCanvas, 990, 680, velikost, 3, 2, 2, boti[1]);
                */
            }


            if (lvl == 1)
            {
                planety = new Planeta[20];   //Vytvoreni planet
                int pocetplanet = 4;
                int pocetbotu = 2;
                hrac = new Hrac(myCanvas, planety, 1);
                boti = new Bot[pocetbotu];
                for (int i = 0; i < pocetbotu; i++)
                {
                    boti[i] = new Bot(myCanvas, planety, i + 2);
                }


                velikost = 50;
                planety[0] = new Planeta(myCanvas, 590, 360, velikost, 0, 1, 2, null);
                planety[1] = new Planeta(myCanvas, 590 - 250, 360 - 144, velikost, 1, 1, 2, hrac);
                planety[2] = new Planeta(myCanvas, 590 + 250, 360 - 144, velikost, 2, 1, 2, boti[0]); //zelenej
                planety[3] = new Planeta(myCanvas, 590, 360 + 288, velikost, 3, 1, 2, boti[1]);
                planety[4] = new Planeta(myCanvas, 590, 360 - 288, velikost, 0, 1, 2, null);
                planety[5] = new Planeta(myCanvas, 590 + 250, 360 + 144, velikost, 0, 1, 2, null);
                planety[6] = new Planeta(myCanvas, 590 - 250, 360 + 144, velikost, 0, 1, 2, null);


                planety[0].sousedniplanety = new[] { planety[1], planety[2], planety[3], planety[4], planety[5], planety[6] };
                planety[1].sousedniplanety = new[] { planety[0], planety[2], planety[3], planety[4], planety[6] };
                planety[2].sousedniplanety = new[] { planety[1], planety[0], planety[3], planety[4], planety[5] };
                planety[3].sousedniplanety = new[] { planety[1], planety[2], planety[0], planety[5], planety[6] };
                planety[4].sousedniplanety = new[] { planety[1], planety[2], planety[0] };
                planety[5].sousedniplanety = new[] { planety[3], planety[2], planety[0] };
                planety[6].sousedniplanety = new[] { planety[1], planety[3], planety[0] };

                /*
                planety[4] = new Planeta(myCanvas, 390, 680, velikost, 3, 2, 2, boti[1]);
                planety[5] = new Planeta(myCanvas, 190, 680, velikost, 3, 2, 2, boti[1]);
                planety[6] = new Planeta(myCanvas, 790, 680, velikost, 3, 2, 2, boti[1]);
                planety[7] = new Planeta(myCanvas, 990, 680, velikost, 3, 2, 2, boti[1]);
                */
            }


            Timer.Start();
        }

        private void PohybJednotek() //pohyb po obezne draze, pohyb mezi a chovani pri dosahu cile
        {

            foreach (Jednotka unit in hrac.jednotky) //Pohyb jendotek hrace
            {
                if (unit == null) break;
                unit.PosunLetu(rychlost);
            }
            hrac.VysypatOdpadky();
            



            foreach (Bot bot in boti) //Pohyb jendotek botu
            {
                foreach (Jednotka unit in bot.jednotky)
                {
                    if (unit == null) break;
                    unit.PosunLetu(rychlost);
                }
                bot.VysypatOdpadky();
            }
        }

        private void LoopPresPlanety()
        {
            foreach (Planeta planeta in planety)
            {
                if (planeta == null) return;
                if (planeta.majitel >= 1 || (planeta.majitel == 0 && planeta.drzitel != 0))
                {
                    planeta.InfoVypispjednotek(666);
                    if (planeta.zdravi < 70 && planeta.majitel > 1) planeta.Lecit();
                }
            }
        }

        public void Spoctiohrozeni()
        {
            foreach (Planeta planeta in planety)
            {
                int kolikohrozuje = 0;
                if (planeta is null) break;
                foreach (Planeta soused in planeta.sousedniplanety)
                {
                    if (soused.majitel != planeta.majitel) kolikohrozuje += soused.pjednotek;
                }
                planeta.miraohrozeni = kolikohrozuje;
            }
        }

        public void Zamichej<T>(T[] pole)
        {
            if (pole == null) return;
            int n = pole.Length;
            while (n > 1)
            {
                int k = rand.Next(n--);
                T temp = pole[n];
                pole[n] = pole[k];
                pole[k] = temp;
            }
        }

    }
    
    
    public abstract class Player
    {
        protected Planeta[] planety;
        public List<Jednotka> jednotky;
        public List<Jednotka> odpadky;
        Canvas myCanvas;
        public int celkemunits;
        public int id;
        public Random rand;
        public Player(Canvas myCanvas, Planeta[] planety, int id)
        {
            this.myCanvas = myCanvas;
            this.planety = planety;
            this.id = id;
            odpadky = new List<Jednotka>();
            rand = new Random();
            //jednotky = new Jednotka[1000];
            jednotky = new List<Jednotka>();
            celkemunits = 0;
        }

        public void TvorbaJednotek()
        {
            foreach (Planeta planeta in planety)
            {
                if (planeta == null) break;
                if (planeta.majitel == id && planeta.pjednotek < planeta.level*100)
                {
                    for (int i = 0; i < planeta.level; i++)
                    {
                        jednotky.Add(new Jednotka(myCanvas, planeta, planeta.majitel, planeta.px + planeta.velikost / 2 + 25, planeta.py + planeta.velikost / 2 + 25, planeta.px, planeta.py, rand));
                        planeta.VytvorJednotku(jednotky.Last());
                        celkemunits++;
                    }
                }
            }
        }

        public void VysypatOdpadky()
        {
            foreach(Jednotka opadek in odpadky)
            {
                //Trace.WriteLine(jednotky.Remove(opadek));
                jednotky.Remove(opadek);
                
            }
            odpadky.Clear();
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

        public void AI()
        {
            foreach (Planeta planeta in planety) //cizi planety; 
            {
                if (planeta is null) break;
                if (planeta.majitel == id || planeta.majitel == 0) continue; //planeta => bude cizi

                int kolikmamokolojednotek = 0;
                int kolikseneposlalo = 0;
                List<Planeta> mojeplanetyokolo = new List<Planeta>();
                foreach (Planeta soused in planeta.sousedniplanety)
                {
                    if (soused.majitel == id)
                    {
                        kolikmamokolojednotek += soused.pjednotek;
                        mojeplanetyokolo.Add(soused);
                    }
                    if (kolikmamokolojednotek > planeta.pjednotek+ 40 + 25*planeta.level - planeta.zdravi/5 && rand.Next(100)>50)
                    {
                        
                        foreach (Planeta mojeplaneta in mojeplanetyokolo)
                        {
                            kolikseneposlalo += mojeplaneta.OdesliJednotky(((planeta.pjednotek + 40 + 25 * planeta.level) / mojeplanetyokolo.Count()) + kolikseneposlalo, planeta);
                        }
                    }
                }
            }


            foreach (Planeta planeta in planety) // moje 
            {
                if (planeta is null) break;
                if (planeta.majitel != id) continue;
                if (planeta.zdravi < 80*planeta.level) planeta.Lecit();




                foreach (Planeta soused in planeta.sousedniplanety)
                {
                    int znamenko = 1;
                    if (soused.majitel == 0 && soused.drzitel == id) znamenko = -1;
                    if (soused.majitel == 0 && planeta.pjednotek > znamenko*(soused.kontrola / 5) + 20 && (rand.Next(100) > 50 || planeta.level == planeta.maxlevel)) //soused je 0, zabirani neobsazeny
                    {
                        planeta.OdesliJednotky(soused.kontrola / 5 + 20 + rand.Next(5), soused);
                    }
                    else if (planeta.pjednotek > 20*planeta.level && planeta.level != planeta.maxlevel && rand.Next(100) > 15) planeta.Upgrade();

                    else if (soused.majitel == id && rand.Next(100) > 10) //soused jsem ja
                    {
                        if (planeta.miraohrozeni < soused.miraohrozeni || soused.zdravi < planeta.zdravi && planeta.pjednotek>20)
                        {
                            planeta.OdesliJednotky(planeta.pjednotek / 2, soused);
                        }
                        if (planeta.miraohrozeni == soused.miraohrozeni && (planeta.pjednotek - soused.pjednotek)>10)
                        {
                            planeta.OdesliJednotky((planeta.pjednotek - soused.pjednotek) / 2, soused);
                        }
                    }
                }
            }
        }

        
    }



    public class Planeta
    {
        public int majitel;
        public int drzitel;
        public Player player;
        public int zdravi;
        public int pjednotek;
        public int jednotekcobude;
        public int velikost;
        public int level;
        public int kontrola;
        public int maxlevel;
        public bool vybrana; //planeta je vybrana kliknutim
        public int px;
        public int py;
        public Label info; //info o statusu planety
        public Ellipse gplaneta; // graficka instance planety
        //Canvas myCanvas;
        public Jednotka[] jednotky;

        public Planeta[] sousedniplanety;
        public int miraohrozeni;

        public Planeta(Canvas myCanvas, int px, int py, int velikost, int majitel, int level, int maxlevel, Player player)
        {
            this.px = px;
            this.py = py;
            this.majitel = majitel;
            //this.myCanvas = myCanvas;
            this.velikost = velikost;
            this.level = level;
            this.maxlevel = maxlevel;
            this.player = player;
            zdravi = 100*level;
            pjednotek = 0;
            miraohrozeni = 0;
            jednotky = new Jednotka[100*maxlevel+50];

            gplaneta = new Ellipse
            {
                Tag = "planeta",
                Height = velikost,
                Width = velikost,
                Fill = NavratBarvy.Navrat(majitel),
                Stroke = NavratBarvy.Navrat(12),
                StrokeThickness = 0
            };
            Canvas.SetTop(gplaneta, py - velikost / 2);
            Canvas.SetLeft(gplaneta, px - velikost / 2);
            myCanvas.Children.Add(gplaneta);

            info = new Label
            {
                Tag = "txt",
                Content = "",
                Foreground = NavratBarvy.Navrat(majitel)
            };
            Canvas.SetTop(info, py-15);
            Canvas.SetLeft(info, px-30);
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

        public void ZabijJednotku()
        {
            Jednotka cozabijim = jednotky[pjednotek - 1];
            cozabijim.SmazZCanvasu();
            jednotky[pjednotek - 1] = null;
            pjednotek--;
            player.celkemunits--;
            player.jednotky.Remove(cozabijim);
        }

        public void VyberseGraficky() //vytvori graficky kruh aby se planeta oznacila
        {

            gplaneta.StrokeThickness = 5;
            //Canvas.SetZIndex(info, 1); //MEMORY LEAK, debilni microsoft????
        }
        
        public void OdeberseGraficky()//odebere kruh 
        {
            gplaneta.StrokeThickness = 0;
        }

        public void InfoVypisPocetVyberu(int kolik) //vypise kolik je vybrano jednotek
        {
            info.Foreground = NavratBarvy.Navrat(666);
            info.Content = $"Vybrano:\n    {kolik}";
        }

        public void InfoVypispjednotek(int barva)
        {
            info.Foreground = NavratBarvy.Navrat(barva);
            if (zdravi == 100*level && majitel != 0 && !vybrana) info.Content = $" Units: {pjednotek}";
            else if (majitel != 0 && !vybrana) info.Content = $" Units: {pjednotek} \n Health: {zdravi/level}%";
            else if (majitel == 0 && kontrola > 0)
            {
                info.Foreground = NavratBarvy.Navrat(drzitel);
                info.Content = $"Obsazeno:\n {kontrola}%";
            }
            //else if (majitel == 1 && vybrana == false) info.Content = $" Units: {pjednotek}";
        }

        public void InfoSmazat() //odstrani zobrazene info at uz je jakykoliv
        {
            info.Content = "x";
            info.Foreground = NavratBarvy.Navrat(majitel);
        }

        public void UberZdravi(int okolik)
        {
            zdravi -= okolik;
            if (zdravi < 1)
            {
                zdravi = 0;
                majitel = 0;
                drzitel = 0;
                kontrola = 0;
                gplaneta.Fill = NavratBarvy.Navrat(0);
                InfoSmazat();
            }
        }

        public void ObsazovaniPrazdne(int okolik, Player utocnik)
        {
            kontrola += okolik; 
            drzitel = utocnik.id;
            player = utocnik;
            //Trace.WriteLine($"jo {drzitel} {majitel} {utocnik.id} {utocnik.jednotky.Count()}");
        }

        public void ObsazovaniVlastni(int okolik)
        {
            kontrola += okolik;
            if (kontrola >= 100)
            {
                majitel = drzitel;
                zdravi = 100*level;
                gplaneta.Fill = NavratBarvy.Navrat(majitel);
                InfoSmazat();
            }
        }

        public void ObsazovaniCizi(int okolik)
        {
            kontrola -= okolik;
            if (kontrola <= 0)
            {
                velikost = velikost - 20 * (level - 1);
                gplaneta.Width = velikost;
                gplaneta.Height = velikost;
                zdravi = 0;
                majitel = 0;
                drzitel = 0;
                kontrola = 0;
                level = 1;
                gplaneta.Fill = NavratBarvy.Navrat(0);
                InfoSmazat();
                Canvas.SetTop(gplaneta, py - velikost / 2);
                Canvas.SetLeft(gplaneta, px - velikost / 2);
            }
        }

        public int OdesliJednotky(int kolik, Planeta planetakam)
        {
            int puvodnekolik = kolik;
            if (planetakam.majitel == majitel)
            {
                kolik = Math.Min(kolik, planetakam.level * 100 - (planetakam.jednotekcobude + planetakam.pjednotek));
                planetakam.jednotekcobude += kolik;
            }
            kolik = Math.Min(kolik, pjednotek);
            for (int i = 0; i < kolik; i++)
            {
                pjednotek--; //snizim pocet jednotek
                jednotky[pjednotek].PoslatNaCestu(planetakam.px, planetakam.py, planetakam); //poslu jednotku
                jednotky[pjednotek] = null;
            }
            vybrana = false;
            return puvodnekolik-kolik;
        }

        public void Lecit()
        {
            int kolikmajednotek = pjednotek;
            for (int i=0; i<kolikmajednotek; i++)
            {
                if (zdravi < 100*level)
                {
                    ZabijJednotku();
                    zdravi += 5;
                }
                if (zdravi >= 100*level)
                {
                    zdravi = 100*level;
                    return;
                }
            }
        }

        public void Upgrade()
        {
            if (level != maxlevel && pjednotek>20*level && zdravi == 100*level)
            {
                level++;
                velikost += 20;
                zdravi = 100 * level;
                
                gplaneta.Height = velikost;
                gplaneta.Width = velikost;
                Canvas.SetTop(gplaneta, py - velikost / 2);
                Canvas.SetLeft(gplaneta, px - velikost / 2);
                
                for (int i = 0; i<20*(level-1); i++)
                {
                    ZabijJednotku();
                }

                //vybere nahodne jakej orbit bude mit
                foreach(Jednotka unit in jednotky)
                {
                    if (unit is null) break;
                    unit.NahodnyOrbit();
                }
                

            }
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
        public int majitel;
        private float orb1;
        private float orb2;
        public Random rand;
        public double radian;
        public Player player;
        public Polygon gjednotka;
        public Planeta planetakamletim;
        public Planeta domovskaplaneta;
        private Canvas myCanvas;

        public Jednotka(Canvas myCanvas, Planeta planeta, int majitel, int px, int py, int stredx, int stredy, Random rand)
        {

            this.px = stredx;
            this.py = py;
            this.stredx = stredx;
            this.stredy = stredy;
            this.majitel = majitel;
            this.player = planeta.player;
            this.obiha = true;
            domovskaplaneta = planeta;
            this.rand = rand;
            this.radian = (double)rand.Next(10);
            this.myCanvas = myCanvas;

            if (domovskaplaneta.level == 1)
            {
                orb1 = 1.1f;
                orb2 = 0.69f;
            }
            else NahodnyOrbit();

            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point(0, 0));
            myPointCollection.Add(new Point(0, 1));
            myPointCollection.Add(new Point(1, 1));

            gjednotka = new Polygon();
            gjednotka.Points = myPointCollection;
            gjednotka.Fill = NavratBarvy.Navrat(majitel);
            gjednotka.Stretch = Stretch.Fill;
            gjednotka.Width = 10;
            gjednotka.Height = 10;
            Canvas.SetTop(gjednotka, stredy);
            Canvas.SetLeft(gjednotka, stredx);
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

        public void PoslatNaCestuNaPlanetu(Planeta planetakam)
        {
            PoslatNaCestu(planetakam.px, planetakam.py, planetakam);
        }

        public void SmazZCanvasu() //smaze ji z platna
        {
            myCanvas.Children.Remove(gjednotka);
        }

        public void PosunLetu(int rychlost)
        {
            double rychlostrotace = 0.02 * rychlost;
            int velikost = domovskaplaneta.velikost;
            if (obiha == true) //jednotka obiha
            {
                
                radian += rychlostrotace;
                //double sin = Math.Sin(radian);
                px = (stredx + Math.Cos(radian) * (velikost/2+25)*orb1 - 5);
                py = (stredy + Math.Sin(radian) * (velikost/2+25)*orb2 - 5);
                if (false) //bugr s pameti - Alokace sileneho mnozstvi - nuti volat GC - je to pomaly?????? wtf
                {
                    gjednotka.Height = 10 + Math.Sin(radian) * 1.3;
                    gjednotka.Width = 10 + Math.Sin(radian) * 1.3;
                }
                
                Canvas.SetTop(gjednotka, py);
                Canvas.SetLeft(gjednotka, px);
                
            }
            else if (naceste == true) //Jednotka je na ceste
            {
                //double eukleides = Math.Sqrt((px - kamx) * (px - kamx) + (py - kamy) * (py - kamy));
                px = (px + ((kamx - px) / Math.Sqrt((px - kamx) * (px - kamx) + (py - kamy) * (py - kamy))) *rychlost);
                py = (py + ((kamy - py) / Math.Sqrt((px - kamx) * (px - kamx) + (py - kamy) * (py - kamy))) *rychlost);
                Canvas.SetTop(gjednotka, py);
                Canvas.SetLeft(gjednotka, px);
                SrazkaSPlanetou();
            }
        }

        public void SrazkaSPlanetou() //testuje jestli se jednotka nesrazila s planetou
        {
            double vzdalenostodplanety = Math.Sqrt((px - planetakamletim.px) * (px - planetakamletim.px) + (py - planetakamletim.py) * (py - planetakamletim.py)); //eukleidovska vzdalenost

            if (vzdalenostodplanety < planetakamletim.velikost/2 + 30 && planetakamletim.majitel == majitel) //jednotka se dostala do orbitu cilove vlastni planety
            {
                planetakamletim.jednotky[planetakamletim.pjednotek] = this;
                planetakamletim.pjednotek++;
                planetakamletim.jednotekcobude--;
                obiha = true;
                naceste = false;
                //unit.px = unit.planetakamletim.px + unit.planetakamletim.velikost / 2 + 15;
                //unit.py = unit.planetakamletim.py + unit.planetakamletim.velikost / 2 + 15;
                stredx = planetakamletim.px;
                stredy = planetakamletim.py;
                domovskaplaneta = planetakamletim;

                //upravit parametry orbitu
                if (domovskaplaneta.level == 1)
                {
                    orb1 = 1.1f;
                    orb2 = 0.69f;
                }
                else NahodnyOrbit();

            }

            if (vzdalenostodplanety < planetakamletim.velikost/2 + 25 && planetakamletim.majitel != majitel && planetakamletim.majitel != 0) //jednotka se dostala do orbitu cizi planety
            {
                if (planetakamletim.pjednotek > 0) //ma jednotky
                {
                    planetakamletim.ZabijJednotku(); //smaze instanci

                    SmazZCanvasu(); //Killnu jednotku co utocila      
                    player.odpadky.Add(this);
                    px = 0;
                    py = 0;

                }
                if (planetakamletim.pjednotek == 0) //nema jednotky, zdravi zacne se ubirat
                {
                    SmazZCanvasu(); //Killnu jednotku co utocila
                    player.odpadky.Add(this);
                    px = 0;
                    py = 0;
                    planetakamletim.UberZdravi(5);

                }

            }
            if (vzdalenostodplanety < planetakamletim.velikost/2 + 5 && planetakamletim.majitel == 0 && planetakamletim.drzitel == 0) //jednotka se srazila s neobsazenou planetou
            {
                SmazZCanvasu(); //Killnu jednotku co utocila
                player.odpadky.Add(this);
                px = 0;
                py = 0;
                planetakamletim.ObsazovaniPrazdne(5, player);
            }
            if (vzdalenostodplanety < planetakamletim.velikost/2 + 5 && planetakamletim.majitel == 0 && planetakamletim.drzitel == majitel) //jednotka se srazila s neobsazenou planetou s vlastni kontrolou
            {
                SmazZCanvasu(); //Killnu jednotku co utocila
                player.odpadky.Add(this);
                px = 0;
                py = 0;
                planetakamletim.ObsazovaniVlastni(5);
            }
            if (vzdalenostodplanety < planetakamletim.velikost/2 + 5 && planetakamletim.majitel == 0 && planetakamletim.drzitel != majitel) //jednotka se srazila s neobsazenou planetou s cizi kontrolou
            {
                SmazZCanvasu(); //Killnu jednotku co utocila
                player.odpadky.Add(this);
                px = 0;
                py = 0;
                planetakamletim.ObsazovaniCizi(5);
            }
        }

        public void NahodnyOrbit()
        {
            int nahodnycislo = rand.Next(2); //2 nebo 1
            if (nahodnycislo == 1)
            {
                orb1 = 1.5f;
                orb2 = 0.7f;
            }
            else
            {
                orb1 = 1f;
                orb2 = 1.5f;
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

