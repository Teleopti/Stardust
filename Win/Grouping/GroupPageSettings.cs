using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Grouping
{
	public partial class GroupPageSettings : BaseDialogForm, ISelfDataHandling
	{
		private DynamicOptionType _groupPageType = DynamicOptionType.BusinessHierarchy;
		private IList<IOptionalColumn> _optionalColumnCollection;
		private readonly IGroupPageHelper _groupPageHelper;
		private readonly OptionalColumnProvider _optionalColumnProvider = new OptionalColumnProvider(new RepositoryFactory());
		private IGroupPage _groupPage;
		private Guid? _optionalColumnId;

		public GroupPageSettings()
		{
			InitializeComponent();
		}

		public GroupPageSettings(IGroupPageHelper groupPageHelper)
			: this()
		{
			_groupPageHelper = groupPageHelper;
			if (!DesignMode)
			{
				SetTexts();
				gradientPanel1.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
				tableLayoutPanel3.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
				tableLayoutPanel5.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
				labelTitle.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();
				label2.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
				label3.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			}
		}

		public IGroupPage NewGroupPage { get { return _groupPage; } }

		private void groupPageSettingsLoad(object sender, EventArgs e)
		{
			// assign them to the combo box
			// load the options dynamicllay from the enum ?
			// load the optional columns and assign to the combo box
			textBoxExtGroupPageName.Text = Resources.NewGroupPageName;
			loadAndBindOptionalColumnCollection();
			radioButtonOptionalColumn.CheckedChanged += radioButtonOptionalColumnCheckedChanged;
			//Height = 400;
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			if(handleCreateGroupPage())
				DialogResult = DialogResult.OK;
		}

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void loadAndBindOptionalColumnCollection()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_optionalColumnProvider.LoadAllOptionalColumns(uow);
				_optionalColumnCollection = _optionalColumnProvider.OptionalColumnCollection;
				_optionalColumnCollection.Insert(0, new OptionalColumn(Resources.SelectOptionalColumn));
				comboBoxAdvOptionalColumns.DataSource = null;
				comboBoxAdvOptionalColumns.DisplayMember = "Name";
				comboBoxAdvOptionalColumns.ValueMember = "Id";
				comboBoxAdvOptionalColumns.DataSource = _optionalColumnCollection;

				comboBoxAdvOptionalColumns.SelectedIndex = 0;    
			}            
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
		}

		public void Persist()
		{
			
		}

		private DynamicOptionType getGroupByOption()
		{
			var result = DynamicOptionType.BusinessHierarchy;

			if (radioButtonDoNotGroup.Checked)
				result = DynamicOptionType.DoNotGroup;

			if (radioButtonGroupByPersonNote.Checked)
				result = DynamicOptionType.PersonNote;

			if (radioButtonGroupByPersonPeriodContract.Checked)
				result = DynamicOptionType.Contract;

			if (radioButtonGroupByPersonPeriodPartTimePercentage.Checked)
				result = DynamicOptionType.PartTimePercentage;

			if (radioButtonGroupByPersonPeriodContractSchedule.Checked)
				result = DynamicOptionType.ContractSchedule;

			if (radioButtonGroupByPersonPeriodRulesetBag.Checked)
				result = DynamicOptionType.RuleSetBag;

			if (radioButtonOptionalColumn.Checked)
				result = DynamicOptionType.OptionalPage;

			return result;
		}

		private bool isUserInputsValid()
		{
			if (string.IsNullOrEmpty(textBoxExtGroupPageName.Text.Trim()))
			{
				ShowErrorMessage(Resources.EnterANameForTheGrouping, Resources.Warning);
				return false;
			}

			// create and save to db and load in the panel
			_groupPageType = getGroupByOption();

			if (_groupPageType == DynamicOptionType.BusinessHierarchy)
			{
				ShowErrorMessage(Resources.SelectAGroupByOption, Resources.Warning);
				return false;
			}

			updateOptionalColumnId();

			if (_groupPageType == DynamicOptionType.OptionalPage && _optionalColumnId == null)
			{
				ShowErrorMessage(Resources.NoOptionalColumnIsSelected, Resources.Warning);
				return false;
			}

			return true;
		}

		private bool handleCreateGroupPage()
		{
			if (isUserInputsValid())
			{
				using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					uow.Reassociate(_groupPageHelper.PersonCollection);
					var treeConstructor = new TreeConstructor(_groupPageHelper);
					_groupPage = treeConstructor.CreateGroupPage(textBoxExtGroupPageName.Text.Trim(),
																 _groupPageType, _optionalColumnId);
				}
				if (_groupPage != null)
				{
					Changes = _groupPageHelper.AddOrUpdateGroupPage(_groupPage);
				}
				return true;
			}
			return false;
		}

		public IEnumerable<IRootChangeInfo> Changes { get; private set; }

		private void updateOptionalColumnId()
		{
			_optionalColumnId = comboBoxAdvOptionalColumns.SelectedItem != null ? ((OptionalColumn)comboBoxAdvOptionalColumns.SelectedItem).Id : null;
		}

		private void radioButtonOptionalColumnCheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonOptionalColumn.Checked == false)
			{
				comboBoxAdvOptionalColumns.SelectedIndex = 0;
			}
			else
			{
				comboBoxAdvOptionalColumns.Focus();
			}
		}

		private void textBoxExtGroupPageNameValidating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (string.IsNullOrEmpty(textBoxExtGroupPageName.Text))
			{
				textBoxExtGroupPageName.Text = Resources.NewGroupPageName;
				textBoxExtGroupPageName.Focus();
				e.Cancel = true;
			}
		}

		private void releaseManagedResources()
		{
			if (_optionalColumnProvider!=null)
				_optionalColumnProvider.Dispose();
		}
	}
}
