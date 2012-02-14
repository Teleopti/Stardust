﻿using System.ServiceModel;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.WcfService.Factory;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.CommandHandler
{
    public class DenyRequestCommandHandler : IHandleCommand<DenyRequestCommandDto>
    {
        private readonly IPersonRequestRepository _personRequestRepository;
        private readonly IPersonRequestCheckAuthorization _authorization;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public DenyRequestCommandHandler(IPersonRequestRepository personRequestRepository, IPersonRequestCheckAuthorization authorization, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _personRequestRepository = personRequestRepository;
            _authorization = authorization;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public CommandResultDto Handle(DenyRequestCommandDto command)
        {
            IPersonRequest personRequest;
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                personRequest = _personRequestRepository.Get(command.PersonRequestId);
                try
                {
                    personRequest.Deny(null, "RequestDenyReasonSupervisor",
                                       _authorization);
                }
                catch (InvalidRequestStateTransitionException e)
                {
                    throw new FaultException(e.Message);
                }
                using (new MessageBrokerSendEnabler())
                {
                    uow.PersistAll();
                }
            }
            return new CommandResultDto { AffectedId = command.PersonRequestId, AffectedItems = 1 };
        }
    }
}
