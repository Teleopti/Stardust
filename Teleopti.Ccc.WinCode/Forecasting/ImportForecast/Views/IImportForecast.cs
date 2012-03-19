using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views
{
    public interface IImportForecast
    {
        ISkill Skill { get;}
        bool IsWorkloadImport { get; set; }
        bool IsStaffingImport { get; set; }
        bool IsStaffingAndWorkloadImport { get; set; }
    }
}
