using System;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Api.Command
{
	public class AddAbsenceHandler : ICommandHandler<AddAbsenceDto>
	{
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IAbsenceRepository _absenceRepository;
		private readonly ILog _logger = LogManager.GetLogger(typeof(AddAbsenceHandler));
		private readonly IPersonAbsenceRepository _personAbsenceRepository;

		public AddAbsenceHandler(IPersonRepository personRepository,
			IScenarioRepository scenarioRepository, IAbsenceRepository absenceRepository, IPersonAbsenceRepository personAbsenceRepository)
		{
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_absenceRepository = absenceRepository;
			_personAbsenceRepository = personAbsenceRepository;
		}

		[UnitOfWork]
		public virtual ResultDto Handle(AddAbsenceDto command)
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
				var absenceLayer = new AbsenceLayer(absence, new DateTimePeriod(command.UtcStartTime.Utc(), command.UtcEndTime.Utc()));
				var personAbsence = new PersonAbsence(person, scenario, absenceLayer);
				((IRepository<IPersonAbsence>)_personAbsenceRepository).Add(personAbsence);
				return new ResultDto
				{
					Id = personAbsence.Id,
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