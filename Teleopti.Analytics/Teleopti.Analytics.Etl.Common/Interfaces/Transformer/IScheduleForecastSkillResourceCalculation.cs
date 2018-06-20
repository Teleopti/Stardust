using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
    public interface IScheduleForecastSkillResourceCalculation
    {
        Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> GetResourceDataExcludingShrinkage(DateTime insertDateTime, ISchedulingResultStateHolder schedulingResultStateHolder);

        Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> GetResourceDataIncludingShrinkage(DateTime insertDateTime, ISchedulingResultStateHolder schedulingResultStateHolder);
    }
}