using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public interface IForecastExportSelection
    {
        IEnumerable<ISkill> ForecastSkillForExport { get;}
        DateOnlyPeriod Period { get; set; }
        string FileName { get; set; }
        IScenario Scenario { get; set; }
        string FilePath { get; set; }
        ExportForecastsMode TypeOfExport { get; set; }
    }

    /// <summary>
    /// Provide choices regarding exporting forecasts
    /// </summary>
    public enum ExportForecastsMode
    {
        /// <summary>
        /// Import workload only
        /// </summary>
        Agent,
        /// <summary>
        /// Import staffing only
        /// </summary>
        Calls,
        /// <summary>
        /// Import both workload and staffing
        /// </summary>
        AgentAndCalls
        
    }

    public interface IForcastSkillForExport
    {
        ISkill Skill { get; set; }
    }
}