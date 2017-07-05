using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public interface IServiceBusPayrollExportFeedback : IPayrollExportFeedback, IDisposable
    {
        void SetPayrollResult(IPayrollResult payrollResult);
    }
}