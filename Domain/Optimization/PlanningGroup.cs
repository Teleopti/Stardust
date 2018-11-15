using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroup : NonversionedAggregateRootWithBusinessUnit, IDeleteTag
	{
		private readonly ISet<IFilter> _filters = new HashSet<IFilter>();
		private readonly IList<PlanningGroupSettings> _settings = new List<PlanningGroupSettings>();
		private bool _isDeleted;

		public PlanningGroup():this(string.Empty)
		{
		}

		public PlanningGroup(string name)
		{
			Name = name;
		}

		public virtual IEnumerable<IFilter> Filters => _filters;
		public virtual string Name { get; protected set; }
		public virtual AllPlanningGroupSettings Settings => new AllPlanningGroupSettings(_settings.OrderByDescending(x=>x.Priority));

		public virtual void ChangeName(string name)
		{
			Name = name;
		}

		public virtual void ClearFilters()
		{
			_filters.Clear();
		}

		public virtual PlanningGroup AddFilter(IFilter filter)
		{
			_filters.Add(filter);
			return this;
		}

		public virtual bool IsDeleted => _isDeleted;
		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}

		public virtual void AddSetting(PlanningGroupSettings planningGroupSettings)
		{
			planningGroupSettings.SetParent(this);
			_settings.Add(planningGroupSettings);
		}

		public virtual void RemoveSetting(PlanningGroupSettings planningGroupSettings)
		{
			if(planningGroupSettings.Default)
				throw new ArgumentException("Cannot remove default PlanningGroupSettings.");
			_settings.Remove(planningGroupSettings);
		}
	}
}