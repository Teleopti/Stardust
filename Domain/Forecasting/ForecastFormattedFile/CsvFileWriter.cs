using System.IO;
using System.Text;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastFormattedFile
{
    public class CsvFileWriter
    {
        private readonly TextWriter _writer;

        public CsvFileWriter(TextWriter writer)
        {
            _writer = writer;
        }

        public void WriteRow(IFileRow row)
        {
            var builder = new StringBuilder();
            var firstColumn = true;
            foreach (var value in row)
            {
                if (!firstColumn)
                    builder.Append(',');
                builder.Append(value);
                firstColumn = false;
            }
            row.LineText = builder.ToString();
            _writer.WriteLine(row.LineText);
        }
    }
}