using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IGroupPersonBuilderForOptimizationFactory
	{
		void Create(IScheduleDictionary schedules, GroupPageLight groupPageLight);
	}
}