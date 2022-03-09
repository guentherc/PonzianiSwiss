using PonzianiSwissLib;

namespace PonzianiSwissGui
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public Tournament Tournament { set; get; }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TournamentDialog td = new TournamentDialog(new());
            if (td.ShowDialog() == DialogResult.OK)
            {
                Tournament = td.Tournament;
                this.Text = Tournament.Name;
            }
        }
    }
}