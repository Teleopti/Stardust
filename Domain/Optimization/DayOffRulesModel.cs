using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffRulesModel
	{
		public DayOffRulesModel()
		{
			Filters = new List<FilterModel>();
		}

		public int MinDayOffsPerWeek { get; set; }
		public int MaxDayOffsPerWeek { get; set; }
		public int MinConsecutiveWorkdays { get; set; }
		public int MaxConsecutiveWorkdays { get; set; }
		public int MinConsecutiveDayOffs { get; set; }
		public int MaxConsecutiveDayOffs { get; set; }
		public Guid Id { get; set; }
		public bool Default { get; set; }
		public IList<FilterModel> Filters { get; set; }
	}
}