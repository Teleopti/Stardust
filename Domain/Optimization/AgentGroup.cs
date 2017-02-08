using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Optimization.Filters;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class AgentGroup : NonversionedAggregateRootWithBusinessUnit
	{
		private readonly ISet<IFilter> _filters = new HashSet<IFilter>();

		public AgentGroup()
		{
			Name = string.Empty;
		}

		public virtual IEnumerable<IFilter> Filters => _filters;
		public virtual string Name { get; set; }

		public void ClearFilters()
		{
			_filters.Clear();
		}

		public void AddFilter(IFilter filter)
		{
			_filters.Add(filter);
		}
	}
}