using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
    public interface IScheduleForecastSkillResourceCalculation
    {
        Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> GetResourceDataExcludingShrinkage(DateTime insertDateTime);

        Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> GetResourceDataIncludingShrinkage(DateTime insertDateTime);
    }
}