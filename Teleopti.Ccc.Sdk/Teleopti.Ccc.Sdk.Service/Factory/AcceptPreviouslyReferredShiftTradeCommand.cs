using System;
using System.Globalization;
using System.ServiceModel;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
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
                    IScenario scenario = _repositoryFactory.CreateScenarioRepository(uow).LoadDefaultScenario();
                    ShiftTradeRequestSetChecksum shiftTradeRequestSetChecksum =
                        new ShiftTradeRequestSetChecksum(scenario, _repositoryFactory.CreateScheduleRepository(uow));

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
                var identity = (TeleoptiIdentity)TeleoptiPrincipal.Current.Identity;
                _serviceBusSender.NotifyServiceBus(new NewShiftTradeRequestCreated
                                                       {
                                                           BusinessUnitId = identity.BusinessUnit.Id.GetValueOrDefault(Guid.Empty),
                                                           Datasource = identity.DataSource.Application.Name,
                                                           Timestamp = DateTime.UtcNow,
                                                           PersonRequestId =
                                                               _personRequestDto.Id.GetValueOrDefault(Guid.Empty)
                                                       });
            }
        }
    }
}