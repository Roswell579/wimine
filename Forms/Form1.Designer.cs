namespace wmine.Forms
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private Button btnIa;
        private Control gmapControl; // Ajout de la déclaration manquante

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // ... votre code existant pour gmapControl et autres contrôles ...

            // Initialisation de gmapControl si nécessaire
            // this.gmapControl = new GMapControl(); // Décommentez et ajustez si vous utilisez GMap.NET

            this.btnIa = new System.Windows.Forms.Button();
            this.btnIa.BackColor = System.Drawing.Color.FromArgb(33, 150, 243);
            this.btnIa.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIa.FlatAppearance.BorderSize = 0;
            this.btnIa.ForeColor = System.Drawing.Color.White;
            this.btnIa.Location = new System.Drawing.Point(10, 10);
            this.btnIa.Name = "btnIa";
            this.btnIa.Size = new System.Drawing.Size(60, 30);
            this.btnIa.Text = "IA";
            this.btnIa.UseVisualStyleBackColor = false;
            this.btnIa.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnIa.Click += new System.EventHandler(this.BtnIa_Click);

            // Ajout du bouton au contrôle gmapControl si celui-ci existe
            if (this.gmapControl != null)
            {
                this.gmapControl.Controls.Add(this.btnIa);
                this.btnIa.BringToFront();
            }
            else
            {
                this.Controls.Add(this.btnIa);
            }
        }
    }
}
