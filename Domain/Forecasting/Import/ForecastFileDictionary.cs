using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public interface IForecastFileContainer
    {
        void AddForecastsRow(DateOnly dateOnly, ForecastsRow forecasts);
        ICollection<ForecastsRow> GetForecastsRows(DateOnly dateOnly);
    }

    public class ForecastFileContainer : IForecastFileContainer
    {
        private readonly IDictionary<DateOnly, ICollection<ForecastsRow>> _forecastedFileDictionary =
            new Dictionary<DateOnly, ICollection<ForecastsRow>>();

        public void AddForecastsRow(DateOnly dateOnly, ForecastsRow forecasts)
        {
            ICollection<ForecastsRow> forecastsRows;
            if (_forecastedFileDictionary.TryGetValue(dateOnly, out forecastsRows))
            {
                if (forecastsRows.Contains(forecasts))
                    return;
                forecastsRows.Add(forecasts);
            }
            else
                _forecastedFileDictionary.Add(dateOnly, new List<ForecastsRow> { forecasts });
        }

        public ICollection<ForecastsRow> GetForecastsRows(DateOnly dateOnly)
        {
            ICollection<ForecastsRow> forecastsRows;
            _forecastedFileDictionary.TryGetValue(dateOnly, out forecastsRows);
            return forecastsRows;
        }
    }
}