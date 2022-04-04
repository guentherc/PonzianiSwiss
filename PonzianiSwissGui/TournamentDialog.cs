using PonzianiSwissLib;
using System.ComponentModel;

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
            Utils.PrepareFederationComboBox(cbFederation);
            if (Tournament.Federation != null) cbFederation.SelectedValue = Tournament.Federation;
            nudRounds.Value = Tournament.CountRounds != 0 ? Tournament.CountRounds : 9;
            if (DateTime.TryParse(Tournament.StartDate, out DateTime date)) dtpStart.Value = date; else dtpStart.Value = DateTime.Now;
            if (DateTime.TryParse(Tournament.EndDate, out date)) dtpEnd.Value = date; else dtpEnd.Value = DateTime.Now;
            tbChiefArbiter.Text = Tournament.ChiefArbiter;
            tbArbiters.Text = Tournament.DeputyChiefArbiter;
            List<KeyValuePair<PairingSystem, string>> psl = new();
            foreach (PairingSystem ps in Enum.GetValues(typeof(PairingSystem)))
            {
                psl.Add(new KeyValuePair<PairingSystem, string>(ps, ((int)ps).ToString()));
            }
            cbPairingSystem.DataSource = psl;
            cbPairingSystem.DisplayMember = "Key";
            cbPairingSystem.ValueMember = "Value";
            cbPairingSystem.SelectedValue = ((int)Tournament.PairingSystem).ToString();
            tbTimeControl.Text = Tournament.TimeControl;
            List<KeyValuePair<TournamentRatingDetermination, string>> rdl = new();
            foreach (TournamentRatingDetermination trd in Enum.GetValues(typeof(TournamentRatingDetermination)))
            {
                rdl.Add(new KeyValuePair<TournamentRatingDetermination, string>(trd, ((int)trd).ToString()));
            }
            cbRatingDetermination.DataSource = rdl;
            cbRatingDetermination.DisplayMember = "Key";
            cbRatingDetermination.ValueMember = "Value";
            cbRatingDetermination.SelectedValue = ((int)Tournament.RatingDetermination).ToString();
            tbPointsForWin.Text = Tournament.ScoringScheme.PointsForWin.ToString("F1");
            tbPointsForDraw.Text = Tournament.ScoringScheme.PointsForDraw.ToString("F1");
            tbPointsForPlayedLoss.Text = Tournament.ScoringScheme.PointsForPlayedLoss.ToString("F1");
            tbPointsForZPB.Text = Tournament.ScoringScheme.PointsForZeroPointBye.ToString("F1");
            tbPointsForForfeit.Text = Tournament.ScoringScheme.PointsForForfeitedLoss.ToString("F1");
            tbPointsForPAB.Text = Tournament.ScoringScheme.PointsForPairingAllocatedBye.ToString("F1");
        }

        private bool ValidateUserInput()
        {
            if (dtpEnd.Value < dtpStart.Value)
            {
                ErrorProvider.SetError(dtpEnd, Properties.Strings.EndDateLTStartDate);
                return false;
            }
            foreach (var control in gpScoringScheme.Controls)
            {
                if (control is TextBox box && box.Name.StartsWith("tbPoints"))
                {
                    if (!float.TryParse(box.Text, out _))
                    {
                        ErrorProvider.SetError((Control)control, Properties.Strings.InvalidFloat);
                        return false;
                    }
                }
            }
            return true;
        }

        private void BtnOk_Click(object sender, EventArgs e)
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
            Tournament.Federation = cbFederation.SelectedValue.ToString();
            Tournament.CountRounds = (int)nudRounds.Value;
            Tournament.StartDate = dtpStart.Value.ToShortDateString();
            Tournament.EndDate = dtpEnd.Value.ToShortDateString();
            Tournament.ChiefArbiter = tbChiefArbiter.Text;
            Tournament.DeputyChiefArbiter = tbArbiters.Text;
            Tournament.PairingSystem = (PairingSystem)cbPairingSystem.SelectedIndex;
            Tournament.TimeControl = tbTimeControl.Text;
            Tournament.RatingDetermination = (TournamentRatingDetermination)(int.Parse((string)cbRatingDetermination.SelectedValue));
            Tournament.ScoringScheme.PointsForWin = float.Parse(tbPointsForWin.Text);
            Tournament.ScoringScheme.PointsForDraw = float.Parse(tbPointsForDraw.Text);
            Tournament.ScoringScheme.PointsForPlayedLoss = float.Parse(tbPointsForPlayedLoss.Text);
            Tournament.ScoringScheme.PointsForZeroPointBye = float.Parse(tbPointsForZPB.Text);
            Tournament.ScoringScheme.PointsForForfeitedLoss = float.Parse(tbPointsForForfeit.Text);
            Tournament.ScoringScheme.PointsForPairingAllocatedBye = float.Parse(tbPointsForPAB.Text);
            Tournament.BakuAcceleration = cbAcceleration.Enabled && cbAcceleration.Checked;
        }

        private void TbName_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = tbName.Text.Length > 0;
        }

        private void TbscoringSchemeFloat_Validating(object sender, CancelEventArgs e)
        {
            if (!float.TryParse(((TextBox)sender).Text, out float val))
                ErrorProvider.SetError((Control)sender, Properties.Strings.InvalidFloat);
            else ((TextBox)sender).Text = val.ToString("F1");

        }

        private void CbPairingSystem_SelectedValueChanged(object sender, EventArgs e)
        {
            PairingSystem ps = (PairingSystem)cbPairingSystem.SelectedIndex;
            cbAcceleration.Enabled = ps != PairingSystem.Burstein;
        }
    }
}
