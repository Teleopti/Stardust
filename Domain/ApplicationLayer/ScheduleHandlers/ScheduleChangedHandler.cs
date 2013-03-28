using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleHandlers
{
	public class ScheduleChangedHandler : 
		IHandleEvent<ScheduleChangedEvent>, 
		IHandleEvent<ScheduleProjectionInitializeTriggeredEvent>,
		IHandleEvent<ScheduleDayInitializeTriggeredEvent>,
		IHandleEvent<PersonScheduleDayInitializeTriggeredEvent>
	{
		private readonly IQuestionablyPublishMoreEvents _bus;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IProjectionChangedEventBuilder _projectionChangedEventBuilder;
		private IScheduleRange _range;
		private DateOnlyPeriod _realPeriod;

		public ScheduleChangedHandler(IQuestionablyPublishMoreEvents bus, IUnitOfWorkFactory unitOfWorkFactory, IScenarioRepository scenarioRepository, IPersonRepository personRepository, IScheduleRepository scheduleRepository, IProjectionChangedEventBuilder projectionChangedEventBuilder)
		{
			_bus = bus;
			_unitOfWorkFactory = unitOfWorkFactory;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_scheduleRepository = scheduleRepository;
			_projectionChangedEventBuilder = projectionChangedEventBuilder;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ScheduleChangedEvent message)
		{
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				if (!getPeriodAndScenario(message)) return;
				_projectionChangedEventBuilder.Build<ProjectionChangedEvent>(message, _range, _realPeriod, d => _bus.Publish(d));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(PersonScheduleDayInitializeTriggeredEvent message)
		{
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				if (!getPeriodAndScenario(message)) return;
				_projectionChangedEventBuilder.Build<ProjectionChangedEventForPersonScheduleDay>(message, _range, _realPeriod, d => _bus.Publish(d));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ScheduleDayInitializeTriggeredEvent message)
		{
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				if (!getPeriodAndScenario(message)) return;
				_projectionChangedEventBuilder.Build<ProjectionChangedEventForScheduleDay>(message, _range, _realPeriod, d => _bus.Publish(d));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ScheduleProjectionInitializeTriggeredEvent message)
		{
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				if (!getPeriodAndScenario(message)) return;
				_projectionChangedEventBuilder.Build<ProjectionChangedEventForScheduleProjection>(message, _range, _realPeriod, d => _bus.Publish(d));
			}
		}

		private bool getPeriodAndScenario(ScheduleChangedEventBase message)
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