using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class SavePersonAbsenceRequestCommandHandler : IHandleCommand<SavePersonAbsenceRequestCommandDto>
    {
        private readonly IPersistPersonRequest _persistPersonRequest;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPersonRequestRepository _personRequestRepository;
        private readonly IServiceBusEventPublisher _serviceBusSender;


        public SavePersonAbsenceRequestCommandHandler(IPersistPersonRequest persistPersonRequest, ICurrentUnitOfWorkFactory unitOfWorkFactory, IPersonRequestRepository personRequestRepository, IServiceBusEventPublisher serviceBusSender)
        {
            _persistPersonRequest = persistPersonRequest;
            _unitOfWorkFactory = unitOfWorkFactory;
            _personRequestRepository = personRequestRepository;
            _serviceBusSender = serviceBusSender;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(SavePersonAbsenceRequestCommandDto command)
        {
            IPersonRequest result;
            using (var unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
            {
                if (!(command.PersonRequestDto.Request is AbsenceRequestDto))
                {
                    // Do not allow other request than absence to be saved
                    throw new FaultException("Invalid request type. Only Absence requests are valid to save in this method.");
                }

                Action<IPersonRequest> requestCallback = setPendingPersonRequestToNew;
                bool enabled = _serviceBusSender.EnsureBus();
                if (!enabled)
                {
                    requestCallback = setNewPersonRequestToPending;
                }
                result = _persistPersonRequest.Persist(command.PersonRequestDto, unitOfWork, requestCallback);
                if (enabled)
                {
                    //Call RSB!
                    var message = new NewAbsenceRequestCreated
		                {
			                PersonRequestId = result.Id.GetValueOrDefault(Guid.Empty)
		                };
                    _serviceBusSender.Publish(message);
                }
            }
			command.Result = new CommandResultDto { AffectedId = result.Id, AffectedItems = 1 };
        }
        
        private void addNewRequest(IPersonRequest personRequest)
        {
            if (!personRequest.Id.HasValue)
            {
                _personRequestRepository.Add(personRequest);
            }
        }

        private void setNewPersonRequestToPending(IPersonRequest personRequest)
        {
            if (!personRequest.Id.HasValue && personRequest.IsNew)
            {
                personRequest.Pending();
            }
            addNewRequest(personRequest);
        }

        private void setPendingPersonRequestToNew(IPersonRequest personRequest)
        {
            addNewRequest(personRequest);
            if (personRequest.IsPending)
            {
                personRequest.SetNew();
            }
        }
    }
}
