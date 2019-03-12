using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Wfm.Api.Command
{
	public class AddMeetingHandler : ICommandHandler<AddMeetingDto>
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ISaveSchedulePartService _saveSchedulePartService;

		public AddMeetingHandler(IScenarioRepository scenarioRepository, IPersonRepository personRepository, IActivityRepository activityRepository, IScheduleStorage scheduleStorage, ISaveSchedulePartService saveSchedulePartService)
		{
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_activityRepository = activityRepository;
			_scheduleStorage = scheduleStorage;
			_saveSchedulePartService = saveSchedulePartService;
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
			var dateOnly = new DateOnly(command.UtcStartTime.Utc());
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), dateOnly.ToDateOnlyPeriod(), scenario);
			var range = scheduleDictionary[person];
			var day = range.ScheduledDay(dateOnly);
			var assignment = day.PersonAssignment(true);
			var activity = _activityRepository.Load(command.ActivityId);
			assignment.AddMeeting(activity, new DateTimePeriod(command.UtcStartTime.Utc(), command.UtcEndTime.Utc()), command.MeetingId);
			_saveSchedulePartService.Save(day,NewBusinessRuleCollection.Minimum(), KeepOriginalScheduleTag.Instance);
			return new ResultDto
			{
				Successful = true
			};
		}
	}
}