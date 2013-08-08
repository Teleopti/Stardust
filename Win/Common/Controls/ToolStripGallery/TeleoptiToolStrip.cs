using System.ComponentModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Common.Controls.ToolStripGallery
{
    public partial class TeleoptiToolStrip : ToolStripEx
    {
        /// <summary>
        /// Default Constructor of the TeleoptiToolStrip class
        /// </summary>
        public TeleoptiToolStrip()
        {
            InitializeComponent();
            base.ContextMenuStrip = new ContextMenuStrip();
        }

        /// <summary>
        /// ToolStripDropDownDirection Property
        /// </summary>
        public ToolStripDropDownDirection DropDownDirection
        {
            get { return DefaultDropDownDirection; }
            set { DefaultDropDownDirection = value; }
        }

        /// <summary>
        /// DockStyle Property
        /// </summary>
        public DockStyle ToolStripDockStyle
        {
            get { return Dock; }
            set { Dock = value; }
        }

        /// <summary>
        /// ToolStripLayoutStyle Property
        /// </summary>
        public ToolStripLayoutStyle ToolStripLayoutStyle
        {
            get { return LayoutStyle; }
            set { LayoutStyle = value; }
        }
    }
}
