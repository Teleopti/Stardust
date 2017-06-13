using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public class MultisiteSkillForExport
    {
        private readonly IList<SkillExportCombination> _subSkillMapping = new List<SkillExportCombination>();

        public IMultisiteSkill MultisiteSkill { get; set; }

        public IEnumerable<SkillExportCombination> SubSkillMapping => _subSkillMapping;

	    public void AddSubSkillMapping(SkillExportCombination skillExportCombination)
        {
            _subSkillMapping.Add(skillExportCombination);
        }
    }
}