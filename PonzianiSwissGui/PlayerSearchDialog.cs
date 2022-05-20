using PonzianiPlayerBase;
using PonzianiSwissLib;
using System.Data;

namespace PonzianiSwissGui
{
    public partial class PlayerSearchDialog : Form
    {
        public Player? Player { get; set; }

        private IPlayerBase PlayerBase = PlayerBaseFactory.Get(PlayerBaseFactory.Base.FIDE);

        public PlayerSearchDialog()
        {
            InitializeComponent();
            cbDataSource.DataSource = PlayerBaseFactory.AvailableBases;
            cbDataSource.DisplayMember = "Value";
            cbDataSource.ValueMember = "Key";
        }

        private void PlayerSearchDialog_Shown(object sender, EventArgs e)
        {

        }

        private void CbDataSource_SelectedValueChanged(object sender, EventArgs e)
        {
            var selectedItem = (KeyValuePair<PlayerBaseFactory.Base, string>)cbDataSource.SelectedItem;
            PlayerBase = PlayerBaseFactory.Get(selectedItem.Key);
        }

        private void TbName_KeyUp(object sender, KeyEventArgs e)
        {
            if (tbName.Text.Length == 4)
            {
                Invoke((MethodInvoker)(() =>
                {
                    tbName.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    tbName.AutoCompleteCustomSource = new();
                    tbName.AutoCompleteCustomSource.AddRange(PlayerBase.Find(tbName.Text).Select(p => p.Name).ToArray());
                }));
            }
            if (tbName.AutoCompleteCustomSource.Count > 0)
            {
                Invoke((MethodInvoker)(() =>
                {
                    var player = PlayerBase.Find(tbName.Text, 1);
                    if (player.Count > 0)
                    {
                        Player = player[0];
                        UpdateUI();
                    }
                }));
            }
        }

        private void UpdateUI()
        {
            if (Player != null)
            {
                tbId.Text = Player.Id;
                tbRating.Text = Player.Rating.ToString();
                tbClub.Text = Player.Club;
                tbFederation.Text = Player.Federation;
                cbFemale.Checked = Player.Sex == Sex.Female;
                tbYearOfBirth.Text = Player.YearOfBirth.ToString();
            }
            else
            {
                tbId.Text = String.Empty;
                tbRating.Text = String.Empty;
                tbClub.Text = String.Empty;
                tbFederation.Text = String.Empty;
                cbFemale.Checked = false;
                tbYearOfBirth.Text = String.Empty;
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (Player != null) DialogResult = DialogResult.OK;
            Close();
        }
    }
}
