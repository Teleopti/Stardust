using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public interface IScheduleDayReadModelsCreator
	{
		ScheduleDayReadModel GetReadModels(DenormalizedScheduleBase schedule);
	}
}