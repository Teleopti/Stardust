﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class ExportBpoFileOld : IExportBpoFile
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly ScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IForecastsRowExtractor _forecastsRowExtractor;

		public ExportBpoFileOld() {}

		public ExportBpoFileOld(ISkillDayRepository skillDayRepository, ICurrentScenario currentScenario, 
			ScheduledStaffingProvider scheduledStaffingProvider, IUserTimeZone userTimeZone, IForecastsRowExtractor forecastsRowExtractor)
		{
			_skillDayRepository = skillDayRepository;
			_currentScenario = currentScenario;
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_userTimeZone = userTimeZone;
			_forecastsRowExtractor = forecastsRowExtractor;
		}

		public string ExportDemand(ISkill skill, DateOnlyPeriod period, IFormatProvider formatProvider, string seperator=",", string dateTimeFormat = "yyyyMMdd HH:mm")
		{
			var skillDays = _skillDayRepository.FindRange(period,skill, _currentScenario.Current());
			var loadSkillSchedule = new Dictionary<ISkill, IEnumerable<ISkillDay>> { { skill, skillDays.ToList() } };
			var skillStaffPeriodHolder = new SkillStaffPeriodHolder(loadSkillSchedule);
			var forecastedData = new StringBuilder();
			var allIntervals = new List<SkillStaffingInterval>();
			allIntervals.AddRange(_scheduledStaffingProvider.StaffingPerSkill(new List<ISkill>{skill},period.Inflate(1).ToDateTimePeriod(TimeZoneInfo.Utc),false,false));

			if (!skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skill, out var skillStaffPeriods))
				return forecastedData.ToString().Trim();
			
			foreach (var skillStaffPeriod in skillStaffPeriods.Values)
			{
				var ssiStartDate = skillStaffPeriod.Period.StartDateTime;
				var ssiEndDate = skillStaffPeriod.Period.EndDateTime;
				var staffingInterval =
					allIntervals.Where(
						x => x.StartDateTime == ssiStartDate && x.EndDateTime == ssiEndDate && x.SkillId == skill.Id.GetValueOrDefault()).ToList();
				var staffing = 0d;
				if (staffingInterval.Any())
				{
					staffing = staffingInterval.First().StaffingLevel;
				}

				var startDateTime = skillStaffPeriod.Period.StartDateTimeLocal(_userTimeZone.TimeZone()).ToString(dateTimeFormat, formatProvider);
				var endDateTime = skillStaffPeriod.Period.EndDateTimeLocal(_userTimeZone.TimeZone()).ToString(dateTimeFormat, formatProvider);
				var newDemand = skillStaffPeriod.FStaff - staffing;
				if (newDemand < 0) newDemand = 0;
				var row = $"{skill.Name}{seperator}{startDateTime}{seperator}{endDateTime}{seperator}" +
						  $"0{seperator}0{seperator}0{seperator}{newDemand.ToString(formatProvider)}";
				forecastedData.AppendLine(row);
			}
			return forecastedData.ToString().Trim();
		}	
	}
}
