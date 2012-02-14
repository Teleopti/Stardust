using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;


namespace Teleopti.Ccc.Win.Main
{
    public partial class Legal : BaseRibbonForm
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
