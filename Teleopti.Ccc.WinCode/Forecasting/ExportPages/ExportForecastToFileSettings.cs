using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.WinCode.Forecasting.ExportPages
{
    [Serializable]
    public class ExportForecastToFileSettings : SettingValue, IExportForecastToFileSettings
    {
        public DateOnlyPeriod Period { get; set; }
    }
}
