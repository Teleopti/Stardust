using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Requests
{
    public interface IPersonRequestViewModelFilter:ISpecification<PersonRequestViewModel>
    {
        ISpecification<IChangeInfo> InnerSpecification { get; }

        DateTime FilterDateTime { get; }
    }
}