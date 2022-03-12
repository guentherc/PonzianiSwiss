namespace PonzianiSwissGui
{
    partial class TournamentDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TournamentDialog));
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblName = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.tbCity = new System.Windows.Forms.TextBox();
            this.lblCity = new System.Windows.Forms.Label();
            this.lblFederation = new System.Windows.Forms.Label();
            this.lblRounds = new System.Windows.Forms.Label();
            this.nudRounds = new System.Windows.Forms.NumericUpDown();
            this.lblDate = new System.Windows.Forms.Label();
            this.dtpStart = new System.Windows.Forms.DateTimePicker();
            this.lblHyphen = new System.Windows.Forms.Label();
            this.dtpEnd = new System.Windows.Forms.DateTimePicker();
            this.ErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.lblChiefArbiter = new System.Windows.Forms.Label();
            this.tbChiefArbiter = new System.Windows.Forms.TextBox();
            this.tbArbiters = new System.Windows.Forms.TextBox();
            this.lblArbiters = new System.Windows.Forms.Label();
            this.lblPairingSystem = new System.Windows.Forms.Label();
            this.cbPairingSystem = new System.Windows.Forms.ComboBox();
            this.tbTimeControl = new System.Windows.Forms.TextBox();
            this.lblTimeControl = new System.Windows.Forms.Label();
            this.gpScoringScheme = new System.Windows.Forms.GroupBox();
            this.tbPointsForPAB = new System.Windows.Forms.TextBox();
            this.lblPointsForPAB = new System.Windows.Forms.Label();
            this.tbPointsForForfeit = new System.Windows.Forms.TextBox();
            this.lblPointsForForfeit = new System.Windows.Forms.Label();
            this.tbPointsForZPB = new System.Windows.Forms.TextBox();
            this.lblPointsForZPB = new System.Windows.Forms.Label();
            this.tbPointsForPlayedLoss = new System.Windows.Forms.TextBox();
            this.lblPointsForPlayedLoss = new System.Windows.Forms.Label();
            this.tbPointsForDraw = new System.Windows.Forms.TextBox();
            this.lblPointsForDraw = new System.Windows.Forms.Label();
            this.tbPointsForWin = new System.Windows.Forms.TextBox();
            this.lblPointsForWin = new System.Windows.Forms.Label();
            this.cbFederation = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudRounds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).BeginInit();
            this.gpScoringScheme.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.Name = "btnOk";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblName
            // 
            resources.ApplyResources(this.lblName, "lblName");
            this.lblName.Name = "lblName";
            // 
            // tbName
            // 
            resources.ApplyResources(this.tbName, "tbName");
            this.tbName.Name = "tbName";
            this.tbName.TextChanged += new System.EventHandler(this.TbName_TextChanged);
            // 
            // tbCity
            // 
            resources.ApplyResources(this.tbCity, "tbCity");
            this.tbCity.Name = "tbCity";
            // 
            // lblCity
            // 
            resources.ApplyResources(this.lblCity, "lblCity");
            this.lblCity.Name = "lblCity";
            // 
            // lblFederation
            // 
            resources.ApplyResources(this.lblFederation, "lblFederation");
            this.lblFederation.Name = "lblFederation";
            // 
            // lblRounds
            // 
            resources.ApplyResources(this.lblRounds, "lblRounds");
            this.lblRounds.Name = "lblRounds";
            // 
            // nudRounds
            // 
            resources.ApplyResources(this.nudRounds, "nudRounds");
            this.nudRounds.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.nudRounds.Name = "nudRounds";
            // 
            // lblDate
            // 
            resources.ApplyResources(this.lblDate, "lblDate");
            this.lblDate.Name = "lblDate";
            // 
            // dtpStart
            // 
            this.dtpStart.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            resources.ApplyResources(this.dtpStart, "dtpStart");
            this.dtpStart.Name = "dtpStart";
            // 
            // lblHyphen
            // 
            resources.ApplyResources(this.lblHyphen, "lblHyphen");
            this.lblHyphen.Name = "lblHyphen";
            // 
            // dtpEnd
            // 
            this.dtpEnd.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            resources.ApplyResources(this.dtpEnd, "dtpEnd");
            this.dtpEnd.Name = "dtpEnd";
            // 
            // ErrorProvider
            // 
            this.ErrorProvider.ContainerControl = this;
            // 
            // lblChiefArbiter
            // 
            resources.ApplyResources(this.lblChiefArbiter, "lblChiefArbiter");
            this.lblChiefArbiter.Name = "lblChiefArbiter";
            // 
            // tbChiefArbiter
            // 
            resources.ApplyResources(this.tbChiefArbiter, "tbChiefArbiter");
            this.tbChiefArbiter.Name = "tbChiefArbiter";
            // 
            // tbArbiters
            // 
            resources.ApplyResources(this.tbArbiters, "tbArbiters");
            this.tbArbiters.Name = "tbArbiters";
            // 
            // lblArbiters
            // 
            resources.ApplyResources(this.lblArbiters, "lblArbiters");
            this.lblArbiters.Name = "lblArbiters";
            // 
            // lblPairingSystem
            // 
            resources.ApplyResources(this.lblPairingSystem, "lblPairingSystem");
            this.lblPairingSystem.Name = "lblPairingSystem";
            // 
            // cbPairingSystem
            // 
            this.cbPairingSystem.FormattingEnabled = true;
            resources.ApplyResources(this.cbPairingSystem, "cbPairingSystem");
            this.cbPairingSystem.Name = "cbPairingSystem";
            // 
            // tbTimeControl
            // 
            resources.ApplyResources(this.tbTimeControl, "tbTimeControl");
            this.tbTimeControl.Name = "tbTimeControl";
            // 
            // lblTimeControl
            // 
            resources.ApplyResources(this.lblTimeControl, "lblTimeControl");
            this.lblTimeControl.Name = "lblTimeControl";
            // 
            // gpScoringScheme
            // 
            resources.ApplyResources(this.gpScoringScheme, "gpScoringScheme");
            this.gpScoringScheme.Controls.Add(this.tbPointsForPAB);
            this.gpScoringScheme.Controls.Add(this.lblPointsForPAB);
            this.gpScoringScheme.Controls.Add(this.tbPointsForForfeit);
            this.gpScoringScheme.Controls.Add(this.lblPointsForForfeit);
            this.gpScoringScheme.Controls.Add(this.tbPointsForZPB);
            this.gpScoringScheme.Controls.Add(this.lblPointsForZPB);
            this.gpScoringScheme.Controls.Add(this.tbPointsForPlayedLoss);
            this.gpScoringScheme.Controls.Add(this.lblPointsForPlayedLoss);
            this.gpScoringScheme.Controls.Add(this.tbPointsForDraw);
            this.gpScoringScheme.Controls.Add(this.lblPointsForDraw);
            this.gpScoringScheme.Controls.Add(this.tbPointsForWin);
            this.gpScoringScheme.Controls.Add(this.lblPointsForWin);
            this.gpScoringScheme.Name = "gpScoringScheme";
            this.gpScoringScheme.TabStop = false;
            // 
            // tbPointsForPAB
            // 
            resources.ApplyResources(this.tbPointsForPAB, "tbPointsForPAB");
            this.tbPointsForPAB.Name = "tbPointsForPAB";
            this.tbPointsForPAB.Validating += new System.ComponentModel.CancelEventHandler(this.TbscoringSchemeFloat_Validating);
            // 
            // lblPointsForPAB
            // 
            resources.ApplyResources(this.lblPointsForPAB, "lblPointsForPAB");
            this.lblPointsForPAB.Name = "lblPointsForPAB";
            // 
            // tbPointsForForfeit
            // 
            resources.ApplyResources(this.tbPointsForForfeit, "tbPointsForForfeit");
            this.tbPointsForForfeit.Name = "tbPointsForForfeit";
            this.tbPointsForForfeit.Validating += new System.ComponentModel.CancelEventHandler(this.TbscoringSchemeFloat_Validating);
            // 
            // lblPointsForForfeit
            // 
            resources.ApplyResources(this.lblPointsForForfeit, "lblPointsForForfeit");
            this.lblPointsForForfeit.Name = "lblPointsForForfeit";
            // 
            // tbPointsForZPB
            // 
            resources.ApplyResources(this.tbPointsForZPB, "tbPointsForZPB");
            this.tbPointsForZPB.Name = "tbPointsForZPB";
            this.tbPointsForZPB.Validating += new System.ComponentModel.CancelEventHandler(this.TbscoringSchemeFloat_Validating);
            // 
            // lblPointsForZPB
            // 
            resources.ApplyResources(this.lblPointsForZPB, "lblPointsForZPB");
            this.lblPointsForZPB.Name = "lblPointsForZPB";
            // 
            // tbPointsForPlayedLoss
            // 
            resources.ApplyResources(this.tbPointsForPlayedLoss, "tbPointsForPlayedLoss");
            this.tbPointsForPlayedLoss.Name = "tbPointsForPlayedLoss";
            this.tbPointsForPlayedLoss.Validating += new System.ComponentModel.CancelEventHandler(this.TbscoringSchemeFloat_Validating);
            // 
            // lblPointsForPlayedLoss
            // 
            resources.ApplyResources(this.lblPointsForPlayedLoss, "lblPointsForPlayedLoss");
            this.lblPointsForPlayedLoss.Name = "lblPointsForPlayedLoss";
            // 
            // tbPointsForDraw
            // 
            resources.ApplyResources(this.tbPointsForDraw, "tbPointsForDraw");
            this.tbPointsForDraw.Name = "tbPointsForDraw";
            this.tbPointsForDraw.Validating += new System.ComponentModel.CancelEventHandler(this.TbscoringSchemeFloat_Validating);
            // 
            // lblPointsForDraw
            // 
            resources.ApplyResources(this.lblPointsForDraw, "lblPointsForDraw");
            this.lblPointsForDraw.Name = "lblPointsForDraw";
            // 
            // tbPointsForWin
            // 
            resources.ApplyResources(this.tbPointsForWin, "tbPointsForWin");
            this.tbPointsForWin.Name = "tbPointsForWin";
            this.tbPointsForWin.Validating += new System.ComponentModel.CancelEventHandler(this.TbscoringSchemeFloat_Validating);
            // 
            // lblPointsForWin
            // 
            resources.ApplyResources(this.lblPointsForWin, "lblPointsForWin");
            this.lblPointsForWin.Name = "lblPointsForWin";
            // 
            // cbFederation
            // 
            resources.ApplyResources(this.cbFederation, "cbFederation");
            this.cbFederation.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbFederation.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbFederation.FormattingEnabled = true;
            this.cbFederation.Name = "cbFederation";
            // 
            // TournamentDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.cbFederation);
            this.Controls.Add(this.gpScoringScheme);
            this.Controls.Add(this.tbTimeControl);
            this.Controls.Add(this.lblTimeControl);
            this.Controls.Add(this.cbPairingSystem);
            this.Controls.Add(this.lblPairingSystem);
            this.Controls.Add(this.tbArbiters);
            this.Controls.Add(this.lblArbiters);
            this.Controls.Add(this.tbChiefArbiter);
            this.Controls.Add(this.lblChiefArbiter);
            this.Controls.Add(this.dtpEnd);
            this.Controls.Add(this.lblHyphen);
            this.Controls.Add(this.dtpStart);
            this.Controls.Add(this.lblDate);
            this.Controls.Add(this.nudRounds);
            this.Controls.Add(this.lblRounds);
            this.Controls.Add(this.lblFederation);
            this.Controls.Add(this.tbCity);
            this.Controls.Add(this.lblCity);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Name = "TournamentDialog";
            this.Shown += new System.EventHandler(this.TournamentDialog_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.nudRounds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).EndInit();
            this.gpScoringScheme.ResumeLayout(false);
            this.gpScoringScheme.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button btnOk;
        private Button btnCancel;
        private Label lblName;
        private TextBox tbName;
        private TextBox tbCity;
        private Label lblCity;
        private Label lblFederation;
        private Label lblRounds;
        private NumericUpDown nudRounds;
        private Label lblDate;
        private DateTimePicker dtpStart;
        private Label lblHyphen;
        private DateTimePicker dtpEnd;
        private ErrorProvider ErrorProvider;
        private TextBox tbChiefArbiter;
        private Label lblChiefArbiter;
        private TextBox tbArbiters;
        private Label lblArbiters;
        private ComboBox cbPairingSystem;
        private Label lblPairingSystem;
        private TextBox tbTimeControl;
        private Label lblTimeControl;
        private GroupBox gpScoringScheme;
        private TextBox tbPointsForPAB;
        private Label lblPointsForPAB;
        private TextBox tbPointsForForfeit;
        private Label lblPointsForForfeit;
        private TextBox tbPointsForZPB;
        private Label lblPointsForZPB;
        private TextBox tbPointsForPlayedLoss;
        private Label lblPointsForPlayedLoss;
        private TextBox tbPointsForDraw;
        private Label lblPointsForDraw;
        private TextBox tbPointsForWin;
        private Label lblPointsForWin;
        private ComboBox cbFederation;
    }
}