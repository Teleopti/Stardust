﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffRules : NonversionedAggregateRootWithBusinessUnit
	{
		private readonly ISet<IFilter> _filters = new HashSet<IFilter>();
		public virtual MinMax<int> DayOffsPerWeek { get; set; }
		public virtual MinMax<int> ConsecutiveWorkdays { get; set; }
		public virtual MinMax<int> ConsecutiveDayOffs { get; set; }
		public virtual bool Default { get; protected set; }

		public DayOffRules()
		{
			Name = string.Empty;
		}

		public static DayOffRules CreateDefault()
		{
			return new DayOffRules
			{
				DayOffsPerWeek = new MinMax<int>(1, 3),
				ConsecutiveDayOffs = new MinMax<int>(1, 3),
				ConsecutiveWorkdays = new MinMax<int>(2, 6),
				Default = true,
				Name = UserTexts.Resources.Default
			};
		}

		public virtual IEnumerable<IFilter> Filters
		{
			get { return _filters; }
		}

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
				bool currFilterValue;
				if(validFilterTypes.TryGetValue(filterType, out currFilterValue) && currFilterValue)
					continue;

				validFilterTypes[filterType] = filter.IsValidFor(person, dateOnly);
      }

			return validFilterTypes.Keys.All(key => validFilterTypes[key]);
		}
	}
}