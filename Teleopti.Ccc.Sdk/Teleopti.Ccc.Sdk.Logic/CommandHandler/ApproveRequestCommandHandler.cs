using System.Collections.Generic;
using System.ServiceModel;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class ApproveRequestCommandHandler : IHandleCommand<ApproveRequestCommandDto>
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IScheduleDictionarySaver _scheduleDictionarySaver;
        private readonly ICurrentScenario _scenarioRepository;
        private readonly IPersonRequestCheckAuthorization _authorization;
        private readonly ISwapAndModifyService _swapAndModifyService;
        private readonly IPersonRequestRepository _personRequestRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
    	private readonly IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
        private readonly IScheduleDictionaryModifiedCallback _scheduleDictionaryModifiedCallback;

		  public ApproveRequestCommandHandler(IScheduleRepository scheduleRepository, IScheduleDictionarySaver scheduleDictionarySaver, ICurrentScenario scenarioRepository, IPersonRequestCheckAuthorization authorization, ISwapAndModifyService swapAndModifyService, IPersonRequestRepository personRequestRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, IMessageBrokerEnablerFactory messageBrokerEnablerFactory, IScheduleDictionaryModifiedCallback scheduleDictionaryModifiedCallback)
        {
            _scheduleRepository = scheduleRepository;
            _scheduleDictionarySaver = scheduleDictionarySaver;
            _scenarioRepository = scenarioRepository;
            _authorization = authorization;
            _swapAndModifyService = swapAndModifyService;
            _personRequestRepository = personRequestRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        	_messageBrokerEnablerFactory = messageBrokerEnablerFactory;
    	    _scheduleDictionaryModifiedCallback = scheduleDictionaryModifiedCallback;
        }

        public virtual IRequestApprovalService GetRequestApprovalServiceScheduler(IScheduleDictionary scheduleDictionary,
                                                    IScenario scenario,
                                                    ISwapAndModifyService swapAndModifyService,
                                                    INewBusinessRuleCollection newBusinessRules)
        {
            

           return new RequestApprovalServiceScheduler(scheduleDictionary,
                                                                          scenario,
                                                                          swapAndModifyService, newBusinessRules,
                                                                          new ResourceCalculationOnlyScheduleDayChangeCallback());
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ApproveRequestCommandDto command)
        {
            using (var uow = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
            {
                var personRequest = _personRequestRepository.Get(command.PersonRequestId);

                var allNewRules = NewBusinessRuleCollection.Minimum();
                var scheduleDictionary = getSchedules(personRequest);

                var approvalService = GetRequestApprovalServiceScheduler(scheduleDictionary,
                                                                          _scenarioRepository.Current(),
                                                                          _swapAndModifyService, allNewRules);
                try
                {
                    personRequest.Approve(approvalService, _authorization);
                }
                catch (InvalidRequestStateTransitionException e)
                {
                    throw new FaultException(e.Message);
                }
                var result = _scheduleDictionarySaver.MarkForPersist(uow, _scheduleRepository,
                                                        scheduleDictionary.DifferenceSinceSnapshot());

                //new ScheduleDictionaryModifiedCallback().Callback(scheduleDictionary, result.ModifiedEntities, result.AddedEntities, result.DeletedEntities);
                _scheduleDictionaryModifiedCallback.Callback(scheduleDictionary, result.ModifiedEntities, result.AddedEntities, result.DeletedEntities);

                using (_messageBrokerEnablerFactory.NewMessageBrokerEnabler())
                {
                    uow.PersistAll();
                }
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
            var scheduleDictionary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(
                new PersonProvider(personList),
				new ScheduleDictionaryLoadOptions(true, false), 
				dateonlyPeriod,
                _scenarioRepository.Current());
            ((ReadOnlyScheduleDictionary)scheduleDictionary).MakeEditable();
            return scheduleDictionary;
        }
    }
}
