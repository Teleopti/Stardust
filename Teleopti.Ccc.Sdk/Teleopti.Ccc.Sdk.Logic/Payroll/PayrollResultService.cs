using System;
using System.IO;
using System.Xml;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.PayrollFormatter;

namespace Teleopti.Ccc.Sdk.Logic.Payroll
{
    public class PayrollResultService
    {
	    private readonly IPayrollResultRepository _payrollResultRepository;

        public PayrollResultService(IPayrollResultRepository payrollResultRepository)
        {
	        _payrollResultRepository = payrollResultRepository;
        }

        public byte[] CreatePayrollResultFileNameById(Guid id)
        {
	        var result = _payrollResultRepository.Load(id);
	        var navigable = new XmlDocument {InnerXml = result.XmlResult.XPathNavigable.CreateNavigator().OuterXml};
            
            var format = DocumentFormat.LoadFromXml(navigable);
            var formatter = PayrollFormatterFactory.CreatePayrollFormatter(format);

            var formattedData = formatter.Format(navigable, format);
            formattedData.Position = 0;

            byte[] buffer;
            using (var reader = new StreamReader(formattedData, format.Encoding))
            {
                buffer = format.Encoding.GetBytes(reader.ReadToEnd());
            }
            return buffer;
        }
    }
}
