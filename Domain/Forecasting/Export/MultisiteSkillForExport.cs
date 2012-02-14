using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public class MultisiteSkillForExport : IMultisiteSkillForExport
    {
        private readonly IList<ISkillExportCombination> _subSkillMapping = new List<ISkillExportCombination>();

        public IMultisiteSkill MultisiteSkill { get; set; }

        public IEnumerable<ISkillExportCombination> SubSkillMapping
        {
            get { return _subSkillMapping; }
        }

        public void AddSubSkillMapping(ISkillExportCombination skillExportCombination)
        {
            _subSkillMapping.Add(skillExportCombination);
        }
    }
}