using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces
{
	public interface IAgentGroup : IAggregateRoot
	{
		IEnumerable<IFilter> Filters { get; }
		string Name { get; set; }
		void ClearFilters();
		void AddFilter(IFilter filter);
	}
}