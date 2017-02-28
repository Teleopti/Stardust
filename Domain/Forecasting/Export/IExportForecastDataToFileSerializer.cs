using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public interface IExportForecastDataToFileSerializer
    {
        IEnumerable<string> SerializeForecastData(ISkill skill, ExportSkillToFileCommandModel model,
                                               IEnumerable<ISkillDay> skillDays);
    }
}

