using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class AgentGroup : NonversionedAggregateRootWithBusinessUnit, IAgentGroup
	{
		private readonly ISet<IFilter> _filters = new HashSet<IFilter>();

		public AgentGroup():this(String.Empty)
		{

		}

		public AgentGroup(string name)
		{
			Name = name;
		}

		public virtual IEnumerable<IFilter> Filters => _filters;
		public virtual string Name { get; private set; }

		public virtual void ChangeName(string name)
		{
			Name = name;
		}


		public virtual void ClearFilters()
		{
			_filters.Clear();
		}

		public virtual void AddFilter(IFilter filter)
		{
			_filters.Add(filter);
		}
	}

}