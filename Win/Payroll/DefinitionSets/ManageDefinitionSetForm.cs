using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Payroll.DefinitionSets
{
    /// <summary>
    /// Facilitates adding new definition sets
    /// </summary>
    public partial class ManageDefinitionSetForm : BaseRibbonForm
    {
        private readonly IDefinitionSetPresenter _explorerPresenter;
        private IList<KeyValuePair<MultiplicatorType, string>> _multiplicatorTypeCollection;
        public event EventHandler<DefinitionSetAddedEventArgs> DefinitionSetAdded;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageDefinitionSetForm"/> class.
        /// </summary>
        /// <param name="presenter">The explorer presenter.</param>
        public ManageDefinitionSetForm(IDefinitionSetPresenter presenter)
        {
            InitializeComponent();
            txtName.Text = UserTexts.Resources.DefaultDefinitionSetName;
            _explorerPresenter = presenter;
            gradientPanel1.BackgroundColor = ColorHelper.ControlGradientPanelBrush();
            LoadDefinitionTypes();
            if (!DesignMode) SetTexts();
        }

        /// <summary>
        /// Loads the definition types.
        /// </summary>
        private void LoadDefinitionTypes()
        {
            _multiplicatorTypeCollection =
                LanguageResourceHelper.TranslateEnumToList<MultiplicatorType>();
            cmbType.DisplayMember = "Value";
            cmbType.ValueMember = "Key";
            cmbType.DataSource = _multiplicatorTypeCollection;
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
                ShowErrorMessage(UserTexts.Resources.DefinitionSetNameBlank, Text);
                DialogResult = DialogResult.None;
                return;
            }
            MultiplicatorType definitionType = (MultiplicatorType)cmbType.SelectedValue;
            MultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet(txtName.Text, definitionType);
            _explorerPresenter.AddNewDefinitionSet(definitionSet);

        	var handler = DefinitionSetAdded;
            if (handler!=null)
            {
            	handler(this, new DefinitionSetAddedEventArgs(definitionSet));
            }
            Close();
        }
    }
}
