using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public class SkillExportSelection
    {
        private readonly IEnumerable<MultisiteSkillForExport> _multisiteSkillForExports;

        public SkillExportSelection(IEnumerable<MultisiteSkillForExport> multisiteSkillForExports)
        {
            _multisiteSkillForExports = multisiteSkillForExports;
        }

        public IEnumerable<MultisiteSkillForExport> MultisiteSkillsForExport => _multisiteSkillForExports;

	    public DateOnlyPeriod Period { get; set; }
    }
}