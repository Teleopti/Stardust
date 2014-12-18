﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	

	public partial class PublishScheduleDateView : BaseDialogForm, IPublishScheduleDateView
	{
		private readonly PublishScheduleDatePresenter _presenter;

		public PublishScheduleDateView(IList<IScheduleDay> selectedSchedules)
		{
			InitializeComponent();
			datePicker.Culture = TeleoptiPrincipal.Current.Regional.Culture;
			_presenter = new PublishScheduleDatePresenter(this, selectedSchedules);
			_presenter.Initialize();
			if (!DesignMode) SetTexts();
		}

		public void SetDate(DateOnly date)
		{
			datePicker.Value = date;
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

		public DateTime PublishScheduleTo
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
