using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy
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