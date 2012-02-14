namespace Teleopti.Ccc.PayrollFormatter
{
    public static class PayrollFormatterFactory
    {
        public static PayrollFormatterBase CreatePayrollFormatter(DocumentFormat documentFormat)
        {
            switch (documentFormat.DocumentFormatType)
            {
                case DocumentFormatType.Csv:
                case DocumentFormatType.FixedWidth:
                    return new FlatFileFormatter();
                case DocumentFormatType.Excel:
                    return new ExcelFormatter();
                default:
                    return new XmlFormatter();
            }
        }
    }
}