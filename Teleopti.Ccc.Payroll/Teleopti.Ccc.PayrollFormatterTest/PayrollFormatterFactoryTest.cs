using NUnit.Framework;
using Teleopti.Ccc.PayrollFormatter;

namespace Teleopti.Ccc.PayrollFormatterTest
{
    [TestFixture]
    public class PayrollFormatterFactoryTest
    {
        private DocumentFormat format;

        [Test]
        public void VerifyCanCreateXmlFormatter()
        {
            format = new DocumentFormat("x", DocumentFormatType.Xml);
            PayrollFormatterBase payrollFormatter = PayrollFormatterFactory.CreatePayrollFormatter(format);

            Assert.IsInstanceOf<XmlFormatter>(payrollFormatter);
        }

        [Test]
        public void VerifyCanCreateXmlFormatterDefault()
        {
            format = new DocumentFormat("x", DocumentFormatType.None);
            PayrollFormatterBase payrollFormatter = PayrollFormatterFactory.CreatePayrollFormatter(format);

            Assert.IsInstanceOf<XmlFormatter>(payrollFormatter);
        }

        [Test]
        public void VerifyCsvFormatter()
        {
            format = new DocumentFormat("x",DocumentFormatType.Csv);
            PayrollFormatterBase payrollFormatter = PayrollFormatterFactory.CreatePayrollFormatter(format);

            Assert.IsInstanceOf<FlatFileFormatter>(payrollFormatter);
        }

        [Test]
        public void VerifyFixedWidthFormatter()
        {
            format = new DocumentFormat("x", DocumentFormatType.FixedWidth);
            PayrollFormatterBase payrollFormatter = PayrollFormatterFactory.CreatePayrollFormatter(format);

            Assert.IsInstanceOf<FlatFileFormatter>(payrollFormatter);
        }

        [Test]
        public void VerifyExcelFormatter()
        {
            format = new DocumentFormat("x", DocumentFormatType.Excel);
            PayrollFormatterBase payrollFormatter = PayrollFormatterFactory.CreatePayrollFormatter(format);

            Assert.IsInstanceOf<ExcelFormatter>(payrollFormatter);
        }
    }
}
