using System;
using System.Windows.Forms;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Main
{
    public partial class Legal : BaseDialogForm
    {
        public Legal()
        {
            InitializeComponent();
            if (DesignMode) return;

            SetTexts();
            setColors();
        }

        private void setColors()
        {
            BackColor = ColorHelper.FormBackgroundColor();
            gradientPanel2.BackgroundColor = ColorHelper.ChartControlBackInterior();
            richTextBoxLegalNotice.BackColor = ColorHelper.DialogBackColor();
        }

        private void buttonAdv2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
