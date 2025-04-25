//algorytm genetyczny 3 (XOR) | Jacek Domeracki | numer albumu: 173518

using System;
using System.Collections.Generic;
using System.Linq;

namespace Zadanie_3
{
    using Wej_ProbkaTyp = ValueTuple<double, double>;

    public class Neuron
    {
        public List<byte[]> lista_wag_wejsc = new List<byte[]>();
        public double wartosc_wyjscie = 0;
    }

    public class Siec_neuronowa         //warstwa nr 0 to wejścia sieci, waga nr 0 to bias neuronu
    {
        public List<Neuron>[] warstwy_neurony;

        public Siec_neuronowa(int[] licznosc_warstw, int ile_chrom)
        {
            warstwy_neurony = new List<Neuron>[licznosc_warstw.Length];
            for (int i = 0; i < licznosc_warstw.Length; i++)
            {
                warstwy_neurony[i] = new List<Neuron>();
                for (int j = 0; j < licznosc_warstw[i]; j++)
                {
                    warstwy_neurony[i].Add(new Neuron());
                    if (i == 0)
                        continue;
                    for (int k = 0; k < licznosc_warstw[i - 1] + 1; k++)        //tyle wag neuronu warstwy i, ile neuronów warstwy i-1, plus bias
                    {
                        warstwy_neurony[i][j].lista_wag_wejsc.Add(new byte[ile_chrom]);
                    }
                }
            }
        }
    }

    public class Osobnik
    {
        public Siec_neuronowa SN;
        public double wart_fun_przyst = 0;

        public Osobnik(int[] schemat_SN, int ile_chrom)
        {
            SN = new Siec_neuronowa(schemat_SN, ile_chrom);
        }

        private void Losuj_parametr(byte[] param)
        {
            Random random = new Random();
            for (int i = 0; i < param.Length; i++)
            {
                param[i] = (byte)random.Next(2);
            }
        }

        public void Losuj_wszystkie_parametry()
        {
            for (int i = 1; i < SN.warstwy_neurony.Length; i++)
            {
                for (int j = 0; j < SN.warstwy_neurony[i].Count; j++)
                {
                    for (int k = 0; k < SN.warstwy_neurony[i][j].lista_wag_wejsc.Count; k++)
                    {
                        Losuj_parametr(SN.warstwy_neurony[i][j].lista_wag_wejsc[k]);
                    }
                }
            }
        }

        private int Konwersja_bin2dec_param(byte[] param)
        {
            int param_dec = 0;
            for (int i = 0; i < param.Length; i++)
            {
                param_dec += param[i] * (int)Math.Pow(2, i);            //LSB ... MSB
            }
            return param_dec;
        }

        private double Funkcja_aktywacji(double beta, double wartosc_bez_fa)
        {
            return 1 / (1 + Math.Exp(-beta * wartosc_bez_fa));
        }

        private double Na_wyjsciu_sieci_neuronowej(Dictionary<int, double> przedzial_dyskretny, Wej_ProbkaTyp wej_probka)
        {
            SN.warstwy_neurony[0][0].wartosc_wyjscie = wej_probka.Item1;
            SN.warstwy_neurony[0][1].wartosc_wyjscie = wej_probka.Item2;            //...tyle podstawień ile liczność warstwy wejściowej

            for (int i = 1; i < SN.warstwy_neurony.Length; i++)
            {
                for (int j = 0; j < SN.warstwy_neurony[i].Count; j++)
                {
                    double sum_wart_wyj = przedzial_dyskretny[Konwersja_bin2dec_param(SN.warstwy_neurony[i][j].lista_wag_wejsc[0])];
                    for (int k = 1; k < SN.warstwy_neurony[i][j].lista_wag_wejsc.Count; k++)
                    {
                        sum_wart_wyj += SN.warstwy_neurony[i - 1][k - 1].wartosc_wyjscie * przedzial_dyskretny[Konwersja_bin2dec_param(SN.warstwy_neurony[i][j].lista_wag_wejsc[k])];
                    }
                    SN.warstwy_neurony[i][j].wartosc_wyjscie = Funkcja_aktywacji(1, sum_wart_wyj);
                }
            }
            return SN.warstwy_neurony[SN.warstwy_neurony.Length - 1][0].wartosc_wyjscie;
        }

