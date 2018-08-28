using System.Collections.Generic;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IFindPersonAssignment
	{
		Task<IEnumerable<IPersonAssignment>> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario);
	}
}