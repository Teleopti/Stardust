using System;
using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.Win.Grouping
{
    public partial class NameDialog : BaseDialogForm
    {
        private string _nameValue = string.Empty;

        public string NameValue
        {
            get { return _nameValue; }
        }

        public NameDialog(string title, string nameLabelValue, string oldName)
        {
            InitializeComponent();

            labelTitle.Text = title;
            autoLabelName.Text = nameLabelValue;
            textBoxName.Text = oldName;

            if (!DesignMode)
            {
                SetTexts();
                gradientPanel1.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
                labelTitle.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();
            }
        }

        private void buttonAdvOkClick(object sender, EventArgs e)
        {
            if (validUserInputs())
            {
                _nameValue = textBoxName.Text.Trim();
                DialogResult = DialogResult.OK;
            }
        }

        private void nameDialogShown(object sender, EventArgs e)
        {
            textBoxName.Focus();
        }

        private bool validUserInputs()
        {
            if (string.IsNullOrEmpty(textBoxName.Text.Trim()))
            {
                ShowErrorMessage(Resources.EnterANameForTheGrouping, Resources.Warning);
                return false;
            }
            return true;
        }
    }
}
