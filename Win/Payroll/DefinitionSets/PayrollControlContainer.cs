#region Imports

using System;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;

#endregion

namespace Teleopti.Ccc.Win.Payroll.DefinitionSets
{
    public partial class PayrollControlContainer : UserControl, IContainedControl
    {
        private UserControl _control;

        public PayrollControlContainer(UserControl userControl , string headerText)
        {
            InitializeComponent();

            _control = userControl;

            SetUserControl();

            labelDefinitionTypeText.Text = headerText;

            ICommonBehavior commonBehavior = _control as ICommonBehavior;
            ToolTip t = new ToolTip();
            t.SetToolTip(buttonAddNew, commonBehavior.ToolTipAddNew);
            t.SetToolTip(buttonDelete, commonBehavior.ToolTipDelete);
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
        private void buttonNew_Click(object sender, EventArgs e)
        {
            if (_control != null)
            {
                ICommonBehavior commonBehavior = _control as ICommonBehavior;
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
        private void buttonAdvDelete_Click(object sender, EventArgs e)
        {
            if (_control != null)
            {
                ICommonBehavior commonBehavior = _control as ICommonBehavior;
                if (commonBehavior != null)
                {
                    commonBehavior.DeleteSelected();
                }
            }
        }

        /// <summary>
        /// Sets the user control.
        /// </summary>
        private void SetUserControl()
        {
            tableLayoutPanel1.Controls.Add(_control, 0, 1);
            _control.Show();
        }
    }
}
