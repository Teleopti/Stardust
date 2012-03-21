using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views
{
    public interface IImportForecast
    {
        ISkill Skill { get;}
        bool IsWorkloadImport { get; }
        bool IsStaffingImport { get; }
        bool IsStaffingAndWorkloadImport { get; }
    }
}
