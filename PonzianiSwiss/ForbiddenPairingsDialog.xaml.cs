using MahApps.Metro.Controls;
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
using System.Windows.Shapes;

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaction logic for ForbiddenPairingsDialog.xaml
    /// </summary>
    public partial class ForbiddenPairingsDialog : MetroWindow
    {
        public ForbiddenPairingsDialog(Tournament tournament)
        {
            InitializeComponent();
            Model = new(tournament);
            DataContext = Model;
        }

        public ForbiddenPairingsModel Model { private set; get; }

        private void ForbiddenPairingsCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ForbiddenPairingsOkButton_Click(object sender, RoutedEventArgs e)
        {
            Model.Tournament.AvoidPairingsFromSameClub = Model.AvoidPairingsFromSameClub;
            Model.Tournament.AvoidPairingsFromSameFederation = Model.AvoidPairingsFromSameFederation;
            Model.Tournament.ForbiddenPairingRules = Model.Rules.ToList();
            this.DialogResult = false;
            this.Close();
        }

        private void Forbidding_Pairing_Rule_Add_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            string tag = btn.Tag?.ToString() ?? "0";
            if (tag == "1" && Model.Player1 != null && Model.Player2 != null)
            {
                Model.Rules.Add(new(Model.Player1, Model.Player2));
                Model.SyncRules();
            }
            else if (tag == "2" && Model.Player1 != null && Model.Federation1 != null)
            {
                Model.Rules.Add(new(Model.Player1, Model.Federation1));
                Model.SyncRules();
            }
            else if (tag == "3" && Model.Federation1 != null && Model.Federation2 != null)
            {
                Model.Rules.Add(new(Model.Federation1, Model.Federation2));
                Model.SyncRules();
            }
        }

        private void ForbiddenPairings_Remove_Click(object sender, RoutedEventArgs e)
        {
            Model.Rules.Remove((ForbiddenPairingRule)((Button)sender).Tag);
            Model.SyncRules();
        }
    }

    public class ForbiddenPairingsModel : ViewModel
    {
        public ForbiddenPairingsModel(Tournament tournament)
        {
            Tournament = tournament ?? new();
            Rules = new(Tournament.ForbiddenPairingRules);
            AvoidPairingsFromSameClub = Tournament.AvoidPairingsFromSameClub;
            AvoidPairingsFromSameFederation = Tournament.AvoidPairingsFromSameFederation;
        }

        public bool AvoidPairingsFromSameFederation { set; get; } = false;

        public bool AvoidPairingsFromSameClub { set; get; } = false;

        public Tournament Tournament { private set; get; }

        public ObservableCollection<ForbiddenPairingRule> Rules { set; get; }

        public Participant? Player1 { set; get; }
        public Participant? Player2 { set; get; }

        public string? Federation1 { set; get; }
        public string? Federation2 { set; get; }
        
        public List<string?> Federations => Tournament.Participants.Select(p => p.Federation).Distinct().OrderBy(f => f).ToList();

        public List<Participant> Participants => Tournament.Participants.OrderBy(p => p.Name).ToList();

        public void SyncRules() => RaisePropertyChange(nameof(Rules));
    }

    public class ForbiddenPairingRuleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rules = (ObservableCollection<ForbiddenPairingRule>)value;
            List<string> result = new();
            foreach (var rule in rules)
            {
                result.Add(rule.ToString());
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AdjustSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value.ToString(), out double number) && double.TryParse(parameter.ToString(), out double offset))
            {
                return Math.Max(0.0, number + offset);
            }
            else return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
