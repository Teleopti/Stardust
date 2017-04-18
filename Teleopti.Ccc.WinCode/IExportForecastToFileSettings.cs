using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages
{
    public interface IExportForecastToFileSettings: ISettingValue
    {
        DateOnlyPeriod Period { get; set; }
    }
}
