using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Payroll;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Payroll
{
    /// <summary>
    /// Facilitates adding new definition sets
    /// </summary>
    public partial class ManageMultiplicatorForm : BaseRibbonForm
    {
        #region Fields

        private readonly IDefinitionSetPresenter _explorerPresenter;
        private IList<MultiplicatorType> _multiplicatorTypeCollection;
        public event EventHandler<DefinitionSetAddedEventArgs> DefinitionSetAddedEventArgs;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageDefinitionSetForm"/> class.
        /// </summary>
        /// <param name="presenter">The explorer presenter.</param>
        public ManageMultiplicatorForm(IDefinitionSetPresenter presenter)
        {
            InitializeComponent();
            txtName.Text = UserTexts.Resources.DefaultDefinitionSetName;
            _explorerPresenter = presenter;
            LoadDefinitionTypes();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the definition types.
        /// </summary>
        private void LoadDefinitionTypes()
        {
            _multiplicatorTypeCollection = (IList<MultiplicatorType>) Enum.GetValues(typeof (MultiplicatorType));
            cmbType.DataSource = _multiplicatorTypeCollection;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the Click event of the btnOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnOk_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(txtName.Text))
            {
                Syncfusion.Windows.Forms.MessageBoxAdv.Show(UserTexts.Resources.DefinitionSetNameBlank,
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1,
                    (RightToLeft == RightToLeft.Yes) ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0);
                return;
            }
            MultiplicatorType definitionType = (MultiplicatorType)cmbType.SelectedValue;
            MultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet(txtName.Text, definitionType);
            _explorerPresenter.AddNewDefinitionSet(definitionSet);
            DefinitionSetAddedEventArgs(this, new DefinitionSetAddedEventArgs(definitionSet));
            btnCancel.PerformClick();

        }

        #endregion
    }
}
