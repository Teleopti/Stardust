using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests
{
    public interface IPersonRequestViewModelFilter:ISpecification<PersonRequestViewModel>
    {
        ISpecification<IChangeInfo> InnerSpecification { get; }

        DateTime FilterDateTime { get; }
    }
}