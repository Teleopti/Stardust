using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public interface IForecastsFileContentProvider
    {
        ICollection<IForecastsFileRow> LoadContent(byte[] fileContent, ICccTimeZoneInfo timeZone);
    }

    public class ForecastsFileContentProvider : IForecastsFileContentProvider
    {
        private readonly IForecastsRowExtractor _rowExtractor;

        public ForecastsFileContentProvider(IForecastsRowExtractor rowExtractor)
        {
            _rowExtractor = rowExtractor;
        }

        public ICollection<IForecastsFileRow> LoadContent(byte[] fileContent, ICccTimeZoneInfo timeZone)
        {
            return Encoding.UTF8.GetString(fileContent).Split(new[] {Environment.NewLine},
                                                           StringSplitOptions.RemoveEmptyEntries).Select(
                                                               r => _rowExtractor.Extract(r, timeZone)).ToList();
        }
    }
}