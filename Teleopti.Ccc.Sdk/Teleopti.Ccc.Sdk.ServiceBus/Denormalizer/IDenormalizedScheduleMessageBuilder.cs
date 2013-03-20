using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public interface IDenormalizedScheduleMessageBuilder
	{
		void Build<T>(ScheduleDenormalizeBase message, IScheduleRange range, DateOnlyPeriod realPeriod, Action<T> actionForItems)
			where T : DenormalizedScheduleBase, new();
	}
}