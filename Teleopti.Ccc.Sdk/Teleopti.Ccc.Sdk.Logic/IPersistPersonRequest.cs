using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic
{
    public interface IPersistPersonRequest
    {
        IPersonRequest Persist(PersonRequestDto personRequestDto, IUnitOfWork unitOfWork, Action<IPersonRequest> handleDomainPersonRequest);
    }
}