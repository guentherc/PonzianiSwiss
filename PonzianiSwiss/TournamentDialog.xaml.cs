using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using MvvmDialogs;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaction logic for TournamentDialog.xaml
    /// </summary>
    public partial class TournamentDialog : MetroWindow
    {
        public TournamentDialog()
        {
            InitializeComponent();
            ComboBox_Federation.ItemsSource = FederationUtil.Federations.OrderBy(e => e.Value);
            ComboBox_Federation.DisplayMemberPath = "Value";
            ComboBox_Federation.SelectedValuePath = "Key";
            ComboBox_PairingSystem.ItemsSource = Enum.GetValues(typeof(PairingSystem));
            ComboBox_RatingDetermination.ItemsSource = Enum.GetValues(typeof(TournamentRatingDetermination));
        }
    }

    public partial class TournamentDialogViewModel : ObservableObject, IModalDialogViewModel
    {
        public TournamentDialogViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;
            TiebreakDialogCommand = new RelayCommand(TiebreakDialog);
        }

        [ObservableProperty]
        private bool? dialogResult;

        [ObservableProperty]
        private Tournament? tournament;

        public ICommand TiebreakDialogCommand { get; }

        [ICommand]
        void Ok()
        {
            DialogResult = true;
        }

        [ICommand]
        void Cancel()
        {
            DialogResult = false;
        }

        private readonly IDialogService dialogService;

        private void TiebreakDialog()
        {
            ShowDialog(viewModel => dialogService?.ShowDialog(this, viewModel));
        }

        private void ShowDialog(Func<TiebreakDialogViewModel, bool?> showDialog)
        {
            var dialogViewModel = App.Current.Services?.GetService<TiebreakDialogViewModel>();

            if (dialogViewModel != null)
            {
                dialogViewModel.Tiebreaks = Tournament?.TieBreak;
                bool? success = showDialog(dialogViewModel);
                if (success == true)
                {
                    if (Tournament != null) Tournament.TieBreak = dialogViewModel?.Tiebreaks ?? new();
                    OnPropertyChanged(nameof(Tournament));
                }
            }
        }

        public DateTime StartDate
        {
            get
            {
                return tournament == null ? DateTime.MinValue : ParseDateTime(tournament?.StartDate != null ? tournament.StartDate : string.Empty);
            }

            set
            {
                if (tournament != null) tournament.StartDate = value.ToString(CultureInfo.InvariantCulture);
                OnPropertyChanged(nameof(StartDate));
                OnPropertyChanged(nameof(Tournament));
            }
        }

        public DateTime EndDate
        {
            get
            {
                return tournament == null ? DateTime.MinValue : ParseDateTime(tournament?.EndDate != null ? tournament.EndDate : string.Empty);
            }

            set
            {
                if (tournament != null) tournament.EndDate = value.ToString(CultureInfo.InvariantCulture);
                OnPropertyChanged(nameof(Tournament));
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
