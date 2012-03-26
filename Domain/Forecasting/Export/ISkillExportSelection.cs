using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public interface ISkillExportSelection
    {
        IEnumerable<IMultisiteSkillForExport> MultisiteSkillsForExport { get; }
        DateOnlyPeriod Period { get; }
    }
}