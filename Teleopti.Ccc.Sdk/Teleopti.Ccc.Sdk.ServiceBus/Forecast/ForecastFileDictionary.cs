using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
    public interface IForecastFileDictionary
    {
        void Add(DateOnly date, IForecastsFileRow forecasts);
        ICollection<IForecastsFileRow> Get(DateOnly date);
    }

    public class ForecastFileDictionary : IForecastFileDictionary
    {
        private readonly IDictionary<DateOnly, ICollection<IForecastsFileRow>> _forecastedFileDictionary =
            new Dictionary<DateOnly, ICollection<IForecastsFileRow>>();

        public void Add(DateOnly date, IForecastsFileRow forecastsRow)
        {
            ICollection<IForecastsFileRow> forecastsRows;
            if (_forecastedFileDictionary.TryGetValue(date, out forecastsRows))
            {
                if (forecastsRows.Contains(forecastsRow))
                    return;
                forecastsRows.Add(forecastsRow);
            }
            else
                _forecastedFileDictionary.Add(date, new List<IForecastsFileRow> { forecastsRow });
        }

        public ICollection<IForecastsFileRow> Get(DateOnly date)
        {
            ICollection<IForecastsFileRow> forecastsRows;
            _forecastedFileDictionary.TryGetValue(date, out forecastsRows);
            return forecastsRows;
        }
    }
}