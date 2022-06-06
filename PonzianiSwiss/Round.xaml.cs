using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaction logic for Round.xaml
    /// </summary>
    public partial class Round : UserControl
    {
        public Round(Tournament tournament, int roundIndex)
        {
            InitializeComponent();
            Model = new(tournament, roundIndex);
            DataContext = Model;
            lvRound.ItemsSource = Model.Pairings;
        }

        public event EventHandler? ResultSet;

        public RoundModel Model { set; get; }

        private void MenuItem_Set_Result(object sender, RoutedEventArgs e)
        {
            RoundPairing? p = lvRound?.SelectedItem as RoundPairing;
            MenuItem? mi = sender as MenuItem;
            if (p != null && mi != null)
            {               
                Result r = (Result)int.Parse((string)mi.Tag);
                p.Pairing.Result = r;
                Model.SyncRound();
                ResultSet?.Invoke(this, new());
            }
        }
    }

    public class RoundModel : ViewModel
    {

        public RoundModel(Tournament tournament, int roundIndex)
        {
            Tournament = tournament ?? throw new ArgumentNullException(nameof(tournament));
            RoundIndex = roundIndex;
            foreach (Pairing pairing in Tournament.Rounds[roundIndex].Pairings)
                Pairings.Add(new RoundPairing(pairing, RoundIndex));
        }

        internal ObservableCollection<RoundPairing> Pairings { get; } = new();

        public Tournament Tournament { get; set; }

        public int RoundIndex { get; set; }

        internal void SyncRound()
        {
            Pairings.Clear();
            foreach (Pairing pairing in Tournament.Rounds[RoundIndex].Pairings)
                Pairings.Add(new RoundPairing(pairing, RoundIndex));
        }

    }

    internal class RoundPairing
    {
        public RoundPairing(Pairing pairing, int roundIndex)
        {
            Pairing = pairing;
            RoundIndex = roundIndex;
        }

        public Pairing Pairing { set; get; }

        public string Result => result_strings[(int)Pairing.Result];
        public string White => $"{Pairing.White.Name?.Trim()} ({Pairing.White.Scorecard?.Score(RoundIndex)})";
        public string Black => $"{Pairing.Black.Name?.Trim()} ({Pairing.Black.Scorecard?.Score(RoundIndex)})";

        public string Background => Pairing.Result == PonzianiSwissLib.Result.Open ? "Transparent" : "LightGray";
        
        private int RoundIndex { set; get; }

        internal static readonly string[] result_strings = new string[14] {
            "*",
            "--+",
            "0-1",
            "0-1",
            "Bye 0",
            "Bye",
            "1/2-1/2",
            "1/2-1/2",
            "Bye 1/2",
            "1-0",
            "1-0",
            "+--",
            "Bye 1",
            "---"
        };
    }
}
