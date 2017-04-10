using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
    public interface IPersonRequestFactory
    {
        ICollection<PersonRequestDto> GetAllRequestsForPerson(PersonDto person);
        ICollection<PersonRequestDto> GetAllRequestModifiedWithinPeriodOrPending(PersonDto person, DateTime utcStartDate, DateTime utcEndDate);
        ICollection<PersonRequestDto> GetPersonRequests(PersonDto person, DateTime localStartDate, DateTime localEndDate);
        void DeletePersonRequest(PersonRequestDto personRequestDto, IUnitOfWork unitOfWork);
        PersonRequestDto SavePersonRequest(PersonRequestDto personRequestDto, IUnitOfWork unitOfWork);
        PersonRequestDto CreateShiftTradeRequest(PersonDto requester, string subject, string message, ICollection<ShiftTradeSwapDetailDto> shiftTradeSwapDetailDtos);
        PersonRequestDto SetShiftTradeRequest(PersonRequestDto personRequestDto, string subject, string message, ICollection<ShiftTradeSwapDetailDto> shiftTradeSwapDetailDtos);
        PersonRequestDto CreatePersonRequest(PersonRequestDto personRequestDto);
        PersonRequestDto AcceptShiftTradeRequest(PersonRequestDto personRequestDto, IUnitOfWork unitOfWork, IPerson person);
        PersonRequestDto DenyShiftTradeRequest(PersonRequestDto personRequestDto, IUnitOfWork unitOfWork);
        PersonRequestDto UpdatePersonRequestMessage(PersonRequestDto personRequestDto, IUnitOfWork unitOfWork);
        PersonRequestDto GetPersonRequestDto(PersonRequestDto personRequestDto);
    }
}