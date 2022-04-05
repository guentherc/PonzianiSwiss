using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PonzianiSwissGui
{
    public partial class HTMLViewer : Form
    {

        public string? Html { set; get; }
        public string? Title { set; get; }
        public HTMLViewer()
        {
            InitializeComponent();
        }

        private void HTMLViewer_Load(object sender, EventArgs e)
        {
            if (Html != null)
            {
                WebViewer.NavigateToString(Html);
            }
            if (Title != null)
            {
                this.Text = Title;
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new()
            {
                Filter = $"{Properties.Strings.HTMLFiles}|*.html|{Properties.Strings.PDFFiles}|*.pdf|{Properties.Strings.AllFiles}|*.*",
                FileName = $"{Title}.html",
                AddExtension = true
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (Path.GetExtension(dialog.FileName) == ".pdf")
                {
                    bool result = await WebViewer.CoreWebView2.PrintToPdfAsync(dialog.FileName).ConfigureAwait(false);
                }
                else File.WriteAllText(dialog.FileName, Html);
            }
        }
    }
}
