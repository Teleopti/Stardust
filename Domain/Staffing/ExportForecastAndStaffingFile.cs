using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.UserTexts;
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
		private readonly INow _now;
		private readonly IStaffingSettingsReader _staffingSettingsReader;
		private readonly IUserUiCulture _userUiCulture;
		private readonly ISkillRepository _skillRepository;
		private readonly IUserCulture _userCulture;
		private const int numberOfDecimals = 2;

		public ExportForecastAndStaffingFile(ISkillDayRepository skillDayRepository, ICurrentScenario currentScenario, 
			ScheduledStaffingProvider scheduledStaffingProvider, IUserTimeZone userTimeZone, 
			ISkillCombinationResourceRepository skillCombinationResourceRepository, INow now, IStaffingSettingsReader staffingSettingsReader,
			IUserUiCulture userUiCulture, ISkillRepository skillRepository, IUserCulture userCulture)
		{
			_skillDayRepository = skillDayRepository;
			_currentScenario = currentScenario;
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_userTimeZone = userTimeZone;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_now = now;
			_staffingSettingsReader = staffingSettingsReader;
			_userUiCulture = userUiCulture;
			_skillRepository = skillRepository;
			_userCulture = userCulture;
		}
		public string GetExportPeriodMessageString()
		{
			var readModelNumberOfDays = _staffingSettingsReader.GetIntSetting(KeyNames.StaffingReadModelNumberOfDays, 14);
			var staffingReadModelHistoricalDays = _staffingSettingsReader.GetIntSetting(KeyNames.StaffingReadModelHistoricalHours, 8 * 24)/24;

			var utcNowDate = new DateOnly(_now.UtcDateTime());
			var exportMinDate = new DateOnly(_now.UtcDateTime().AddDays(-staffingReadModelHistoricalDays));
			var exportPeriodMaxDate = utcNowDate.AddDays(readModelNumberOfDays);

			var validExportPeriodText =
				$"{exportMinDate.ToShortDateString(_userCulture.GetCulture())} - {exportPeriodMaxDate.ToShortDateString(_userCulture.GetCulture())}";
			var exportPeriodMessage = string.Format(
				Resources.ResourceManager.GetString(nameof(Resources.BpoOnlyExportPeriodBetweenDates), _userUiCulture.GetUiCulture()), validExportPeriodText);
			return exportPeriodMessage;
		}

		public virtual ExportStaffingReturnObject ExportForecastAndStaffing(Guid skillId, DateTime exportStartDate, DateTime exportEndDate, bool useShrinkage)
		{
			var returnVal = new ExportStaffingReturnObject();
			var exportStartDateOnly = new DateOnly(exportStartDate);
			var exportEndDateOnly = new DateOnly(exportEndDate);
			var readModelNumberOfDays = _staffingSettingsReader.GetIntSetting(KeyNames.StaffingReadModelNumberOfDays, 14);
			var staffingReadModelHistoricalDays = _staffingSettingsReader.GetIntSetting(KeyNames.StaffingReadModelHistoricalHours, 8 * 24)/24;
			var utcNowDate = new DateOnly(_now.UtcDateTime());
			var exportPeriodMinDate = utcNowDate.AddDays(-staffingReadModelHistoricalDays);
			var exportPeriodMaxDate = utcNowDate.AddDays(readModelNumberOfDays);
			if (exportStartDateOnly > exportEndDateOnly)
			{
				returnVal.ErrorMessage = Resources.ResourceManager.GetString(nameof(Resources.BpoExportPeriodStartDateBeforeEndDate), _userUiCulture.GetUiCulture());
				return returnVal;
			}
			if (exportStartDateOnly < exportPeriodMinDate || exportEndDateOnly > exportPeriodMaxDate)
			{
				var validExportPeriodText = GetExportPeriodMessageString();
				returnVal.ErrorMessage = validExportPeriodText;
				return returnVal;
			}

			var skill = _skillRepository.Get(skillId);
			if (skill == null)
			{
				returnVal.ErrorMessage = $"Cannot find skill with id: {skillId}";
				return returnVal;
			}
			var exportedContent = ExportDemand(skill,
				new DateOnlyPeriod(exportStartDateOnly, exportEndDateOnly), useShrinkage);

			returnVal.Content = exportedContent;
			returnVal.ErrorMessage = "";
			return returnVal;
		}

		public string ExportDemand(ISkill skill, DateOnlyPeriod period, bool useShrinkage = false)
		{
			var userCulture = _userCulture.GetCulture();
			var separator = userCulture.TextInfo.ListSeparator;
			
			var skillDays = _skillDayRepository.FindRange(period,skill, _currentScenario.Current());
			var loadSkillSchedule = new Dictionary<ISkill, IEnumerable<ISkillDay>> { { skill, skillDays.ToList() } };
			var skillStaffPeriodHolder = new SkillStaffPeriodHolder(loadSkillSchedule);
			var forecastedData = new StringBuilder();
			var allIntervals = new List<SkillStaffingInterval>();
			var staffingPerSkill = _scheduledStaffingProvider.StaffingPerSkill(new List<ISkill> {skill},
				period.Inflate(1).ToDateTimePeriod(TimeZoneInfo.Utc), useShrinkage, true);
			//staffingPerSkill[0]
			
			allIntervals.AddRange(staffingPerSkill);

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
