using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval
{
	public interface ITeamBlockOpenHourService
	{
		TimePeriod? OpenHoursForTeamBlock(ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder);
	}

	public class TeamBlockOpenHourService : ITeamBlockOpenHourService
	{
		private readonly IOpenHourForDate _openHourForDate;
		private readonly ICreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;

		public TeamBlockOpenHourService(IOpenHourForDate openHourForDate, ICreateSkillIntervalDataPerDateAndActivity createSkillIntervalDataPerDateAndActivity)
		{
			_openHourForDate = openHourForDate;
			_createSkillIntervalDataPerDateAndActivity = createSkillIntervalDataPerDateAndActivity;
		}

		public TimePeriod? OpenHoursForTeamBlock(ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> dayIntervalDataPerDateAndActivity;
			dayIntervalDataPerDateAndActivity = _createSkillIntervalDataPerDateAndActivity.CreateFor(teamBlockInfo, schedulingResultStateHolder);
			var openHoursForBlock = new TimePeriod(TimeSpan.MaxValue, TimeSpan.MinValue);
			foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
			{
				var openHours = _openHourForDate.OpenHours(dateOnly, dayIntervalDataPerDateAndActivity[dateOnly]);
				var narrowed = getNarrowTimePeriod(openHoursForBlock, openHours);
				if (!narrowed.HasValue)
					return null;

				openHoursForBlock = narrowed.Value;
			}

			return openHoursForBlock;
		}

		private TimePeriod? getNarrowTimePeriod(TimePeriod existingTimePeriod, TimePeriod newTimePeriod)
		{
			if (!newTimePeriod.Intersect(existingTimePeriod))
				return null;

			var newStart = existingTimePeriod.StartTime;
			var newEnd = existingTimePeriod.EndTime;

			if (newTimePeriod.StartTime > newStart)
				newStart = newTimePeriod.StartTime;

			if (newTimePeriod.EndTime < newEnd)
				newEnd = newTimePeriod.EndTime;

			return new TimePeriod(newStart, newEnd);
		}
	}

	public interface IOpenHourForDate
	{
		TimePeriod OpenHours(DateOnly dateOnly, IDictionary<IActivity, IList<ISkillIntervalData>> dayIntervalDataPerActivityForDate);
	}

	public class OpenHourForDate : IOpenHourForDate
	{
		private readonly ISkillIntervalDataOpenHour _skillIntervalDataOpenHour;

		public OpenHourForDate(ISkillIntervalDataOpenHour skillIntervalDataOpenHour)
		{
			_skillIntervalDataOpenHour = skillIntervalDataOpenHour;
		}

		public TimePeriod OpenHours(DateOnly dateOnly, IDictionary<IActivity, IList<ISkillIntervalData>> dayIntervalDataPerActivityForDate)
		{
			var minOpen = TimeSpan.MaxValue;
			var maxOpen = TimeSpan.MinValue;
			foreach (var activity in dayIntervalDataPerActivityForDate.Keys)
			{
				var openPeriod = _skillIntervalDataOpenHour.GetOpenHours(dayIntervalDataPerActivityForDate[activity], dateOnly);

				if (openPeriod.StartTime < minOpen)
					minOpen = openPeriod.StartTime;

				if (openPeriod.EndTime > maxOpen)
					maxOpen = openPeriod.EndTime;
			}

			return new TimePeriod(minOpen, maxOpen);
		}
	}

	public interface ICreateSkillIntervalDataPerDateAndActivity
	{
		Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> CreateFor(ITeamBlockInfo teamBlockInfo,
		                                                                                  ISchedulingResultStateHolder
			                                                                                  schedulingResultStateHolder);
	}

	public class CreateSkillIntervalDataPerDateAndActivity : ICreateSkillIntervalDataPerDateAndActivity
	{
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private readonly ICreateSkillIntervalDatasPerActivtyForDate _createSkillIntervalDatasPerActivtyForDate;

		public CreateSkillIntervalDataPerDateAndActivity(IGroupPersonSkillAggregator groupPersonSkillAggregator,
		                                                 ICreateSkillIntervalDatasPerActivtyForDate
			                                                 createSkillIntervalDatasPerActivtyForDate)
		{
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
			_createSkillIntervalDatasPerActivtyForDate = createSkillIntervalDatasPerActivtyForDate;
		}

		public Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> CreateFor(ITeamBlockInfo teamBlockInfo,
														  ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var dayIntervalDataPerDateAndActivity = new Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>>();
			var groupPerson = teamBlockInfo.TeamInfo.GroupPerson;
			var blockPeriod = teamBlockInfo.BlockInfo.BlockPeriod;
			var skills = _groupPersonSkillAggregator.AggregatedSkills(groupPerson, blockPeriod).ToList();
			

			foreach (var dateOnly in blockPeriod.DayCollection())
			{
				var dayIntervalDataPerActivity = _createSkillIntervalDatasPerActivtyForDate.CreateFor(dateOnly, skills,
																									  schedulingResultStateHolder);
				dayIntervalDataPerDateAndActivity.Add(dateOnly, dayIntervalDataPerActivity);
			}

			var extraDate = blockPeriod.EndDate.AddDays(1);
			var extraDayIntervalDataPerActivity =
				_createSkillIntervalDatasPerActivtyForDate.CreateFor(blockPeriod.EndDate.AddDays(1), skills,
																	 schedulingResultStateHolder);
			dayIntervalDataPerDateAndActivity.Add(extraDate, extraDayIntervalDataPerActivity);
			return dayIntervalDataPerDateAndActivity;
		}
	}

	public interface ICalculateAggregatedDataForActivtyAndDate
	{
		IList<ISkillIntervalData> CalculateFor(List<ISkillDay> skillDaysForPersonalSkill, IActivity skillActivity, int resolution);
	}

	public class CalculateAggregatedDataForActivtyAndDate : ICalculateAggregatedDataForActivtyAndDate
	{
		private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
		private readonly ISkillIntervalDataSkillFactorApplier _skillIntervalDataSkillFactorApplier;
		private readonly ISkillIntervalDataAggregator _intervalDataAggregator;
		private readonly ISkillIntervalDataDivider _intervalDataDivider;

		public CalculateAggregatedDataForActivtyAndDate(
			ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper,
			ISkillIntervalDataSkillFactorApplier skillIntervalDataSkillFactorApplier,
			ISkillIntervalDataAggregator intervalDataAggregator,
			ISkillIntervalDataDivider intervalDataDivider)
		{
			_skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
			_skillIntervalDataSkillFactorApplier = skillIntervalDataSkillFactorApplier;
			_intervalDataAggregator = intervalDataAggregator;
			_intervalDataDivider = intervalDataDivider;
		}

		public IList<ISkillIntervalData> CalculateFor(List<ISkillDay> skillDaysForPersonalSkill, IActivity skillActivity, int resolution)
		{
			
			var skillIntervalDatasForActivity = new List<IList<ISkillIntervalData>>();
			foreach (var skillDay in skillDaysForPersonalSkill)
			{
				var skillStaffPeriods = skillDay.SkillStaffPeriodCollection;
				if(skillStaffPeriods.Count == 0)
					continue;

				var skill = skillDay.Skill;
				if (skill.Activity == skillActivity)
				{
					var skillIntervalDatas =
						_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillStaffPeriods);
					var splittedDatas = _intervalDataDivider.SplitSkillIntervalData(skillIntervalDatas, resolution);
					var adjustedIntervalDatas = new List<ISkillIntervalData>();
					foreach (var skillIntervalData in splittedDatas)
					{
						var adjustedIntervalData = _skillIntervalDataSkillFactorApplier.ApplyFactors(skillIntervalData, skill);
						adjustedIntervalDatas.Add(adjustedIntervalData);
					}
					skillIntervalDatasForActivity.Add(adjustedIntervalDatas);
				}
			}
			var dayIntervalData = _intervalDataAggregator.AggregateSkillIntervalData(skillIntervalDatasForActivity);
			return dayIntervalData;
		}
	}

	public interface ICreateSkillIntervalDatasPerActivtyForDate
	{
		Dictionary<IActivity, IList<ISkillIntervalData>> CreateFor(DateOnly dateOnly, List<ISkill> skills,
		                                                                           ISchedulingResultStateHolder
			                                                                           schedulingResultStateHolder);
	}

	public class CreateSkillIntervalDatasPerActivtyForDate : ICreateSkillIntervalDatasPerActivtyForDate
	{
		private readonly ICalculateAggregatedDataForActivtyAndDate _calculateAggregatedDataForActivtyAndDate;
		private readonly ISkillResolutionProvider _resolutionProvider;

		public CreateSkillIntervalDatasPerActivtyForDate(ICalculateAggregatedDataForActivtyAndDate calculateAggregatedDataForActivtyAndDate, ISkillResolutionProvider resolutionProvider)
		{
			_calculateAggregatedDataForActivtyAndDate = calculateAggregatedDataForActivtyAndDate;
			_resolutionProvider = resolutionProvider;
		}

		public Dictionary<IActivity, IList<ISkillIntervalData>> CreateFor(DateOnly dateOnly, List<ISkill> skills,
		                                                                  ISchedulingResultStateHolder
			                                                                  schedulingResultStateHolder)
		{
			var minimumResolution = _resolutionProvider.MinimumResolution(skills);
			var skilldaysForDate = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> {dateOnly});
			var skillDaysForPersonalSkill = new List<ISkillDay>();
			foreach (var skillDay in skilldaysForDate)
			{
				if (skills.Contains(skillDay.Skill))
					skillDaysForPersonalSkill.Add(skillDay);
			}

			var skillActivities = new HashSet<IActivity>();
			foreach (var skillDay in skillDaysForPersonalSkill)
			{
				skillActivities.Add(skillDay.Skill.Activity);
			}

			var dayIntervalDataPerActivity = new Dictionary<IActivity, IList<ISkillIntervalData>>();
			foreach (var skillActivity in skillActivities)
			{
				var dayIntervalData = _calculateAggregatedDataForActivtyAndDate.CalculateFor(skillDaysForPersonalSkill, skillActivity, minimumResolution);
				dayIntervalDataPerActivity.Add(skillActivity, dayIntervalData);
			}
			return dayIntervalDataPerActivity;
		}
	}
}