using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.ExportPages
{
    public interface IExportForecastToFileSettingsProvider
    {
        IExportForecastToFileSettings ExportForecastToFileSettings { get; }
        void Save();
        void TransformToSerializableModel(DateOnlyPeriod period);
    }
}
