namespace PonzianiSwissGui
{
    partial class PlayerSearchDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlayerSearchDialog));
            this.lblDataSource = new System.Windows.Forms.Label();
            this.cbDataSource = new System.Windows.Forms.ComboBox();
            this.lblName = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.lblId = new System.Windows.Forms.Label();
            this.tbId = new System.Windows.Forms.TextBox();
            this.gpPlayerData = new System.Windows.Forms.GroupBox();
            this.tbYearOfBirth = new System.Windows.Forms.TextBox();
            this.lblYearOfBirth = new System.Windows.Forms.Label();
            this.cbFemale = new System.Windows.Forms.CheckBox();
            this.tbFederation = new System.Windows.Forms.TextBox();
            this.lblFederation = new System.Windows.Forms.Label();
            this.tbClub = new System.Windows.Forms.TextBox();
            this.lblClub = new System.Windows.Forms.Label();
            this.tbRating = new System.Windows.Forms.TextBox();
            this.lblRating = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.gpPlayerData.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblDataSource
            // 
            resources.ApplyResources(this.lblDataSource, "lblDataSource");
            this.lblDataSource.Name = "lblDataSource";
            // 
            // cbDataSource
            // 
            resources.ApplyResources(this.cbDataSource, "cbDataSource");
            this.cbDataSource.FormattingEnabled = true;
            this.cbDataSource.Name = "cbDataSource";
            this.cbDataSource.SelectedValueChanged += new System.EventHandler(this.CbDataSource_SelectedValueChanged);
            // 
            // lblName
            // 
            resources.ApplyResources(this.lblName, "lblName");
            this.lblName.Name = "lblName";
            // 
            // tbName
            // 
            resources.ApplyResources(this.tbName, "tbName");
            this.tbName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.tbName.Name = "tbName";
            this.tbName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TbName_KeyUp);
            // 
            // lblId
            // 
            resources.ApplyResources(this.lblId, "lblId");
            this.lblId.Name = "lblId";
            // 
            // tbId
            // 
            resources.ApplyResources(this.tbId, "tbId");
            this.tbId.Name = "tbId";
            this.tbId.ReadOnly = true;
            // 
            // gpPlayerData
            // 
            resources.ApplyResources(this.gpPlayerData, "gpPlayerData");
            this.gpPlayerData.Controls.Add(this.tbYearOfBirth);
            this.gpPlayerData.Controls.Add(this.lblYearOfBirth);
            this.gpPlayerData.Controls.Add(this.cbFemale);
            this.gpPlayerData.Controls.Add(this.tbFederation);
            this.gpPlayerData.Controls.Add(this.lblFederation);
            this.gpPlayerData.Controls.Add(this.tbClub);
            this.gpPlayerData.Controls.Add(this.lblClub);
            this.gpPlayerData.Controls.Add(this.tbRating);
            this.gpPlayerData.Controls.Add(this.lblRating);
            this.gpPlayerData.Controls.Add(this.tbId);
            this.gpPlayerData.Controls.Add(this.lblId);
            this.gpPlayerData.Name = "gpPlayerData";
            this.gpPlayerData.TabStop = false;
            // 
            // tbYearOfBirth
            // 
            resources.ApplyResources(this.tbYearOfBirth, "tbYearOfBirth");
            this.tbYearOfBirth.Name = "tbYearOfBirth";
            this.tbYearOfBirth.ReadOnly = true;
            // 
            // lblYearOfBirth
            // 
            resources.ApplyResources(this.lblYearOfBirth, "lblYearOfBirth");
            this.lblYearOfBirth.Name = "lblYearOfBirth";
            // 
            // cbFemale
            // 
            resources.ApplyResources(this.cbFemale, "cbFemale");
            this.cbFemale.Name = "cbFemale";
            this.cbFemale.UseVisualStyleBackColor = true;
            // 
            // tbFederation
            // 
            resources.ApplyResources(this.tbFederation, "tbFederation");
            this.tbFederation.Name = "tbFederation";
            this.tbFederation.ReadOnly = true;
            // 
            // lblFederation
            // 
            resources.ApplyResources(this.lblFederation, "lblFederation");
            this.lblFederation.Name = "lblFederation";
            // 
            // tbClub
            // 
            resources.ApplyResources(this.tbClub, "tbClub");
            this.tbClub.Name = "tbClub";
            this.tbClub.ReadOnly = true;
            // 
            // lblClub
            // 
            resources.ApplyResources(this.lblClub, "lblClub");
            this.lblClub.Name = "lblClub";
            // 
            // tbRating
            // 
            resources.ApplyResources(this.tbRating, "tbRating");
            this.tbRating.Name = "tbRating";
            this.tbRating.ReadOnly = true;
            // 
            // lblRating
            // 
            resources.ApplyResources(this.lblRating, "lblRating");
            this.lblRating.Name = "lblRating";
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.Name = "btnOk";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // PlayerSearchDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.gpPlayerData);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.cbDataSource);
            this.Controls.Add(this.lblDataSource);
            this.Name = "PlayerSearchDialog";
            this.Shown += new System.EventHandler(this.PlayerSearchDialog_Shown);
            this.gpPlayerData.ResumeLayout(false);
            this.gpPlayerData.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label lblDataSource;
        private ComboBox cbDataSource;
        private Label lblName;
        private TextBox tbName;
        private Label lblId;
        private TextBox tbId;
        private GroupBox gpPlayerData;
        private TextBox tbRating;
        private Label lblRating;
        private TextBox tbClub;
        private Label lblClub;
        private TextBox tbFederation;
        private Label lblFederation;
        private CheckBox cbFemale;
        private TextBox tbYearOfBirth;
        private Label lblYearOfBirth;
        private Button btnCancel;
        private Button btnOk;
    }
}