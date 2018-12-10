using System;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Budgeting.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Views;

using Size = System.Drawing.Size;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Budgeting
{
	public partial class EditShrinkageForm : BaseDialogForm, IEditShrinkageForm
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly ICustomShrinkage _customShrinkage;
		internal event EventHandler<CustomEventArgs<CustomShrinkageUpdatedEventArgs>> CustomShrinkageUpdated;
		private const int maxLength = 25;
		private string _helpId;
		private readonly EditShrinkagePresenter _presenter;
		private readonly IGracefulDataSourceExceptionHandler _dataSourceExceptionHandler = new GracefulDataSourceExceptionHandler();

		public EditShrinkageForm(IEventAggregator eventAggregator, ICustomShrinkage  customShrinkage, IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory)
		{
			_eventAggregator = eventAggregator;
			_customShrinkage = customShrinkage;
			InParameter.NotNull("customShrinkage", customShrinkage);
			InitializeComponent();
			_helpId = Name;
			if (!DesignMode) SetTexts();
			_presenter = new EditShrinkagePresenter(this, new EditShrinkageModel(customShrinkage, unitOfWorkFactory, repositoryFactory));
			_presenter.Initialize();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			textBoxShrinkageName.Text = _presenter.ShrinkageName;
			textBoxShrinkageName.MaxLength = maxLength;
			if (checkBoxInclude != null)
			{
				hideAbsencePanel();
				checkBoxInclude.Checked = _presenter.IncludedInAllowance;
				
				initializeAbsences();
				initializeAddedAbsences();
			}
		}

		private void initializeAddedAbsences()
		{
			foreach (var absence in _presenter.AddedAbsences)
			{
				listBoxAbsences.Items.Remove(absence);
				listBoxAddedAbsences.Items.Add(absence);
			}
		}

		private void initializeAbsences()
		{
			listBoxAbsences.DisplayMember = "Name";
			listBoxAddedAbsences.DisplayMember = "Name";
			listBoxAbsences.Items.AddRange(_presenter.Absences.ToArray());
		}

		public void HideIncludedInRequestAllowance()
		{
			SuspendLayout();
			var height = tableLayoutPanelFields.GetRowHeights()[1];
			tableLayoutPanelFields.RowStyles[1].Height = 0;
			tableLayoutPanelFields.Controls.Remove(tableLayoutPanelAllowance);
			Size = new Size(Size.Width, Size.Height - height);
			tableLayoutPanelAllowance = null;
			checkBoxInclude = null;
			ResumeLayout();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			textBoxShrinkageName.Focus();
			textBoxShrinkageName.SelectAll();
		}

		private void buttonAdvSave_Click(object sender, EventArgs e)
		{
			var shrinkageName = textBoxShrinkageName.Text.Trim();
			if (string.IsNullOrEmpty(shrinkageName))
			{
				errorProvider.SetError(textBoxShrinkageName, UserTexts.Resources.TheInputTextCanNotBeEmptyDot);
				errorProvider.SetIconPadding(textBoxShrinkageName, -20);
				return;
			}
			var handler = CustomShrinkageUpdated;
			if (handler != null)
			{
				_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(
					() =>
						{
							saveUpdatedCustomShrinkage(shrinkageName);
							_eventAggregator.GetEvent<BudgetGroupTreeNeedsRefresh>().Publish("saveUpdatedCustomShrinkage");
							notifySubsribers(handler);
						});
			}
			Close();
		}

		private void notifySubsribers(EventHandler<CustomEventArgs<CustomShrinkageUpdatedEventArgs>> handler)
		{
			var eventArgs = new CustomShrinkageUpdatedEventArgs {CustomShrinkage = _customShrinkage};
			handler.Invoke(this, new CustomEventArgs<CustomShrinkageUpdatedEventArgs>(eventArgs));
		}

		private void saveUpdatedCustomShrinkage(string shrinkageName)
		{
			var customShrinkage = new CustomShrinkage(shrinkageName);
			if (checkBoxInclude != null)
				customShrinkage.IncludedInAllowance = checkBoxInclude.Checked;
			foreach (var absence in listBoxAddedAbsences.Items)
			{
				customShrinkage.AddAbsence((IAbsence) absence);
			}
			_presenter.Save(customShrinkage);
		}

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			Close();
		}

		public override string HelpId
		{
			get
			{
				return _helpId;
			}
		}

		public void SetHelpId(string helpId)
		{
			_helpId = helpId;
		}

		private void buttonAdvAddAllClick(object sender, EventArgs e)
		{
			listBoxAddedAbsences.Items.AddRange(listBoxAbsences.Items);
			listBoxAbsences.Items.Clear();
		}

		private void buttonAdvAddSelectedClick(object sender, EventArgs e)
		{
			_presenter.AddAbsences();
		}

		public void AddSelectedAbsences()
		{
			foreach (var item in listBoxAbsences.SelectedItems)
			{
				listBoxAddedAbsences.Items.Add(item);
			}

			foreach (var item in listBoxAddedAbsences.Items)
			{
				listBoxAbsences.Items.Remove(item);
			}
		}
		
		public void RemoveSelectedAbsences()
		{
			foreach (var selectedItem in listBoxAddedAbsences.SelectedItems)
			{
				listBoxAbsences.Items.Add(selectedItem);
			}

			foreach (var item in listBoxAbsences.Items)
			{
				listBoxAddedAbsences.Items.Remove(item);
			}
		}

		private void buttonAdvRemoveSelectedClick(object sender, EventArgs e)
		{
			_presenter.RemoveAllAbsences();
		}

		private void buttonAdvRemoveAllClick(object sender, EventArgs e)
		{
			listBoxAbsences.Items.AddRange(listBoxAddedAbsences.Items);
			listBoxAddedAbsences.Items.Clear();
		}

		private void listBoxAbsencesDoubleClick(object sender, EventArgs e)
		{
			_presenter.AddAbsences();
		}

		private void listBoxAddedAbsencesDoubleClick(object sender, EventArgs e)
		{
			_presenter.RemoveAllAbsences();
		}

		private void hideAbsencePanel()
		{
			SuspendLayout();
			tableLayoutPanelAllowance.Controls.Remove(tableLayoutPanelAbsence);
			tableLayoutPanelAllowance.RowStyles[1].Height = 0;
			tableLayoutPanelFields.RowStyles[1].Height = tableLayoutPanelAllowance.Height;
			Size = new Size(Size.Width, Size.Height - tableLayoutPanelAbsence.Height);
			ResumeLayout();
		}

		private void showAbsencePanel()
		{
			SuspendLayout();
			var absencePanelHeight = tableLayoutPanelAbsence.Height;
			tableLayoutPanelAllowance.RowStyles[1].Height = absencePanelHeight;
			tableLayoutPanelAllowance.Controls.Add(tableLayoutPanelAbsence, 0, 1);
			tableLayoutPanelFields.RowStyles[1].Height = tableLayoutPanelAllowance.RowStyles[0].Height +
														 absencePanelHeight;
			Size = new Size(Size.Width, Size.Height + absencePanelHeight);
			ResumeLayout();
		}

		private void checkBoxInclude_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxInclude.Checked)
				showAbsencePanel();
			else
				hideAbsencePanel();
		}

		private void textBoxExt1_TextChanged(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(textBoxShrinkageName.Text.Trim()) &&
				!string.IsNullOrEmpty(errorProvider.GetError(textBoxShrinkageName)))
				errorProvider.SetError(textBoxShrinkageName, string.Empty);
		}
	}
}
