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
		private readonly ISkillIntervalDataOpenHour _skillIntervalDataOpenHour;
		private readonly ICreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;

		public TeamBlockOpenHourService(ISkillIntervalDataOpenHour skillIntervalDataOpenHour, ICreateSkillIntervalDataPerDateAndActivity createSkillIntervalDataPerDateAndActivity)
		{
			_skillIntervalDataOpenHour = skillIntervalDataOpenHour;
			_createSkillIntervalDataPerDateAndActivity = createSkillIntervalDataPerDateAndActivity;
		}

		public TimePeriod? OpenHoursForTeamBlock(ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> dayIntervalDataPerDateAndActivity;
			dayIntervalDataPerDateAndActivity = _createSkillIntervalDataPerDateAndActivity.CreateFor(teamBlockInfo, schedulingResultStateHolder);
			var openHoursForBlock = new TimePeriod(TimeSpan.MaxValue, TimeSpan.MinValue);
			foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
			{
				var openHours = openHoursForDate(dateOnly, dayIntervalDataPerDateAndActivity);
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

		//class
		private TimePeriod openHoursForDate(DateOnly dateOnly, Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> dayIntervalDataPerDateAndActivity)
		{
			var minOpen = TimeSpan.MaxValue;
			var maxOpen = TimeSpan.MinValue;
			var intervalDataPerActivity = dayIntervalDataPerDateAndActivity[dateOnly];
			foreach (var activity in intervalDataPerActivity.Keys)
			{
				var openPeriod = _skillIntervalDataOpenHour.GetOpenHours(intervalDataPerActivity[activity], dateOnly);

				if (openPeriod.StartTime < minOpen)
					minOpen = openPeriod.StartTime;

				if (openPeriod.EndTime > maxOpen)
					maxOpen = openPeriod.StartTime;
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
		IList<ISkillIntervalData> CalculateFor(List<ISkillDay> skillDaysForPersonalSkill, IActivity skillActivity);
	}

	public class CalculateAggregatedDataForActivtyAndDate : ICalculateAggregatedDataForActivtyAndDate
	{
		private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
		private readonly ISkillIntervalDataSkillFactorApplier _skillIntervalDataSkillFactorApplier;
		private readonly ISkillIntervalDataAggregator _intervalDataAggregator;

		public CalculateAggregatedDataForActivtyAndDate(
			ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper,
			ISkillIntervalDataSkillFactorApplier skillIntervalDataSkillFactorApplier,
			ISkillIntervalDataAggregator intervalDataAggregator)
		{
			_skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
			_skillIntervalDataSkillFactorApplier = skillIntervalDataSkillFactorApplier;
			_intervalDataAggregator = intervalDataAggregator;
		}

		public IList<ISkillIntervalData> CalculateFor(List<ISkillDay> skillDaysForPersonalSkill, IActivity skillActivity)
		{
			var skillIntervalDatasForActivity = new List<IList<ISkillIntervalData>>();
			foreach (var skillDay in skillDaysForPersonalSkill)
			{
				var skill = skillDay.Skill;
				if (skill.Activity == skillActivity)
				{
					var skillIntervalDatas =
						_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillDay.SkillStaffPeriodCollection);
					var adjustedIntervalDatas = new List<ISkillIntervalData>();
					foreach (var skillIntervalData in skillIntervalDatas)
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

		public CreateSkillIntervalDatasPerActivtyForDate(ICalculateAggregatedDataForActivtyAndDate calculateAggregatedDataForActivtyAndDate)
		{
			_calculateAggregatedDataForActivtyAndDate = calculateAggregatedDataForActivtyAndDate;
		}

		public Dictionary<IActivity, IList<ISkillIntervalData>> CreateFor(DateOnly dateOnly, List<ISkill> skills,
		                                                                  ISchedulingResultStateHolder
			                                                                  schedulingResultStateHolder)
		{
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
				var dayIntervalData = _calculateAggregatedDataForActivtyAndDate.CalculateFor(skillDaysForPersonalSkill, skillActivity);
				dayIntervalDataPerActivity.Add(skillActivity, dayIntervalData);
			}
			return dayIntervalDataPerActivity;
		}
	}
}