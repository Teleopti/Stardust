using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public interface IForecastsFileContentProvider
    {
        ICollection<IForecastsRow> LoadContent(byte[] fileContent, ICccTimeZoneInfo timeZone);
    }

    public class ForecastsFileContentProvider : IForecastsFileContentProvider
    {
        private readonly IForecastsRowExtractor _rowExtractor;

        public ForecastsFileContentProvider(IForecastsRowExtractor rowExtractor)
        {
            _rowExtractor = rowExtractor;
        }

        public ICollection<IForecastsRow> LoadContent(byte[] fileContent, ICccTimeZoneInfo timeZone)
        {
            var rowNumber = 1;
            var result = new List<IForecastsRow>();
            try
            {
                foreach (var line in Encoding.UTF8.GetString(fileContent).Split(new[] { Environment.NewLine },
                                                                                StringSplitOptions.RemoveEmptyEntries))
                {
                    result.Add(_rowExtractor.Extract(line, timeZone));
                    rowNumber++;
                }
            }
            catch (ValidationException exception)
            {
                throw new ValidationException(string.Format(CultureInfo.InvariantCulture,"Line {0}, Error:{1}", rowNumber, exception.Message));
            }
            return result;
        }
    }
}