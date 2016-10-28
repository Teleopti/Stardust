using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IGroupScheduleGroupPageDataProvider : IGroupPageDataProvider
	{
		IEnumerable<IPerson> AllLoadedPersons { get; }
	}
}