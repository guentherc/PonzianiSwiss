namespace PonzianiSwissGui
{
    partial class HTMLViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HTMLViewer));
            this.WebViewer = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.WebViewer)).BeginInit();
            this.SuspendLayout();
            // 
            // WebViewer
            // 
            this.WebViewer.CreationProperties = null;
            this.WebViewer.DefaultBackgroundColor = System.Drawing.Color.White;
            resources.ApplyResources(this.WebViewer, "WebViewer");
            this.WebViewer.Name = "WebViewer";
            this.WebViewer.Source = new System.Uri("https://www.google.de", System.UriKind.Absolute);
            this.WebViewer.ZoomFactor = 1D;
            // 
            // HTMLViewer
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.WebViewer);
            this.Name = "HTMLViewer";
            this.Load += new System.EventHandler(this.HTMLViewer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.WebViewer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 WebViewer;
    }
}