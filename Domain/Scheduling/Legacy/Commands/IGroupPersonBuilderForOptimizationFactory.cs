using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IGroupPersonBuilderForOptimizationFactory
	{
		void Create(IEnumerable<IPerson> allPermittedPersons, IScheduleDictionary schedules, GroupPageLight groupPageLight);
	}
}