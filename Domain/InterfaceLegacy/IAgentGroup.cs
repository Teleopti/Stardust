using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces
{
	public interface IAgentGroup : IAggregateRoot
	{
		IEnumerable<IFilter> Filters { get; }
		string Name { get; }
		void ClearFilters();
		IAgentGroup AddFilter(IFilter filter);
		void ChangeName(string name);
	}
}