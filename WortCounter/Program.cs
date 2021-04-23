using System;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace WortCounter
{
    class Program
    {

        static void Main(string[] args)
        {
            //File.ReadAllText(@"C:\Users\frede\Documents\Pi\pi1m.txt");
            Random rand = new Random(1);


            while (true)
            {
                var b = new byte[8];
                rand.NextBytes(b);

                UInt64 n = BitConverter.ToUInt64(b);
                n %= 1000000000000;
                Console.WriteLine(Zahlengenerator.NatürlicheZahl(n));
                Thread.Sleep(5000);
            }
        }
    }
}
