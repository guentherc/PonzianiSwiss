using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MvvmDialogs;
using PonzianiSwiss.Resources;
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

    public partial class TournamentDialogViewModel : ViewModel, IModalDialogViewModel
    {
        public TournamentDialogViewModel(IDialogService dialogService, ILogger logger)
        {
            Logger = logger;
            this.dialogService = dialogService;
            TiebreakDialogCommand = new RelayCommand(TiebreakDialog);
            AdditionalRankingDialogCommand = new RelayCommand<AdditionalRanking?>((a) => AdditionalRankingDialog(a), (a) => true);
            DeleteAdditionalRankingCommand = new RelayCommand<AdditionalRanking?>((a) => DeleteAdditionalRanking(a), (a) => true);
        }

        [ObservableProperty]
        private bool? dialogResult;

        [ObservableProperty]
        private bool? teamRankingActive;

        [ObservableProperty]
        private int? teamSize;

        private Tournament? tournament;

        public Tournament? Tournament
        {
            get => tournament;
            set
            {
                tournament = value;
                if (tournament != null)
                {
                    foreach (var ar in tournament.AdditionalRankings)
                        _additionalRankings.Add(ar);
                    TeamRankingActive = tournament.TeamSize != null && tournament.TeamSize > 1;
                    TeamSize = tournament.TeamSize ?? 4;
                }
                OnPropertyChanged(nameof(Tournament));
            }
        }

        public ICommand TiebreakDialogCommand { get; }
        public ICommand AdditionalRankingDialogCommand { get; }
        public ICommand DeleteAdditionalRankingCommand { get; }

        [RelayCommand]
        void Ok()
        {
            DialogResult = true;
            if (tournament != null)
                tournament.TeamSize = TeamRankingActive.GetValueOrDefault(false) ? TeamSize : null;
        }

        [RelayCommand]
        void Cancel()
        {
            DialogResult = false;
        }

        private readonly IDialogService dialogService;

        private void TiebreakDialog()
        {
            ShowDialog(viewModel => dialogService?.ShowDialog(this, viewModel));
        }

        void AdditionalRankingDialog(AdditionalRanking? additionalRanking)
        {
            ShowAdditionalRankingDialog(viewModel => dialogService?.ShowDialog(this, viewModel), additionalRanking);
        }

        void DeleteAdditionalRanking(AdditionalRanking? additionalRanking)
        {
            if (additionalRanking != null)
            {
                Tournament?.AdditionalRankings.Remove(additionalRanking);
                _additionalRankings.Remove(additionalRanking);
            }
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

        private void ShowAdditionalRankingDialog(Func<AdditionalRankingDialogViewModel, bool?> showDialog, AdditionalRanking? additionalRanking)
        {
            LogCommand(additionalRanking?.Title ?? String.Empty);
            var dialogViewModel = App.Current.Services?.GetService<AdditionalRankingDialogViewModel>();

            if (dialogViewModel != null)
            {
                bool add = additionalRanking == null;
                dialogViewModel.AdditionalRanking = additionalRanking ?? new(LocalizedStrings.Instance["New_Additional_Ranking_Placeholder"]);
                bool? success = showDialog(dialogViewModel);
                if (success == true)
                {
                    if (dialogViewModel.AdditionalRanking != null)
                    {
                        if (add)
                        {
                            Tournament?.AdditionalRankings.Add(dialogViewModel.AdditionalRanking);
                            _additionalRankings.Add(dialogViewModel.AdditionalRanking);
                        }
                    }
                }
            }
        }

        private ObservableCollection<AdditionalRanking> _additionalRankings = new();

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

        public ObservableCollection<AdditionalRanking> AdditionalRankings { get => _additionalRankings; set => _additionalRankings = value; }

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
