using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic
{
    public interface IPersistPersonRequest
    {
        IPersonRequest Persist(PersonRequestDto personRequestDto, IUnitOfWork unitOfWork, Action<IPersonRequest> handleDomainPersonRequest);
    }
}