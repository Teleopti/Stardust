using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public class ExportSkillToFileCommandModel
    {
        public TypeOfExport ExportType { get; set; } 
        public string FileName { get; set; }
        public ISkill Skill { get; set; }
        public DateOnlyPeriod Period { get; set; }
        public IScenario Scenario { get; set; }
    }

    public enum TypeOfExport
    {
        Agents,
        Calls,
        AgentsAndCalls
    }
}