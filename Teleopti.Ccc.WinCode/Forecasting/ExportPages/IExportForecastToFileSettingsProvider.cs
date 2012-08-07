using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.ExportPages
{
    public interface IExportForecastToFileSettingsProvider
    {
        IExportForecastToFileSettings ExportForecastToFileSettings { get; }
        void Save();
        void TransformToSerilizableModel(DateOnlyPeriod period);
    }
}
