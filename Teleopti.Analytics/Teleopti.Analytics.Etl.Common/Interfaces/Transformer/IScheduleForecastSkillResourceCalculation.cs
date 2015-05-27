using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
    public interface IScheduleForecastSkillResourceCalculation
    {
        Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> GetResourceDataExcludingShrinkage(DateTime insertDateTime);

        Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> GetResourceDataIncludingShrinkage(DateTime insertDateTime);
    }
}