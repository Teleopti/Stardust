using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public interface IDenormalizedScheduleMessageBuilder
	{
		void Build(DenormalizeScheduleProjection message, IScheduleRange range, DateOnlyPeriod realPeriod, Action<DenormalizedSchedule> actionForEachItem);
	}
}