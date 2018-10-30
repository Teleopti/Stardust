using System;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

/* DO NOT USE! 
It only contains vary simple logic. 
Used for staffhub integration only until further notice */

//TODO personal account
namespace Teleopti.Wfm.Api.Command
{
	public class RemoveAbsenceHandler : ICommandHandler<RemoveAbsenceDto>
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof(RemoveAbsenceHandler));
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ISaveSchedulePartService _saveSchedulePartService;
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;

		public RemoveAbsenceHandler(IScenarioRepository scenarioRepository, IPersonRepository personRepository, 
			IPersonAbsenceRepository personAbsenceRepository, IScheduleStorage scheduleStorage, 
			ISaveSchedulePartService saveSchedulePartService, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate)
		{
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_personAbsenceRepository = personAbsenceRepository;
			_scheduleStorage = scheduleStorage;
			_saveSchedulePartService = saveSchedulePartService;
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
		}

		[UnitOfWork]
		public virtual ResultDto Handle(RemoveAbsenceDto command)
		{
			try
			{
				if (command.PeriodStartUtc >= command.PeriodEndUtc)
					return new ResultDto
					{
						Successful = false,
						Message = "PeriodEndUtc must be greater than PeriodStartUtc"
					};

				var scenario = command.ScenarioId == null
					? _scenarioRepository.LoadDefaultScenario()
					: _scenarioRepository.Load(command.ScenarioId.GetValueOrDefault());

				var person = _personRepository.Load(command.PersonId);
				var period = new DateTimePeriod(command.PeriodStartUtc.Utc(), command.PeriodEndUtc.Utc());
				var dateOnlyPeriod = period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());

				var scheduleRange = _scheduleStorage
					.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), dateOnlyPeriod.Inflate(1),
						scenario)[person];
				var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);

				foreach (var dateOnly in dateOnlyPeriod.DayCollection())
				{
					var scheduleDay = scheduleRange.ScheduledDay(dateOnly);
					var absences = scheduleDay.PersonAbsenceCollection(true);
					foreach (var personAbsence in absences)
					{
						var splitPeriods = personAbsence.Split(period);
						if (splitPeriods.Count > 0 || period.Contains(personAbsence.Layer.Period))
						{
							scheduleDay.Remove(personAbsence);
							foreach (var splitPeriod in splitPeriods)
							{
								scheduleDay.Add(splitPeriod);
							}
						}
					}
					_saveSchedulePartService.Save(scheduleDay, rules, scheduleDay.ScheduleTag());
				}

				return new ResultDto
				{
					Successful = true
				};
			}
			catch (Exception e)
			{
				_logger.Error(e.Message + e.StackTrace);
				return new ResultDto
				{
					Successful = false
				};
			}
		}
	}
}