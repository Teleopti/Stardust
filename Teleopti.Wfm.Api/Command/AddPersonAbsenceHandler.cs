using System;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Wfm.Api.Command
{
	public class AddPersonAbsenceHandler : ICommandHandler<AddPersonAbsenceDto>
	{
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IAbsenceRepository _absenceRepository;
		private readonly ILog _logger = LogManager.GetLogger(typeof(AddPersonAbsenceHandler));
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonAbsenceCreator _personAbsenceCreator; 

		public AddPersonAbsenceHandler(IPersonRepository personRepository,
			IScenarioRepository scenarioRepository, IAbsenceRepository absenceRepository, IScheduleStorage scheduleStorage, IPersonAbsenceCreator personAbsenceCreator)
		{
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_absenceRepository = absenceRepository;
			_scheduleStorage = scheduleStorage;
			_personAbsenceCreator = personAbsenceCreator;
		}

		[UnitOfWork] 
		public virtual ResultDto Handle(AddPersonAbsenceDto command)
		{
			try
			{
				if (command.UtcStartTime >= command.UtcEndTime)
					return new ResultDto
					{
						Successful = false,
						Message = "UtcEndTime must be greater than UtcStartTime"
					};

				var scenario = command.ScenarioId == null
					? _scenarioRepository.LoadDefaultScenario()
					: _scenarioRepository.Load(command.ScenarioId.GetValueOrDefault());

				var person = _personRepository.Load(command.PersonId);
				var absence = _absenceRepository.Load(command.AbsenceId);
				var dateTimePeriod = new DateTimePeriod(command.UtcStartTime.Utc(), command.UtcEndTime.Utc());
				var dateOnlyPeriod = dateTimePeriod.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());

				var scheduleRange = _scheduleStorage
					.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), dateOnlyPeriod.Inflate(1),
						scenario)[person];
				var scheduleDay = scheduleRange.ScheduledDay(dateOnlyPeriod.StartDate);

				_personAbsenceCreator.Create(new AbsenceCreatorInfo
				{
					Absence = absence,
					AbsenceTimePeriod = dateTimePeriod,
					Person = person,
					ScheduleDay = scheduleDay,
					ScheduleRange = scheduleRange
				}, false);

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