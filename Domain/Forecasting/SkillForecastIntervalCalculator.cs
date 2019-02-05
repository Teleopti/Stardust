using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer;
using Teleopti.Ccc.Domain.Intraday.To_Staffing;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class SkillForecastIntervalCalculator
	{
		private readonly ICurrentScenario _currentScenario;
		private readonly ISkillForecastReadModelRepository _skillForecastReadModelRepository;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly SkillForecastReadModelPeriodBuilder _skillForecastReadModelPeriodBuilder;

		public SkillForecastIntervalCalculator(ISkillForecastReadModelRepository skillForecastReadModelRepository,
			IIntervalLengthFetcher intervalLengthFetcher, ISkillDayRepository skillDayRepository, ICurrentScenario currentScenario, SkillForecastReadModelPeriodBuilder skillForecastReadModelPeriodBuilder)
		{
			_skillForecastReadModelRepository = skillForecastReadModelRepository;
			_intervalLengthFetcher = intervalLengthFetcher;
			_skillDayRepository = skillDayRepository;
			_currentScenario = currentScenario;
			_skillForecastReadModelPeriodBuilder = skillForecastReadModelPeriodBuilder;
		}

		public void Calculate(IEnumerable<ISkillDay> skillDays)
		{

			//var periods = skillDays
			//	.SelectMany(x =>
			//		x.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(_intervalLengthFetcher.GetIntervalLength()), false)
			//			.Select(i => new { SkillDay = x, StaffPeriod = i }));
			//var periodsWithShrinkage = skillDays
			//	.SelectMany(x =>
			//		x.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(_intervalLengthFetcher.GetIntervalLength()), true)
			//			.Select(i => new { SkillDay = x, StaffPeriod = i }));

			var periods = skillDays
				.SelectMany(x =>
					x.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(x.Skill.DefaultResolution), false)
						.Select(i => new { SkillDay = x, StaffPeriod = i }));
			var periodsWithShrinkage = skillDays
				.SelectMany(x =>
					x.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(x.Skill.DefaultResolution), true)
						.Select(i => new { SkillDay = x, StaffPeriod = i }));

			var agentsWithShrinkage = periodsWithShrinkage.ToDictionary(
				x => new { SkillId = x.SkillDay.Skill.Id.GetValueOrDefault(), StartDateTime = x.StaffPeriod.Period.StartDateTime },
				y => y.StaffPeriod.FStaff);

			var result = new List<SkillForecast>();
			periods.ForEach(x =>
			{
				var skillId = x.SkillDay.Skill.Id.GetValueOrDefault();
				var startDateTime = x.StaffPeriod.Period.StartDateTime;
				var item = new { SkillId = skillId, StartDateTime = startDateTime };
				result.Add(new SkillForecast
				{
					SkillId = skillId,
					StartDateTime = startDateTime,
					EndDateTime = x.StaffPeriod.Period.EndDateTime,
					Agents = x.StaffPeriod.FStaff,
					Calls = x.StaffPeriod.ForecastedTasks,
					AverageHandleTime = x.StaffPeriod.AverageHandlingTaskTime.TotalSeconds,
					AgentsWithShrinkage = agentsWithShrinkage.ContainsKey(item) ? agentsWithShrinkage[item] : 0,
					IsBackOffice = SkillTypesWithBacklog.IsBacklogSkillType(x.SkillDay.Skill),
					AnsweredWithinSeconds = x.StaffPeriod.AnsweredWithinSeconds,
					PercentAnswered = x.StaffPeriod.PercentAnswered.Value

				});

			});

			_skillForecastReadModelRepository.PersistSkillForecast(result);
		}
	}

	public class SkillForecastReadModelPeriodBuilder
	{
		private INow _now;
		private readonly SkillForecastSettingsReader _skillForecastSettingsReader;
		private readonly IStaffingSettingsReader _staffingSettingsReader;

		public SkillForecastReadModelPeriodBuilder(INow now, SkillForecastSettingsReader skillForecastSettingsReader, IStaffingSettingsReader staffingSettingsReader)
		{
			_now = now;
			_skillForecastSettingsReader = skillForecastSettingsReader;
			_staffingSettingsReader = staffingSettingsReader;
		}

		public DateTimePeriod BuildFullPeriod()
		{
			var startDate = _now.UtcDateTime().Date.AddDays(-_skillForecastSettingsReader.NumberOfDaysInPast);
			var endDate = _now.UtcDateTime().Date.AddDays(_staffingSettingsReader.GetIntSetting("StaffingReadModelNumberOfDays", 49));
			endDate = endDate.AddDays(_skillForecastSettingsReader.NumberOfExtraDaysInFuture);
			return new DateTimePeriod(startDate.Date,endDate.Date);
		}

		public DateTimePeriod BuildNextPeriod(DateTime lastRun)
		{
			var staffingDaysNum = _staffingSettingsReader.GetIntSetting("StaffingReadModelNumberOfDays", 49);
			var extraDaysForForecast = _skillForecastSettingsReader.NumberOfExtraDaysInFuture;
			var startDate = lastRun.AddDays(staffingDaysNum + extraDaysForForecast);
			var endDate = startDate.AddDays(extraDaysForForecast);
			return new DateTimePeriod(startDate.Date,endDate.Date);
		}
	}

	public class SkillForecastSettingsReader 
	{
		public int NumberOfDaysInPast { get; }
		public int NumberOfExtraDaysInFuture { get; }

		public SkillForecastSettingsReader()
		{
			NumberOfDaysInPast = 8;
			NumberOfExtraDaysInFuture = 9;
		}
	}
}