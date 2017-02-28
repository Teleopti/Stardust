﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class SkillStaffPeriodView : ISkillStaffPeriodView
    {
        public DateTimePeriod Period { get; set; }
        public double ForecastedIncomingDemand { get; set; }
        public double CalculatedResource { get; set; }
        public double FStaff { get; set; }
    }
}
