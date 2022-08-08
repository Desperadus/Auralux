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
        int rychlost = 1; //rychlost hry - max hodnota je 4
        int lvl; //aktualni level - urcite si vyberem v menu
        Hrac hrac; //hrac
        int velikost; //obecna zakladni velikost
        int procent; //kolik procent jednotek je vybrano
        int kolik; //kolik jednotek je vybrano po kliknuti na planetu
        Bot[] boti; // pole s boty
        Planeta[] planety; //pole se planetama 
        Planeta vybrano; //planeta vybrana klikem
        Random rand;
        ImageBrush background = new ImageBrush();
        bool jenjednou = false; //pomocna promenna aby dana vec nastala pouze jednou - prohra
        bool vyhrajednou = false; //to samy - vyhraje clovek jen jednou - pomocna promenna
        public MainWindow(int lvl)
        {
            InitializeComponent();
            this.lvl = lvl;
            background.ImageSource = new BitmapImage(new Uri("hubblevesmir.jpeg", UriKind.Relative)); //nastavi pozadi na foto z Hublova teleskopu - POZOR mozna nutno zmenit drahu u vas
            myCanvas.Background = background;

            Timer.Tick += Engine;
            Timer.Interval = TimeSpan.FromMilliseconds(15); 
            //System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency; //BACHA - meni GC mode
            Start();
        }

        private void Klik(object sender, MouseButtonEventArgs e) //kliknuti levym tlacitkem mysi
        {
            //Trace.WriteLine(e.OriginalSource);
            if (e.OriginalSource is TextBlock || e.OriginalSource is Ellipse || e.OriginalSource is Border) //kliknuti na nejakou planetku
            {
                var souradnicekliku = e.GetPosition(this); 
                int sourx = (int)(souradnicekliku.X);
                int soury = (int)(souradnicekliku.Y);


                foreach (Planeta planeta in planety) //zjisti na jakou se kliklo v tomto cyklu
                {
                    if (planeta is null) break;
                    if (Math.Sqrt((sourx - planeta.px) * (sourx - planeta.px) + (soury - planeta.py) * (soury - planeta.py)) < planeta.velikost / 2) //hrac klikl na planetu
                    {
                        if (vybrano == planeta) //kliklo se na vybranou planetu
                        {
                            procent -= 33; //snizi procento vybranych jednotek
                            kolik = (int)(vybrano.pjednotek * ((double)procent / 100)); //prepocet poctu vybranych jednotek
                            vybrano.InfoVypisPocetVyberu(kolik); //zmeni text
                            if (procent == 1) //pokud hrac klikl na planetu po ctvrty - zruseni vyberu
                            {

                                vybrano.OdeberseGraficky();
                                vybrano.vybrana = false; //uz nebude vybrana
                                vybrano = null;
                                return;
                            }
                        }
                        if (vybrano is null && planeta.majitel == 1) //novy vyber - kliknuti na vlastni planetu, ktera neni vybrana
                        {
                            vybrano = planeta; //vybere se
                            vybrano.vybrana = true;
                            procent = 100; //hrac oznacil 100 jednotek
                            kolik = (int)(vybrano.pjednotek * ((double)procent / 100));
                            vybrano.VyberseGraficky(); //udela vyberovy kruh
                            vybrano.InfoVypisPocetVyberu(kolik);
                            return;
                        }

                        if (vybrano != planeta && vybrano is not null) //poslani jednotek na nejakou jinou planetu  
                        {
                            if (vybrano.majitel == planeta.majitel) kolik = Math.Min(kolik, planeta.level*100 - (planeta.pjednotek+planeta.jednotekcobude)); //aby to neslo pres 100/200 nebo proste hranici max kapacity
                            vybrano.OdesliJednotky(kolik, planeta); //posle jednotky
                            vybrano.OdeberseGraficky(); //odebere se graficky vyber
                            vybrano = null; 
                            return;
                            //
                        }
                    }
                }


            }
        }

        private void Unklik(object sender, MouseButtonEventArgs e) // kliknuti oravym tlacitkem - leceni 
        {
            //Trace.WriteLine(e.OriginalSource);
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
                        planeta.Lecit(); //leci se
                    }
                }


            }

        }

        private void Upgrade(object sender, MouseWheelEventArgs e) //
        {
            Trace.WriteLine(e.OriginalSource);
            if (e.OriginalSource is TextBlock || e.OriginalSource is Ellipse || e.OriginalSource is Border) //scrollnuti na nejake ellipse
            {
                var souradnicekliku = e.GetPosition(this);
                int sourx = (int)(souradnicekliku.X);
                int soury = (int)(souradnicekliku.Y);

                foreach (Planeta planeta in planety) //zjisti na jakou se scrollnulo
                {
                    if (planeta is null) break;
                    if (Math.Sqrt((sourx - planeta.px) * (sourx - planeta.px) + (soury - planeta.py) * (soury - planeta.py)) < planeta.velikost / 2 && planeta.majitel == 1) //scrollnul na svoji planetu
                    {
                        planeta.Upgrade(); //zkusi upgrade
                    }
                }

            }
        }

        private void ZmacklaKlavesa(object sender, KeyEventArgs e) //stisknuti klavesy na klavesnici
        {
            if (e.Key == Key.Add) //zmacklo ce plus
            {
                rychlost = (rychlost + 1) % 5; //zvysi se rychlost
                if (rychlost == 0) rychlost = 1; //pokud se protocil cyklus tak zpatky na jedna (1->2->3->4->1->2...)
            }
            else if (e.Key == Key.Subtract) //kliklo se na minus
            {
                rychlost = (rychlost - 1) % 5; //snizuje tady
                if (rychlost <= 0) rychlost = 1;
            }
            //else Trace.WriteLine(e.Key);
        }

        private void Engine(object sender, EventArgs e) //opakovane se vola kazdych 15ms
        {
            pticku++; //zvysi counter ticků
            PohybJednotek(); //pohne jednotkami
            LoopPresPlanety(); //udela texty se statusem na planetach

            if (pticku % (72/rychlost) == 0) //urcite doba v zavilosti na rychlosti
            {
                hrac.TvorbaJednotek(); //tvori jedntoky na planetach hrace
                foreach (Bot bot in boti)
                {
                    bot.TvorbaJednotek(); //tvorba jednotek u botu
                    
                }

            }
            if (pticku % (432 / rychlost) == 0) //spusti ai
            {
                TestVyhry(); //zjisti jestli hrac nevyhral
                foreach (Planeta planeta in planety)
                {
                    if (planeta is null) break;
                    Zamichej(planeta.sousedniplanety); //zamicha s poradim planet u botu jak je maj ulozeny - Ai bude pokazdy jina v poradi utoku
                }

                Spoctiohrozeni(); //spocita ohrozeni planet
                foreach (Bot bot in boti)
                {
                    if (bot is null) break;
                    if (bot.jednotky.Count == 0) continue;
                    bot.AI(); //zavola AI
                }
                if (hrac.jednotky.Count() == 0 && jenjednou == false) //prohra
                {
                    jenjednou = true;
                    MessageBox.Show("Prohral jsi :(");
                    rychlost = 4;
                }

            }
        }

        private void Start() //vytvori level
        {
            rand = new Random();
            
            if (lvl == 1)
            {
                planety = new Planeta[8];   //Vytvoreni planet
                int pocetbotu = 2; //kolik botu bude hrat
                hrac = new Hrac(myCanvas, planety, 1); //hrac
                boti = new Bot[pocetbotu]; //pole botu
                for (int i = 0; i < pocetbotu; i++)
                {
                    boti[i] = new Bot(myCanvas, planety, i + 2); //prida boty do pole - jejich id je o 2 vetsi nez jejich pozice v poli
                }

                
                velikost = 60; //zakladni velikost planet
                //tady se tvori planety s jejich pozici, velikosti, id majitele a botem/hracem
                planety[0] = new Planeta(myCanvas, 200, 350, velikost, 1, 1, 2, hrac); 
                planety[1] = new Planeta(myCanvas, 200 + 200, 350, velikost, 0, 1, 2, null);

                planety[2] = new Planeta(myCanvas, 400 + 173, 250, velikost, 0, 1, 2, null);
                planety[3] = new Planeta(myCanvas, 600 + 173, 250, velikost, 0, 1, 2, null);
                planety[4] = new Planeta(myCanvas, 800 + 173, 250, velikost, 2, 1, 2, boti[0]);

                planety[5] = new Planeta(myCanvas, 400 + 173, 450, velikost, 0, 1, 2, null);
                planety[6] = new Planeta(myCanvas, 600 + 173, 450, velikost, 0, 1, 2, null);
                planety[7] = new Planeta(myCanvas, 800 + 173, 450, velikost, 3, 1, 2, boti[1]);

                //vytvori seznam sousedu - graf ktery vyuziva AI
                planety[0].sousedniplanety = new[] { planety[1] };
                planety[1].sousedniplanety = new[] { planety[0], planety[2], planety[3], planety[5], planety[6] };
                planety[2].sousedniplanety = new[] { planety[1], planety[5], planety[6], planety[3] };
                planety[3].sousedniplanety = new[] { planety[1], planety[2], planety[4], planety[5], planety[6], planety[7] };
                planety[4].sousedniplanety = new[] { planety[3], planety[6], planety[7], planety[6] };
                planety[5].sousedniplanety = new[] { planety[1], planety[2], planety[6], planety[3] };
                planety[6].sousedniplanety = new[] { planety[1], planety[2], planety[4], planety[5], planety[3], planety[7] };
                planety[7].sousedniplanety = new[] { planety[3], planety[6], planety[4], planety[6] };
            }

            if (lvl == 2)
            {
                planety = new Planeta[7];   //Vytvoreni planet
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

            }
            
            if (lvl == 3)
            {
                planety = new Planeta[10];   //Vytvoreni planet
                int pocetbotu = 2;
                hrac = new Hrac(myCanvas, planety, 1);
                boti = new Bot[pocetbotu];
                for (int i = 0; i < pocetbotu; i++)
                {
                    boti[i] = new Bot(myCanvas, planety, i + 2);
                }


                velikost = 50;
                planety[0] = new Planeta(myCanvas, 300, 125, velikost, 1, 1, 2, hrac);
                planety[1] = new Planeta(myCanvas, 300+100, 125+173, velikost, 0, 1, 2, null);
                planety[2] = new Planeta(myCanvas, 300 + 100*2, 125 + 173*2, velikost, 0, 1, 2, null);
                planety[3] = new Planeta(myCanvas, 300 + 100*3 , 125 + 173*3, velikost, 2, 1, 2, boti[0]);

                planety[4] = new Planeta(myCanvas, 300 + 200, 125, velikost, 0, 1, 2, null);
                planety[5] = new Planeta(myCanvas, 300 + 300, 125+173, velikost, 0, 1, 2, null);
                planety[6] = new Planeta(myCanvas, 300 + 400, 125+173*2, velikost, 0, 1, 2, null);

                planety[7] = new Planeta(myCanvas, 300 + 400, 125, velikost, 0, 1, 2, null);
                planety[8] = new Planeta(myCanvas, 300 + 500, 125+173, velikost, 0, 1, 2, null);

                planety[9] = new Planeta(myCanvas, 300 + 600, 125, velikost, 3, 1, 2, boti[1]);

                planety[0].sousedniplanety = new[] { planety[1], planety[4]};
                planety[1].sousedniplanety = new[] { planety[1], planety[4], planety[5], planety[2], planety[7], planety[6] };
                planety[2].sousedniplanety = new[] { planety[1], planety[3], planety[5], planety[2], planety[4], planety[8] };
                planety[3].sousedniplanety = new[] { planety[2], planety[6] };
                planety[4].sousedniplanety = new[] { planety[0], planety[7], planety[1], planety[5], planety[2], planety[8] };
                planety[5].sousedniplanety = new[] { planety[0], planety[1], planety[2], planety[4], planety[6], planety[7], planety[8], planety[9] };
                planety[6].sousedniplanety = new[] { planety[2], planety[3], planety[1], planety[5], planety[7], planety[8] };
                planety[7].sousedniplanety = new[] { planety[6], planety[4], planety[9], planety[5], planety[1], planety[8] };
                planety[8].sousedniplanety = new[] { planety[6], planety[4], planety[9], planety[5], planety[7], planety[2] };
                planety[9].sousedniplanety = new[] { planety[7], planety[8] };

            }

            if (lvl == 4)
            {
                planety = new Planeta[15];   //Vytvoreni planet
                int pocetbotu = 3;
                hrac = new Hrac(myCanvas, planety, 1);
                boti = new Bot[pocetbotu];
                for (int i = 0; i < pocetbotu; i++)
                {
                    boti[i] = new Bot(myCanvas, planety, i + 2);
                }


                velikost = 50;
                planety[1] = new Planeta(myCanvas, 225, 250, velikost, 0, 1, 2, null);
                planety[6] = new Planeta(myCanvas, 225+250*1, 250, velikost, 0, 1, 2, null);
                planety[13] = new Planeta(myCanvas, 225 + 250 * 2, 250, velikost, 0, 1, 2, null);
                planety[8] = new Planeta(myCanvas, 225 + 250 * 3, 250, velikost, 0, 1, 2, null);

                planety[3] = new Planeta(myCanvas, 225, 450, velikost, 0, 1, 2, null);
                planety[5] = new Planeta(myCanvas, 225 + 250 * 1, 450, velikost, 0, 1, 2, null);
                planety[12] = new Planeta(myCanvas, 225 + 250 * 2, 450, velikost, 0, 1, 2, null);
                planety[10] = new Planeta(myCanvas, 225 + 250 * 3, 450, velikost, 0, 1, 2, null);

                planety[0] = new Planeta(myCanvas, 225+125, 250-100, velikost, 2, 1, 2, boti[0]);
                planety[7] = new Planeta(myCanvas, 225 + 125+500, 250 - 100, velikost, 3, 1, 2, boti[1]);
                planety[4] = new Planeta(myCanvas, 225 + 125, 450 + 100, velikost, 1, 1, 2, hrac);
                planety[11] = new Planeta(myCanvas, 225 + 125 + 500, 450 + 100, velikost, 4, 1, 2, boti[2]);

                planety[2] = new Planeta(myCanvas, 225 - 100, 250 + 100, velikost, 0, 1, 2, null);
                planety[14] = new Planeta(myCanvas, 225 +375, 250 + 100, velikost, 0, 1, 2, null);
                planety[9] = new Planeta(myCanvas, 225 + 850, 250 + 100, velikost, 0, 1, 2, null);

                planety[0].sousedniplanety = new[] { planety[0], planety[1], planety[3], planety[4], planety[5], planety[6] };
                planety[1].sousedniplanety = new[] { planety[0], planety[2], planety[3], planety[4], planety[5], planety[6], planety[14] };
                planety[2].sousedniplanety = new[] { planety[1], planety[2], planety[3], planety[5], planety[6], planety[14] };
                planety[3].sousedniplanety = new[] { planety[0], planety[1], planety[2], planety[4], planety[5], planety[6], planety[14] };
                planety[4].sousedniplanety = new[] { planety[0], planety[1], planety[3], planety[4], planety[5], planety[6] };
                planety[5].sousedniplanety = new[] { planety[0], planety[1], planety[2], planety[3], planety[4], planety[6], planety[14], planety[12] };
                planety[6].sousedniplanety = new[] { planety[0], planety[1], planety[2], planety[3], planety[4], planety[5], planety[14], planety[13] };

                planety[0+7].sousedniplanety = new[] { planety[0+7], planety[1+7], planety[3+7], planety[4+7], planety[5+7], planety[6+7] };
                planety[1+7].sousedniplanety = new[] { planety[0+7], planety[2+7], planety[3+7], planety[4+7], planety[5+7], planety[6+7], planety[14] };
                planety[2+7].sousedniplanety = new[] { planety[1+7], planety[2+7], planety[3+7], planety[5+7], planety[6+7], planety[14] };
                planety[3+7].sousedniplanety = new[] { planety[0+7], planety[1+7], planety[2+7], planety[4+7], planety[5+7], planety[6+7], planety[14] };
                planety[4+7].sousedniplanety = new[] { planety[0+7], planety[1+7], planety[3+7], planety[4+7], planety[5+7], planety[6+7] };
                planety[5+7].sousedniplanety = new[] { planety[0+7], planety[1+7], planety[2+7], planety[3+7], planety[4+7], planety[6+7], planety[14], planety[5] };
                planety[6+7].sousedniplanety = new[] { planety[0+7], planety[1+7], planety[2+7], planety[3+7], planety[4+7], planety[5+7], planety[14], planety[6] };

                planety[14].sousedniplanety = new[] { planety[6], planety[13], planety[5], planety[12] };
            }

            if (lvl == 5)
            {
                planety = new Planeta[10];   //Vytvoreni planet
                int pocetplanet = 4;
                int pocetbotu = 2;
                hrac = new Hrac(myCanvas, planety, 1);
                boti = new Bot[pocetbotu];
                for (int i = 0; i < pocetbotu; i++)
                {
                    boti[i] = new Bot(myCanvas, planety, i + 2);
                }


                velikost = 55;
                planety[0] = new Planeta(myCanvas, 590, 380, velikost, 0, 1, 2, null);
                planety[1] = new Planeta(myCanvas, 590-173, 280, velikost, 1, 1, 2, hrac);
                planety[2] = new Planeta(myCanvas, 590+173, 280, velikost, 2, 1, 2, boti[0]);
                planety[3] = new Planeta(myCanvas, 590, 580, velikost, 3, 1, 2, boti[1]);
                
                planety[4] = new Planeta(myCanvas, 590 - 173+30, 280-200, velikost, 0, 1, 2, null);
                planety[8] = new Planeta(myCanvas, 402, 654, velikost, 0, 1, 2, null);
                planety[6] = new Planeta(myCanvas, 921, 406, velikost, 0, 1, 2, null);

                planety[5] = new Planeta(myCanvas, 590 + 173-30, 280-200, velikost, 0, 1, 2, null);
                planety[9] = new Planeta(myCanvas, 259, 406, velikost, 0, 1, 2, null);
                planety[7] = new Planeta(myCanvas, 778, 654, velikost, 0, 1, 2, null);

                planety[0].sousedniplanety = new[] { planety[1], planety[2], planety[3], planety[4], planety[5], planety[6], planety[7], planety[8], planety[9] };
                planety[1].sousedniplanety = new[] { planety[0], planety[2], planety[3], planety[4], planety[5], planety[9], planety[8] };
                planety[2].sousedniplanety = new[] { planety[1], planety[0], planety[3], planety[4], planety[5], planety[6], planety[7] };
                planety[3].sousedniplanety = new[] { planety[1], planety[2], planety[0], planety[9], planety[8], planety[7], planety[6] };
                
                planety[4].sousedniplanety = new[] { planety[1], planety[2], planety[5], planety[9], planety[0] };
                planety[5].sousedniplanety = new[] { planety[1], planety[4], planety[2], planety[6], planety[0] };
                planety[6].sousedniplanety = new[] { planety[2], planety[3], planety[7], planety[5], planety[0] };
                planety[7].sousedniplanety = new[] { planety[2], planety[3], planety[8], planety[6], planety[0] };
                planety[8].sousedniplanety = new[] { planety[3], planety[1], planety[9], planety[7], planety[0] };
                planety[9].sousedniplanety = new[] { planety[3], planety[1], planety[8], planety[4], planety[0] };
            }




            Timer.Start(); //odstartuje hru - spusti se Timer
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
                bot.VysypatOdpadky(); //sype odpadky - jednotky co uz jsou mrtvy jsou odstraneny
            }
        }

        private void LoopPresPlanety()
        {
            foreach (Planeta planeta in planety)
            {
                if (planeta == null) return;
                if (planeta.majitel >= 1 || (planeta.majitel == 0 && planeta.drzitel != 0))
                {
                    planeta.InfoVypispjednotek(666); //666 spusti default v switchi pro barvu a to je cervena
                }
            }
        }

        private void Spoctiohrozeni() //spocita pro kazdou planetu kolik nepratelskych jednotek je okolo ni na zaklade grafu hry
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

        private void Zamichej<T>(T[] pole) //nahodne zamicha prvky v poli - Knuthuv algoritmus
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

        private void TestVyhry() //zjisti jestli hrac nevyhral
        {
            if (vyhrajednou) return; //uz vyhral - return
            foreach (Bot bot in boti) //pokud neajaky bot ma vic jak 0 jednotek hra neskoncila - return
            {
                if (bot is null) break;
                if (bot.jednotky.Count() > 0) return;
            }
            vyhrajednou = true; //aby to bylo jen jednou viz vyse
            MessageBox.Show("Vyhral jsi!!!"); //vypise ze vyhral

            
        }

    }
    
    
    public abstract class Player //odtud dedi hrac i boti
    {
        protected Planeta[] planety; //reference na planety vytvoreny pri startu
        public List<Jednotka> jednotky; //pole jednotek co dany player ma
        public List<Jednotka> odpadky; //jednotky co umrely a musi se smazat
        Canvas myCanvas; //ref na canvas
        public int celkemunits; //kolik ma celkem jednotek
        public int id; //proste id
        public Random rand; //rng
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

        public void TvorbaJednotek() //vytvoreni jednotek na planetach co vlastni
        {
            foreach (Planeta planeta in planety)
            {
                if (planeta == null) break;
                if (planeta.majitel == id && planeta.pjednotek < planeta.level*100) //planeta je majitele a nema jeste max jednotek
                {
                    for (int i = 0; i < planeta.level; i++)
                    {
                        jednotky.Add(new Jednotka(myCanvas, planeta, planeta.majitel, planeta.px + planeta.velikost / 2 + 25, planeta.py + planeta.velikost / 2 + 25, planeta.px, planeta.py, rand));
                        planeta.VytvorJednotku(jednotky.Last()); //prida vyse vytvorenou jednotku do seznamu jednotek co ma planeta
                        celkemunits++;
                    }
                }
            }
        }

        public void VysypatOdpadky() //zabiji jednotky
        {
            foreach(Jednotka opadek in odpadky)
            {
                //Trace.WriteLine(jednotky.Remove(opadek));
                jednotky.Remove(opadek); //odstrani referenci ze seznamu
                
            }
            odpadky.Clear();
        }
    }
      
    public class Hrac : Player //staci jen detit, je to samostatna trida kvuli pripadnemu rozsireni programu
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
            foreach (Planeta planeta in planety) //chceme zde cizi planety; 
            {
                if (planeta is null) break;
                if (planeta.majitel == id || planeta.majitel == 0) continue; //planeta => bude cizi

                //vice vysvetleno v programatorske dokumentaci
                int kolikmamokolojednotek = 0; //kolik ma vlastnich jednotek okolo
                int kolikseneposlalo = 0;
                List<Planeta> mojeplanetyokolo = new List<Planeta>();
                foreach (Planeta soused in planeta.sousedniplanety)
                {
                    if (soused.majitel == id)
                    {
                        kolikmamokolojednotek += soused.pjednotek;
                        mojeplanetyokolo.Add(soused);
                    }
                    if (kolikmamokolojednotek > planeta.pjednotek+ 25*planeta.level + planeta.zdravi/5 && rand.Next(100)>80) //nastane s 20% sanci jeli mozno utocit dle podminky
                    {
                        
                        foreach (Planeta mojeplaneta in mojeplanetyokolo)
                        {
                            kolikseneposlalo += mojeplaneta.OdesliJednotky(((planeta.pjednotek + 25 * planeta.level + planeta.zdravi / 5) / mojeplanetyokolo.Count()) + kolikseneposlalo, planeta);
                        }
                    }
                }
            }


            foreach (Planeta planeta in planety) // moje - planeta bude ta tohohle bota 
            {
                if (planeta is null) break;
                if (planeta.majitel != id) continue;
                if (planeta.zdravi < 80*planeta.level) planeta.Lecit(); //chce se lecit




                foreach (Planeta soused in planeta.sousedniplanety)
                {
                    int znamenko = 1;
                    if (soused.majitel == 0 && soused.drzitel == id) znamenko = -1;
                    if (soused.majitel == 0 && planeta.pjednotek > znamenko*(soused.kontrola / 5) + 20 && (rand.Next(100) > 80 || planeta.level == planeta.maxlevel)) //soused je 0, zabirani neobsazeny
                    {
                        planeta.OdesliJednotky(soused.kontrola / 5 + 20 + rand.Next(5), soused);
                    }
                    else if (planeta.pjednotek > 20*planeta.level && planeta.level != planeta.maxlevel && rand.Next(100) > 10) planeta.Upgrade(); //upgraduje

                    else if (soused.majitel == id && rand.Next(100) > 10) //soused jsem ja
                    {
                        if (planeta.miraohrozeni < soused.miraohrozeni || soused.zdravi < planeta.zdravi && planeta.pjednotek>20 && rand.Next(100) > 30) //posilani jendotek planetam s vetsim ohrozenim
                        {
                            planeta.OdesliJednotky((int)(planeta.pjednotek *(soused.miraohrozeni/ ((double)planeta.miraohrozeni+1))), soused);
                        }
                        if (planeta.miraohrozeni == soused.miraohrozeni && (planeta.pjednotek - soused.pjednotek)>10 && rand.Next(100) > 50) //balancovani poctu mezi sebou jeli ohrozeni stejny
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
        public int majitel; //majitel - ten kdo ji plne vlastni
        public int drzitel; //uvazovat drzitele ma smysl ve chvili kdyz planeta je jeste seda a nekdo ma urcitou kontrolu nad ni
        public Player player; //reference na majitele
        public int zdravi;
        public int pjednotek; //kolik ma jednotek
        public int jednotekcobude; //kolik jednotek na ni leti od jejiho majitele
        public int velikost; //velikost na canvasu
        public int level; //jaky ma level - tohle se upgraduje
        public int kontrola; //kolik jednotek na ni ma drzitel
        public int maxlevel; //jaky ma max level pri upgradovani
        public bool vybrana; //planeta je vybrana kliknutim
        public int px; //pozice souradnice x
        public int py; //y
        public Label info; //info o statusu planety
        public Ellipse gplaneta; // graficka instance planety
        //Canvas myCanvas;
        public Jednotka[] jednotky; //jeji jednotky

        public Planeta[] sousedniplanety; //pouziva bot - seznam sousedu
        public int miraohrozeni; //pouziva bot - jak moc ji ohrozujou ostatni

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

            gplaneta = new Ellipse //vytvori ji na platne
            {
                Tag = "planeta",
                Height = velikost,
                Width = velikost,
                Fill = NavratBarvy.Navrat(majitel),
                Stroke = NavratBarvy.Navrat(12), //svetle zelena - vyberovy kruh
                StrokeThickness = 0 //kruh se na na 0 kdyz neni vybrana
            };
            Canvas.SetTop(gplaneta, py - velikost / 2);
            Canvas.SetLeft(gplaneta, px - velikost / 2);
            myCanvas.Children.Add(gplaneta);

            info = new Label //status
            {
                Tag = "txt",
                Content = "",
                Foreground = NavratBarvy.Navrat(majitel)
            };
            Canvas.SetTop(info, py-15);
            Canvas.SetLeft(info, px-30);
            myCanvas.Children.Add(info);

        }

        public void VytvorJednotku(Jednotka jednotka) //prida jendotku do pole jednotek
        {
            if (pjednotek < level * 100) //hranice dokdy tvorit jednotky
            {
                jednotky[pjednotek] = jednotka;
                pjednotek++;
            }
        }

        public void ZabijJednotku() //killne jednotku
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
            //Canvas.SetZIndex(info, 1); //EXPONENCIALNI RUST PAMETI, blbej microsoft???? - obesel jsem to
        }
        
        public void OdeberseGraficky()//odebere vyberovy kruh 
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
            if (zdravi == 100*level && majitel != 0 && !vybrana) info.Content = $" Units: {pjednotek}"; //zdrava planeta
            else if (majitel != 0 && !vybrana) info.Content = $" Units: {pjednotek} \n Health: {zdravi/level}%"; //nemocna :)
            else if (majitel == 0 && kontrola > 0) //neobsazena
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

        public void UberZdravi(int okolik) //ubere zdravi - utok na cizi planetu co je obsazene
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

        public void ObsazovaniPrazdne(int okolik, Player utocnik) //zavolano kdyz jednotka dorazi na planetu co je prazdna
        {
            velikost = velikost - 20 * (level - 1); //reset do puvodni velikosti pokud byla vetsi diky levelu
            gplaneta.Width = velikost;
            gplaneta.Height = velikost;
            Canvas.SetTop(gplaneta, py - velikost / 2);
            Canvas.SetLeft(gplaneta, px - velikost / 2);
            kontrola += okolik;
            level = 1;
            drzitel = utocnik.id; //nekdo bude jeji drzitel
            player = utocnik;
            //Trace.WriteLine($"jo {drzitel} {majitel} {utocnik.id} {utocnik.jednotky.Count()}");
        }

        public void ObsazovaniVlastni(int okolik) //jednotka kdyz prilitne neobsazene planete a je drzena samou osobou
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

        public void ObsazovaniCizi(int okolik) //utok na cizi neobsazenou planetu
        {
            kontrola -= okolik;
            if (kontrola <= 0) //ztrata kontorly
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
                jednotky[pjednotek].PoslatNaCestu(planetakam.px, planetakam.py, planetakam, this); //poslu jednotku
                jednotky[pjednotek] = null;
            }
            vybrana = false;
            return puvodnekolik-kolik;
        }

        public void Lecit()
        {
            int kolikmajednotek = pjednotek;
            for (int i=0; i<kolikmajednotek; i++) //postupne leceni planetu co nejvice jednotkama
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
        public int kamx; //kam se pohybuje - x souradnice (pokud neni na obezne draze)
        public int kamy; //y
        public int id; //id
        public bool obiha; //jestli obiha planetu = true, jinak false
        public double px; //souradnice jeji
        public double py;
        public int stredx; //stred obehu
        public int stredy; 
        public int majitel;
        private float orb1; //modifikuje obeznou drahu
        private float orb2;
        public Random rand;
        public double radian; //kolik radianu vychileni na obezne draze
        public Player player; //reference na jejiho majitele
        public Polygon gjednotka; //graficka instance
        public Planeta planetakamletim; //kam leti pripadne
        public Planeta odkudletim; //odkud leti
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

            if (domovskaplaneta.level == 1) //zakladni definice obezne drahy pokud level = 1
            {
                orb1 = 1.1f;
                orb2 = 0.69f;
            }
            else NahodnyOrbit();

            //vytvori ji na platne
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

        public void PoslatNaCestu(int kamx, int kamy, Planeta planetakamletim, Planeta odkudletim) //odesle se na jinou planetu
        {
            naceste = true;
            obiha = false;
            this.kamx = kamx;
            this.kamy = kamy;
            this.planetakamletim = planetakamletim;
            this.odkudletim = odkudletim;
        }


        public void SmazZCanvasu() //smaze ji z platna
        {
            myCanvas.Children.Remove(gjednotka);
        }

        public void PosunLetu(int rychlost)
        {
            double rychlostrotace = 0.02 * rychlost; //jak rychle bude obihat / letet
            int velikost = domovskaplaneta.velikost;
            if (obiha == true) //jednotka obiha
            {
                
                radian += rychlostrotace;
                //double sin = Math.Sin(radian);
                px = (stredx + Math.Cos(radian) * (velikost/2+25)*orb1 - 5); //vypocetni souradnice x 
                py = (stredy + Math.Sin(radian) * (velikost/2+25)*orb2 - 5);
                if (false) //bugr s pameti - Alokace sileneho mnozstvi - nuti volat GC - je to pomaly?????? wtf - dano teda false ze to nepobehne
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
                if (planetakamletim.pjednotek>=planetakamletim.level*100+49)
                {
                    this.PoslatNaCestu(odkudletim.px, odkudletim.py, odkudletim, planetakamletim);
                    return;
                }

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

        public void NahodnyOrbit() //urci nahodne orbit pro jednotku co obiha planetu s levelem vetsim jak 1
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

    class NavratBarvy //vraci bravicky dle ID
    {
        public static SolidColorBrush Navrat(int majitel)
        {
            switch (majitel)
            {
                case 0:
                    return new SolidColorBrush(Colors.Gray); //neobsazena planeta
                case 1:
                    return new SolidColorBrush(Colors.Blue); //hrac
                case 2:
                    return new SolidColorBrush(Colors.Green); //pro boty
                case 3:
                    return new SolidColorBrush(Colors.Orange);
                case 4:
                    return new SolidColorBrush(Colors.Purple);
                case 11:
                    return new SolidColorBrush(Colors.Black);
                case 12:
                    return new SolidColorBrush(Colors.GreenYellow); //vyberova barva pro kruh
                default:
                    return new SolidColorBrush(Colors.Red); //zakladni barva
            }
        }
    }
}