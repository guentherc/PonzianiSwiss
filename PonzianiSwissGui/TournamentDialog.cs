using PonzianiSwissLib;
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
    public partial class TournamentDialog : Form
    {
        public TournamentDialog(Tournament tournament)
        {
            InitializeComponent();
            Tournament = tournament;
        }

        public Tournament Tournament { private set; get; }

        private void TournamentDialog_Shown(object sender, EventArgs e)
        {
            tbName.Text = Tournament.Name;
            btnOk.Enabled = tbName.Text?.Length > 0;
            tbCity.Text = Tournament.City;
            tbFederation.Text = Tournament.Federation;
            nudRounds.Value = Tournament.CountRounds;
            if (DateTime.TryParse(Tournament.StartDate, out DateTime date)) dtpStart.Value = date; else dtpStart.Value = DateTime.Now;
            if (DateTime.TryParse(Tournament.EndDate, out date)) dtpEnd.Value = date; else dtpEnd.Value = DateTime.Now;
            tbChiefArbiter.Text = Tournament.ChiefArbiter;
            tbArbiters.Text = Tournament.DeputyChiefArbiter;
            List<KeyValuePair<PairingSystem, string>> psl = new List<KeyValuePair<PairingSystem, string>>();
            foreach (PairingSystem ps in Enum.GetValues(typeof(PairingSystem)))
            {
                psl.Add(new KeyValuePair<PairingSystem, string>(ps, ((int)ps).ToString()));
            }
            cbPairingSystem.DataSource = psl;
            cbPairingSystem.DisplayMember = "Key";
            cbPairingSystem.ValueMember = "Value";
            cbPairingSystem.SelectedValue = ((int)Tournament.PairingSystem).ToString();
            tbTimeControl.Text = Tournament.TimeControl;
            tbPointsForWin.Text = Tournament.ScoringScheme.PointsForWin.ToString("F1");
            tbPointsForDraw.Text = Tournament.ScoringScheme.PointsForDraw.ToString("F1");
            tbPointsForPlayedLoss.Text = Tournament.ScoringScheme.PointsForPlayedLoss.ToString("F1");
            tbPointsForZPB.Text = Tournament.ScoringScheme.PointsForZeroPointBye.ToString("F1");
            tbPointsForForfeit.Text = Tournament.ScoringScheme.PointsForForfeitedLoss.ToString("F1");
            tbPointsForPAB.Text = Tournament.ScoringScheme.PointsForPairingAllocatedBye.ToString("F1");
        }

        private bool ValidateUserInput()
        {
            if (dtpEnd.Value <= dtpStart.Value)
            {
                ErrorProvider.SetError(dtpEnd, Properties.Strings.EndDateLTStartDate);
                return false;
            }
            foreach (var control in gpScoringScheme.Controls)
            {
                if (control is TextBox && ((TextBox)control).Name.StartsWith("tbPoints"))
                {
                    if (!float.TryParse(((TextBox)control).Text, out _))
                    {
                        ErrorProvider.SetError((Control)control, Properties.Strings.InvalidFloat);
                        return false;
                    }
                }
            }
            return true;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!ValidateUserInput()) return;
            UpdateFromUI();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void UpdateFromUI()
        {
            Tournament.Name = tbName.Text;
            Tournament.City = tbCity.Text;
            Tournament.Federation = tbFederation.Text;
            Tournament.CountRounds = (int)nudRounds.Value;
            Tournament.StartDate = dtpStart.Value.ToShortDateString();
            Tournament.EndDate = dtpEnd.Value.ToShortDateString();
            Tournament.ChiefArbiter = tbChiefArbiter.Text;
            Tournament.DeputyChiefArbiter = tbArbiters.Text;
            Tournament.PairingSystem = (PairingSystem)int.Parse((string)cbPairingSystem.SelectedValue);
            Tournament.TimeControl = tbTimeControl.Text;
            Tournament.ScoringScheme.PointsForWin = float.Parse(tbPointsForWin.Text);
            Tournament.ScoringScheme.PointsForDraw = float.Parse(tbPointsForDraw.Text);
            Tournament.ScoringScheme.PointsForPlayedLoss = float.Parse(tbPointsForPlayedLoss.Text);
            Tournament.ScoringScheme.PointsForZeroPointBye = float.Parse(tbPointsForZPB.Text);
            Tournament.ScoringScheme.PointsForForfeitedLoss = float.Parse(tbPointsForForfeit.Text);
            Tournament.ScoringScheme.PointsForPairingAllocatedBye = float.Parse(tbPointsForPAB.Text);
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = tbName.Text.Length > 0;
        }

        private void tbscoringSchemeFloat_Validating(object sender, CancelEventArgs e)
        {
            if (!float.TryParse(((TextBox)sender).Text, out float val)) 
                ErrorProvider.SetError((Control)sender, Properties.Strings.InvalidFloat);
            else ((TextBox)sender).Text = val.ToString("F1");

        }
    }
}
