﻿using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.ExportPages
{
    public interface IExportForecastToFileSettings: ISettingValue
    {
        DateOnlyPeriod Period { get; set; }
    }
}
