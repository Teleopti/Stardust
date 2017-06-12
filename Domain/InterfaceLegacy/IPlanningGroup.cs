using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy
{
	public interface IPlanningGroup : IAggregateRoot
	{
		IEnumerable<IFilter> Filters { get; }
		string Name { get; }
		void ClearFilters();
		IPlanningGroup AddFilter(IFilter filter);
		void ChangeName(string name);
	}
}