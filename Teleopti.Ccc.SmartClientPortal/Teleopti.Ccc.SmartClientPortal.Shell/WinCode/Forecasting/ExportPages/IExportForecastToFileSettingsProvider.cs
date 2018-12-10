using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Forecasting.ExportPages;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages
{
    public interface IExportForecastToFileSettingsProvider
    {
        IExportForecastToFileSettings ExportForecastToFileSettings { get; }
        void Save();
        void TransformToSerializableModel(DateOnlyPeriod period);
    }
}
