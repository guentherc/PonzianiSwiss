using MahApps.Metro.Controls;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaction logic for TournamentDialog.xaml
    /// </summary>
    public partial class TournamentDialog : MetroWindow
    {
        public TournamentDialog(Tournament tournament)
        {
            Model = new(tournament);
            InitializeComponent();
            this.DataContext = Model;
            ComboBox_Federation.ItemsSource = FederationUtil.Federations.OrderBy(e => e.Value);
            ComboBox_Federation.DisplayMemberPath = "Value";
            ComboBox_Federation.SelectedValuePath = "Key";
            ComboBox_Federation.SelectedValue = Model.Tournament.Federation != null && Model.Tournament.Federation != String.Empty ? Model.Tournament.Federation : "FIDE";
            this.Title = (tournament.Name == null || tournament.Name == String.Empty) ? this.Title = "Create new Tournament" : this.Title = $"Edit {Model.Tournament.Name}";
            ComboBox_PairingSystem.ItemsSource = Enum.GetValues(typeof(PairingSystem));
            ComboBox_RatingDetermination.ItemsSource = Enum.GetValues(typeof(TournamentRatingDetermination));
        }

        public TournamentModel Model { set; get; }

        private void TournamentDialogOkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void TournamentDialogCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Button_Edit_Tiebreak_Click(object sender, RoutedEventArgs e)
        {
            TiebreakDialog dialog = new(Model.Tournament.TieBreak)
            {
                Owner = this
            };
            if (dialog.ShowDialog() == true)
            {
                Model.Tournament.TieBreak = dialog.Tiebreaks;
                Model.Sync();
            }
        }
    }

    public class TournamentModel : ViewModel
    {
        private Tournament tournament;

        public Tournament Tournament
        {
            get => tournament; set
            {
                tournament = value;
                RaisePropertyChange();
            }
        }

        public void Sync() => RaisePropertyChange(nameof(Tournament));

        public TournamentModel(Tournament tournament)
        {
            this.tournament = tournament;
        }

        [DependentProperties("Tournament")]
        public DateTime StartDate
        {
            get
            {
                return tournament == null ? DateTime.MinValue : ParseDateTime(tournament?.StartDate != null ? tournament.StartDate : string.Empty);
            }

            set
            {
                if (tournament != null) tournament.StartDate = value.ToString(CultureInfo.InvariantCulture);
                RaisePropertyChange();
            }
        }

        [DependentProperties("tournament")]
        public DateTime EndDate
        {
            get
            {
                return tournament == null ? DateTime.MinValue : ParseDateTime(tournament?.EndDate != null ? tournament.EndDate : string.Empty);
            }

            set
            {
                if (tournament != null) tournament.EndDate = value.ToString(CultureInfo.InvariantCulture);
                RaisePropertyChange();
            }
        }

        private static DateTime ParseDateTime(string dateTime)
        {
            if (DateTime.TryParse(dateTime, CultureInfo.CurrentUICulture, DateTimeStyles.AllowWhiteSpaces, out DateTime result)) return result;
            if (DateTime.TryParse(dateTime, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out result)) return result;
            return result;
        }
    }

    public class TiebreakConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is List<TieBreak> tiebreaks ? string.Join(" -> ", tiebreaks) : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
