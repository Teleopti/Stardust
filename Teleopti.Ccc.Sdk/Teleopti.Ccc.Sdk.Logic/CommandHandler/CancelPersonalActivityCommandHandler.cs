using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class CancelPersonalActivityCommandHandler : IHandleCommand<CancelPersonalActivityCommandDto>
	{
		private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
		private readonly IScheduleTagAssembler _scheduleTagAssembler;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
		private readonly IScheduleSaveHandler _scheduleSaveHandler;

		public CancelPersonalActivityCommandHandler(IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler, IScheduleTagAssembler scheduleTagAssembler, IScheduleStorage scheduleStorage, IPersonRepository personRepository, IScenarioRepository scenarioRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate, IScheduleSaveHandler scheduleSaveHandler)
		{
			_dateTimePeriodAssembler = dateTimePeriodAssembler;
			_scheduleTagAssembler = scheduleTagAssembler;
			_scheduleStorage = scheduleStorage;
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_scheduleSaveHandler = scheduleSaveHandler;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void Handle(CancelPersonalActivityCommandDto command)
		{
			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var person = _personRepository.Load(command.PersonId);
				var scenario = getDesiredScenario(command);
				var dateTimePeriod = _dateTimePeriodAssembler.DtoToDomainEntity(command.Period);
				var startDate = command.Date.ToDateOnly();

				var scheduleDictionary =
					_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
						person, new ScheduleDictionaryLoadOptions(false, false),
						new DateOnlyPeriod(startDate, startDate.AddDays(1)), scenario);

				var scheduleRange = scheduleDictionary[person];
				var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);
				var scheduleDay = scheduleRange.ScheduledDay(startDate);
				var personAssignment = scheduleDay.PersonAssignment();
				if (personAssignment != null)
				{
					cancelPersonalActivity(personAssignment, dateTimePeriod);					
				}

				var scheduleTagEntity = _scheduleTagAssembler.DtoToDomainEntity(new ScheduleTagDto { Id = command.ScheduleTagId });

				_scheduleSaveHandler.ProcessSave(scheduleDay, rules, scheduleTagEntity);
				uow.PersistAll();
			}
			command.Result = new CommandResultDto { AffectedId = command.PersonId, AffectedItems = 1 };
		}

		private IScenario getDesiredScenario(CancelPersonalActivityCommandDto command)
		{
			return command.ScenarioId.HasValue ? _scenarioRepository.Get(command.ScenarioId.Value) : _scenarioRepository.LoadDefaultScenario();
		}

		private static void cancelPersonalActivity(IPersonAssignment personAssignment, DateTimePeriod period)
		{
			var layers = personAssignment.PersonalActivities().ToList();
			foreach (var layer in layers)
			{
				var layerPeriod = layer.Period;
				if (!layerPeriod.Intersect(period)) continue;

				personAssignment.RemoveActivity(layer);

				var newPeriods = layerPeriod.Subtract(period);
				foreach (var dateTimePeriod in newPeriods)
				{
					if (dateTimePeriod.ElapsedTime() > TimeSpan.Zero)
					{
						personAssignment.AddPersonalActivity(layer.Payload, dateTimePeriod);
					}
				}
			}
		}
	}
}