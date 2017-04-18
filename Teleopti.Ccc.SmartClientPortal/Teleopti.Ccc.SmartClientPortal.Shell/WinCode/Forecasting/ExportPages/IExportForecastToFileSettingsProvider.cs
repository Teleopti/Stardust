using Teleopti.Ccc.WinCode.Forecasting.ExportPages;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages
{
    public interface IExportForecastToFileSettingsProvider
    {
        IExportForecastToFileSettings ExportForecastToFileSettings { get; }
        void Save();
        void TransformToSerializableModel(DateOnlyPeriod period);
    }
}
