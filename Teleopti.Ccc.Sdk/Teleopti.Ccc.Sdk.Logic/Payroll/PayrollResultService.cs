using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.PayrollFormatter;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.Payroll
{
    public class PayrollResultService
    {
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPayrollResultRepository _payrollResultRepository;

        public PayrollResultService(ICurrentUnitOfWorkFactory unitOfWorkFactory, IPayrollResultRepository payrollResultRepository)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _payrollResultRepository = payrollResultRepository;
        }

        public byte[] CreatePayrollResultFileNameById(Guid id)
        {
            XmlDocument navigable;
            IPayrollResult result;

            using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                result = _payrollResultRepository.Load(id);
                navigable = new XmlDocument {InnerXml = result.XmlResult.XPathNavigable.CreateNavigator().OuterXml};
            }

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
