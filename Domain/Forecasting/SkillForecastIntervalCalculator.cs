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
		//private readonly ILoadSkillDaysWithPeriodFlexibility _loadSkillDaysWithPeriodFlexibility;
		private readonly ICurrentScenario _currentScenario;
		private readonly ISkillForecastReadModelRepository _skillForecastReadModelRepository;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly SkillForecastReadModelPeriodBuilder _skillForecastReadModelPeriodBuilder;

		public SkillForecastIntervalCalculator(ISkillForecastReadModelRepository skillForecastReadModelRepository,
			IIntervalLengthFetcher intervalLengthFetcher, ISkillDayRepository skillDayRepository, ICurrentScenario currentScenario, SkillForecastReadModelPeriodBuilder skillForecastReadModelPeriodBuilder)
		{
			//_loadSkillDaysWithPeriodFlexibility = loadSkillDaysWithPeriodFlexibility;
			_skillForecastReadModelRepository = skillForecastReadModelRepository;
			_intervalLengthFetcher = intervalLengthFetcher;
			_skillDayRepository = skillDayRepository;
			_currentScenario = currentScenario;
			_skillForecastReadModelPeriodBuilder = skillForecastReadModelPeriodBuilder;
		}

		////remove this method later on
		//public void Calculate(List<ISkill> skills, DateOnlyPeriod dtp)
		//{
		//	//no child skill should be filterd out
		//	//call this CalculateForecastedAgentsForEmailSkills

		//	var skillDaysBySkills =
		//		_loadSkillDaysWithPeriodFlexibility.Load(dtp, skills, _scenarioRepository.LoadDefaultScenario());
		//	var skillDays = skillDaysBySkills.SelectMany(x => x.Value);
		//	Calculate(skillDays);
		//}

		//this method will be the one later on we will remove the other method TDD

		public void Calculate(IEnumerable<Guid> skillDayIds)
		{
			var justSkillDays = filterSkillDays(skillDayIds);
			var skills = justSkillDays.Select(x => x.Skill);
			var period = new DateOnlyPeriod(justSkillDays.Min(x => x.CurrentDate),justSkillDays.Max(x => x.CurrentDate));
			var skillDays = _skillDayRepository.FindReadOnlyRange(period, skills, _currentScenario.Current());

			var periods = skillDays
				.SelectMany(x =>
					x.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(_intervalLengthFetcher.GetIntervalLength()), false)
						.Select(i => new { SkillDay = x, StaffPeriod = i }));
			var periodsWithShrinkage = skillDays
				.SelectMany(x =>
					x.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(_intervalLengthFetcher.GetIntervalLength()), true)
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
					IsBackOffice = SkillTypesWithBacklog.IsBacklogSkillType(x.SkillDay.Skill)

				});

			});

			_skillForecastReadModelRepository.PersistSkillForecast(result);
		}

		private IEnumerable<ISkillDay> filterSkillDays(IEnumerable<Guid> skillDayIds)
		{
			var skillDays = _skillDayRepository.LoadSkillDays(skillDayIds);
			var validPeriod = _skillForecastReadModelPeriodBuilder.Build();
			return skillDays.Where(x => x.CurrentDate.Date >= validPeriod.StartDateTime && x.CurrentDate.Date <= validPeriod.EndDateTime);
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

		public DateTimePeriod Build()
		{
			var startDate = _now.UtcDateTime().Date.AddDays(-_skillForecastSettingsReader.NumberOfDaysInPast);
			var endDate = _now.UtcDateTime().Date.AddDays(_staffingSettingsReader.GetIntSetting("StaffingReadModelNumberOfDays", 49));
			endDate = endDate.AddDays(_skillForecastSettingsReader.NumberOfExtraDaysInFuture);
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