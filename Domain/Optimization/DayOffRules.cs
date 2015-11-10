using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffRules : NonversionedAggregateRootWithBusinessUnit
	{
		private readonly IList<IFilter> _filters = new List<IFilter>();
		public virtual MinMax<int> DayOffsPerWeek { get; set; }
		public virtual MinMax<int> ConsecutiveWorkdays { get; set; }
		public virtual MinMax<int> ConsecutiveDayOffs { get; set; }
		public virtual bool Default { get; protected set; }

		public static DayOffRules CreateDefault()
		{
      return new DayOffRules
			{
				DayOffsPerWeek = new MinMax<int>(1, 3),
				ConsecutiveDayOffs = new MinMax<int>(1, 3),
				ConsecutiveWorkdays = new MinMax<int>(2, 6),
				Default = true
			};
		}

		public void AddFilter(IFilter filter)
		{
			_filters.Add(filter);
		}

		public bool IsValidForAgent(IPerson person, DateOnly dateOnly)
		{
			return _filters.Any(filter => filter.IsValidFor(person, dateOnly));
		}
	}
}