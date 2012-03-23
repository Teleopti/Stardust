namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views
{
    public interface IImportForecastView
    {
        bool IsWorkloadImport { get; }
        bool IsStaffingImport { get; }
        bool IsStaffingAndWorkloadImport { get; }
    }
}
