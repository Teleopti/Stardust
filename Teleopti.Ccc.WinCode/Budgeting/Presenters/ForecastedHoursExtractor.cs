﻿using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Presenters
{
	public class ForecastedHoursExtractor
	{
		private readonly IBudgetSkillStaffPeriodContainer _skillStaffPeriodContainer;

		public ForecastedHoursExtractor(IBudgetSkillStaffPeriodContainer skillStaffPeriodContainer)
		{
			_skillStaffPeriodContainer = skillStaffPeriodContainer;
		}

		public double ForecastedHoursForPeriod(DateTimePeriod dateTimePeriod)
		{
			TimeSpan sumOfForecastedHours = TimeSpan.Zero;
			var skillStaffPeriodsInsidePeriod = _skillStaffPeriodContainer.ForPeriod(dateTimePeriod);
			foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriodsInsidePeriod)
			{
				sumOfForecastedHours = sumOfForecastedHours.Add(skillStaffPeriod.ForecastedIncomingDemand());
			}
			return sumOfForecastedHours.TotalHours;
		}
	}
}