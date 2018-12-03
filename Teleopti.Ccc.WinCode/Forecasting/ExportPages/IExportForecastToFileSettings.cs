using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.ExportPages
{
    public interface IExportForecastToFileSettings: ISettingValue
    {
        DateOnlyPeriod Period { get; set; }
    }
}
