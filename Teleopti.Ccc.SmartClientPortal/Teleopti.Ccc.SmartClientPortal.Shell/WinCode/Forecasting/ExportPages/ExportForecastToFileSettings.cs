using System;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages
{
    [Serializable]
    public class ExportForecastToFileSettings : SettingValue, IExportForecastToFileSettings
    {
        public DateOnlyPeriod Period { get; set; }
    }
}
