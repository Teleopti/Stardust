using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public interface IForecastFileContainer
    {
        void AddForecastsRow(DateOnly dateOnly, IForecastsRow forecasts);
        ICollection<IForecastsRow> GetForecastsRows(DateOnly dateOnly);
    }

    public class ForecastFileContainer : IForecastFileContainer
    {
        private readonly IDictionary<DateOnly, ICollection<IForecastsRow>> _forecastedFileDictionary =
            new Dictionary<DateOnly, ICollection<IForecastsRow>>();

        public void AddForecastsRow(DateOnly dateOnly, IForecastsRow forecasts)
        {
            ICollection<IForecastsRow> forecastsRows;
            if (_forecastedFileDictionary.TryGetValue(dateOnly, out forecastsRows))
            {
                if (forecastsRows.Contains(forecasts))
                    return;
                forecastsRows.Add(forecasts);
            }
            else
                _forecastedFileDictionary.Add(dateOnly, new List<IForecastsRow> { forecasts });
        }

        public ICollection<IForecastsRow> GetForecastsRows(DateOnly dateOnly)
        {
            ICollection<IForecastsRow> forecastsRows;
            _forecastedFileDictionary.TryGetValue(dateOnly, out forecastsRows);
            return forecastsRows;
        }
    }
}