        private double Funkcja_przystosowania(Dictionary<int, double> przedzial_dyskretny, Dictionary<Wej_ProbkaTyp, double> probki_funkcji_wewy)
        {
            double wart_fun_przyst_rob = 0;
            foreach (var probka in probki_funkcji_wewy)
            {
                wart_fun_przyst_rob += Math.Pow(probka.Value - Na_wyjsciu_sieci_neuronowej(przedzial_dyskretny, probka.Key), 2);
            }
            return Math.Round(wart_fun_przyst_rob, 6);
        }

        public void Oblicz_funkcje_przystosowania(Dictionary<int, double> przedzial_dyskretny, Dictionary<Wej_ProbkaTyp, double> probki_funkcji_wewy)
        {
            wart_fun_przyst = Funkcja_przystosowania(przedzial_dyskretny, probki_funkcji_wewy);
        }

        public void SkopiujZ(Osobnik osob_do_skopiowania, int ile_chrom)
        {
            for (int i = 0; i < SN.warstwy_neurony.Length; i++)
            {
                for (int j = 0; j < SN.warstwy_neurony[i].Count; j++)
                {
                    SN.warstwy_neurony[i][j].wartosc_wyjscie = 0;           //zerowanie neuronów, nie ma znaczenia, bo i tak zostaną przeliczone
                    if (i == 0)
                        continue;
                    for (int k = 0; k < SN.warstwy_neurony[i][j].lista_wag_wejsc.Count; k++)
                    {
                        for (int l = 0; l < ile_chrom; l++)
                        {
                            this.SN.warstwy_neurony[i][j].lista_wag_wejsc[k][l] = osob_do_skopiowania.SN.warstwy_neurony[i][j].lista_wag_wejsc[k][l];
                        }
                    }
                }
            }
            this.wart_fun_przyst = osob_do_skopiowania.wart_fun_przyst;
        }

        private void Pokaz_parametr(byte[] param)
        {
            for (int i = param.Length - 1; i >= 0; i--)
            {
                Console.Write("{0,3}", param[i]);               //MSB ... LSB
            }
        }

        public void Pokaz_wszystkie_parametry(Dictionary<int, double> przedzial_dyskretny)
        {
            Pokaz_parametr(SN.warstwy_neurony[1][0].lista_wag_wejsc[0]);            //tak naprawdę kilka wybranych parametrów
            Console.Write("  |");
            Pokaz_parametr(SN.warstwy_neurony[1][0].lista_wag_wejsc[1]);
            Console.Write("  |");
            Pokaz_parametr(SN.warstwy_neurony[1][0].lista_wag_wejsc[2]);
            Console.Write("  |");
            Console.Write("{0,5}", Konwersja_bin2dec_param(SN.warstwy_neurony[1][0].lista_wag_wejsc[0]));
            Console.Write("  |");
            Console.Write("{0,5}", Konwersja_bin2dec_param(SN.warstwy_neurony[1][0].lista_wag_wejsc[1]));
            Console.Write("  |");
            Console.Write("{0,5}", Konwersja_bin2dec_param(SN.warstwy_neurony[1][0].lista_wag_wejsc[2]));
            Console.Write("  |");
            Console.Write("{0,8:F2}", przedzial_dyskretny[Konwersja_bin2dec_param(SN.warstwy_neurony[1][0].lista_wag_wejsc[0])]);
            Console.Write("  |");
            Console.Write("{0,8:F2}", przedzial_dyskretny[Konwersja_bin2dec_param(SN.warstwy_neurony[1][0].lista_wag_wejsc[1])]);
            Console.Write("  |");
            Console.Write("{0,8:F2}", przedzial_dyskretny[Konwersja_bin2dec_param(SN.warstwy_neurony[1][0].lista_wag_wejsc[2])]);
            Console.Write("  |");
            Console.Write("{0,12:F6}", wart_fun_przyst);
            Console.WriteLine("  |");
        }
    }

    internal class Program
    {
        static readonly int[] SCHEMAT_SIECI_NEURONOWEJ = { 2, 2, 1 };
        const double PRZEDZ_MIN = -10;
        const double PRZEDZ_MAX = 10;
        //const int ILE_PARAM = 9;
        const int ILE_CHROM_NP = 6;
        const int ILE_OSOB = 13;
        const int ILE_OS_TUR = 3;
        const int ILE_ITERACJE = 100;

