using System;
using System.Collections.Generic;
using System.Text;

namespace WortCounter
{
    //Ich empfehle von unten nach oben zu lesen, warum weiß ich uch nicht
    public static class Zahlengenerator
    {
        public static string NatürlicheZahl(UInt64 n)
        {
            if (n == 0)
                return "null"; //Die besonderste aller Zahlen
            if (n <= 12)
                return BisZwöl(n);
            if (n <= 19)
                return Zehner(n);
            if (n <= 99)
                return BisHundert(n);
            if (n <= 999)
                return BisTausend(n);
            else
                return JetztWirdsGroß(n);
        }

        static string JetztWirdsGroß(UInt64 n)
        {
            //double e = (UInt64)Math.Log10(n); //log 10 praktisch!  10 ^ e = n

            string es = "";
            UInt64 exp;

            //nicht schön aber selten
            if (n < 1000000)
            {
                es = "tausend";
                exp = 1000;
            }
            else if (n < 1000000000)
            {
                es = "million";
                exp = 1000000;
            }
            else if (n < 1000000000000)
            {
                es = "milliarden";
                exp = 1000000000;
            }
            else if (n < 1000000000000000)
            {
                es = "billionen";
                exp = 1000000000000;
            }
            else if (n < 1000000000000000)
            {
                es = "billiarden";
                exp = 1000000000000;
            }
            else
                return "Wüsste ich auch gern";

            UInt64 h = n / exp;
            UInt64 l = n % exp;
            var r = NatürlicheZahl(h);     //<--- und ZURÜCK
            if (h == 1)
                r = BisNeunAberWarum(h);
            r += " " + es;
            if (l != 0)
                r += " " + NatürlicheZahl(l);  //<<------ Double Rekursion?!?!
            return r;
        }

        static string BisTausend(UInt64 n)
        {
            var r = "hundert";
            if (n % 100 != 0)
                r += " " + NatürlicheZahl(n % 100);  //<<------ Rekursion Baby!
            if (n > 199)
                r = BisNeunAberWarum(n / 100) + " " + r;
            return r;
        }


        //20 -> 99
        static string BisHundert(UInt64 n)
        {
            var l = n % 10;
            UInt64 h = n / 10;
            var hs = BisZwöl(h) + "z";

            switch (h)
            {
                case 2: hs = "zwanz"; break;
                case 3: hs = "dreiß"; break;
                case 6: hs = "sechz"; break;
                case 7: hs = "siebz"; break;
                case 8: hs = "achtz"; break;
                case 9: hs = "neunz"; break;
            }

            hs += "ig";

            if (l != 0)
                return BisNeunAberWarum(l) + " und " + hs;
            else
                return hs;
        }


        //1 -> 9 aber für "einhundert" und "drei und fünfzig"
        static string BisNeunAberWarum(UInt64 n)
        {
            switch (n)
            {
                case 1: return "ein";
            }

            return BisZwöl(n);
        }

        //13 -> 19
        static string Zehner(UInt64 n)
        {

            var l = n % 10;
            var ls = BisZwöl(l);
            if (n == 17)
                ls = "sieb";
            return ls + "" + BisZwöl(10);
        }


        //1 -> 12
        static string BisZwöl(UInt64 n)
        {
            switch (n)
            {
                case 1: return "eins";
                case 2: return "zwei";
                case 3: return "drei";
                case 4: return "vier";
                case 5: return "fünf";
                case 6: return "sechs";
                case 7: return "sieben";
                case 8: return "acht";
                case 9: return "neun";
                case 10: return "zehn";
                case 11: return "elf";
                case 12: return "zwölf";
            }
            return "wüsste ich auch gern";
        }
    }
}

