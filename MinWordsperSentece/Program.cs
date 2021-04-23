using System;
using System.Collections.Generic;
using System.Threading;
using WortCounter;

namespace MinWordsperSentece
{
    class Program
    {

        static void Main(string[] args)
        {
            var numbers = new List<string>();

            for (ulong n = 0; n <= 100; n++)
                numbers.Add(Zahlengenerator.NatürlicheZahl(n));


            var sent = MinWordPerSentence.MakeSentence(numbers);

            Console.WriteLine();
            Console.WriteLine(">Not optimized");
            Console.WriteLine(sent);


            MinWordPerSentence.SimplifySentence(sent,1);
            Console.WriteLine();
            Console.WriteLine(">optimized");
            Console.WriteLine(sent);


            var subS = sent.Replaces;

            for (int n = 0; n < subS.Count; n++)
            {
                Console.WriteLine();
                Console.WriteLine($">optimized with sentence nr {n} lit");
                Console.WriteLine(MinWordPerSentence.GetLitUpSentence(sent, subS[n]));
                Console.ReadKey();
            }
        }
    }
}
