using System.Collections.Generic;
using System.Xml.XPath;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public interface IPayrollDataExtractor
    {
        IXPathNavigable Extract(IPayrollExport payrollExport, RunPayrollExport message, IEnumerable<PersonDto> personDtos, IServiceBusPayrollExportFeedback serviceBusPayrollExportFeedback);
    }
}