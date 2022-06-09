using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
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
using System.IO;

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaction logic for HTMLViewer.xaml
    /// </summary>
    public partial class HTMLViewer : MetroWindow
    {

        public string? Html { set; get; } = string.Empty;

        public HTMLViewer()
        {
            InitializeComponent();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new()
            {
                Filter = $"HTML  Files|*.html|PDF Files|*.pdf|All Files|*.*",
                FileName = $"{Title}.html",
                AddExtension = true
            };
            if (dialog.ShowDialog() ?? false)
            {
                if (Path.GetExtension(dialog.FileName) == ".pdf")
                {
                    await HTMLView.CoreWebView2.PrintToPdfAsync(dialog.FileName);
                }
                else File.WriteAllText(dialog.FileName, Html);
            }
        }

        private async void HTMLViewer_Load(object sender, RoutedEventArgs e)
        {
            if (Html != null)
            {
                await HTMLView.EnsureCoreWebView2Async();
                HTMLView.NavigateToString(Html);
            }
        }
    }

}
