using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls;
using MvvmDialogs;
using PonzianiSwiss.Resources;
using PonzianiSwissLib;
using System.Collections.Generic;

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaktionslogik für AdditionalAttributesDialog.xaml
    /// </summary>
    public partial class AdditionalRankingDialog : MetroWindow
    {

        private static Dictionary<AdditionalRankingDialogViewModel.SexExtended, string> SexItems = new()
        {
            { AdditionalRankingDialogViewModel.SexExtended.All, LocalizedStrings.Instance["SexExtended_All"] },
            { AdditionalRankingDialogViewModel.SexExtended.Male, LocalizedStrings.Instance["SexExtended_Male"] },
            { AdditionalRankingDialogViewModel.SexExtended.Female, LocalizedStrings.Instance["SexExtended_Female"] }
        };

        public AdditionalRankingDialog()
        {
            InitializeComponent();
            ComboBox_AdditionalRanking_Sex.ItemsSource = SexItems;
            ComboBox_AdditionalRanking_Sex.DisplayMemberPath = "Value";
            ComboBox_AdditionalRanking_Sex.SelectedValuePath = "Key";
        }
    }

    public partial class AdditionalRankingDialogViewModel : ViewModel, IModalDialogViewModel
    {

        public enum SexExtended { All, Male, Female }

        [ObservableProperty]
        private bool? dialogResult;

        [ObservableProperty]
        private AdditionalRanking? additionalRanking;

        public SexExtended Sex
        {
            get
            {
                if (AdditionalRanking?.Sex == null) return SexExtended.All;
                else
                {
                    return AdditionalRanking.Sex == PonzianiSwissLib.Sex.Male ? SexExtended.Male : SexExtended.Female;
                }
            }

            set
            {
                if (AdditionalRanking != null)
                {
                    switch (value)
                    {
                        case SexExtended.All: AdditionalRanking.Sex = null; break;
                        case SexExtended.Male: AdditionalRanking.Sex = PonzianiSwissLib.Sex.Male; break;
                        case SexExtended.Female: AdditionalRanking.Sex = PonzianiSwissLib.Sex.Female; break;
                    }
                }
                OnPropertyChanged(nameof(Sex));
            }
        }

        [RelayCommand]
        void Ok()
        {
            DialogResult = true;
        }

        [RelayCommand]
        void Cancel()
        {
            DialogResult = false;
        }

    }
}
