using AutoCompleteTextBox.Editors;
using MahApps.Metro.Controls;
using Microsoft.Extensions.Logging;
using PonzianiPlayerBase;
using PonzianiSwissLib;
using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaction logic for PlayerSearchDialog.xaml
    /// </summary>
    public partial class PlayerSearchDialog : MetroWindow
    {
        public PlayerSearchDialog(ILogger? logger)
        {
            Logger = logger;
            InitializeComponent();
            Model = new PlayerSearchModel(Logger);
            DataContext = Model;
            ComboBox_Base.ItemsSource = Enum.GetValues(typeof(PlayerBaseFactory.Base));
        }

        private readonly ILogger? Logger;

        public PlayerSearchModel Model { get; set; }


        internal static PlayerBaseFactory.Base PlayerBase = PlayerBaseFactory.Base.FIDE;

        private void PlayerSearchDialogOkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void PlayerSearchDialogCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ComboBox_Base_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlayerBase = (PlayerBaseFactory.Base)ComboBox_Base.SelectedItem;
            PlayerBaseFactory.Get(PlayerBase, Logger);
        }
    }

    public class PlayerSearchModel : ViewModel
    {
        private Player? player;

        public PlayerSearchModel(ILogger? logger)
        {
            Logger = logger;
        }

        [DependentProperties("IsFemale")]
        public Player? Player
        {
            get => player;

            set
            {
                player = value;
                RaisePropertyChange();
            }
        }

        public string SelectedName
        {
            set
            {
                if (value != null) UpdateFromSuggest(value);
            }
        }

        public bool IsFemale => Player?.Sex == Sex.Female;

        private void UpdateFromSuggest(string value)
        {
            string token = value.Split(' ').Last();
            string id = token[1..^1];
            try
            {
                string[] ids = id.Split('_');
                if (ids.Length == 2)
                    Player = PlayerBaseFactory.Get(PlayerSearchDialog.PlayerBase, Logger).GetById(ids[1]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public class NParticipantProvider : ISuggestionProvider
    {
        public IEnumerable GetSuggestions(string filter)
        {
            return PlayerBaseFactory.Get(PlayerSearchDialog.PlayerBase, null).Find(filter)
                .Select(p => $"{(p.Title != FideTitle.NONE ? p.Title : string.Empty)} {p.Name} {"(" + p.Id + ")"}".Trim());
        }
    }
}
