using System;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public interface IServiceBusPayrollExportFeedback : IPayrollExportFeedback, IDisposable
    {
        void SetPayrollResult(IPayrollResult payrollResult);
    }
}