using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.Intraday.ApplicationLayer
{
	public interface IIntradayApplicationStaffingService
	{
		ScheduledStaffingViewModel GenerateStaffingViewModel(Guid[] skillIdList, int dayOffset);

		ScheduledStaffingViewModel GenerateStaffingViewModel(Guid[] skillIdList, DateOnly? dateInLocalTime = null,
			bool useShrinkage = false);
	}

	public class IntradayAppStaffingService : IIntradayApplicationStaffingService
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IStaffingViewModelCreator _staffingViewModelCreator;

		public IntradayAppStaffingService(INow now, IUserTimeZone timeZone, IStaffingViewModelCreator staffingViewModelCreator)
		{
			_now = now;
			_timeZone = timeZone;
			_staffingViewModelCreator = staffingViewModelCreator;
		}

		public ScheduledStaffingViewModel GenerateStaffingViewModel(Guid[] skillIdList, int dayOffset)
		{
			var localTimeWithOffset = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()).AddDays(dayOffset);
			return GenerateStaffingViewModel(skillIdList, new DateOnly(localTimeWithOffset));
		}
		
		public ScheduledStaffingViewModel GenerateStaffingViewModel(
			Guid[] skillIdList, 
			DateOnly? dateInLocalTime = null,
			bool useShrinkage = false)
		{
			return _staffingViewModelCreator.Load(skillIdList, dateInLocalTime, useShrinkage);
		}

		//private DateTime[] generateTimeSeries(DateTime start, DateTime stop, int resolution)
		//{
		//	var times = Enumerable
		//		.Range(0, (int)Math.Ceiling((decimal)(stop - start).TotalMinutes / resolution))
		//		.Select(offset => start.AddMinutes(offset * resolution))
		//		.ToArray();
		//	return times;
		//}

		//public IList<StaffingIntervalModel> GetForecastedStaffing(
		//	IList<Guid> skillIdList,
		//	DateTime fromTimeUtc,
		//	DateTime toTimeUtc,
		//	TimeSpan resolution,
		//	bool useShrinkage)
		//{
		//	if (resolution.TotalMinutes <= 0) throw new Exception($"IntervalLength is cannot be {resolution.TotalMinutes}!");

		//	var scenario = _scenarioRepository.LoadDefaultScenario();
		//	var skills = _supportedSkillsInIntradayProvider.GetSupportedSkills(skillIdList.ToArray());
		//	if (!skills.Any())
		//		return new List<StaffingIntervalModel>();

		//	//var skillDays =
		//	//	_skillDayLoadHelper.LoadSchedulerSkillDays(new DateOnlyPeriod(new DateOnly(fromTimeUtc), new DateOnly(toTimeUtc)), skills, scenario)
		//	var skillDays =
		//		_loadSkillDaysWithPeriodFlexibility.Load(new DateOnlyPeriod(new DateOnly(fromTimeUtc), new DateOnly(toTimeUtc)), skills, scenario)
		//	.SelectMany(x => x.Value);

		//	var forecast = skillDays
		//		.SelectMany(x => x.SkillStaffPeriodViewCollection(resolution,useShrinkage).Select(i => new {SkillDay = x, StaffPeriod = i}))
		//		.Where(x => x.StaffPeriod.Period.StartDateTime >= fromTimeUtc && x.StaffPeriod.Period.EndDateTime <= toTimeUtc);

		//	if (!forecast.Any())
		//		return new List<StaffingIntervalModel>();

		//	return forecast.Select(x => new StaffingIntervalModel
		//		{
		//			StartTime = TimeZoneInfo.ConvertTimeFromUtc(x.StaffPeriod.Period.StartDateTime, _timeZone.TimeZone()),
		//			SkillId = x.SkillDay.Skill.Id.Value,
		//			Agents = x.StaffPeriod.FStaff
		//		}).ToList();
		//}

		//public IList<SkillStaffingIntervalLightModel> GetScheduledStaffing(
		//	Guid[] skillIdList, 
		//	DateTime fromTimeUtc, 
		//	DateTime toTimeUtc, 
		//	TimeSpan resolution, 
		//	bool useShrinkage)
		//{
		//	var scheduledStaffing = _staffingService.GetScheduledStaffing(skillIdList, fromTimeUtc, toTimeUtc, resolution, useShrinkage);

		//	return scheduledStaffing.Select(x => new SkillStaffingIntervalLightModel
		//		{
		//			Id = x.Id,
		//			StartDateTime = TimeZoneInfo.ConvertTimeFromUtc(x.StartDateTime, _timeZone.TimeZone()),
		//			EndDateTime = TimeZoneInfo.ConvertTimeFromUtc(x.EndDateTime, _timeZone.TimeZone()),
		//			StaffingLevel = x.StaffingLevel
		//		})
		//		.ToList();
		//}
	}

	
}
