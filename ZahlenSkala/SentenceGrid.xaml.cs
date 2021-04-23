using MinWordsperSentece;
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

namespace ZahlenSkala
{
    /// <summary>
    /// Interaktionslogik für SentenceGrid.xaml
    /// </summary>
    public partial class SentenceGrid : UserControl
    {
        public SentenceGrid()
        {
            InitializeComponent();
        }

        public void AttachSentence(Sentence s)
        {
            var w = s.Words;

            for (int i = 0; i < w.Count; i++)
            {
                var tB = new LightUpText();
                tB.AttachWord(w[i]);
                gridWords.Children.Add(tB);
            }
        }
    }
}
