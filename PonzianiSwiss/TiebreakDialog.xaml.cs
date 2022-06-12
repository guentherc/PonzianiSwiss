using MahApps.Metro.Controls;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for TiebreakDialog.xaml
    /// </summary>
    public partial class TiebreakDialog : MetroWindow
    {
        public TiebreakDialog(List<TieBreak> tieBreaks)
        {
            InitializeComponent();
            Model = new TiebreakModel(tieBreaks);
            DataContext = Model;
            lvAvailable.ItemsSource = Model.Available;
            lvSelected.ItemsSource = Model.Selected;
        }

        public TiebreakModel Model { get; set; }

        private void TiebreakDialogOkButton_Click(object sender, RoutedEventArgs e)
        {
            Model.Update();
            this.DialogResult = true;
            this.Close();
        }

        private void TiebreakDialogCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox)
            {
                var clickedTieBreak = (TieBreak)((CheckBox)(e.OriginalSource)).Tag;
                if (Model.IsSelected(clickedTieBreak))
                    Model.Unselect(clickedTieBreak);
                else 
                    Model.Select(clickedTieBreak);
            }

        }

        private void MenuItem_Move_Up(object sender, RoutedEventArgs e)
        {
            TiebreakExtended? t = lvSelected?.SelectedItem as TiebreakExtended;
            Model.MoveUp(t);
        }

        private void MenuItem_Move_Down(object sender, RoutedEventArgs e)
        {
            TiebreakExtended? t = lvSelected?.SelectedItem as TiebreakExtended;
            Model.MoveDown(t);
        }
    }

    public class TiebreakModel : ViewModel
    {
        public TiebreakModel(List<TieBreak> tieBreak)
        {
            this.Tiebreaks = tieBreak;
            var tiebreaks = Enum.GetValues<TieBreak>();
            foreach (var tiebreak in tiebreaks)
            {
                bool selected = Tiebreaks.Contains(tiebreak);
                TiebreakExtended tb = new(tiebreak, selected);
                Available.Add(tb);
                if (selected) Selected.Add(tb);
            }
        }
        public List<TieBreak> Tiebreaks { get; set; }

        public ObservableCollection<TiebreakExtended> Available { get; set; } = new();

        public ObservableCollection<TiebreakExtended> Selected { get; set; } = new();

        internal bool IsSelected(TieBreak tb)
        {
            return Selected.Any(t => t.Key == tb);
        }

        internal void Update()
        {
            Tiebreaks.Clear();
            foreach (var tiebreak in Selected) Tiebreaks.Add(tiebreak.Key);
        }

        internal void Select(TieBreak tb)
        {
            if (!IsSelected(tb))
            {
                var tbo = Available.First(t => t.Key == tb);
                tbo.IsSelected = true;
                Selected.Add(tbo);
            }
        }

        internal void Unselect(TieBreak tb)
        {
            if (IsSelected(tb))
            {
                var tbo = Selected.First(t => t.Key == tb);
                tbo.IsSelected = false;
                Selected.Remove(tbo);
            }
        }

        internal void MoveUp(TiebreakExtended tb)
        {
            int index = Selected.IndexOf(tb);
            if (index > 0)
            {
                Selected.RemoveAt(index);
                Selected.Insert(index - 1, tb);
            }
        }

        internal void MoveDown(TiebreakExtended tb)
        {
            int index = Selected.IndexOf(tb);
            if (index < Selected.Count - 1)
            {
                Selected.RemoveAt(index);
                Selected.Insert(index + 1, tb);
            }
        }
    }

    public class TiebreakExtended
    {
        public TiebreakExtended(TieBreak key, bool isSelected = false)
        {
            this.Key = key;
            this.IsSelected = isSelected;
        }

        public TieBreak Key { get; set; }

        public string Text => Key.ToString();
        public bool IsSelected { get; set; } = false;
    }

}
