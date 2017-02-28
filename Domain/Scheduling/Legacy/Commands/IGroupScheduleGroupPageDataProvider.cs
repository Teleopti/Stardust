using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IGroupScheduleGroupPageDataProvider : IGroupPageDataProvider
	{
		IEnumerable<IPerson> AllLoadedPersons { get; }
	}
}