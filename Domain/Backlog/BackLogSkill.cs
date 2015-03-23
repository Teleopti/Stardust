using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Backlog
{
	public class BacklogSkill
	{
		private readonly IList<IncomingTask> _incomingTasks = new List<IncomingTask>();
		private readonly IList<IncomingTask> _ongoingTasks = new List<IncomingTask>();

		public IEnumerable<IncomingTask> IncomingTasks
		{
			get { return _incomingTasks; }
		}

		public void AddIncomingTask(DateOnly startDate, IServiceLevel serviceLevel, double totalWorkItems, TimeSpan averageWorkTimePerItem)
		{
			var endDate = startDate.AddDays((int) TimeSpan.FromSeconds(serviceLevel.Seconds).TotalDays - 1);
			var task = new IncomingTask(new DateOnlyPeriod(startDate, endDate), TimeSpan.FromSeconds(serviceLevel.Seconds), totalWorkItems, averageWorkTimePerItem);
			_incomingTasks.Add(task);
		}

		public IEnumerable<IncomingTask> TasksAffected(DateOnly date)
		{
			return _incomingTasks.Where(t => t.SpanningPeriod.Contains(date)).OrderBy(t => t.SpanningPeriod.StartDate);
		}
	}
}