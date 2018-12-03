using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class ExportBpoFile : IExportBpoFile
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IForecastsRowExtractor _forecastsRowExtractor;
		private readonly ISkillGroupRepository _skillGroupRepository;
		private readonly ISkillRepository _skillRepository;

		public ExportBpoFile(ISkillDayRepository skillDayRepository, ICurrentScenario currentScenario, 
			IScheduledStaffingProvider scheduledStaffingProvider, IUserTimeZone userTimeZone, IForecastsRowExtractor forecastsRowExtractor, ISkillGroupRepository skillGroupRepository, ISkillRepository skillRepository)
		{
			_skillDayRepository = skillDayRepository;
			_currentScenario = currentScenario;
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_userTimeZone = userTimeZone;
			_forecastsRowExtractor = forecastsRowExtractor;
			_skillGroupRepository = skillGroupRepository;
			_skillRepository = skillRepository;
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
			
			forecastedData.AppendLine(_forecastsRowExtractor.HeaderRow);
			
			foreach (var skillStaffPeriod in skillStaffPeriods.Values)
			{
				var ssiStartDate = skillStaffPeriod.Period.StartDateTime;
				var ssiEndDate = skillStaffPeriod.Period.EndDateTime;
				var staffingInterval =
					allIntervals.Where(
						x => x.StartDateTime == ssiStartDate && x.EndDateTime == ssiEndDate && x.SkillId == skill.Id.GetValueOrDefault());
				var staffing = 0d;
				if (staffingInterval.Any())
				{
					staffing = staffingInterval.First().StaffingLevel;
				}

				var startDateTime = skillStaffPeriod.Period.StartDateTimeLocal(_userTimeZone.TimeZone()).ToString(dateTimeFormat, formatProvider);
				var endDateTime = skillStaffPeriod.Period.EndDateTimeLocal(_userTimeZone.TimeZone()).ToString(dateTimeFormat, formatProvider);
				var newDemand = skillStaffPeriod.FStaff - staffing;
				if (newDemand < 0) newDemand = 0;
				newDemand = Math.Round(newDemand, 2);
				var row = $"{skill.Name}{seperator}{startDateTime}{seperator}{endDateTime}{seperator}" +
						  $"0{seperator}0{seperator}0{seperator}{newDemand.ToString(formatProvider)}";
				forecastedData.AppendLine(row);
			}
			return forecastedData.ToString().Trim();
		}

		public string ExportDemand(Guid skillGroupId, DateOnlyPeriod period, IFormatProvider formatProvider,
			string seperator = ",", string dateTimeFormat = "yyyyMMdd HH:mm")
		{
			var skrinkage = false;
			var skillGroup = _skillGroupRepository.Get(skillGroupId);
			var forecastedData = new StringBuilder();
			var allIntervals = new List<SkillStaffingInterval>();
			var skills = _skillRepository.LoadSkills(skillGroup.Skills.Select(x => x.Id));
			var allSkillStaffPeriod = new List<Tuple<ISkill, ISkillStaffPeriodView>>();
			var minResolution = skills.Min(x => x.DefaultResolution);
			foreach (var skill in skills)
			{
				var skillDays = _skillDayRepository.FindRange(period, skill, _currentScenario.Current());
				foreach (var skillDay in skillDays)
				{
					var skillStaffViewIntervals =
						skillDay.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minResolution), skrinkage);
					allSkillStaffPeriod.AddRange(skillStaffViewIntervals.Select(x=> new Tuple<ISkill,ISkillStaffPeriodView>( skill, new  SkillStaffPeriodView(){Period = x.Period,FStaff = x.FStaff})));
				}
			}

			if (!allSkillStaffPeriod.Any())
				return forecastedData.ToString().Trim();
			allIntervals.AddRange(_scheduledStaffingProvider.StaffingPerSkill(skills.ToList(),
				period.Inflate(1).ToDateTimePeriod(TimeZoneInfo.Utc), false, false));

			forecastedData.AppendLine(_forecastsRowExtractor.HeaderRow);

			var skillGroupName = getSkillGroupName(skills);
			var uniquePeriods = allSkillStaffPeriod.Select(x => x.Item2.Period).Distinct().OrderBy(x => x.StartDateTime);

			foreach (var interval in uniquePeriods)
			{
				var staffingOnInterval =
					allIntervals.Where(
						//x => x.StartDateTime == interval.StartDateTime && x.EndDateTime == interval.EndDateTime && x.SkillId == skill.Id.GetValueOrDefault());
						x => x.StartDateTime == interval.StartDateTime && x.EndDateTime == interval.EndDateTime);
				var staffing = 0d;
				if (staffingOnInterval.Any())
				{
					staffing = staffingOnInterval.Sum(x => x.StaffingLevel);
				}

				var startDateTime = interval.StartDateTimeLocal(_userTimeZone.TimeZone()).ToString(dateTimeFormat, formatProvider);
				var endDateTime = interval.EndDateTimeLocal(_userTimeZone.TimeZone()).ToString(dateTimeFormat, formatProvider);
				var staffPeriodsOnInterval = allSkillStaffPeriod.Where(x => x.Item2.Period == interval);

				var collectiveFStaff = staffPeriodsOnInterval.Sum(x => x.Item2.FStaff);
				var newDemand = collectiveFStaff - staffing;
				if (newDemand < 0) newDemand = 0;
				newDemand = Math.Round(newDemand, 2);
				var row =
					$"{skillGroupName}{seperator}{startDateTime}{seperator}{endDateTime}{seperator}" +
					$"0{seperator}0{seperator}0{seperator}{newDemand.ToString(formatProvider)}";
				forecastedData.AppendLine(row);
			}

			return forecastedData.ToString().Trim();
		}

		private string getSkillGroupName(IEnumerable<ISkill> skills)
		{
			var nameList = skills.Select(x => x.Name).OrderBy(x => x).Distinct();
			return string.Join("|", nameList);
		}
		
	}
	
}
