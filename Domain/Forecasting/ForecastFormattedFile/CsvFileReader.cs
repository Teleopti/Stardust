using System;
using System.IO;
using System.Linq;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastFormattedFile
{
    public class CsvFileReader
    {
        private readonly TextReader _reader;
        private const char separator = ',';

        public CsvFileReader(TextReader reader)
        {
            _reader = reader;
        }

        public bool ReadNextRow(IFileRow row)
        {
            var line = _reader.ReadLine();
            if (String.IsNullOrEmpty(line))
                return false;
            var fields = line.Split(separator);
            for (var i = 0; i < fields.Count(); i++)
            {
                row.Add(fields[i]);
            }
            return row.Count > 0;
        }
    }
}