        static void Dyskretyzacja_przedzialu(Dictionary<int, double> przedzial_dyskretny, double przedz_min, double przedz_max, int ile_chrom)
        {
            int dyskretny_max = (int)Math.Pow(2, ile_chrom) - 1;
            double delta = Math.Round((przedz_max - przedz_min) / dyskretny_max, 4);        //dokładność zaokrąglenia ma znaczenie
            przedzial_dyskretny.Add(0, przedz_min);
            przedzial_dyskretny.Add(dyskretny_max, przedz_max);
            for (int i = 1; i < dyskretny_max; i++)
            {
                przedzial_dyskretny.Add(i, przedz_min + i * delta);
            }
        }

        static void Operator_selekcji_hot_deck(Osobnik[] pula_osobnikow, ref Osobnik osob_hot_deck, int ile_chrom)
        {
            Osobnik osob_hot_deck_rob;
            osob_hot_deck_rob = pula_osobnikow[0];
            for (int i = 1; i < pula_osobnikow.Length; i++)
            {
                if (osob_hot_deck_rob.wart_fun_przyst > pula_osobnikow[i].wart_fun_przyst) osob_hot_deck_rob = pula_osobnikow[i];
            }
            osob_hot_deck.SkopiujZ(osob_hot_deck_rob, ile_chrom);
        }

        static void Operator_selekcji_turniejowej(Osobnik[] pula_osobnikow, ref Osobnik osob_zwyc_turnieju, int ile_osob_tur, int ile_chrom)
        {
            List<int> indeksy_puli = new List<int>();
            for (int i = 0; i < pula_osobnikow.Length; i++)
            {
                indeksy_puli.Add(i);
            }
            Random random = new Random();
            int i_ip = random.Next(indeksy_puli.Count);
            int n_osob = indeksy_puli[i_ip];
            indeksy_puli.Remove(n_osob);
            int n_osob_rywal;
            for (int i = 0; i < ile_osob_tur - 1; i++)
            {
                i_ip = random.Next(indeksy_puli.Count);
                n_osob_rywal = indeksy_puli[i_ip];
                indeksy_puli.Remove(n_osob_rywal);
                if (pula_osobnikow[n_osob].wart_fun_przyst > pula_osobnikow[n_osob_rywal].wart_fun_przyst) n_osob = n_osob_rywal;
            }
            osob_zwyc_turnieju.SkopiujZ(pula_osobnikow[n_osob], ile_chrom);
        }

        static void Operator_mutowanie(ref Osobnik osob_zmutowany, int[] licznosc_warstw, int ile_chrom,
                                            Dictionary<int, double> przedzial_dyskretny, Dictionary<Wej_ProbkaTyp, double> probki_funkcji_wewy)
        {
            int liczba_param = 0;
            for (int i = 1; i < licznosc_warstw.Length; i++)
            {
                liczba_param += licznosc_warstw[i] * (licznosc_warstw[i - 1] + 1);
            }
            Random random = new Random();
            int n_par = random.Next(liczba_param);                  //wylosowany parametr
            int n_bit = random.Next(ile_chrom);                     //wylosowany bit do zmiany

            int biez_par = 0;
            for (int i = 1; i < licznosc_warstw.Length; i++)
            {
                for (int j = 0; j < licznosc_warstw[i]; j++)
                {
                    for (int k = 0; k < licznosc_warstw[i - 1] + 1; k++)
                    {
                        if (biez_par++ != n_par)
                            continue;
                        osob_zmutowany.SN.warstwy_neurony[i][j].lista_wag_wejsc[k][n_bit] = (byte)(1 - osob_zmutowany.SN.warstwy_neurony[i][j].lista_wag_wejsc[k][n_bit]);
                        //Console.WriteLine("---|TEST biez_: n_par| {0} : {1}", biez_par, n_par);
                    }
                }
            }
            osob_zmutowany.Oblicz_funkcje_przystosowania(przedzial_dyskretny, probki_funkcji_wewy);
        }

