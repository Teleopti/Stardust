using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public interface IUpdateScheduleProjectionReadModel
	{
		void Execute(IScenario scenario,DateTimePeriod period,IPerson person);
		void SetSkipDelete(bool skipDelete);
	}
}