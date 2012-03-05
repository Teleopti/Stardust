using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views
{
    public interface IImportForecast
    {
        ISkill Skill { get; set;}
        IWorkload Workload { get; set; }
        bool IsWorkloadImport { get; set; }
        bool IsStaffingImport { get; set; }
        bool IsStaffingAndWorkloadImport { get; set; }
    }
}
