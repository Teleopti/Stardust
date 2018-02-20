﻿using System.ServiceModel;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class DenyRequestCommandHandler : IHandleCommand<DenyRequestCommandDto>
    {
        private readonly IPersonRequestRepository _personRequestRepository;
        private readonly IPersonRequestCheckAuthorization _authorization;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

    	public DenyRequestCommandHandler(IPersonRequestRepository personRequestRepository, IPersonRequestCheckAuthorization authorization, ICurrentUnitOfWorkFactory unitOfWorkFactory)
        {
            _personRequestRepository = personRequestRepository;
            _authorization = authorization;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(DenyRequestCommandDto command)
        {
			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                var personRequest = _personRequestRepository.Get(command.PersonRequestId);
                try
                {
                    personRequest.Deny(nameof(UserTexts.Resources.RequestDenyReasonSupervisor),
                                       _authorization);
                }
                catch (InvalidRequestStateTransitionException e)
                {
                    throw new FaultException(e.Message);
                }
                    uow.PersistAll();
            }
			command.Result = new CommandResultDto { AffectedId = command.PersonRequestId, AffectedItems = 1 };
        }
    }
}
