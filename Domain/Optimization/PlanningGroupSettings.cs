using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroupSettings : AggregateEntity
	{
		private readonly ISet<IFilter> _filters = new HashSet<IFilter>();
		public virtual MinMax<int> DayOffsPerWeek { get; set; }
		public virtual MinMax<int> ConsecutiveWorkdays { get; set; }
		public virtual MinMax<int> ConsecutiveDayOffs { get; set; }
		public virtual bool Default { get; protected set; }
		public virtual BlockFinderType BlockFinderType { get; set; }
		public virtual bool BlockSameShiftCategory { get; set; }
		public virtual bool BlockSameStartTime { get; set; }
		public virtual bool BlockSameShift { get; set; }
		public virtual int Priority { get; set; }
		public virtual MinMax<int> FullWeekendsOff { get; set; }
		public virtual MinMax<int> WeekendDaysOff { get; set; }
		public virtual Percent PreferenceValue { get; set; }

		public PlanningGroupSettings()
		{
			Name = string.Empty;
			DayOffsPerWeek = new MinMax<int>(1, 3);
			ConsecutiveDayOffs = new MinMax<int>(1, 3);
			ConsecutiveWorkdays = new MinMax<int>(2, 6);
			BlockFinderType = BlockFinderType.SingleDay;
			BlockSameShiftCategory = false;
			BlockSameStartTime = false;
			BlockSameShift = false;
			FullWeekendsOff = new MinMax<int>(0, 8);
			WeekendDaysOff = new MinMax<int>(0, 16);
		}

		public virtual void SetAsDefault()
		{
			Default = true;
			Name = UserTexts.Resources.Default;
			Priority = -1;
		}

		public virtual IEnumerable<IFilter> Filters => _filters;

		public virtual string Name { get; set; }

		public virtual void AddFilter(IFilter filter)
		{
			_filters.Add(filter);
		}

		public virtual void ClearFilters()
		{
			_filters.Clear();
		}

		public virtual bool IsValidForAgent(IPerson person, DateOnly dateOnly)
		{
			var validFilterTypes = new Dictionary<string, bool>();
			foreach (var filter in _filters)
			{
				var filterType = filter.FilterType;
				if(validFilterTypes.TryGetValue(filterType, out var currFilterValue) && currFilterValue)
					continue;

				validFilterTypes[filterType] = filter.IsValidFor(person, dateOnly);
			}

			return validFilterTypes.Keys.All(key => validFilterTypes[key]);
		}
	}
}