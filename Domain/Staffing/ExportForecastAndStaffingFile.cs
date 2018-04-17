using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
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
		private readonly ICurrentScenario _currentScenario;
		private readonly ScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly INow _now;
		private readonly IStaffingSettingsReader _staffingSettingsReader;
		private readonly IUserUiCulture _userUiCulture;
		private readonly ISkillRepository _skillRepository;
		private readonly IUserCulture _userCulture;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private const int numberOfDecimals = 2;

		public ExportForecastAndStaffingFile(ICurrentScenario currentScenario, 
			ScheduledStaffingProvider scheduledStaffingProvider, IUserTimeZone userTimeZone, 
			ISkillCombinationResourceRepository skillCombinationResourceRepository, INow now, IStaffingSettingsReader staffingSettingsReader,
			IUserUiCulture userUiCulture, ISkillRepository skillRepository, IUserCulture userCulture, ISkillDayLoadHelper skillDayLoadHelper)
		{
			_currentScenario = currentScenario;
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_userTimeZone = userTimeZone;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_now = now;
			_staffingSettingsReader = staffingSettingsReader;
			_userUiCulture = userUiCulture;
			_skillRepository = skillRepository;
			_userCulture = userCulture;
			_skillDayLoadHelper = skillDayLoadHelper;
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
			
			var forecastedData = new StringBuilder();
			var allIntervals = new List<SkillStaffingInterval>();
			var staffingPerSkill = _scheduledStaffingProvider.StaffingPerSkill(new List<ISkill> {skill},
				period.Inflate(1).ToDateTimePeriod(TimeZoneInfo.Utc), useShrinkage, true);
			
			allIntervals.AddRange(staffingPerSkill);

			var usedIntervals = allIntervals.Where(i => period.Contains(new DateOnly(TimeZoneHelper.ConvertFromUtc(i.StartDateTime, _userTimeZone.TimeZone())))).ToList();
			if(usedIntervals.IsEmpty())
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
			
			foreach (var interval in usedIntervals)
			{
				var ssiStartDate = interval.StartDateTime;
				var ssiEndDate = interval.EndDateTime;
				//var staffingInterval =
				//	allIntervals.Where(
				//		x => x.StartDateTime == ssiStartDate && x.EndDateTime == ssiEndDate && x.SkillId == skill.Id.GetValueOrDefault()).ToList();
				//var staffing = 0d;
				//var demand = 0d;
				
				//if (interval.Any())
				
				var staffing = interval.StaffingLevel;
				var demand = interval.FStaff;

				//var startDateTime = interval.Period.StartDateTimeLocal(_userTimeZone.TimeZone()).ToString("g",userCulture);
				//var endDateTime = interval.Period.EndDateTimeLocal(_userTimeZone.TimeZone()).ToString("g", userCulture);
				var startDateTime = TimeZoneHelper.ConvertFromUtc(interval.StartDateTime, _userTimeZone.TimeZone()).ToString("g",userCulture);
				var endDateTime = TimeZoneHelper.ConvertFromUtc(interval.EndDateTime, _userTimeZone.TimeZone()).ToString("g",userCulture);
				
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
