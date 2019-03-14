using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Wfm.Api.Command
{
	public class RemoveMeetingHandler : ICommandHandler<RemoveMeetingDto>
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ISaveSchedulePartService _saveSchedulePartService;

		public RemoveMeetingHandler(IScenarioRepository scenarioRepository, IPersonRepository personRepository, IScheduleStorage scheduleStorage, ISaveSchedulePartService saveSchedulePartService)
		{
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_saveSchedulePartService = saveSchedulePartService;
		}

		[UnitOfWork]
		public virtual ResultDto Handle(RemoveMeetingDto command)
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
			var dateTimePeriod = new DateTimePeriod(command.UtcStartTime, command.UtcEndTime);
			var dateOnlyPeriod = dateTimePeriod.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
			
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), dateOnlyPeriod, scenario);
			var range = scheduleDictionary[person];
			var scheduleDays = range.ScheduledDayCollection(dateOnlyPeriod).ToArray();
			foreach (var scheduleDay in scheduleDays)
			{
				var assignment = scheduleDay.PersonAssignment();
				var meetingShiftLayers = assignment.Meetings().ToArray();
				foreach (var meetingLayer in meetingShiftLayers)
				{
					if ( meetingLayer.Meeting.Id == command.MeetingId && dateTimePeriod.Equals(meetingLayer.Period))
					{
						assignment.RemoveActivity(meetingLayer);
					}
				}
			}
			_saveSchedulePartService.Save(scheduleDays, NewBusinessRuleCollection.Minimum(), KeepOriginalScheduleTag.Instance);
			return new ResultDto
			{
				Successful = true
			};
		}
	}
}