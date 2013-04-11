using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class LoadProjectedScheduleConsumer : ConsumerOf<ScheduleChanged>, ConsumerOf<ScheduleProjectionInitialize>, ConsumerOf<ScheduleDayInitialize>, ConsumerOf<PersonScheduleDayInitialize>
	{
		private readonly IServiceBus _bus;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IDenormalizedScheduleMessageBuilder _denormalizedScheduleMessageBuilder;
		private IScheduleRange _range;
		private DateOnlyPeriod _realPeriod;

		public LoadProjectedScheduleConsumer(IServiceBus bus, ICurrentUnitOfWorkFactory unitOfWorkFactory, IScenarioRepository scenarioRepository, IPersonRepository personRepository, IScheduleRepository scheduleRepository, IDenormalizedScheduleMessageBuilder denormalizedScheduleMessageBuilder)
		{
			_bus = bus;
			_unitOfWorkFactory = unitOfWorkFactory;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_scheduleRepository = scheduleRepository;
			_denormalizedScheduleMessageBuilder = denormalizedScheduleMessageBuilder;
		    
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(ScheduleChanged message)
		{
			using (_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				if (!getPeriodAndScenario(message)) return;
				_denormalizedScheduleMessageBuilder.Build<DenormalizedSchedule>(message, _range, _realPeriod, d => _bus.SendToSelf(d));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(PersonScheduleDayInitialize message)
		{
			using (_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				if (!getPeriodAndScenario(message)) return;
				_denormalizedScheduleMessageBuilder.Build<DenormalizedScheduleForPersonScheduleDay>(message, _range, _realPeriod, d => _bus.SendToSelf(d));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(ScheduleDayInitialize message)
		{
			using (_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				if (!getPeriodAndScenario(message)) return;
				_denormalizedScheduleMessageBuilder.Build<DenormalizedScheduleForScheduleDay>(message, _range, _realPeriod, d => _bus.SendToSelf(d));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(ScheduleProjectionInitialize message)
		{
			using (_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				if (!getPeriodAndScenario(message)) return;
				_denormalizedScheduleMessageBuilder.Build<DenormalizedScheduleForScheduleProjection>(message, _range, _realPeriod, d => _bus.SendToSelf(d));
			}
		}

		private bool getPeriodAndScenario(ScheduleDenormalizeBase message)
		{
			var scenario = _scenarioRepository.Get(message.ScenarioId);
			if (!scenario.DefaultScenario) return false;

			var period = new DateTimePeriod(message.StartDateTime, message.EndDateTime);
			var person = _personRepository.FindPeople(new []{ message.PersonId}).FirstOrDefault();
			if (person == null) return false;
                    
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var dateOnlyPeriod = period.ToDateOnlyPeriod(timeZone);
			var schedule =
				_scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(new[] {person}) {DoLoadByPerson = true},
				                                                   new ScheduleDictionaryLoadOptions(false, false),
				                                                   dateOnlyPeriod.ToDateTimePeriod(timeZone), scenario);

			_range = schedule[person];

			DateTimePeriod? actualPeriod = message.SkipDelete ? _range.TotalPeriod() : period;

			if (!actualPeriod.HasValue) return false;

			_realPeriod = actualPeriod.Value.ToDateOnlyPeriod(timeZone);
			return true;
		}
	}
}