using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroup : NonversionedAggregateRootWithBusinessUnit, IDeleteTag
	{
		private readonly ISet<IFilter> _filters = new HashSet<IFilter>();
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
	}
}