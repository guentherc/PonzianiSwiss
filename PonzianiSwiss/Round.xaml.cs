using Microsoft.Toolkit.Mvvm.ComponentModel;
using PonzianiSwissLib;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

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

        public event EventHandler<ResultSetEventArgs>? ResultSet;

        public RoundModel Model { set; get; }

        private void MenuItem_Set_Result(object sender, RoutedEventArgs e)
        {
            if (lvRound?.SelectedItem is RoundPairing p && sender is MenuItem mi)
            {
                Result r = (Result)int.Parse((string)mi.Tag);
                p.Result = r;
                if (p.Pairing != null)
                    ResultSet?.Invoke(this, new(p.Pairing, r));
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
                        if (p.Pairing != null)
                            ResultSet?.Invoke(this, new(p.Pairing, p.Result));
                    }
                }
            }
        }
    }

    public partial class RoundModel : ObservableObject
    {
        public RoundModel(Tournament? tournament, int roundIndex)
        {
            if (tournament != null)
            {
                Tournament = tournament;
                RoundIndex = roundIndex;
                foreach (Pairing pairing in Tournament?.Rounds[roundIndex].Pairings ?? new())
                    Pairings.Add(new RoundPairing(pairing, RoundIndex));
            }
        }

        internal ObservableCollection<RoundPairing> Pairings { get; } = new();

        [ObservableProperty]
        private Tournament? tournament;

        [ObservableProperty]
        private int roundIndex;

        internal void SyncRound()
        {
            if (Tournament != null)
            {
                Pairings.Clear();
                foreach (Pairing pairing in Tournament.Rounds[RoundIndex].Pairings)
                    Pairings.Add(new RoundPairing(pairing, RoundIndex));
            }
        }

    }

    public class ResultSetEventArgs : EventArgs
    {
        public ResultSetEventArgs(Pairing pairing, Result result)
        {
            Pairing = pairing ?? throw new ArgumentNullException(nameof(pairing));
            Result = result;
        }

        public Pairing Pairing { get; private set; }
        public Result Result { get; private set; }
    }

    internal partial class RoundPairing : ObservableObject
    {
        public RoundPairing(Pairing pairing, int roundIndex)
        {
            Pairing = pairing;
            RoundIndex = roundIndex;
        }

        [ObservableProperty]
        private Pairing? pairing;
        public string White => $"{Pairing?.White?.Name?.Trim()} ({Pairing?.White?.Scorecard?.Score(RoundIndex) ?? 0})";
        public string Black => $"{Pairing?.Black?.Name?.Trim()} ({Pairing?.Black?.Scorecard?.Score(RoundIndex) ?? 0})";

        private int RoundIndex { set; get; }

        [ObservableProperty]
        private Result result;
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
