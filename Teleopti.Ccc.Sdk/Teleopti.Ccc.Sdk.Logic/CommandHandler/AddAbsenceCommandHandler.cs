using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class AddScheduleChangesListenerCommandHandler : IHandleCommand<AddScheduleChangesListenerCommandDto>
	{
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public AddScheduleChangesListenerCommandHandler(IGlobalSettingDataRepository globalSettingDataRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_globalSettingDataRepository = globalSettingDataRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public void Handle(AddScheduleChangesListenerCommandDto command)
		{
			Uri uri;
			validateListener(command.Listener, out uri);
			validatePermissions();
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var setting = _globalSettingDataRepository.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions());
				setting.Add(new ScheduleChangeListener
				{
					Name = command.Listener.Name,
					RelativeDateRange =
						new MinMax<int>(command.Listener.DaysStartFromCurrentDate, command.Listener.DaysEndFromCurrentDate),
					Uri = uri
				});
				_globalSettingDataRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, setting);
				uow.PersistAll();
			}
			command.Result = new CommandResultDto { AffectedId = null, AffectedItems = 1 };
		}

		private void validateListener(ScheduleChangesListenerDto listener, out Uri uri)
		{
			if (listener == null || string.IsNullOrWhiteSpace(listener.Name))
			{
				throw new FaultException("The listener must have a name defined.");
			}
			if (listener.DaysEndFromCurrentDate < listener.DaysStartFromCurrentDate)
			{
				throw new FaultException("The listener relative start date cannot be set after the end date.");
			}
			if (!Uri.TryCreate(listener.Url, UriKind.Absolute, out uri))
			{
				throw new FaultException("The given url cannot be parsed as a valid uri.");
			}
		}

		private static void validatePermissions()
		{
			var principalAuthorization = PrincipalAuthorization.Instance();
			if (!principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage) &&
				!principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.WebPermissions))
			{
				throw new FaultException("This function requires higher permissions.");
			}
		}
	}

	public class AddAbsenceCommandHandler : IHandleCommand<AddAbsenceCommandDto>
    {
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
        private readonly IAbsenceRepository _absenceRepository;
        private readonly IScheduleStorage _scheduleStorage;
        private readonly IPersonRepository _personRepository;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
    	private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
		private readonly IScheduleTagAssembler _scheduleTagAssembler;
		private readonly IScheduleSaveHandler _scheduleSaveHandler;


		public AddAbsenceCommandHandler(IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler, IAbsenceRepository absenceRepository, IScheduleStorage scheduleStorage, IPersonRepository personRepository, IScenarioRepository scenarioRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate, IScheduleTagAssembler scheduleTagAssembler, IScheduleSaveHandler scheduleSaveHandler)
        {
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
            _absenceRepository = absenceRepository;
            _scheduleStorage = scheduleStorage;
            _personRepository = personRepository;
            _scenarioRepository = scenarioRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
    		_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_scheduleTagAssembler = scheduleTagAssembler;
			_scheduleSaveHandler = scheduleSaveHandler;
        }

		public void Handle(AddAbsenceCommandDto command)
		{
			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var person = _personRepository.Load(command.PersonId);
				var scenario = getDesiredScenario(command);
				var startDate = command.Date.ToDateOnly();
				var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
					person, new ScheduleDictionaryLoadOptions(false, false),
					new DateOnlyPeriod(startDate, startDate.AddDays(1)), scenario);

				var scheduleRange = scheduleDictionary[person];
				var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);

				var scheduleDay = scheduleRange.ScheduledDay(startDate);
				var absence = _absenceRepository.Load(command.AbsenceId);
				var absenceLayer = new AbsenceLayer(absence, _dateTimePeriodAssembler.DtoToDomainEntity(command.Period));
				scheduleDay.CreateAndAddAbsence(absenceLayer);

				var scheduleTagEntity = _scheduleTagAssembler.DtoToDomainEntity(new ScheduleTagDto {Id = command.ScheduleTagId});

				_scheduleSaveHandler.ProcessSave(scheduleDay, rules, scheduleTagEntity);
				uow.PersistAll();
			}
			command.Result = new CommandResultDto {AffectedId = command.PersonId, AffectedItems = 1};
		}

		private IScenario getDesiredScenario(AddAbsenceCommandDto command)
    	{
    		return command.ScenarioId.HasValue ? _scenarioRepository.Get(command.ScenarioId.Value) : _scenarioRepository.LoadDefaultScenario();
    	}
    }
}
