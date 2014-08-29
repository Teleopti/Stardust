using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Payroll.DefinitionSets
{
	public partial class ManageDefinitionSetForm : BaseDialogForm
	{
		private readonly IDefinitionSetPresenter _explorerPresenter;
		private IList<KeyValuePair<MultiplicatorType, string>> _multiplicatorTypeCollection;
		public event EventHandler<DefinitionSetAddedEventArgs> DefinitionSetAdded;

		public ManageDefinitionSetForm(IDefinitionSetPresenter presenter)
		{
			InitializeComponent();
			txtName.Text = UserTexts.Resources.DefaultDefinitionSetName;
			_explorerPresenter = presenter;
			loadDefinitionTypes();
			if (!DesignMode) SetTexts();
		}

		private void loadDefinitionTypes()
		{
			_multiplicatorTypeCollection =
				 LanguageResourceHelper.TranslateEnumToList<MultiplicatorType>();
			cmbType.DisplayMember = "Value";
			cmbType.ValueMember = "Key";
			cmbType.DataSource = _multiplicatorTypeCollection;
		}

		private void btnOkClick(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(txtName.Text))
			{
				ShowErrorMessage(UserTexts.Resources.DefinitionSetNameBlank, Text);
				DialogResult = DialogResult.None;
				return;
			}
			var definitionType = (MultiplicatorType)cmbType.SelectedValue;
			var definitionSet = new MultiplicatorDefinitionSet(txtName.Text, definitionType);
			_explorerPresenter.AddNewDefinitionSet(definitionSet);

			var handler = DefinitionSetAdded;
			if (handler != null)
			{
				handler(this, new DefinitionSetAddedEventArgs(definitionSet));
			}
			Close();
		}
	}
}
