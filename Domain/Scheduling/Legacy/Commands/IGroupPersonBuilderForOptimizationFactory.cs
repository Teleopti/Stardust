using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IGroupPersonBuilderForOptimizationFactory
	{
		void Create(IEnumerable<IPerson> allPermittedPersons, IScheduleDictionary schedules, GroupPageLight groupPageLight);
	}
}