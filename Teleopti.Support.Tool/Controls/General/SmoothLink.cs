using System.Drawing.Text;
using System.Windows.Forms;

namespace Teleopti.Support.Tool.Controls.General
{
    public partial class SmoothLink : LinkLabel
    {
        private TextRenderingHint _hint = TextRenderingHint.SystemDefault;
        public TextRenderingHint TextRenderingHint
       
        {
            get { return this._hint; }
            set { this._hint = value; }
        }

        public SmoothLink()
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
