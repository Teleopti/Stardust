using System;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Common.Controls.Drawing
{
    /// <summary>
    /// General transparent usercontrol for controls with transparent background.
    /// </summary>
    public partial class TransparentUserControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransparentUserControl"/> class.
        /// </summary>
        public TransparentUserControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Move"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnMove(EventArgs e)
        {
            RecreateHandle();
        }

        /// <summary>
        /// Paints the background of the control.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // do nothing
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20;
                return cp;
             }
        }
    }
}
