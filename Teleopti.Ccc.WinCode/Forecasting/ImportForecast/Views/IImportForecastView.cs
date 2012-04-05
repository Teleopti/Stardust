using System;

namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views
{
    public interface IImportForecastView
    {
        void SetWorkloadName(string name);
        void SetSkillName(string name);
        void ShowValidationException(string message);
        void EnableImport();
        void ShowError(string errorMessage);
        void ShowStatusDialog(Guid jobId);
    }
}
