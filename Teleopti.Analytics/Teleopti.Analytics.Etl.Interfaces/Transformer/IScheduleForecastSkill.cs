﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
    public interface IScheduleForecastSkill : IScheduleForecastSkillKey
    {
        new DateTime StartDateTime { get; }
        new int IntervalId { get; }
        new Guid SkillCode { get; }
        new Guid ScenarioCode { get; }
        double ForecastedResourcesMinutes { get; set; }
        double ForecastedResources { get; set; }
        double ForecastedResourcesIncludingShrinkageMinutes { get; set; }
        double ForecastedResourcesIncludingShrinkage { get; set; }
        double ScheduledResourcesMinutes { get; set; }
        double ScheduledResources { get; set; }
        double ScheduledResourcesIncludingShrinkageMinutes { get; set; }
        double ScheduledResourcesIncludingShrinkage { get; set; }
        Guid BusinessUnitCode { get; set; }
        string BusinessUnitName { get; set; }
        int DataSourceId { get; set; }
        DateTime InsertDate { get; set; }
        DateTime UpdateDate { get; set; }
    }
}
