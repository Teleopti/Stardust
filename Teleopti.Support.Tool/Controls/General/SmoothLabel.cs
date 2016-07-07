using System.Drawing.Text;
using System.Windows.Forms;

namespace Teleopti.Support.Tool.Controls.General
{
    public partial class SmoothLabel : Label
    {
        private TextRenderingHint _hint = TextRenderingHint.SystemDefault;
        public TextRenderingHint TextRenderingHint
       
        {
            get { return this._hint; }
            set { this._hint = value; }
        }

        public SmoothLabel()
        {
            InitializeComponent();
        }


        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.TextRenderingHint = TextRenderingHint;
            base.OnPaint(pe);
        }
    }
}
