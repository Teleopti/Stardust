using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Backlog
{
	public class BacklogSkill
	{
		private readonly IncomingTaskFactory _incomingTaskFactory;
		private readonly IList<IncomingTask> _incomingTasks = new List<IncomingTask>();

		public BacklogSkill(IncomingTaskFactory incomingTaskFactory)
		{
			_incomingTaskFactory = incomingTaskFactory;
		}

		public void AddIncomingTask(DateOnly startDate, IServiceLevel serviceLevel, int totalWorkItems, TimeSpan averageWorkTimePerItem)
		{
			var endDate = startDate.AddDays((int) TimeSpan.FromSeconds(serviceLevel.Seconds).TotalDays - 1);
			var task =  _incomingTaskFactory.Create(new DateOnlyPeriod(startDate, endDate), totalWorkItems, averageWorkTimePerItem);
			_incomingTasks.Add(task);
		}

		public void SetManualTimeOnDate(DateOnly date, TimeSpan time)
		{
			
		}

		private IEnumerable<IncomingTask> tasksAffected(DateOnly date)
		{
			return _incomingTasks.Where(t => t.SpanningPeriod.Contains(date)).OrderBy(t => t.SpanningPeriod.StartDate);
		}


	}
}