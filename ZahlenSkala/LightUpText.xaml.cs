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
    /// Interaktionslogik für LightUpText.xaml
    /// </summary>
    public partial class LightUpText : UserControl
    {
        Word word;
        SolidColorBrush bgLit;
        SolidColorBrush bgOff;

        public LightUpText()
        {
            InitializeComponent();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            bgLit = new SolidColorBrush(Colors.Orange);
            bgLit.Freeze();
            bgOff = new SolidColorBrush(Colors.Gray);
            bgOff.Freeze();
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if(word!=null)
            {
                text.Text = word.Text;
                if (word.IsLit)
                    border.Background = bgLit;
                else
                    border.Background = bgOff;
            }
        }

        public void AttachWord(Word w)
        {
            word = w;
        }
    }
}
