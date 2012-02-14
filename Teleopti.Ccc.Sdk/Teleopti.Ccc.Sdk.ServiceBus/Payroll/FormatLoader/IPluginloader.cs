using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
    public interface IPlugInLoader
    {
        IList<IPayrollExportProcessor> Load();
    }
}