﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class ExportBpoFile
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly ScheduledStaffingProvider _scheduledStaffingProvider;


		public ExportBpoFile(ISkillDayRepository skillDayRepository, ICurrentScenario currentScenario, ScheduledStaffingProvider scheduledStaffingProvider)
		{
			_skillDayRepository = skillDayRepository;
			_currentScenario = currentScenario;
			_scheduledStaffingProvider = scheduledStaffingProvider;
		}

		public string ForecastData(ISkill skill, DateOnlyPeriod period, IFormatProvider importFormatProvider, string seperator=",", string dateTimeFormat = "yyyyMMdd HH:mm")
		{
			var skillDays = _skillDayRepository.FindRange(period,skill, _currentScenario.Current());
			var loadSkillSchedule = new Dictionary<ISkill, IEnumerable<ISkillDay>> { { skill, skillDays.ToList() } };
			var skillStaffPeriodHolder = new SkillStaffPeriodHolder(loadSkillSchedule);
			ISkillStaffPeriodDictionary skillStaffPeriods;
			var forecastedData = new StringBuilder();
			var allIntervals = new List<SkillStaffingIntervalLightModel>();
			foreach (var dateOnly in period.DayCollection())
			{
				allIntervals.AddRange(_scheduledStaffingProvider.StaffingPerSkill(new List<ISkill>(){skill},skill.DefaultResolution,dateOnly ));
			}
			
			if (skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skill, out skillStaffPeriods))
			{
				foreach (var skillStaffPeriod in skillStaffPeriods.Values)
				{
					//var startDateTime = skillStaffPeriod.Period.StartDateTimeLocal(skill.TimeZone)
					//	.ToString(dateTimeFormat, CultureInfo.InvariantCulture);
					//var endDateTime = skillStaffPeriod.Period.StartDateTimeLocal(skill.TimeZone)
					//	.ToString(dateTimeFormat, CultureInfo.InvariantCulture);
					var ssiStartDate = skillStaffPeriod.Period.StartDateTime;
					var ssiEndDate = skillStaffPeriod.Period.EndDateTime;
					var staffingInterval =
						allIntervals.Where(
							x => x.StartDateTime == ssiStartDate && x.EndDateTime == ssiEndDate && x.Id == skill.Id.GetValueOrDefault());
					var staffing = 0d;
					if (staffingInterval.Any())
					{
						staffing = staffingInterval.First().StaffingLevel;
					}

					var startDateTime = ssiStartDate.ToString(dateTimeFormat, importFormatProvider);
					var endDateTime = ssiEndDate.ToString(dateTimeFormat, importFormatProvider);
					var newDemand = skillStaffPeriod.FStaff - staffing;
					var row = $"{skill.Name}{seperator}{startDateTime}{seperator}{endDateTime}{seperator}" +
							  $"0{seperator}0{seperator}0{seperator}{newDemand}";
					forecastedData.AppendLine(row);
				}
			}
			return forecastedData.ToString().Trim();
		}

		
	}
}
