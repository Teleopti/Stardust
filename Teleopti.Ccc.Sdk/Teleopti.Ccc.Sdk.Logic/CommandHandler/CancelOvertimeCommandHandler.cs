using System;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class CancelOvertimeCommandHandler : IHandleCommand<CancelOvertimeCommandDto>
    {
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
		private readonly IScheduleTagAssembler _scheduleTagAssembler;
		private readonly IScheduleRepository _scheduleRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISaveSchedulePartService _saveSchedulePartService;
    	private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;

    	public CancelOvertimeCommandHandler(IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler, IScheduleTagAssembler scheduleTagAssembler, IScheduleRepository scheduleRepository, IPersonRepository personRepository, IScenarioRepository scenarioRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, ISaveSchedulePartService saveSchedulePartService, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate)
        {
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
    		_scheduleTagAssembler = scheduleTagAssembler;
    		_scheduleRepository = scheduleRepository;
            _personRepository = personRepository;
            _scenarioRepository = scenarioRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _saveSchedulePartService = saveSchedulePartService;
    		_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
        }

		public void Handle(CancelOvertimeCommandDto command)
		{
			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var person = _personRepository.Load(command.PersonId);
				var scenario = getDesiredScenario(command);
				var dateTimePeriod = _dateTimePeriodAssembler.DtoToDomainEntity(command.Period);
				var startDate = command.Date.ToDateOnly();
				var scheduleDictionary =
					_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(
						person, new ScheduleDictionaryLoadOptions(false, false),
						new DateOnlyPeriod(startDate, startDate.AddDays(1)), scenario);

				var scheduleRange = scheduleDictionary[person];
				var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);
				var scheduleDay = scheduleRange.ScheduledDay(startDate);
				var personAssignment = scheduleDay.PersonAssignment();
				if (personAssignment != null)
				{
					cancelOvertime(personAssignment, dateTimePeriod);
				}

				var scheduleTagEntity =
					_scheduleTagAssembler.DtoToDomainEntity(new ScheduleTagDto {Id = command.ScheduleTagId});

				try
				{
					_saveSchedulePartService.Save(scheduleDay, rules, scheduleTagEntity);
					uow.PersistAll();
				}
				catch (BusinessRuleValidationException ex)
				{
					throw new FaultException(ex.Message);
				}

			}
			command.Result = new CommandResultDto {AffectedId = command.PersonId, AffectedItems = 1};
		}

		private IScenario getDesiredScenario(CancelOvertimeCommandDto command)
    	{
    		return command.ScenarioId.HasValue ? _scenarioRepository.Get(command.ScenarioId.Value) : _scenarioRepository.LoadDefaultScenario();
    	}

			private static void cancelOvertime(IPersonAssignment personAssignment, DateTimePeriod period)
			{
				var layers = personAssignment.OvertimeActivities().ToList();
				foreach (var layer in layers)
				{
					var layerPeriod = layer.Period;
					if (!layerPeriod.Intersect(period)) continue;

					personAssignment.RemoveActivity(layer);

					var newPeriods = layerPeriod.ExcludeDateTimePeriod(period);
					foreach (var dateTimePeriod in newPeriods)
					{
						if (dateTimePeriod.ElapsedTime() > TimeSpan.Zero)
						{
							personAssignment.AddOvertimeActivity(layer.Payload, dateTimePeriod, layer.DefinitionSet);
						}
					}
				}
			}
    }
}