        static void Operator_krzyzowanie(ref Osobnik osob_krzyzow_1, ref Osobnik osob_krzyzow_2, int[] licznosc_warstw, int ile_chrom,
                                                Dictionary<int, double> przedzial_dyskretny, Dictionary<Wej_ProbkaTyp, double> probki_funkcji_wewy)
        {
            int liczba_param = 0;
            for (int i = 1; i < licznosc_warstw.Length; i++)
            {
                liczba_param += licznosc_warstw[i] * (licznosc_warstw[i - 1] + 1);
            }
            Random random = new Random();
            int n_par = random.Next(1, liczba_param);              //miejsce cięcia przed wylosowanym parametrem

            byte rob;
            int biez_par = 0;
            for (int i = 1; i < licznosc_warstw.Length; i++)
            {
                for (int j = 0; j < licznosc_warstw[i]; j++)
                {
                    for (int k = 0; k < licznosc_warstw[i - 1] + 1; k++)
                    {
                        if (biez_par++ < n_par)
                            continue;
                        for (int l = 0; l < ile_chrom; l++)
                        {
                            rob = osob_krzyzow_1.SN.warstwy_neurony[i][j].lista_wag_wejsc[k][l];
                            osob_krzyzow_1.SN.warstwy_neurony[i][j].lista_wag_wejsc[k][l] = osob_krzyzow_2.SN.warstwy_neurony[i][j].lista_wag_wejsc[k][l];
                            osob_krzyzow_2.SN.warstwy_neurony[i][j].lista_wag_wejsc[k][l] = rob;
                        }
                        //Console.WriteLine("---|TEST biez_: n_par| {0} : {1}", biez_par, n_par);
                    }
                }
            }
            osob_krzyzow_1.Oblicz_funkcje_przystosowania(przedzial_dyskretny, probki_funkcji_wewy);
            osob_krzyzow_2.Oblicz_funkcje_przystosowania(przedzial_dyskretny, probki_funkcji_wewy);
        }

        static double Najlepsza_wart_fun_przyst(Osobnik[] pula_osobnikow)
        {
            double wart_fun_przyst = pula_osobnikow[0].wart_fun_przyst;
            for (int i = 1; i < pula_osobnikow.Length; i++)
            {
                if (wart_fun_przyst > pula_osobnikow[i].wart_fun_przyst) wart_fun_przyst = pula_osobnikow[i].wart_fun_przyst;
            }
            return wart_fun_przyst;
        }

        static double Srednia_wart_fun_przyst(Osobnik[] pula_osobnikow)
        {
            double sum_wart_fun_przyst = 0;
            for (int i = 0; i < pula_osobnikow.Length; i++)
            {
                sum_wart_fun_przyst += pula_osobnikow[i].wart_fun_przyst;
            }
            return Math.Round(sum_wart_fun_przyst / pula_osobnikow.Length, 6);
        }

        static void TEST_1<T>(string nazwa, Dictionary<T, double> Przedzial_dyskretny)       //< dziedzina funkcji, double>
        {
            Console.WriteLine("(TEST) " + nazwa);
            foreach (var p_dyskr in Przedzial_dyskretny.OrderBy(x => x.Key))
            {
                Console.WriteLine("{0,5}  -  {1,8:F6}", p_dyskr.Key, p_dyskr.Value);
            }
            Console.WriteLine();
        }

        static void TEST_2(string nazwa, Osobnik[] pula_osobnikow, Dictionary<int, double> Przedzial_dyskretny)
        {
            Console.WriteLine("(TEST) " + nazwa);
            for (int i = 0; i < pula_osobnikow.Length; i++)
            {
                Console.Write("{0,3}|", i + 1);
                pula_osobnikow[i].Pokaz_wszystkie_parametry(Przedzial_dyskretny);
            }
            Console.WriteLine();
        }

