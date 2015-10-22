using System;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IIntradayStatistics
	{
		Guid SkillId { get; set; }
		string SkillName { get; set; }
		DateTime Interval { get; set; }
		double StatAnsweredTasks { get; set; }
		double StatOfferedTasks { get; set; }
		double StatAbandonedTasks { get; set; }
		int StatAbandonedShortTasks { get; set; }
		double StatAbandonedTasksWithinSL { get; set; }
		double StatOverflowOutTasks { get; set; }
		double StatOverflowInTasks { get; set; }
		double StatAverageTaskTimeSeconds { get; set; }
		double StatAverageAfterTaskTimeSeconds { get; set; }
	}
}