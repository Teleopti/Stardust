using System.Collections.Generic;
using System.Xml.XPath;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public interface IPayrollDataExtractor
    {
        IXPathNavigable Extract(IPayrollExport payrollExport, RunPayrollExportEvent message, IEnumerable<PersonDto> personDtos, IServiceBusPayrollExportFeedback serviceBusPayrollExportFeedback);
    }
}