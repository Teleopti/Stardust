using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetStaffingUsingReadModel : ILoadSkillDaysByPeriod
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ISkillRepository _skillRepository;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly SkillDayLoadHelper _skillDayLoadHelper;
		private readonly IActivityRepository _activityRepository;
		private readonly IScheduledStaffingProvider _scheduledStaffingProvider;

		public GetStaffingUsingReadModel(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
			ISkillRepository skillRepository,
			ICurrentScenario scenarioRepository,
			SkillDayLoadHelper skillDayLoadHelper,
			IActivityRepository activityRepository,
			IScheduledStaffingProvider scheduledStaffingProvider)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_skillRepository = skillRepository;
			_scenarioRepository = scenarioRepository;
			_skillDayLoadHelper = skillDayLoadHelper;
			_activityRepository = activityRepository;
			_scheduledStaffingProvider = scheduledStaffingProvider;
		}

		public ICollection<SkillDayDto> GetSkillDayDto(GetSkillDaysByPeriodQueryDto query)
		{
			var dateOnlyPeriod = query.Period.ToDateOnlyPeriod();
			if (dateOnlyPeriod.DayCount() > 31)
				throw new FaultException();
			ICollection<SkillDayDto> returnList = new List<SkillDayDto>();
			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(query.TimeZoneId);
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				using (uow.DisableFilter(QueryFilter.Deleted))
				{
					_skillRepository.LoadAll();
					_activityRepository.LoadAll();
				}
				var requestedScenario = _scenarioRepository.Current();
				var period = dateOnlyPeriod.ToDateTimePeriod(timeZoneInfo);
				var dayCollection = dateOnlyPeriod.DayCollection();
				var skillDaysPeriod = new DateTimePeriod(period.StartDateTime.AddDays(-1), period.EndDateTime.AddDays(1));
				var skills = _skillRepository.FindAllWithSkillDays(skillDaysPeriod.ToDateOnlyPeriod(timeZoneInfo)).ToArray();
				var skillDaysBySkills = _skillDayLoadHelper.LoadSchedulerSkillDays(skillDaysPeriod.ToDateOnlyPeriod(timeZoneInfo), skills, requestedScenario);
				var skillsToFetch = skills.Any(x => x is MultisiteSkill) ? skillDaysBySkills.Keys
										: skills;
				var staffingPerSkill = _scheduledStaffingProvider.StaffingPerSkill(skillsToFetch.ToList(),  skillDaysPeriod, false);

				SetSkillStaffPeriodWithFStaffAndEsl(staffingPerSkill.ToList(), skills, skillDaysBySkills);

				foreach (var skill in skills)
				{
					if (skill is IChildSkill)
						continue;
					var skillDays = skillDaysBySkills[skill];
					var staffingList = staffingPerSkill.Where(x => x.SkillId == skill.Id);
					foreach (var dateOnly in dayCollection)
					{
						var dateTimeUtc = dateOnly.ToDateOnlyPeriod().ToDateTimePeriod(timeZoneInfo);
						var staffingDay = staffingList.Where(y => y.StartDateTime >= dateTimeUtc.StartDateTime && y.EndDateTime <= dateTimeUtc.EndDateTime);
						var skillDayDto = new SkillDayDto { DisplayDate = new DateOnlyDto { DateTime = dateOnly.Date } };
						var skillDay = skillDays.FirstOrDefault(x => x.CurrentDate == dateOnly);
						if (skillDay == null)
							continue;
						var skillStaffPeriods = skillDay.SkillStaffPeriodCollection;
						skillDayDto.Esl = SkillStaffPeriodHelper.EstimatedServiceLevel(skillStaffPeriods).Value;
						skillDayDto.SkillId = skill.Id.GetValueOrDefault();
						skillDayDto.SkillName = skill.Name;
						skillDayDto.SkillDataCollection = GetSkillDataCollection(staffingDay, timeZoneInfo);
						returnList.Add(skillDayDto);
					}
				}


			}
			return returnList;
		}

		private void SetSkillStaffPeriodWithFStaffAndEsl(List<SkillStaffingInterval> skillStaffingIntervals, ISkill[] skills, IDictionary<ISkill, IEnumerable<ISkillDay>> skillDaysPerSkill)
		{

			foreach (var skill in skills)
			{
				var staffingIntervals = skillStaffingIntervals.Where(x => x.SkillId == skill.Id.Value);
				var skillDays = skillDaysPerSkill[skill];
				foreach (var skillDay in skillDays)
				{
					foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
					{
						var intervalStartLocal = skillStaffPeriod.Period.StartDateTime;
						var scheduledStaff =
							staffingIntervals.FirstOrDefault(x => x.StartDateTime == intervalStartLocal);
						if (scheduledStaff == null)
							continue;
						skillStaffPeriod.SetCalculatedResource65(0);
						if (scheduledStaff.StaffingLevel > 0)
						{
							skillStaffPeriod.SetCalculatedResource65(scheduledStaff.StaffingLevel);
						}
					}
				}

				foreach (var skillStaffingInterval in staffingIntervals)
				{
					var skillDay = skillDays.FirstOrDefault(x => x.CurrentDate.Date ==
																 skillStaffingInterval.StartDateTime.Date);
					var skillStaffPeriod = skillDay?.SkillStaffPeriodCollection
						.FirstOrDefault(y => y.Period.StartDateTime == skillStaffingInterval.StartDateTime);
					if (skillStaffPeriod == null)
						continue;
					skillStaffingInterval.Forecast = skillStaffPeriod.FStaff;
					skillStaffingInterval.EstimatedServiceLevel = skillStaffPeriod.EstimatedServiceLevel;
				}
			}
		}

		private ICollection<SkillDataDto> GetSkillDataCollection(IEnumerable<SkillStaffingInterval> staffingDay, TimeZoneInfo timeZoneInfo)
		{
			ICollection<SkillDataDto> skillDataCollection = new List<SkillDataDto>();
			foreach (var staffing in staffingDay)
			{
				var skillDataPerPeriod = new SkillDataDto
				{
					ForecastedAgents = staffing.FStaff,
					ScheduledAgents = staffing.StaffingLevel,
					ScheduledHeads = staffing.CalculatedLoggedOn,
					IntervalStandardDeviation = staffing.IntraIntervalDeviation,
					EstimatedServiceLevel = staffing.EstimatedServiceLevel.Value,
					Period = new DateTimePeriodDto()
				};

				skillDataPerPeriod.Period.UtcStartTime = staffing.StartDateTime;
				skillDataPerPeriod.Period.UtcEndTime = staffing.EndDateTime;
				skillDataPerPeriod.Period.LocalStartDateTime = TimeZoneHelper.ConvertFromUtc(staffing.StartDateTime, timeZoneInfo);
				skillDataPerPeriod.Period.LocalEndDateTime = TimeZoneHelper.ConvertFromUtc(staffing.EndDateTime, timeZoneInfo);

				skillDataCollection.Add(skillDataPerPeriod);
			}
			return skillDataCollection;
		}
	}
}