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
using System.Windows.Threading;
using WortCounter;

namespace ZahlenSkala
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer t1 = new DispatcherTimer();

        Sentence sent;
        List<Sentence> toLight;
        int cnt = 0;

        public MainWindow()
        {
            InitializeComponent();


            var numbers = new List<string>();

            for (int n = 0; n <= 10000; n++)
                numbers.Add(Zahlengenerator.NatürlicheZahl((ulong)n));

           sent = MinWordPerSentence.MakeSentence(numbers);

            MinWordPerSentence.SimplifySentence(sent,20);

            toLight = sent.Replaces;

            sentenceGrid.AttachSentence(sent);

            t1.Tick += T1_Tick;
            t1.Interval = TimeSpan.FromSeconds(1);
            t1.Start();
        }

        void SetNum()
        {
            var cMod = cnt % toLight.Count;
            MinWordPerSentence.GetLitUpSentence(sent, toLight[cMod]);
            textNumSet.Text = cMod.ToString();
        }

        private void T1_Tick(object sender, EventArgs e)
        {
            if (checkStep.IsChecked.Value)
            {
                cnt++;
                SetNum();
            }
        }

        private void buttonSet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cnt = int.Parse(textNum.Text);
                SetNum();
            }
            catch (Exception)
            {
            }
        }
    }
}
