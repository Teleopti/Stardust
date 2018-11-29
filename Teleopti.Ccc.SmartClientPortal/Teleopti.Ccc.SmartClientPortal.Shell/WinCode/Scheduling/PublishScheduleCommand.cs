using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public class PublishScheduleCommand : IExecutableCommand
	{
		private readonly ICollection<IWorkflowControlSet> _workflowControlSets;
		private readonly DateOnly _publishToDate;
		private readonly SchedulingScreenState _schedulingScreenState;


		public PublishScheduleCommand(ICollection<IWorkflowControlSet> workflowControlSets, DateOnly publishToDate, SchedulingScreenState schedulingScreenState)
		{
			_workflowControlSets = workflowControlSets;
			_publishToDate = publishToDate;
			_schedulingScreenState = schedulingScreenState;
		}

		public void Execute()
		{
			foreach (var modifiedControlSet in _workflowControlSets)
			{
				foreach (var controlSet in _schedulingScreenState.WorkflowControlSets)
				{
					if (controlSet.Equals(modifiedControlSet))
					{
						controlSet.SchedulePublishedToDate = _publishToDate.Date;
						_schedulingScreenState.ModifiedWorkflowControlSets.Add(controlSet);
					}
				}	
			}
		}
	}
}
