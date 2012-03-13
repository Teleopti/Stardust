using System;
using System.IO;
using System.Linq;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public class CsvFileReader
    {
        private readonly TextReader _reader;
        private const char Separator = ',';

        public CsvFileReader(TextReader reader)
        {
            _reader = reader;
        }

        public bool ReadNextRow(IFileRow row)
        {
            var line = _reader.ReadLine();
            row.LineText = line;
            if (String.IsNullOrEmpty(line))
                return false;
            var fields = line.Split(Separator);
            for (var i = 0; i < fields.Count(); i++)
            {
                row.Add(fields[i]);
            }
            return row.Count > 0;
        }
    }
}