namespace PonzianiSwissGui
{
    partial class PlayerDialog
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
            this.lblFideId = new System.Windows.Forms.Label();
            this.tbFideId = new System.Windows.Forms.TextBox();
            this.tbName = new System.Windows.Forms.TextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.cbTitle = new System.Windows.Forms.ComboBox();
            this.lblFederation = new System.Windows.Forms.Label();
            this.cbFederation = new System.Windows.Forms.ComboBox();
            this.lblRating = new System.Windows.Forms.Label();
            this.lblAltRating = new System.Windows.Forms.Label();
            this.nudRating = new System.Windows.Forms.NumericUpDown();
            this.nudAltRating = new System.Windows.Forms.NumericUpDown();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.cbFemale = new System.Windows.Forms.CheckBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.tbYearOfBirth = new System.Windows.Forms.TextBox();
            this.lblYearOfBirth = new System.Windows.Forms.Label();
            this.lblClub = new System.Windows.Forms.Label();
            this.tbClub = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAltRating)).BeginInit();
            this.SuspendLayout();
            // 
            // lblFideId
            // 
            this.lblFideId.AutoSize = true;
            this.lblFideId.Location = new System.Drawing.Point(12, 9);
            this.lblFideId.Name = "lblFideId";
            this.lblFideId.Size = new System.Drawing.Size(42, 15);
            this.lblFideId.TabIndex = 0;
            this.lblFideId.Text = "Fide Id";
            // 
            // tbFideId
            // 
            this.tbFideId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFideId.Location = new System.Drawing.Point(111, 6);
            this.tbFideId.Name = "tbFideId";
            this.tbFideId.Size = new System.Drawing.Size(609, 23);
            this.tbFideId.TabIndex = 1;
            this.tbFideId.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TbFideId_KeyPress);
            this.tbFideId.Leave += new System.EventHandler(this.TbFideId_Leave);
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.tbName.Location = new System.Drawing.Point(111, 35);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(609, 23);
            this.tbName.TabIndex = 3;
            this.tbName.Enter += new System.EventHandler(this.TbName_Enter);
            this.tbName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TbName_KeyUp);
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(12, 38);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(39, 15);
            this.lblName.TabIndex = 2;
            this.lblName.Text = "Name";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(12, 67);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(29, 15);
            this.lblTitle.TabIndex = 4;
            this.lblTitle.Text = "Title";
            // 
            // cbTitle
            // 
            this.cbTitle.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbTitle.FormattingEnabled = true;
            this.cbTitle.Location = new System.Drawing.Point(111, 64);
            this.cbTitle.Name = "cbTitle";
            this.cbTitle.Size = new System.Drawing.Size(69, 23);
            this.cbTitle.TabIndex = 5;
            // 
            // lblFederation
            // 
            this.lblFederation.AutoSize = true;
            this.lblFederation.Location = new System.Drawing.Point(197, 67);
            this.lblFederation.Name = "lblFederation";
            this.lblFederation.Size = new System.Drawing.Size(63, 15);
            this.lblFederation.TabIndex = 6;
            this.lblFederation.Text = "Federation";
            // 
            // cbFederation
            // 
            this.cbFederation.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbFederation.FormattingEnabled = true;
            this.cbFederation.Location = new System.Drawing.Point(304, 64);
            this.cbFederation.Name = "cbFederation";
            this.cbFederation.Size = new System.Drawing.Size(134, 23);
            this.cbFederation.TabIndex = 7;
            // 
            // lblRating
            // 
            this.lblRating.AutoSize = true;
            this.lblRating.Location = new System.Drawing.Point(13, 94);
            this.lblRating.Name = "lblRating";
            this.lblRating.Size = new System.Drawing.Size(41, 15);
            this.lblRating.TabIndex = 8;
            this.lblRating.Text = "Rating";
            // 
            // lblAltRating
            // 
            this.lblAltRating.AutoSize = true;
            this.lblAltRating.Location = new System.Drawing.Point(197, 97);
            this.lblAltRating.Name = "lblAltRating";
            this.lblAltRating.Size = new System.Drawing.Size(101, 15);
            this.lblAltRating.TabIndex = 10;
            this.lblAltRating.Text = "Alternative Rating";
            // 
            // nudRating
            // 
            this.nudRating.Location = new System.Drawing.Point(112, 92);
            this.nudRating.Maximum = new decimal(new int[] {
            4000,
            0,
            0,
            0});
            this.nudRating.Name = "nudRating";
            this.nudRating.Size = new System.Drawing.Size(68, 23);
            this.nudRating.TabIndex = 11;
            this.nudRating.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            // 
            // nudAltRating
            // 
            this.nudAltRating.Location = new System.Drawing.Point(304, 95);
            this.nudAltRating.Maximum = new decimal(new int[] {
            4000,
            0,
            0,
            0});
            this.nudAltRating.Name = "nudAltRating";
            this.nudAltRating.Size = new System.Drawing.Size(65, 23);
            this.nudAltRating.TabIndex = 12;
            this.nudAltRating.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnCancel.Location = new System.Drawing.Point(726, 158);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 14;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnOk.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnOk.Location = new System.Drawing.Point(645, 158);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 13;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // cbFemale
            // 
            this.cbFemale.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbFemale.AutoSize = true;
            this.cbFemale.Location = new System.Drawing.Point(737, 8);
            this.cbFemale.Name = "cbFemale";
            this.cbFemale.Size = new System.Drawing.Size(64, 19);
            this.cbFemale.TabIndex = 16;
            this.cbFemale.Text = "Female";
            this.cbFemale.UseVisualStyleBackColor = true;
            // 
            // btnSearch
            // 
            this.btnSearch.Image = global::PonzianiSwissGui.Properties.Resources.search;
            this.btnSearch.Location = new System.Drawing.Point(726, 35);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSearch.TabIndex = 17;
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
            // 
            // tbYearOfBirth
            // 
            this.tbYearOfBirth.Location = new System.Drawing.Point(554, 64);
            this.tbYearOfBirth.Name = "tbYearOfBirth";
            this.tbYearOfBirth.Size = new System.Drawing.Size(37, 23);
            this.tbYearOfBirth.TabIndex = 19;
            // 
            // lblYearOfBirth
            // 
            this.lblYearOfBirth.AutoSize = true;
            this.lblYearOfBirth.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lblYearOfBirth.Location = new System.Drawing.Point(458, 67);
            this.lblYearOfBirth.Name = "lblYearOfBirth";
            this.lblYearOfBirth.Size = new System.Drawing.Size(71, 15);
            this.lblYearOfBirth.TabIndex = 18;
            this.lblYearOfBirth.Text = "Year of Birth";
            // 
            // lblClub
            // 
            this.lblClub.AutoSize = true;
            this.lblClub.Location = new System.Drawing.Point(13, 125);
            this.lblClub.Name = "lblClub";
            this.lblClub.Size = new System.Drawing.Size(32, 15);
            this.lblClub.TabIndex = 20;
            this.lblClub.Text = "Club";
            // 
            // tbClub
            // 
            this.tbClub.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbClub.Location = new System.Drawing.Point(112, 122);
            this.tbClub.Name = "tbClub";
            this.tbClub.Size = new System.Drawing.Size(608, 23);
            this.tbClub.TabIndex = 21;
            // 
            // PlayerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(813, 193);
            this.Controls.Add(this.tbClub);
            this.Controls.Add(this.lblClub);
            this.Controls.Add(this.tbYearOfBirth);
            this.Controls.Add(this.lblYearOfBirth);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.cbFemale);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.nudAltRating);
            this.Controls.Add(this.nudRating);
            this.Controls.Add(this.lblAltRating);
            this.Controls.Add(this.lblRating);
            this.Controls.Add(this.cbFederation);
            this.Controls.Add(this.lblFederation);
            this.Controls.Add(this.cbTitle);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.tbFideId);
            this.Controls.Add(this.lblFideId);
            this.Name = "PlayerDialog";
            this.Text = "PlayerDialog";
            this.Shown += new System.EventHandler(this.PlayerDialog_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAltRating)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label lblFideId;
        private TextBox tbFideId;
        private TextBox tbName;
        private Label lblName;
        private Label lblTitle;
        private ComboBox cbTitle;
        private Label lblFederation;
        private ComboBox cbFederation;
        private Label lblRating;
        private Label lblAltRating;
        private NumericUpDown nudRating;
        private NumericUpDown nudAltRating;
        private Button btnCancel;
        private Button btnOk;
        private CheckBox cbFemale;
        private Button btnSearch;
        private TextBox tbYearOfBirth;
        private Label lblYearOfBirth;
        private Label lblClub;
        private TextBox tbClub;
    }
}