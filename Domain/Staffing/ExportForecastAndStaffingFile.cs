using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class ExportForecastAndStaffingFile
	{
		private readonly IScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IUserCulture _userCulture;
		private readonly ExportStaffingPeriodValidationProvider _periodValidationProvider;
		private const int numberOfDecimals = 2;

		public ExportForecastAndStaffingFile(IScheduledStaffingProvider scheduledStaffingProvider, IUserTimeZone userTimeZone, 
			ISkillCombinationResourceRepository skillCombinationResourceRepository, ISkillRepository skillRepository, 
			IUserCulture userCulture, ExportStaffingPeriodValidationProvider periodValidationProvider)
		{
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_userTimeZone = userTimeZone;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_skillRepository = skillRepository;
			_userCulture = userCulture;
			_periodValidationProvider = periodValidationProvider;
		}
		
		public virtual ExportStaffingReturnObject ExportForecastAndStaffing(Guid skillId, DateOnly exportStartDateOnly, DateOnly exportEndDateOnly, bool useShrinkage)
		{
			var returnVal = new ExportStaffingReturnObject();

			var validationObject = _periodValidationProvider.ValidateExportStaffingPeriod(exportStartDateOnly, exportEndDateOnly);
			if (validationObject.Result == false)
			{
				returnVal.ErrorMessage = validationObject.ErrorMessage;
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
			
			var bpoResources = _skillCombinationResourceRepository.BpoResourcesForSkill(skill.Id.GetValueOrDefault(), period.Inflate(1)).ToList();
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
				var staffing = interval.StaffingLevel;
				var demand = interval.FStaff;
				
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
