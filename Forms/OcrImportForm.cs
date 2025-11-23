namespace wmine.Forms
{
    // Stub minimal pour permettre la compilation lorsque le formulaire original est absent.
    // Surcharge de constructeurs ajoutée pour correspondre aux appels existants dans le projet.
    public class OcrImportForm : Form
    {
        private object? _context;

        public OcrImportForm()
        {
            InitializeComponent();
        }

        // Accept any single argument to match appels existants (ex.: owner, data, options)
        public OcrImportForm(object? context) : this()
        {
            _context = context;
        }

        // Si le code appelle avec un parent Form en paramètre, cette surcharge sera sélectionnée.
        public OcrImportForm(Form? owner) : this()
        {
            if (owner != null)
            {
                Owner = owner;
            }
        }

        private void InitializeComponent()
        {
            this.Text = "OCR Import (placeholder)";
            this.Width = 600;
            this.Height = 400;
        }
    }
}
