using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "PlugIn")]
    public interface IPlugInLoader
    {
        IList<IPayrollExportProcessor> Load();
    	IList<PayrollFormatDto> LoadDtos();
    }
}