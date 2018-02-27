using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class ExportForecastAndStaffingFile
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly ScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private const int numberOfDecimals = 2;

		public ExportForecastAndStaffingFile(ISkillDayRepository skillDayRepository, ICurrentScenario currentScenario, 
			ScheduledStaffingProvider scheduledStaffingProvider, IUserTimeZone userTimeZone, IForecastsRowExtractor forecastsRowExtractor,
			ISkillCombinationResourceRepository skillCombinationResourceRepository, ILoggedOnUser loggedOnUser)
		{
			_skillDayRepository = skillDayRepository;
			_currentScenario = currentScenario;
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_userTimeZone = userTimeZone;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_loggedOnUser = loggedOnUser;
		}

		public string ExportDemand(ISkill skill, DateOnlyPeriod period)
		{
			var userCulture = _loggedOnUser.CurrentUser().PermissionInformation.Culture();
			var separator = userCulture.TextInfo.ListSeparator;
			
			var skillDays = _skillDayRepository.FindRange(period,skill, _currentScenario.Current());
			var loadSkillSchedule = new Dictionary<ISkill, IEnumerable<ISkillDay>> { { skill, skillDays.ToList() } };
			var skillStaffPeriodHolder = new SkillStaffPeriodHolder(loadSkillSchedule);
			var forecastedData = new StringBuilder();
			var allIntervals = new List<SkillStaffingInterval>();
			allIntervals.AddRange(_scheduledStaffingProvider.StaffingPerSkill(new List<ISkill>{skill},period.Inflate(1).ToDateTimePeriod(TimeZoneInfo.Utc),false,true));

			if (!skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skill, out var skillStaffPeriods))
				return forecastedData.ToString().Trim();

			var bpoResources = _skillCombinationResourceRepository.BpoResourcesForSkill(skill.Id.GetValueOrDefault(), period).ToList();
			var bpoNames = bpoResources.Select(r => r.Source).Distinct().ToList();
			var resourcesByBpo = bpoResources.ToLookup(r => r.Source, r => r);

			var bposString = string.Join(separator, bpoNames);
			if (!bposString.IsNullOrEmpty())
				bposString = separator + bposString;
			
			var scheduledHeads =
				_skillCombinationResourceRepository.ScheduledHeadsForSkill(skill.Id.GetValueOrDefault(), period.Inflate(1)).ToList();
			
			forecastedData.AppendLine($"skill{separator}startdatetime{separator}enddatetime{separator}forecasted agents" +
				$"{separator}total scheduled agents{separator}total diff{separator}total scheduled heads{bposString}");
			
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

				var startDateTime = skillStaffPeriod.Period.StartDateTimeLocal(_userTimeZone.TimeZone()).ToString("g",userCulture);
				var endDateTime = skillStaffPeriod.Period.EndDateTimeLocal(_userTimeZone.TimeZone()).ToString("g", userCulture);
				var demand = skillStaffPeriod.FStaff;
			
				var bpoResourceTuple = createBpoResourcesString(bpoNames, resourcesByBpo, ssiStartDate, ssiEndDate, separator, userCulture);
				var totalDiff = staffing - demand;

				var internalScheduledHeads =
					scheduledHeads.SingleOrDefault(h => h.StartDateTime == ssiStartDate && h.EndDateTime == ssiEndDate)?.Heads ?? 0;
				
				var row = $"{skill.Name}{separator}{startDateTime}{separator}{endDateTime}{separator}" +
						  $"{Math.Round(demand,numberOfDecimals).ToString(userCulture)}{separator}{Math.Round(staffing,numberOfDecimals).ToString(userCulture)}{separator}" +
						  $"{Math.Round(totalDiff,numberOfDecimals).ToString(userCulture)}{separator}{internalScheduledHeads+bpoResourceTuple.bpoHeads}{bpoResourceTuple.bpoResourceString}";
				forecastedData.AppendLine(row);
			}
			return forecastedData.ToString().Trim();
		}

		private static (string bpoResourceString, double bpoHeads) createBpoResourcesString(IEnumerable<string> bpoNames,
			ILookup<string, SkillCombinationResourceForBpo> resourcesByBpo,
			DateTime startDate, DateTime endDate, string separator, IFormatProvider userCulture)
		{
			var resourcesString = new StringBuilder();
			var bpoHeadsSum = 0.0;
			foreach (var bpoName in bpoNames)
			{
				resourcesString.Append(separator);
				var resources = resourcesByBpo[bpoName];
				var resourceForPeriod = resources.SingleOrDefault(r => r.StartDateTime == startDate && r.EndDateTime == endDate);

				var resourceValue = resourceForPeriod?.Resource ?? 0;
				bpoHeadsSum += resourceValue;
				resourcesString.Append($"{resourceValue.ToString(userCulture)}");
			}

			return (bpoResourceString: resourcesString.ToString(), bpoHeads: bpoHeadsSum);
		}
	}
}
