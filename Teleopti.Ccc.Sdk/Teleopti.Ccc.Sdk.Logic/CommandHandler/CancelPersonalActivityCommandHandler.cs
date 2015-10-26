using System;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class CancelPersonalActivityCommandHandler : IHandleCommand<CancelPersonalActivityCommandDto>
	{
		private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
		private readonly IScheduleTagAssembler _scheduleTagAssembler;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISaveSchedulePartService _saveSchedulePartService;
		private readonly IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;

		public CancelPersonalActivityCommandHandler(IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler, IScheduleTagAssembler scheduleTagAssembler, IScheduleRepository scheduleRepository, IPersonRepository personRepository, IScenarioRepository scenarioRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, ISaveSchedulePartService saveSchedulePartService, IMessageBrokerEnablerFactory messageBrokerEnablerFactory, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate)
		{
			_dateTimePeriodAssembler = dateTimePeriodAssembler;
			_scheduleTagAssembler = scheduleTagAssembler;
			_scheduleRepository = scheduleRepository;
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_saveSchedulePartService = saveSchedulePartService;
			_messageBrokerEnablerFactory = messageBrokerEnablerFactory;
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void Handle(CancelPersonalActivityCommandDto command)
		{
			using (var uow = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
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
					cancelPersonalActivity(personAssignment, dateTimePeriod);					
				}

				var scheduleTagEntity =
					_scheduleTagAssembler.DtoToDomainEntity(new ScheduleTagDto { Id = command.ScheduleTagId });

				try
				{
					_saveSchedulePartService.Save (scheduleDay, rules, scheduleTagEntity);
					using (_messageBrokerEnablerFactory.NewMessageBrokerEnabler())
					{
						uow.PersistAll();
					}
				}
				catch (BusinessRuleValidationException ex)
				{
					throw new FaultException(ex.Message);
				}

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
			foreach (IPersonalShiftLayer layer in layers)
			{
				var layerPeriod = layer.Period;
				if (!layerPeriod.Intersect(period)) continue;

				personAssignment.RemoveActivity(layer);

				var newPeriods = layerPeriod.ExcludeDateTimePeriod(period);
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