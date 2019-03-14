using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;

namespace Teleopti.Wfm.Api.Command
{
	public class AddMeetingHandler : ICommandHandler<AddMeetingDto>
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ISaveSchedulePartService _saveSchedulePartService;
		private readonly IExternalMeetingRepository _externalMeetingRepository;

		public AddMeetingHandler(IScenarioRepository scenarioRepository, IPersonRepository personRepository, IActivityRepository activityRepository, IScheduleStorage scheduleStorage, ISaveSchedulePartService saveSchedulePartService, IExternalMeetingRepository externalMeetingRepository)
		{
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_activityRepository = activityRepository;
			_scheduleStorage = scheduleStorage;
			_saveSchedulePartService = saveSchedulePartService;
			_externalMeetingRepository = externalMeetingRepository;
		}

		[UnitOfWork]
		public virtual ResultDto Handle(AddMeetingDto command)
		{
			if (command.UtcStartTime >= command.UtcEndTime)
				return new ResultDto
				{
					Successful = false,
					Message = "UtcEndTime must be greater than UtcStartTime"
				};

			var scenario = !command.ScenarioId.HasValue
				? _scenarioRepository.LoadDefaultScenario()
				: _scenarioRepository.Load(command.ScenarioId.Value);
			
			var person = _personRepository.Load(command.PersonId);
			var dateOnlyPeriod = new DateTimePeriod(command.UtcStartTime, command.UtcEndTime).ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), dateOnlyPeriod, scenario);
			var range = scheduleDictionary[person];
			var day = range.ScheduledDay(dateOnlyPeriod.StartDate);
			var assignment = day.PersonAssignment(true);
			var activity = _activityRepository.Load(command.ActivityId);
			var externalMeeting = _externalMeetingRepository.Get(command.MeetingId);
			if (externalMeeting == null)
			{
				externalMeeting = new ExternalMeeting
				{
					Title = command.Title,
					Agenda = command.Agenda
				};
				externalMeeting.SetId(command.MeetingId);
				_externalMeetingRepository.Add(externalMeeting);
			}
			assignment.AddMeeting(activity, new DateTimePeriod(command.UtcStartTime, command.UtcEndTime), externalMeeting);
			_saveSchedulePartService.Save(day, NewBusinessRuleCollection.Minimum(), KeepOriginalScheduleTag.Instance);
			return new ResultDto
			{
				Successful = true
			};
		}
	}
}