using System;
using System.Collections.Generic;
using System.Text;

namespace MinWordsperSentece
{
    public interface IPartOfSentence
    {
        int? Position { get; set; } //Null if not part of sentence
    }


    /// <summary>
    /// A sentence can contain words  or Sentences(IPartOfSentence)
    /// A Sentence Must Contain at leas one IPartOfSentence
    /// Indexing can be by Position => Direct from Elements
    /// WordPosition, first unrfold all sentences into words => position in that list
    /// </summary>
    public class Sentence : IPartOfSentence
    {
        public Sentence(string from, char sep)
        {
            Word last = null;
            foreach (var item in from.Split(sep))
            {
                var w = new Word(item);
                w.LastWord = last;
                if (last != null)
                    last.NextWord = w;
                Elements.Add(w);
                last = w;
            }
        }



        public Sentence(IEnumerable<Sentence> ele)
        {
            Elements.AddRange(ele);
        }

        public int? Position { get; set; }
        public List<IPartOfSentence> Elements { get; private set; } = new List<IPartOfSentence>();


        //Only Gets immediate children
        public List<Word> GetWords()
        {
            var ret = new List<Word>();
            foreach (var item in Elements)
            {
                if (item is Word)
                    ret.Add((Word)item);
            }
            return ret;
        }


        //Direct Subsentences
        public List<Sentence> GetSubSentences()
        {
            var ret = new List<Sentence>();
            foreach (var item in Elements)
            {
                if (item is Sentence)
                    ret.Add((Sentence)item);
            }
            return ret;
        }

        //Unroll All Subsentrences
        public List<Sentence> GetSubSentencesRec()
        {
            var subS = GetSubSentences();
            var ret = new List<Sentence>();
            foreach (var item in subS)
            {
                ret.Add(item);
                ret.AddRange(item.GetSubSentences());
            }
            return ret;
        }

        public List<Word> bufferedUnrolled;

        //Unrolls sentences into Words
        public List<Word> UnrolledWords
        {
            get
            {
                if (bufferedUnrolled == null)
                {
                    var ret = new List<Word>();
                    foreach (var item in Elements)
                        if (item is Sentence)
                            ret.AddRange(((Sentence)item).UnrolledWords);
                        else if (item is Word)
                            ret.Add((Word)item);
                        else
                            throw new Exception("Only Words and Sentences allowed");
                    bufferedUnrolled = ret;
                }

                return bufferedUnrolled;
            }
        }

        public int GetCurrentPosition(Word target)
        {
            if (target.ReplacedBy == null)
                return VisibleWords.FindIndex(w => target == w); //Unrolled
            else
                return GetCurrentPosition(target.ReplacedBy);
        }


        public List<int> FindAlternatePositions(Word target)
        {
            if (target.ReplaceCandidates == null)
            {
                var c = new List<Word>();
                foreach (var item in UnrolledWords)
                {
                    if (item.Text == target.Text && item != target)
                    {
                        c.Add(item);
                    }
                }
                target.ReplaceCandidates = c;
            }


            var rL = new List<int>();
            foreach (var item in target.ReplaceCandidates)
            {
                if (item.ReplacedBy == null)
                    rL.Add(GetCurrentPosition(item));
            }
            return rL;
        }

        /// <summary>
        /// Unfolds Sentences into Words to find absolute Word Position
        /// </summary>
        /// <param name="wordPos">Word at Position</param>
        /// <returns></returns>
        public Word GetFromWordPosition(int wordPos)
        {
            return VisibleWords[wordPos]; //Unrolled
        }

        public List<Word> VisibleWords
        {
            get
            {
                var ret = new List<Word>();

                foreach (var item in UnrolledWords)
                {
                    if (item.ReplacedBy == null)
                        ret.Add(item);
                }
                return ret;
            }
        }

        public override string ToString()
        {
            string ret = "";
            foreach (var item in VisibleWords)
            {
                    ret += item + " ";
            }
            return ret;
        }
    }

