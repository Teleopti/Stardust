using System;
using System.Globalization;
using System.ServiceModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
    public class PersistPersonRequest : IPersistPersonRequest
    {
        private readonly IAssembler<IPersonRequest, PersonRequestDto> _personRequestAssembler;

        public PersistPersonRequest(IAssembler<IPersonRequest, PersonRequestDto> personRequestAssembler)
        {
            _personRequestAssembler = personRequestAssembler;
        }

        public IPersonRequest Persist(PersonRequestDto personRequestDto, IUnitOfWork unitOfWork, Action<IPersonRequest> handleDomainPersonRequest)
        {
            IPersonRequest personRequest;


                try
                {
                    personRequest = _personRequestAssembler.DtoToDomainEntity(personRequestDto);
                }
                catch (ShiftTradeRequestStatusException exception)
                {
                    throw new FaultException(
                        new FaultReason(
                            new FaultReasonText(string.Format(CultureInfo.InvariantCulture,
                                                              "The action failed. The error was: {0}.",
                                                              exception.Message),
                                                CultureInfo.InvariantCulture)));
                }
                handleDomainPersonRequest.Invoke(personRequest);
                unitOfWork.PersistAll();

            return personRequest;
        }
    }
}
