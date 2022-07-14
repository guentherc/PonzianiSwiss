using MahApps.Metro.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using MvvmDialogs;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaction logic for ForbiddenPairingsDialog.xaml
    /// </summary>
    public partial class ForbiddenPairingsDialog : MetroWindow
    {
        public ForbiddenPairingsDialog()
        {
            InitializeComponent();
        }
    }

    public partial class ForbiddenPairingsDialogViewModel : ObservableObject, IModalDialogViewModel
    {
        [ObservableProperty]
        private bool? dialogResult;

        [ObservableProperty]
        private bool avoidPairingsFromSameFederation = false;
        [ObservableProperty]
        private bool avoidPairingsFromSameClub = false;

        public Tournament? Tournament
        {
            get => tournament;
            set
            {
                tournament = value;
                if (tournament != null)
                {
                    Rules = new ObservableCollection<ForbiddenPairingRule>(tournament.ForbiddenPairingRules);
                    AvoidPairingsFromSameClub = tournament.AvoidPairingsFromSameClub;
                    AvoidPairingsFromSameFederation = tournament.AvoidPairingsFromSameFederation;
                }
            }
        }

        public ObservableCollection<ForbiddenPairingRule>? Rules { set; get; }

        [ObservableProperty]
        private Participant? player1;
        [ObservableProperty]
        private Participant? player2;

        [ObservableProperty]
        private string? federation1;
        [ObservableProperty]
        private string? federation2;
        private Tournament? tournament;

        public List<string?> Federations => Tournament?.Participants.Select(p => p.Federation).Distinct().OrderBy(f => f).ToList() ?? new();

        public List<Participant> Participants => Tournament?.Participants.OrderBy(p => p.Name).ToList() ?? new();

        public ICommand AddRuleCommand { set; get; }
        public ICommand RemoveRuleCommand { set; get; }


        public ForbiddenPairingsDialogViewModel()
        {
            AddRuleCommand = new RelayCommand<string?>((t) => AddRule(t), (t) => true);
            RemoveRuleCommand = new RelayCommand<ForbiddenPairingRule?>((r) => RemoveRule(r), (r) => true);
        }

        [ICommand]
        void Ok()
        {
            if (Tournament != null)
            {
                Tournament.AvoidPairingsFromSameClub = AvoidPairingsFromSameClub;
                Tournament.AvoidPairingsFromSameFederation = AvoidPairingsFromSameFederation;
                Tournament.ForbiddenPairingRules = Rules?.ToList() ?? new();
            }
            DialogResult = true;
        }

        [ICommand]
        void Cancel()
        {
            DialogResult = false;
        }

        private void AddRule(string? tag)
        {
            if (Rules != null)
            {
                if (tag == "1" && Player1 != null && Player2 != null)
                {
                    Rules.Add(new(Player1, Player2));
                }
                else if (tag == "2" && Player1 != null && Federation1 != null)
                {
                    Rules.Add(new(Player1, Federation1));
                }
                else if (tag == "3" && Federation1 != null && Federation2 != null)
                {
                    Rules.Add(new(Federation1, Federation2));
                }
            }
        }

        private void RemoveRule(ForbiddenPairingRule? rule)
        {
            if (rule != null) Rules?.Remove(rule);
        }
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
