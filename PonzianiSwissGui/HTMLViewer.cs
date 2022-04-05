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
        }
    }
}
