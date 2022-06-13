using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
            if (lvRound?.SelectedItem is RoundPairing p && sender is MenuItem mi)
            {
                Result r = (Result)int.Parse((string)mi.Tag);
                p.Result = r;
                ResultSet?.Invoke(this, new());
            }
        }

        private void Listview_Round_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource is ListViewItem currentItem)
            {
                if (e.Key == Key.D0 || e.Key == Key.D1 || e.Key == Key.OemPlus || e.Key == Key.D8)
                {
                    ListViewItem item = currentItem;
                    if (item.DataContext is RoundPairing p)
                    {
                        if (e.Key == Key.D0 && e.IsToggled)
                            p.Result = Result.Draw;
                        else if (e.Key == Key.D0)
                            p.Result = Result.Loss;
                        else if (e.Key == Key.D1)
                            p.Result = Result.Win;
                        else if (e.Key == Key.OemPlus || e.Key == Key.D8)
                            p.Result = Result.Open;
                        else return;
                        ResultSet?.Invoke(this, new());
                    }
                }
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

    internal class RoundPairing: ViewModel
    {
        public RoundPairing(Pairing pairing, int roundIndex)
        {
            Pairing = pairing;
            RoundIndex = roundIndex;
        }

        public Pairing Pairing { set; get; }
        public string White => $"{Pairing.White.Name?.Trim()} ({Pairing.White.Scorecard?.Score(RoundIndex)})";
        public string Black => $"{Pairing.Black.Name?.Trim()} ({Pairing.Black.Scorecard?.Score(RoundIndex)})";

        private int RoundIndex { set; get; }

        public Result Result
        {
            set
            {
                if (value != Pairing.Result)
                {
                    Pairing.Result = value;
                    RaisePropertyChange();
                }
            }
            get => Pairing.Result;
        }

    }

    public class ResultConverter : IValueConverter
    {
        private static readonly string[] result_strings = new string[14] {
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
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Result r)
                return result_strings[(int)r];
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                for (int i = 0; i < result_strings.Length; i++)
                {
                    if (result_strings[i] == s)
                        return (Result)i;
                }
            }
            return Result.Open;
        }
    }

}
