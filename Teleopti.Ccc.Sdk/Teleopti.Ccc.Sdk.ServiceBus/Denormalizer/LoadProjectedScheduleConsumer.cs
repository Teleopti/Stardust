using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class LoadProjectedScheduleConsumer : ConsumerOf<DenormalizeScheduleProjection>
	{
		private readonly IServiceBus _bus;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IDenormalizedScheduleMessageBuilder _denormalizedScheduleMessageBuilder;

		public LoadProjectedScheduleConsumer(IServiceBus bus, IUnitOfWorkFactory unitOfWorkFactory, IScenarioRepository scenarioRepository, IPersonRepository personRepository, IScheduleRepository scheduleRepository, IDenormalizedScheduleMessageBuilder denormalizedScheduleMessageBuilder)
		{
			_bus = bus;
			_unitOfWorkFactory = unitOfWorkFactory;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_scheduleRepository = scheduleRepository;
			_denormalizedScheduleMessageBuilder = denormalizedScheduleMessageBuilder;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(DenormalizeScheduleProjection message)
		{
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var scenario = _scenarioRepository.Get(message.ScenarioId);
				if (!scenario.DefaultScenario) return;

				var period = new DateTimePeriod(message.StartDateTime, message.EndDateTime);
				var person = _personRepository.Get(message.PersonId);

				var timeZone = person.PermissionInformation.DefaultTimeZone();
				var dateOnlyPeriod = period.ToDateOnlyPeriod(timeZone);
				var schedule =
					_scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(new[] { person }) { DoLoadByPerson = true },
					                                                   new ScheduleDictionaryLoadOptions(false, false), dateOnlyPeriod.ToDateTimePeriod(timeZone), scenario);

				var range = schedule[person];

				DateTimePeriod? actualPeriod = message.SkipDelete ? range.TotalPeriod() : period;

				if (!actualPeriod.HasValue) return;

				DateOnlyPeriod realPeriod = actualPeriod.Value.ToDateOnlyPeriod(timeZone);
				_denormalizedScheduleMessageBuilder.Build(message, range, realPeriod, d => _bus.SendToSelf(d));
			}
		}
	}
}