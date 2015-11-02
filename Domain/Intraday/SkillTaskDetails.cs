using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class SkillTaskDetails : IEquatable<SkillTaskDetails>
	{
		public Guid SkillId { get; set; }
		public string SkillName { get; set; }
		public List<IntervalTasks> IntervalTasks { get; set; }

		public bool Equals(SkillTaskDetails other)
		{
			return SkillId==other.SkillId;
		}
	}

	public interface ISkillForecastedTasksProvider
	{
		IList<SkillTaskDetails> GetForecastedTasks(DateTime now);
	}

	public class IntervalTasks
	{
		public DateTime IntervalStart { get; set; }
		public double Task { get; set; }
	}
}