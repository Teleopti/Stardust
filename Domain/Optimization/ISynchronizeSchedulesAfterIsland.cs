using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface ISynchronizeSchedulesAfterIsland
	{
		void Synchronize(IScheduleDictionary modifiedScheduleDictionary, DateOnlyPeriod period);
	}
}