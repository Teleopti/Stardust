using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public class ForecastExportSelection: IForecastExportSelection
    {
        private readonly IEnumerable<ISkill> _SkillForecastForExports;

        public ForecastExportSelection(IEnumerable<ISkill> skillForecastForExports)
        {
            _SkillForecastForExports = skillForecastForExports;
        }
        
        public IEnumerable<ISkill> ForecastSkillForExport
        {
            get { return _SkillForecastForExports; }
        }

        public DateOnlyPeriod Period { get; set; }

        public string FileName { get; set; }

        public IScenario Scenario { get; set; }

        public string FilePath { get; set; }

        public ExportForecastsMode TypeOfExport { get; set; }
    }
}