        static void TEST_3(string nazwa, Osobnik[] pula_osobnikow, ref Osobnik osobnik_rob_1, ref Osobnik osobnik_rob_2, Dictionary<int, double> Przedzial_dyskretny, Dictionary<Wej_ProbkaTyp, double> Probki_funkcji_wewy)
        {
            Console.WriteLine("(TEST) " + nazwa);

            Operator_selekcji_hot_deck(pula_osobnikow, ref osobnik_rob_1, ILE_CHROM_NP);
            Console.WriteLine("Hot Deck :");
            osobnik_rob_1.Pokaz_wszystkie_parametry(Przedzial_dyskretny);
            Console.WriteLine();

            Operator_selekcji_turniejowej(pula_osobnikow, ref osobnik_rob_1, ILE_OS_TUR, ILE_CHROM_NP);
            Console.WriteLine("Turniej :");
            osobnik_rob_1.Pokaz_wszystkie_parametry(Przedzial_dyskretny);
            Console.WriteLine();

            osobnik_rob_1.SkopiujZ(pula_osobnikow[ILE_OSOB - 3], ILE_CHROM_NP);
            Operator_mutowanie(ref osobnik_rob_1, SCHEMAT_SIECI_NEURONOWEJ, ILE_CHROM_NP, Przedzial_dyskretny, Probki_funkcji_wewy);
            Console.WriteLine("Zmutowanie :");
            osobnik_rob_1.Pokaz_wszystkie_parametry(Przedzial_dyskretny);
            Console.WriteLine("Przed mutacją :");
            pula_osobnikow[ILE_OSOB - 3].Pokaz_wszystkie_parametry(Przedzial_dyskretny);
            Console.WriteLine();

            osobnik_rob_1.SkopiujZ(pula_osobnikow[ILE_OSOB - 2], ILE_CHROM_NP);
            osobnik_rob_2.SkopiujZ(pula_osobnikow[ILE_OSOB - 1], ILE_CHROM_NP);
            Operator_krzyzowanie(ref osobnik_rob_1, ref osobnik_rob_2, SCHEMAT_SIECI_NEURONOWEJ, ILE_CHROM_NP, Przedzial_dyskretny, Probki_funkcji_wewy);
            Console.WriteLine("Skrzyżowane :");
            osobnik_rob_1.Pokaz_wszystkie_parametry(Przedzial_dyskretny);
            osobnik_rob_2.Pokaz_wszystkie_parametry(Przedzial_dyskretny);
            Console.WriteLine("Przed skrzyżowaniem :");
            pula_osobnikow[ILE_OSOB - 2].Pokaz_wszystkie_parametry(Przedzial_dyskretny);
            pula_osobnikow[ILE_OSOB - 1].Pokaz_wszystkie_parametry(Przedzial_dyskretny);
            Console.WriteLine();
        }

