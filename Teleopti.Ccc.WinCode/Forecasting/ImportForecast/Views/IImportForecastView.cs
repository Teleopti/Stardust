using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views
{
    public interface IImportForecastView
    {
        void SetWorkloadName(string name);
        void SetSkillName(string name);
        void ShowValidationException(string message);
        void ShowError(string errorMessage);
        void ShowStatusDialog(Guid jobId);
        void SetVisibility(ISkillType skillType);
    }
}