    public class Word : IPartOfSentence
    {
        public Word(string t) => Text = t;
        public string Text { get; set; }
        public Word ReplacedBy { get; set; }
        public List<Word> Replaces { get; set; } = new List<Word>();
        public List<Word> ReplaceCandidates { get; set; }
        public Word NextWord { get; set; }
        public Word LastWord { get; set; }
        public int? Position { get; set; }
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
        static bool CheckWordPos(Sentence parent, Word w, int newPos)
        {
            int pDown = 0;
            if(w.LastWord != null)
                pDown = parent.GetCurrentPosition(w.LastWord);

            int pUp = int.MaxValue;
            if (w.NextWord != null)
                pUp = parent.GetCurrentPosition(w.NextWord);

            return (newPos >= pDown && newPos <= pUp);
        }

        /// <summary>
        /// Finds shortest Sentence, that contains all Sub Sentences in correct order
        /// GetSimplifiedSentence for more info
        /// </summary>
        public static void SimplifySentence(Sentence sent, char wordSeperator = ' ')
        {
            int startCount = sent.VisibleWords.Count;

            var allSubs = sent.GetSubSentences();

            if (allSubs.Count == 0)
                return; //nothing to optimize

            //Make sure all lower levels are simplified
            foreach (var subS in allSubs)
                SimplifySentence(subS, wordSeperator);

            foreach (var subS in allSubs)
            {
                var words = subS.VisibleWords; //Flattened Version of the tree,unrolled

                for (int n = 0; n < words.Count; n++)
                {
                    var pos = sent.GetCurrentPosition(words[n]);
                    var occ = sent.FindAlternatePositions(words[n]);
                    //find maximum distance to replace
                    int maxDist = int.MaxValue;
                    Word currentTarget = null;
                    foreach (var item in occ)
                    {
                        var el = sent.GetFromWordPosition(item);
                        bool canPartOfSent = subS.UnrolledWords.Contains(el);
                        bool posOK = CheckWordPos(sent, words[n], item);
                        bool targetOkForReplaced = true;
                        foreach (var c in words[n].Replaces)
                        {
                            targetOkForReplaced = targetOkForReplaced && CheckWordPos(sent, c, item);
                        }

                        if (targetOkForReplaced && !canPartOfSent && posOK)
                        {
                            int dist = Math.Abs(item - pos);
                            if (dist < maxDist)
                            {
                                maxDist = dist;
                                currentTarget = el;
                            }
                        }
                    }

                    if (words[n].ReplacedBy != null)
                        words[n].ReplacedBy.Replaces.Remove(words[n]);


                    if (words[n].Replaces.Count != 0 && currentTarget != null)
                    {
                        foreach (var item in words[n].Replaces)
                        {
                            item.ReplacedBy = currentTarget;
                            if (!currentTarget.Replaces.Contains(item))
                                currentTarget.Replaces.Add(item);
                        }
                        words[n].Replaces.Clear();
                    }

                    words[n].ReplacedBy = currentTarget;
                    if (currentTarget != null && !currentTarget.Replaces.Contains(words[n]))
                        currentTarget.Replaces.Add(words[n]);
                }
            }

            Console.WriteLine(">Simplify Sentence iteration complete, words:" + sent.VisibleWords.Count);

            if (startCount != sent.VisibleWords.Count)
                SimplifySentence(sent);
        }

        /// <summary>
        /// Finds shortest Sentence, that contains all Sub Sentences in correct order
        /// GetSimplifiedSentence for more info
        /// </summary>
        public static void SimplifySentence2(Sentence sent, char wordSeperator = ' ')
        {
            var allSubs = sent.GetSubSentences();

            if (allSubs.Count == 0)
                return; //nothing to optimize

            //Make sure all lower levels are simplified
            foreach (var subS in allSubs)
                SimplifySentence(subS, wordSeperator);

            var BaseSentence = allSubs[0];

            for (int i = 1; i < allSubs.Count; i++)
            {

            }
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
            var wordsToLight = new List<Word>();
            foreach (var w in lightUp.UnrolledWords)
                wordsToLight.Add(w.GetRec());

            var words = simplifiedSentence.UnrolledWords;
            string ret = "";
            for (int i = 0; i < words.Count; i++)
            {
                if (words[i].ReplacedBy == null)
                {
                    if (wordsToLight.Contains(words[i]))
                    {
                        ret += litUpMarker;
                        words[i].IsLit = true;
                    }
                    else
                    {
                        ret += notLitMarker;
                        words[i].IsLit = false;
                    }
                    ret += words[i] + " ";
                }
                else
                    words[i].IsLit = false;
            }
            return ret;
        }
    }
}