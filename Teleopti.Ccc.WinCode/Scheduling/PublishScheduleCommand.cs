using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class PublishScheduleCommand : IExecutableCommand
	{
		private readonly ICollection<IWorkflowControlSet> _workflowControlSets;
		private readonly DateOnly _publishToDate;
		private readonly ICommonStateHolder _commonStateHolder;


		public PublishScheduleCommand(ICollection<IWorkflowControlSet> workflowControlSets, DateOnly publishToDate, ICommonStateHolder commonStateHolder)
		{
			_workflowControlSets = workflowControlSets;
			_publishToDate = publishToDate;
			_commonStateHolder = commonStateHolder;
		}

		public void Execute()
		{
			foreach (var modifiedControlSet in _workflowControlSets)
			{
				foreach (var controlSet in _commonStateHolder.WorkflowControlSets)
				{
					if (controlSet.Equals(modifiedControlSet))
					{
						controlSet.SchedulePublishedToDate = _publishToDate.Date;
						_commonStateHolder.ModifiedWorkflowControlSets.Add(controlSet);
					}
				}	
			}
		}
	}
}
