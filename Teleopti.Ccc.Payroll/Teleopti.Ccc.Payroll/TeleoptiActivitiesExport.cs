using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using log4net;

namespace Teleopti.Ccc.Payroll
{
    public class TeleoptiActivitiesExport : IPayrollExportProcessorWithFeedback
    {
        private static readonly PayrollFormatDto Format = new PayrollFormatDto(new Guid("{0e531434-a463-4ab6-8bf1-4696ddc9b296}"), "Teleopti Activities Export");
        public PayrollFormatDto PayrollFormat
        {
            get { return Format; }
        }

        public IXPathNavigable ProcessPayrollData(ITeleoptiSchedulingService schedulingService,
                                                  ITeleoptiOrganizationService organizationService, PayrollExportDto payrollExport)
        {
            return null;
        }

        public IPayrollExportFeedback PayrollExportFeedback { get; set; }

        
    }
}
