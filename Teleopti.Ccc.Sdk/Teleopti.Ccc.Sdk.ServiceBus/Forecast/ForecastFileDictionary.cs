using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
    public interface IForecastFileContainer
    {
        void AddForecastsRow(DateOnly dateOnly, IForecastsFileRow forecasts);
        ICollection<IForecastsFileRow> GetForecastsRows(DateOnly dateOnly);
    }

    public class ForecastFileContainer : IForecastFileContainer
    {
        private readonly IDictionary<DateOnly, ICollection<IForecastsFileRow>> _forecastedFileDictionary =
            new Dictionary<DateOnly, ICollection<IForecastsFileRow>>();

        public void AddForecastsRow(DateOnly dateOnly, IForecastsFileRow forecasts)
        {
            ICollection<IForecastsFileRow> forecastsRows;
            if (_forecastedFileDictionary.TryGetValue(dateOnly, out forecastsRows))
            {
                if (forecastsRows.Contains(forecasts))
                    return;
                forecastsRows.Add(forecasts);
            }
            else
                _forecastedFileDictionary.Add(dateOnly, new List<IForecastsFileRow> { forecasts });
        }

        public ICollection<IForecastsFileRow> GetForecastsRows(DateOnly dateOnly)
        {
            ICollection<IForecastsFileRow> forecastsRows;
            _forecastedFileDictionary.TryGetValue(dateOnly, out forecastsRows);
            return forecastsRows;
        }
    }
}