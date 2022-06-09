using AutoCompleteTextBox.Editors;
using MahApps.Metro.Controls;
using PonzianiPlayerBase;
using PonzianiSwissLib;
using System;
using System.Collections;
using System.Collections.Generic;
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
    /// Interaction logic for PlayerSearchDialog.xaml
    /// </summary>
    public partial class PlayerSearchDialog : MetroWindow
    {
        public PlayerSearchDialog()
        {
            InitializeComponent();
            Model = new PlayerSearchModel();
            DataContext = Model;
            ComboBox_Base.ItemsSource = Enum.GetValues(typeof(PlayerBaseFactory.Base));
        }

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
        }
    }

    public class PlayerSearchModel : ViewModel
    {
        private Player? player;

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
                    Player = PlayerBaseFactory.Get(PlayerSearchDialog.PlayerBase).GetById(ids[1]);
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
            return PlayerBaseFactory.Get(PlayerSearchDialog.PlayerBase).Find(filter)
                .Select(p => $"{(p.Title != FideTitle.NONE ? p.Title : string.Empty)} {p.Name} {"(" + p.Id + ")"}".Trim());
        }
    }
}
