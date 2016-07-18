
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
    public class ApproveRequestsWithValidatorsEventHandler:IHandleEvent<ApproveRequestsWithValidatorsEvent>,IRunOnStardust
    {
        private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
        private readonly IAbsenceRequestProcessor _absenceRequestProcessor;
        private readonly IPersonRequestRepository _personRequestRepository;
        private readonly IList<BeforeApproveAction> _beforeApproveActions;
        private readonly IWriteProtectedScheduleCommandValidator _writeProtectedScheduleCommandValidator;

        public ApproveRequestsWithValidatorsEventHandler(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IAbsenceRequestProcessor absenceRequestProcessor, IPersonRequestRepository personRequestRepository, IWriteProtectedScheduleCommandValidator writeProtectedScheduleCommandValidator)
        {
            _currentUnitOfWorkFactory = currentUnitOfWorkFactory;
            _absenceRequestProcessor = absenceRequestProcessor;
            _personRequestRepository = personRequestRepository;
            _writeProtectedScheduleCommandValidator = writeProtectedScheduleCommandValidator;
            _beforeApproveActions = new List<BeforeApproveAction>
            {
                checkPersonRequest,
                checkAbsenceRequest
            };
        }

        [AsSystem]
        public virtual void Handle(ApproveRequestsWithValidatorsEvent @event)
        {
           
            using (var unitOfWork = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                var validators = GetAbsenceRequestValidators(@event.Validator);
                foreach (var personRequestId in @event.PersonRequestIdList)
                {
                    var personRequest = _personRequestRepository.Get(personRequestId);
                    if (!_writeProtectedScheduleCommandValidator.ValidateCommand(personRequest.RequestedDate,
                        personRequest.Person, new ApproveBatchRequestsCommand()))
                    {
                        return;
                    }
                    if (_beforeApproveActions.Any(action => action.Invoke(personRequest)))
                    {
                        continue;
                    }
                    var absenceRequest = personRequest.Request as IAbsenceRequest;
                    _absenceRequestProcessor.ApproveAbsenceRequestWithValidators(personRequest, absenceRequest, unitOfWork, validators);
                  
                }
            }

        }

        private static bool checkPersonRequest(IPersonRequest personRequest)
        {
            return (personRequest == null || !personRequest.IsPending); 
        }

        private static bool checkAbsenceRequest(IPersonRequest personRequest)
        {
            return !(personRequest.Request is IAbsenceRequest);

        }

        private static IEnumerable<IAbsenceRequestValidator> GetAbsenceRequestValidators(RequestValidatorsFlag validator)
        {
            if (validator.HasFlag(RequestValidatorsFlag.BudgetAllotmentValidator))
            {
                yield return new BudgetGroupHeadCountValidator();
            }
        }

        private delegate bool BeforeApproveAction(IPersonRequest personRequest);
    }
}
