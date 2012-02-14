using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public class SkillExportSelection : ISkillExportSelection
    {
        private readonly IEnumerable<IMultisiteSkillForExport> _multisiteSkillForExports;

        public SkillExportSelection(IEnumerable<IMultisiteSkillForExport> multisiteSkillForExports)
        {
            _multisiteSkillForExports = multisiteSkillForExports;
        }

        public IEnumerable<IMultisiteSkillForExport> MultisiteSkillsForExport
        {
            get { return _multisiteSkillForExports; }
        }

        public DateOnlyPeriod Period { get; set; }

        public int Incremental { get; set; }
    }
}