        static void Main()
        {
            Console.WriteLine("ALGORYTM GENETYCZNY ( ZADANIE 3 )");
            Console.WriteLine();

            Dictionary<int, double> Przedzial_dyskretny = new Dictionary<int, double>();
            Dyskretyzacja_przedzialu(Przedzial_dyskretny, PRZEDZ_MIN, PRZEDZ_MAX, ILE_CHROM_NP);
            //TEST_1("Dyskretyzacja:", Przedzial_dyskretny);

            Dictionary<Wej_ProbkaTyp, double> Probki_funkcji_xor = new Dictionary<Wej_ProbkaTyp, double>
            {
                { (0, 0), 0 },
                { (0, 1), 1 },
                { (1, 0), 1 },
                { (1, 1), 0 }
            };
            //TEST_1("Próbki funkcji xor:", Probki_funkcji_xor);

            Osobnik[] pula_osobnikow = new Osobnik[ILE_OSOB];
            Osobnik[] nowa_pula_osobnikow = new Osobnik[ILE_OSOB];
            Osobnik[] pula_osobnikow_rob;
            Osobnik osobnik_rob_1 = new Osobnik(SCHEMAT_SIECI_NEURONOWEJ, ILE_CHROM_NP);
            Osobnik osobnik_rob_2 = new Osobnik(SCHEMAT_SIECI_NEURONOWEJ, ILE_CHROM_NP);

            for (int i = 0; i < ILE_OSOB; i++)
            {
                pula_osobnikow[i] = new Osobnik(SCHEMAT_SIECI_NEURONOWEJ, ILE_CHROM_NP);
                pula_osobnikow[i].Losuj_wszystkie_parametry();
                pula_osobnikow[i].Oblicz_funkcje_przystosowania(Przedzial_dyskretny, Probki_funkcji_xor);

                nowa_pula_osobnikow[i] = new Osobnik(SCHEMAT_SIECI_NEURONOWEJ, ILE_CHROM_NP);
            }
            //TEST_2("Pula osobników:", pula_osobnikow, Przedzial_dyskretny);

            //TEST_3("Operatory genetyczne:", pula_osobnikow, ref osobnik_rob_1, ref osobnik_rob_2, Przedzial_dyskretny, Probki_funkcji_xor);

            Console.WriteLine("-->  START");
            Console.WriteLine("Najlepsza wartość funkcji przystosowania :{0,10:F6}", Najlepsza_wart_fun_przyst(pula_osobnikow));
            Console.WriteLine("  Średnia wartość funkcji przystosowania :{0,10:F6}", Srednia_wart_fun_przyst(pula_osobnikow));
            Console.WriteLine();

            for (int i = 0; i < ILE_ITERACJE; i++)
            {
                for (int j = 0; j < ILE_OSOB / 2; j++)          //z puli osobników bierzemy po 2 kolejne osobniki
                {
                    Operator_selekcji_turniejowej(pula_osobnikow, ref osobnik_rob_1, ILE_OS_TUR, ILE_CHROM_NP);         //zwycięzca pierwszego turnieju
                    Operator_selekcji_turniejowej(pula_osobnikow, ref osobnik_rob_2, ILE_OS_TUR, ILE_CHROM_NP);         //zwycięzca drugiego turnieju

                    if (j / 2 == 0 || j / 2 == 2)           //zerowa lub druga czwórka osobników
                    {
                        //Console.WriteLine("--------|TEST|  KRZYŻOWANIE :{0,3}  --{1,3}", j, j / 2);
                        Operator_krzyzowanie(ref osobnik_rob_1, ref osobnik_rob_2, SCHEMAT_SIECI_NEURONOWEJ, ILE_CHROM_NP, Przedzial_dyskretny, Probki_funkcji_xor);
                                                                                                                                        //skrzyżowanie pierwszego i drugiego osobnika
                    }
                    if (j / 2 == 1 || j / 2 == 2)           //pierwsza lub druga czwórka osobników
                    {
                        //Console.WriteLine("--------|TEST|      MUTACJA :{0,3}  --{1,3}", j, j / 2);
                        Operator_mutowanie(ref osobnik_rob_1, SCHEMAT_SIECI_NEURONOWEJ, ILE_CHROM_NP, Przedzial_dyskretny, Probki_funkcji_xor);           //zmutowanie pierwszego osobnika
                        Operator_mutowanie(ref osobnik_rob_2, SCHEMAT_SIECI_NEURONOWEJ, ILE_CHROM_NP, Przedzial_dyskretny, Probki_funkcji_xor);           //zmutowanie drugiego osobnika
                    }

                    nowa_pula_osobnikow[j * 2].SkopiujZ(osobnik_rob_1, ILE_CHROM_NP);
                    nowa_pula_osobnikow[j * 2 + 1].SkopiujZ(osobnik_rob_2, ILE_CHROM_NP);
                }
                if (ILE_OSOB % 2 == 1)          //tylko gdy nieparzysta liczba osobników w puli
                {
                    Operator_selekcji_hot_deck(pula_osobnikow, ref osobnik_rob_1, ILE_CHROM_NP);            //najlepszy osobnik w puli
                    nowa_pula_osobnikow[ILE_OSOB - 1].SkopiujZ(osobnik_rob_1, ILE_CHROM_NP);
                }
                Console.WriteLine("-->  ITERACJA NR :{0,3}", i + 1);
                Console.WriteLine("Najlepsza wartość funkcji przystosowania :{0,10:F6}", Najlepsza_wart_fun_przyst(nowa_pula_osobnikow));
                Console.WriteLine("  Średnia wartość funkcji przystosowania :{0,10:F6}", Srednia_wart_fun_przyst(nowa_pula_osobnikow));
                Console.WriteLine();

                pula_osobnikow_rob = pula_osobnikow;
                pula_osobnikow = nowa_pula_osobnikow;
                nowa_pula_osobnikow = pula_osobnikow_rob;
            }
            Console.WriteLine("-->  KONIEC");
            Console.WriteLine();
        }
    }
}
