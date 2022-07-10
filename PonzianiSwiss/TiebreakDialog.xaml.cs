using MahApps.Metro.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

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

            DataContext = new TiebreakDialogViewModel()
            {
                Tiebreaks = new ObservableCollection<TieBreak>(tieBreaks)
            };
            ((TiebreakDialogViewModel)DataContext).Initialize();
            lvAvailable.ItemsSource = ((TiebreakDialogViewModel)DataContext).Available;
            lvSelected.ItemsSource = ((TiebreakDialogViewModel)DataContext).Selected;

        }

        public List<TieBreak> Tiebreaks => ((TiebreakDialogViewModel)DataContext)?.Tiebreaks?.ToList() ?? new();

    }

    public partial class TiebreakDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<TieBreak>? tiebreaks;

        [ObservableProperty]
        private ObservableCollection<TiebreakExtended> available = new();

        [ObservableProperty]
        private ObservableCollection<TiebreakExtended> selected = new();

        internal void Initialize()
        {
            var tiebreaks = Enum.GetValues<TieBreak>();
            foreach (var tiebreak in tiebreaks)
            {
                bool selected = Tiebreaks?.Contains(tiebreak) ?? false;
                TiebreakExtended tb = new(tiebreak, selected);
                available.Add(tb);
                if (selected) Selected.Add(tb);
            }
        }

        private bool IsSelected(TieBreak tb)
        {
            return Selected.Any(t => t.Key == tb);
        }

        [ICommand]
        void Update()
        {
            Tiebreaks?.Clear();
            foreach (var tiebreak in Selected) Tiebreaks?.Add(tiebreak.Key);
        }

        [ICommand]
        void Select(TieBreak tb)
        {
            if (!IsSelected(tb))
            {
                var tbo = Available.First(t => t.Key == tb);
                tbo.IsSelected = true;
                Selected.Add(tbo);
            }
        }

        [ICommand]
        void Unselect(TieBreak tb)
        {
            if (IsSelected(tb))
            {
                var tbo = Selected.First(t => t.Key == tb);
                tbo.IsSelected = false;
                Selected.Remove(tbo);
            }
        }

        [ICommand]
        void MoveUp(TiebreakExtended tb)
        {
            int index = Selected.IndexOf(tb);
            if (index > 0)
            {
                Selected.RemoveAt(index);
                Selected.Insert(index - 1, tb);
            }
        }

        [ICommand]
        void MoveDown(TiebreakExtended tb)
        {
            int index = Selected.IndexOf(tb);
            if (index < Selected.Count - 1)
            {
                Selected.RemoveAt(index);
                Selected.Insert(index + 1, tb);
            }
        }

        [ICommand]
        void Ok(object parameter)
        {
            Update();
            MetroWindow wnd = (MetroWindow)parameter;
            wnd.DialogResult = true;
            wnd.Close();
        }

        [ICommand]
        void Cancel(object parameter)
        {
            MetroWindow wnd = (MetroWindow)parameter;
            wnd.DialogResult = false;
            wnd.Close();
        }

        [ICommand]
        void Toggle(object parameter)
        {
            TieBreak clickedTiebreak = (TieBreak)parameter;
            if (IsSelected(clickedTiebreak))
                Unselect(clickedTiebreak);
            else
                Select(clickedTiebreak);
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
