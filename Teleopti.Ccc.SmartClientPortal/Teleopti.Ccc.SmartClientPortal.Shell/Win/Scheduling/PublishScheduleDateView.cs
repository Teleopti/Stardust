using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	

	public partial class PublishScheduleDateView : BaseDialogForm, IPublishScheduleDateView
	{
		private readonly PublishScheduleDatePresenter _presenter;

		public PublishScheduleDateView(IList<IScheduleDay> selectedSchedules)
		{
			InitializeComponent();
			datePicker.Culture = TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture;
			_presenter = new PublishScheduleDatePresenter(this, selectedSchedules);
			_presenter.Initialize();
			if (!DesignMode) SetTexts();
		}

		public void SetDate(DateOnly date)
		{
			datePicker.Value = date.Date;
		}

		public void SetWorkflowControlSets(IList<IWorkflowControlSet> workflowControlSets)
		{
			listViewControlSets.Columns.Add(UserTexts.Resources.WorkflowControlSet, -2, HorizontalAlignment.Left);
			listViewControlSets.Columns.Add(UserTexts.Resources.Current, -2, HorizontalAlignment.Left);

			foreach (var workflowControlSet in workflowControlSets)
			{
				var current= workflowControlSet.SchedulePublishedToDate.HasValue? workflowControlSet.SchedulePublishedToDate.Value.ToShortDateString() : UserTexts.Resources.NotPublished;
				var item = new ListViewItem(new[] {workflowControlSet.Name, current});
				listViewControlSets.Items.Add(item);
			}			
		}

		private void listViewControlSetsItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			if (e.IsSelected) e.Item.Selected = false;
		}

		private void datePickerValueChanged(object sender, EventArgs e)
		{
			_presenter.PublishToDate = new DateOnly(datePicker.Value);
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Hide();
		}

		public DateOnly PublishScheduleTo
		{
			get { return _presenter.PublishToDate; }
		}

		public IList<IWorkflowControlSet> WorkflowControlSets
		{
			get { return _presenter.WorkflowControlSets; }
		}

		public void DisableOk()
		{
			buttonAdvOk.Enabled = false;
		}
	}
}
