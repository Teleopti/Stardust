using System;
using System.Globalization;
using System.ServiceModel;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    public class AcceptPreviouslyReferredShiftTradeCommand : IExecutableCommand
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IServiceBusSender _serviceBusSender;
        private readonly PersonRequestDto _personRequestDto;

        public AcceptPreviouslyReferredShiftTradeCommand(IRepositoryFactory repositoryFactory, IServiceBusSender serviceBusSender, PersonRequestDto personRequestDto)
        {
            _repositoryFactory = repositoryFactory;
            _serviceBusSender = serviceBusSender;
            _personRequestDto = personRequestDto;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void Execute()
        {
            IPersonRequest domainPersonRequest;
            using (new MessageBrokerSendEnabler())
            {
                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    IPersonRequestRepository personRequestRepository =
                        _repositoryFactory.CreatePersonRequestRepository(uow);
                    ShiftTradeRequestSetChecksum shiftTradeRequestSetChecksum =
						new ShiftTradeRequestSetChecksum(new DefaultScenarioFromRepository(_repositoryFactory.CreateScenarioRepository(uow)), _repositoryFactory.CreateScheduleRepository(uow));

                    domainPersonRequest =
                        personRequestRepository.Load(_personRequestDto.Id.GetValueOrDefault(Guid.Empty));
                    try
                    {
                        domainPersonRequest.Request.Accept(domainPersonRequest.Person, shiftTradeRequestSetChecksum, new SdkPersonRequestAuthorizationCheck());
                        domainPersonRequest.TrySetMessage(_personRequestDto.Message);
                    }
                    catch (ShiftTradeRequestStatusException exception)
                    {
                        throw new FaultException(
                            new FaultReason(
                                new FaultReasonText(string.Format(CultureInfo.InvariantCulture,
                                                                  "The accept action failed. The error was: {0}.",
                                                                  exception.Message),
                                                    CultureInfo.InvariantCulture)));
                    }
                    uow.PersistAll();
                }
            }
            if (_serviceBusSender.EnsureBus())
            {
	            var message = new NewShiftTradeRequestCreated
		            {
			            PersonRequestId =
				            _personRequestDto.Id.GetValueOrDefault(Guid.Empty)
		            };
				message.SetMessageDetail();
                _serviceBusSender.Send(message);
            }
        }
    }
}