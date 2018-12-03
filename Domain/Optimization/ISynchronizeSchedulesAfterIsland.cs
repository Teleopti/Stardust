using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface ISynchronizeSchedulesAfterIsland
	{
		void Synchronize(IScheduleDictionary modifiedScheduleDictionary, DateOnlyPeriod period);
	}
}