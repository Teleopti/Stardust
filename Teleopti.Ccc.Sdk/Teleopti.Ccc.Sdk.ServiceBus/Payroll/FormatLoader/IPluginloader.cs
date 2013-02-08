using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
    public interface IPlugInLoader
    {
        IList<IPayrollExportProcessor> Load();
    	IList<PayrollFormatDto> LoadDtos();
    }
}