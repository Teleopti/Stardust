using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using log4net;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

/* DO NOT USE! 
It only contains vary simple logic. 
Used for staffhub integration only until further notice */

namespace Teleopti.Wfm.Api.Command
{
	public class SetMainShiftHandler : ICommandHandler<SetMainShiftDto>
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IShiftCategoryRepository _shiftCategoryRepository;
		private readonly ISaveSchedulePartService _saveSchedulePartService;

		private static readonly ILog logger = LogManager.GetLogger(typeof(SetMainShiftHandler));

		public SetMainShiftHandler(IScenarioRepository scenarioRepository, 
			IPersonRepository personRepository, IActivityRepository activityRepository, 
			IScheduleStorage scheduleStorage, 
			IShiftCategoryRepository shiftCategoryRepository, ISaveSchedulePartService saveSchedulePartService)
		{
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_activityRepository = activityRepository;
			_scheduleStorage = scheduleStorage;
			_shiftCategoryRepository = shiftCategoryRepository;
			_saveSchedulePartService = saveSchedulePartService;
		}

		[UnitOfWork]
		public virtual ResultDto Handle(SetMainShiftDto command)
		{
			try
			{
				var scenario = command.ScenarioId == null
					? _scenarioRepository.LoadDefaultScenario()
					: _scenarioRepository.Load(command.ScenarioId.GetValueOrDefault());
				var person = _personRepository.Load(command.PersonId);
				var dateOnly = command.Date.ToDateOnly();

				var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), dateOnly.ToDateOnlyPeriod(), scenario);
				var range = scheduleDictionary[person];
				var day = range.ScheduledDay(dateOnly);
				var assignment = day.PersonAssignment(true);
				var shiftCategory = _shiftCategoryRepository.Get(command.ShiftCategory);
				if (shiftCategory!=null)
					assignment.SetShiftCategory(shiftCategory);

				assignment.ClearMainActivities();

				foreach (var layer in command.LayerCollection)
				{
					var activity = _activityRepository.Load(layer.ActivityId);
					assignment.AddActivity(activity, new DateTimePeriod(layer.UtcStartDateTime.Utc(), layer.UtcEndDateTime.Utc()),
						true);
				}
				
				_saveSchedulePartService.Save(day,NewBusinessRuleCollection.Minimum(), KeepOriginalScheduleTag.Instance);

				return new ResultDto
				{
					Successful = true
				};
			}
			catch (Exception e)
			{
				logger.Error(e.Message + e.StackTrace);
				return new ResultDto
				{
					Successful = false
				};
			}
		}
	}
}