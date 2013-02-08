using Teleopti.Interfaces.Messages.Payroll;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public interface ILogOnSdkForPayrollExport
    {
        void LogOn(RunPayrollExport message);
    }
}