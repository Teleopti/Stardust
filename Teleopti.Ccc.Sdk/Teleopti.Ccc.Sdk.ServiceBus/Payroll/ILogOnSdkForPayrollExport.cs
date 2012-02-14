using Teleopti.Interfaces.Messages;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public interface ILogOnSdkForPayrollExport
    {
        void LogOn(RunPayrollExport message);
    }
}