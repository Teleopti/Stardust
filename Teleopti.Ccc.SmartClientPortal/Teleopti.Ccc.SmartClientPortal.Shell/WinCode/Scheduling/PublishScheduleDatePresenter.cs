using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public interface IPublishScheduleDateView
	{
		void SetDate(DateOnly date);
		void SetWorkflowControlSets(IList<IWorkflowControlSet> workflowControlSets);
		DateOnly PublishScheduleTo { get; }
		IList<IWorkflowControlSet> WorkflowControlSets { get; }
		void DisableOk();
	}

	public class PublishScheduleDatePresenter
	{
		private readonly IPublishScheduleDateView _view;
		private readonly IList<IScheduleDay> _selectedScheduleDays;
		private readonly List<IWorkflowControlSet> _workflowControlSets ;
		private DateOnly _dateOnly;

		public PublishScheduleDatePresenter(IPublishScheduleDateView view, IList<IScheduleDay> selectedScheduleDays)
		{
			_view = view;
			_selectedScheduleDays = selectedScheduleDays;
			_workflowControlSets = new List<IWorkflowControlSet>();
			_dateOnly = new DateOnly(DateTime.MinValue);
		}

		public void Initialize()
		{
			if (_selectedScheduleDays.Count == 0)
			{
				_view.DisableOk();
				return;
			}

			foreach (var selectedScheduleDay in _selectedScheduleDays)
			{
				var person = selectedScheduleDay.Person;
				var date = selectedScheduleDay.DateOnlyAsPeriod.DateOnly;
				var workflowControlSet = person.WorkflowControlSet;

				if(workflowControlSet != null && !_workflowControlSets.Contains(workflowControlSet))
					_workflowControlSets.Add(workflowControlSet);

				if (date > _dateOnly)
					_dateOnly = date;
			}

			if (_workflowControlSets.Count == 0)
			{
				_view.DisableOk();
				return;
			}

			_workflowControlSets.Sort((contrlSet1, contrlSet2) => String.Compare(contrlSet1.Name, contrlSet2.Name, TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture, CompareOptions.None));

			_view.SetDate(_dateOnly);
			_view.SetWorkflowControlSets(_workflowControlSets);
		}

		public IList<IWorkflowControlSet> WorkflowControlSets
		{
			get { return _workflowControlSets; }
		}

		public DateOnly PublishToDate
		{
			get { return _dateOnly; }
			set { _dateOnly = value; }
		}
	}
}
