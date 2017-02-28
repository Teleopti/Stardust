using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public interface IMultisiteSkillForExport
    {
        IMultisiteSkill MultisiteSkill { get; }
        IEnumerable<ISkillExportCombination> SubSkillMapping { get; }
        void AddSubSkillMapping(ISkillExportCombination skillExportCombination);
    }
}