using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinWordsperSentece
{
    public class Sentence
    {
        char seperator = ' ';

        public Sentence(IEnumerable<Sentence> toReplace)
        {
            Replaces.AddRange(toReplace);
        }

        public Sentence(string from, char sep)
        {
            seperator = sep;
            Word last = null;
            foreach (var item in from.Split(sep))
            {
                var w = new Word(item);
                w.LastWord = last;
                if (last != null)
                    last.NextWord = w;
                Words.Add(w);
                last = w;
            }
        }

        Sentence() { }

        public Sentence GetCopyToOptimize()
        {
            var ret = new Sentence();
            foreach (var item in Replaces)
            {
                var ns = new Sentence(item.ToString(), seperator);
                ret.Replaces.Add(ns);
            }
            return ret;
        }

        public void InitForReplace()
        {
            foreach (var item in Replaces)
            {
                foreach (var w in item.Words)
                {
                    w.ReplacedBy = null;
                }
            }
            Words.Clear();
        }

        public List<Word> Words { get; private set; } = new List<Word>();
        public List<Sentence> Replaces { get; set; } = new List<Sentence>();

        //Position of this object not a word with same text
        public int FindPos(Word word) =>  Words.IndexOf(word);

        public void InsertWord(int pos, Word w) => Words.Insert(pos, w);

        public List<Word> FindCandidates(Word target)
        {
            var rL = new List<Word>();
            foreach (var item in Words)
            {
                //Traverse Next, Last to check if part of sentence
                if (item.ReplacedBy == null && item.Text == target.Text && item != target)
                    rL.Add(item);
            }
            return rL;
        }

        public override string ToString()
        {
            string ret = "";
            foreach (var item in Words)
            {
                    ret += item + " ";
            }
            return ret;
        }
    }

    public class Word
    {
        public Word(string t) => Text = t;
        public string Text { get; set; }
        public Word ReplacedBy { get; set; }
        public Word NextWord { get; set; }
        public Word LastWord { get; set; }
        public override string ToString()
        {
            return Text;
        }

        public Word GetRec()
        {
            if (ReplacedBy == null)
                return this;
            return ReplacedBy.GetRec();
        }


        public bool IsLit { get; set; }
    }

    public static class MinWordPerSentence
    {

        static void InsertSentRec(Sentence sent, Sentence sentIn, int numWrdIn, Random permutation)
        {
            var wordIn = sentIn.Words[numWrdIn];
            var lastWordPos = -1;
            if (numWrdIn > 0)
            {
                var lastW = sentIn.Words[numWrdIn - 1];
                if (lastW.ReplacedBy != null)
                    lastWordPos = sent.FindPos(lastW.ReplacedBy);
                else
                    lastWordPos = sent.FindPos(lastW);
            }


            var replaceOptions = sent.FindCandidates(wordIn);
            int firstPossibleInsertPoint = lastWordPos + 1;
            bool inserted = false;


            for (int i = 0; !inserted && i < replaceOptions.Count; i++)
            {
                if (lastWordPos < sent.FindPos(replaceOptions[i]))
                {
                    if (permutation.NextDouble() < chanceToTakeRep / (double)i)
                    {
                        wordIn.ReplacedBy = replaceOptions[i];
                        inserted = true;
                    }
                }
            }

            for (int i = firstPossibleInsertPoint; !inserted && i <= sent.Words.Count; i++)
            {
                double midPoint = (sent.Words.Count + firstPossibleInsertPoint)/2.0;
                double normPos = (i- midPoint) / (double)(sent.Words.Count - firstPossibleInsertPoint + 1);
                double preferEdges = normPos * normPos;
                if (permutation.NextDouble() < preferEdges + chanceToTakeInsert / (replaceOptions.Count+1))
                {
                    sent.InsertWord(i, wordIn);
                    inserted = true;
                }
            }

            if(!inserted)
                InsertSentRec(sent, sentIn, numWrdIn, permutation);
            else if (numWrdIn < sentIn.Words.Count - 1)
                InsertSentRec(sent, sentIn, (numWrdIn + 1), permutation);
            else
                return;
        }

        static void InsertAll(Sentence sent, int perm)
        {
            sent.InitForReplace();

            for (int i = 0; i < sent.Replaces.Count; i++)
            {
                var currSentToInsert = sent.Replaces[i];
                InsertSentRec(sent, currSentToInsert, 0, new Random(perm));
            }
        }


        static double chanceToTakeRep = 0.89;
        static double chanceToTakeInsert = 0.3;
        /// <summary>
        /// Finds shortest Sentence, that contains all Sub Sentences in correct order
        /// GetSimplifiedSentence for more info
        /// </summary>
        public static void SimplifySentence(Sentence sent, int permutations)
        {
            var orgList = sent.Replaces;
            sent.Replaces = sent.Replaces.OrderBy(sen => -sen.Words.Count).ToList();
            var wCount = new int[permutations];
            //for (int n = 0; n < permutations; n++)
            Parallel.For(0, permutations, n =>
            {
              var s = sent.GetCopyToOptimize();
              InsertAll(s, n);
              wCount[n] = s.Words.Count;
            });

            var cList = wCount.ToList();

            int min = wCount.Min();
            int minPerm = cList.FindIndex(wc => wc == min);
            double avg = cList.Sum() / cList.Count;

            int numInp = 0;
            foreach (var item in sent.Replaces)
                numInp += item.Words.Count;

            double compression = min / (double)numInp;
            cList.Sort();

            InsertAll(sent, minPerm);
            sent.Replaces = orgList;
        }

        /// <summary>
        /// Finds shortest Sentence, given a list of sentences
        /// containing all sentences with words in right order.
        /// There are usually multiple slution, 
        /// Input
        /// Walking at night
        /// coding by night
        /// Output:
        /// walking at coding by night
        /// or
        /// walking coding at by night
        /// or ...
        /// 
        /// not:
        /// walking at by coding night
        /// </summary>
        public static Sentence MakeSentence(IEnumerable<string> sentencesToSimplify, char wordSeperator = ' ')
        {
            var l = new List<Sentence>();
            foreach (var item in sentencesToSimplify)
            {
                var s = new Sentence(item, wordSeperator);
                l.Add(s);

            }
            var ret = new Sentence(l);
            return ret;
        }


        /// <summary>
        /// Using The simplified Sentence, Show which words are part of the original sentence
        /// *Walking coding *at by *night
        /// </summary>
        /// <param name="simplifiedSentence">Get from GetSimplifiedSentence</param>
        /// <param name="lightUp">The Sentence in simplifiedSentence that should be lit</param>
        /// <param name="litUpMarker">String that marks a lit up word</param>
        /// <param name="notLitMarker">String that marks words that are not lit</param>
        /// <returns></returns>
        public static string GetLitUpSentence(Sentence simplifiedSentence, Sentence lightUp, char wordSeperator = ' ', string litUpMarker = "*", string notLitMarker = "")
        {
            foreach (var item in simplifiedSentence.Words)
            {
                item.IsLit = false;
            }

            foreach (var item in lightUp.Words)
            {
                item.GetRec().IsLit = true;
            }

            return "";
        }
    }
}