using System;
using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.Aop;
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

		public RemoveAbsenceHandler(IScenarioRepository scenarioRepository, IPersonRepository personRepository, 
			IPersonAbsenceRepository personAbsenceRepository)
		{
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_personAbsenceRepository = personAbsenceRepository;
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
				var personAbsences = _personAbsenceRepository.Find(new[] {person},
					period, scenario);
				foreach (var absence in personAbsences)
				{
					IEnumerable<IPersonAbsence> splittedAbsences = new List<IPersonAbsence>();
					if (absence.Period.StartDateTime < command.PeriodStartUtc || absence.Period.EndDateTime > command.PeriodEndUtc)
					{
						splittedAbsences = absence.Split(period);
					}
					((IRepository<IPersonAbsence>)_personAbsenceRepository).Remove(absence);
					foreach (var splittedAbsence in splittedAbsences)
					{
						((IRepository<IPersonAbsence>)_personAbsenceRepository).Add(splittedAbsence);
					}
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