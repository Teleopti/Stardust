using System;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class UpdateScheduleProjectionReadModel : IUpdateScheduleProjectionReadModel
	{
		private readonly IDenormalizedScheduleMessageBuilder _denormalizedScheduleMessageBuilder;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;

		public UpdateScheduleProjectionReadModel(IDenormalizedScheduleMessageBuilder denormalizedScheduleMessageBuilder,
		                                         IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository)
		{
			_denormalizedScheduleMessageBuilder = denormalizedScheduleMessageBuilder;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			MessageId = "0")]
		public void Execute(IScheduleRange scheduleRange, DateOnlyPeriod dateOnlyPeriod)
		{
			_denormalizedScheduleMessageBuilder.Build<DenormalizedSchedule>(
				new ScheduleChanged
					{
						ScenarioId = scheduleRange.Scenario.Id.GetValueOrDefault(),
						PersonId = scheduleRange.Person.Id.GetValueOrDefault()
					}, scheduleRange, dateOnlyPeriod, updateReadModel);
		}

		private void updateReadModel(DenormalizedSchedule[] messages)
		{
			foreach (var message in messages)
			{
				var date = new DateOnly(message.Date);
				if (!message.IsInitialLoad)
				{
					_scheduleProjectionReadOnlyRepository.ClearPeriodForPerson(
						new DateOnlyPeriod(date, date), message.ScenarioId, message.PersonId);
				}

				foreach (var layer in message.Layers)
				{
					_scheduleProjectionReadOnlyRepository.AddProjectedLayer(date, message.ScenarioId, message.PersonId, layer);
				}
			}
		}
	}
}