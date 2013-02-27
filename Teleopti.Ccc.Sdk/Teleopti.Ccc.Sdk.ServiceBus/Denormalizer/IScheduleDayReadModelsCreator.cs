using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public interface IScheduleDayReadModelsCreator
	{
		ScheduleDayReadModel GetReadModel(DenormalizedScheduleDay schedule, IPerson person);
	}
}