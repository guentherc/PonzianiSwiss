using AutoCompleteTextBox.Editors;
using MahApps.Metro.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using MvvmDialogs;
using PonzianiPlayerBase;
using PonzianiSwissLib;
using System;
using System.Collections;
using System.Linq;

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaction logic for PlayerSearchDialog.xaml
    /// </summary>
    public partial class PlayerSearchDialog : MetroWindow
    {
        public PlayerSearchDialog()
        {
            InitializeComponent();
            ComboBox_Base.ItemsSource = Enum.GetValues(typeof(PlayerBaseFactory.Base));
        }

    }

    public partial class PlayerSearchDialogViewModel : ViewModel, IModalDialogViewModel
    {
        internal static PlayerBaseFactory.Base playerBase = PlayerBaseFactory.Base.FIDE;

        public PlayerSearchDialogViewModel(ILogger logger)
        {
            Logger = logger;
        }

        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(IsFemale))]
        private Player? player;

        public bool IsFemale => Player?.Sex == Sex.Female;

        public string SelectedName
        {
            set
            {
                if (value != null) UpdateFromSuggest(value);
            }
        }

        public static PlayerBaseFactory.Base PlayerBase { get => playerBase; set => playerBase = value; }

        [ObservableProperty]
        private bool? dialogResult;

        private void UpdateFromSuggest(string value)
        {
            string token = value.Split(' ').Last();
            string id = token[1..^1];
            try
            {
                string[] ids = id.Split('_');
                Player = PlayerBaseFactory.Get(PlayerBase, Logger).GetById(ids.Last());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

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

        [ICommand]
        void SelectionChanged(object parameter)
        {
            PlayerBase = (PlayerBaseFactory.Base)parameter;
            _ = PlayerBaseFactory.Get(PlayerBase, Logger);
        }
    }

    public class NParticipantProvider : ISuggestionProvider
    {
        public IEnumerable GetSuggestions(string filter)
        {
            return PlayerBaseFactory.Get(PlayerSearchDialogViewModel.playerBase, null).Find(filter)
                .Select(p => $"{(p.Title != FideTitle.NONE ? p.Title : string.Empty)} {p.Name} {"(" + p.Id + ")"}".Trim());
        }
    }
}
