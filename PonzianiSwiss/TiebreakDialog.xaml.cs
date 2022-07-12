using MahApps.Metro.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using MvvmDialogs;
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
        public TiebreakDialog()
        {
            InitializeComponent();

        }
    }

    public partial class TiebreakDialogViewModel : ObservableObject, IModalDialogViewModel
    {
        private List<TieBreak>? tiebreaks;

        [ObservableProperty]
        private ObservableCollection<TiebreakExtended> available = new();

        [ObservableProperty]
        private ObservableCollection<TiebreakExtended> selected = new();

        public List<TieBreak>? Tiebreaks
        {
            get => tiebreaks;

            set
            {
                tiebreaks = value;
                Initialize();
            }
        }

        [ObservableProperty]
        public bool? dialogResult;

        private void Initialize()
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
        void Ok()
        {
            Update();
            DialogResult = true;
        }

        [ICommand]
        void Cancel()
        {
            DialogResult = false;
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
