using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class AgentGroup : NonversionedAggregateRootWithBusinessUnit, IAgentGroup, IDeleteTag
	{
		private readonly ISet<IFilter> _filters = new HashSet<IFilter>();
		private bool _isDeleted;

		public AgentGroup():this(string.Empty)
		{
		}

		public AgentGroup(string name)
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

		public virtual IAgentGroup AddFilter(IFilter filter)
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