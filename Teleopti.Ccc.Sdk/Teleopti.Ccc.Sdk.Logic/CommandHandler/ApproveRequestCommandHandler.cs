using System.Collections.Generic;
using System.ServiceModel;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class ApproveRequestCommandHandler : IHandleCommand<ApproveRequestCommandDto>
	{
		private readonly IScheduleStorage _scheduleStorage;
				private readonly IScheduleDifferenceSaver _scheduleDictionarySaver;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IPersonRequestCheckAuthorization _authorization;
		private readonly ISwapAndModifyService _swapAndModifyService;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceService;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;
		private readonly ICheckingPersonalAccountDaysProvider _checkingPersonalAccountDaysProvider;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly ITimeZoneGuard _timeZoneGuard;

		public ApproveRequestCommandHandler(IScheduleStorage scheduleStorage, 
																								IScheduleDifferenceSaver scheduleDictionarySaver, 
																								ICurrentScenario scenarioRepository, 
																								IPersonRequestCheckAuthorization authorization, 
																								ISwapAndModifyService swapAndModifyService, 
																								IPersonRequestRepository personRequestRepository, 
																								ICurrentUnitOfWorkFactory unitOfWorkFactory, 
																								IDifferenceCollectionService<IPersistableScheduleData> differenceService,
																								IGlobalSettingDataRepository globalSettingDataRepository, 
																								ICheckingPersonalAccountDaysProvider checkingPersonalAccountDaysProvider,
																								IScheduleDayChangeCallback scheduleDayChangeCallback,
																								ITimeZoneGuard timeZoneGuard)
		{
			_scheduleStorage = scheduleStorage;
			_scheduleDictionarySaver = scheduleDictionarySaver;
			_scenarioRepository = scenarioRepository;
			_authorization = authorization;
			_swapAndModifyService = swapAndModifyService;
			_personRequestRepository = personRequestRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_differenceService = differenceService;
			_globalSettingDataRepository = globalSettingDataRepository;
			_checkingPersonalAccountDaysProvider = checkingPersonalAccountDaysProvider;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_timeZoneGuard = timeZoneGuard;
		}

		public virtual IRequestApprovalService GetRequestApprovalServiceScheduler(IScheduleDictionary scheduleDictionary,
													IScenario scenario,
													ISwapAndModifyService swapAndModifyService,
													INewBusinessRuleCollection newBusinessRules, IPersonRequest personRequest)
		{
			switch (personRequest.Request.RequestType)
			{
				case RequestType.AbsenceRequest:
					return new AbsenceRequestApprovalService(scenario, scheduleDictionary, newBusinessRules, _scheduleDayChangeCallback,
						_globalSettingDataRepository, _checkingPersonalAccountDaysProvider);
				case RequestType.ShiftTradeRequest:
					return new ShiftTradeRequestApprovalService(scheduleDictionary,
						new SwapAndModifyService(new SwapService(), _scheduleDayChangeCallback, _timeZoneGuard), newBusinessRules, _authorization, _personRequestRepository);
			}

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ApproveRequestCommandDto command)
		{
			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var personRequest = _personRequestRepository.Get(command.PersonRequestId);

				var allNewRules = NewBusinessRuleCollection.Minimum();
				var scheduleDictionary = getSchedules(personRequest);

				var approvalService = GetRequestApprovalServiceScheduler(scheduleDictionary,
																		  _scenarioRepository.Current(),
																		  _swapAndModifyService, allNewRules, personRequest);
				try
				{
					personRequest.Approve(approvalService, _authorization);
				}
				catch (InvalidRequestStateTransitionException e)
				{
					throw new FaultException(e.Message);
				}
				foreach (var range in scheduleDictionary.Values)
				{
					var diff = range.DifferenceSinceSnapshot(_differenceService);
					_scheduleDictionarySaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate) range);
				}

				uow.PersistAll();
			}
			command.Result = new CommandResultDto { AffectedId = command.PersonRequestId, AffectedItems = 1 };
		}

		private IScheduleDictionary getSchedules(IPersonRequest personRequest)
		{
			var personList = new List<IPerson>();

			var absenceRequest = personRequest.Request as IAbsenceRequest;
			if (absenceRequest != null)
			{
				personList.Add(absenceRequest.Person);
				
			}
			var shiftTradeRequest = personRequest.Request as IShiftTradeRequest;
			if (shiftTradeRequest != null)
			{
				personList.AddRange(shiftTradeRequest.InvolvedPeople());
			}
			var scheduleDictionary = getScheduleDictionary(personRequest, personList);
			return scheduleDictionary;
		}

		private IScheduleDictionary getScheduleDictionary(IPersonRequest personRequest, IEnumerable<IPerson> personList)
		{
			var timePeriod = personRequest.Request.Period;
			var dateonlyPeriod = new DateOnlyPeriod(new DateOnly(timePeriod.StartDateTime.AddDays(-1)),
													new DateOnly(timePeriod.EndDateTime.AddDays(1)));
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				personList,
				new ScheduleDictionaryLoadOptions(true, false), 
				dateonlyPeriod,
				_scenarioRepository.Current());
			((IReadOnlyScheduleDictionary)scheduleDictionary).MakeEditable();
			return scheduleDictionary;
		}
	}
}
