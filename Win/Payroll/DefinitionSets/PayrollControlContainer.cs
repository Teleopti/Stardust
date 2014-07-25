#region Imports

using System;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;

#endregion

namespace Teleopti.Ccc.Win.Payroll.DefinitionSets
{
    public partial class PayrollControlContainer : UserControl, IContainedControl
    {
        private readonly UserControl _control;

        public PayrollControlContainer(UserControl userControl , string headerText)
        {
            InitializeComponent();

            _control = userControl;

            setUserControl();

            labelDefinitionTypeText.Text = headerText;

            var commonBehavior = _control as ICommonBehavior;
            var t = new ToolTip();
            t.SetToolTip(buttonAddNew, commonBehavior.ToolTipAddNew);
            t.SetToolTip(buttonDelete, commonBehavior.ToolTipDelete);

            tableLayoutPanel4.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelDefinitionTypeText.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
        }

        /// <summary>
        /// Gets or sets the common behaviour instance.
        /// </summary>
        /// <value>The common behaviour instance.</value>
        public ICommonBehavior CommonBehaviorInstance
        {
            get { return _control as ICommonBehavior; }
        }

        /// <summary>
        /// Gets the contained control.
        /// </summary>
        /// <value>The contained control.</value>
        public UserControl UserControl
        {
            get { return this; }
        }

        /// <summary>
        /// Handles the Click event of the buttonNew control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void buttonNewClick(object sender, EventArgs e)
        {
            if (_control != null)
            {
                var commonBehavior = _control as ICommonBehavior;
                if (commonBehavior != null)
                {
                    commonBehavior.AddNew();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the buttonAdvDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void buttonAdvDeleteClick(object sender, EventArgs e)
        {
            if (_control != null)
            {
                var commonBehavior = _control as ICommonBehavior;
                if (commonBehavior != null)
                {
                    commonBehavior.DeleteSelected();
                }
            }
        }

        /// <summary>
        /// Sets the user control.
        /// </summary>
        private void setUserControl()
        {
            tableLayoutPanel1.Controls.Add(_control, 0, 1);
            _control.Show();
        }
    }
}
