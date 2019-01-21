using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class SkillDayCalculator : ISkillDayCalculator
    {
        private readonly IEnumerable<ISkillDay> _skillDays;
        private readonly ISkill _skill;
        private bool _isCalculatedWithinDay;
        private DateOnlyPeriod _visiblePeriod;

	    private readonly IDictionary<IWorkload, Dictionary<DateOnly, IWorkloadDay>> _workloadDaysCache;
        private IDictionary<DateTime, ISkillStaffPeriod> _skillStaffPeriods = new Dictionary<DateTime, ISkillStaffPeriod>();

        public SkillDayCalculator(ISkill skill, IEnumerable<ISkillDay> skillDays, DateOnlyPeriod visiblePeriod)
        {
            _skillDays = skillDays;
            _skill = skill;
            _visiblePeriod = visiblePeriod;
            _skillDays.ForEach(s => s.SkillDayCalculator = this);

			var workloadDays = skillDays.SelectMany(skillDay => skillDay.WorkloadDayCollection).ToLookup(w => w.Workload);
			_workloadDaysCache = workloadDays.ToDictionary(k => k.Key,v => v.ToDictionary(k => k.CurrentDate));
        }

		public event EventHandler<CustomEventArgs<IUnsavedDaysInfo>> DaysUnsaved;

        public void InvokeDatesUnsaved(IUnsavedDaysInfo unsavedDays)
    	{
            if (unsavedDays == null) return;
			DaysUnsaved?.Invoke(this, new CustomEventArgs<IUnsavedDaysInfo>(unsavedDays));
		}

    	public ISkill Skill => _skill;

	    public IEnumerable<ISkillDay> SkillDays => _skillDays;

	    /// <summary>
        /// Gets a value indicating whether this instance is calculated within day.
        /// Only valid after calculation of task periods.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is calculated within day; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-02
        /// </remarks>
        public bool IsCalculatedWithinDay => _isCalculatedWithinDay;

	    public DateOnlyPeriod VisiblePeriod
        {
            get => _visiblePeriod;
		    set => _visiblePeriod = value;
	    }

        public IEnumerable<ISkillDay> VisibleSkillDays
        {
            get { return _skillDays.Where(s => _visiblePeriod.Contains(s.CurrentDate)); }
        }

        public int SkillStaffPeriodCount => _skillStaffPeriods.Count;

	    public IEnumerable<ITemplateTaskPeriod> CalculateTaskPeriods(ISkillDay skillDay, bool enableSpillover)
        {
            var result = new List<ITemplateTaskPeriod>();

            if (skillShouldBeCalculatedWithinDay())
            {
                result.AddRange(skillDay.WorkloadDayCollection.SelectMany(w => w.OpenTaskPeriodList));

                _isCalculatedWithinDay = true;
            }
            else
            {
                foreach (var workload in _skill.WorkloadCollection)
                {
					if (!_workloadDaysCache.TryGetValue(workload, out var foundList))
                        continue;

					if (!foundList.TryGetValue(skillDay.CurrentDate,out var workloadDay))
                        continue;

                    var unsortedOpenTaskPeriods = workloadDay.OpenTaskPeriodList.ToList();
                    if (unsortedOpenTaskPeriods.Count == 0) continue;

                    var openTaskPeriods = unsortedOpenTaskPeriods.OrderBy(t => t.Period.StartDateTime);
                    var firstOpenTaskPeriod = openTaskPeriods.First();
                    var closedTaskPeriodsBefore =
                        GetClosedTaskPeriodsBeforeDateTime(workload, workloadDay.CurrentDate, firstOpenTaskPeriod.Period.StartDateTime);

                    var numberOfTasks = closedTaskPeriodsBefore.Sum(taskPeriod => taskPeriod.TotalTasks);

                    firstOpenTaskPeriod.AggregatedTasks = numberOfTasks;
                    result.AddRange(openTaskPeriods);

                    if (!enableSpillover) continue;
                    var nextSkillDay = FindNextOpenDay(workload, workloadDay.CurrentDate);
                    if (nextSkillDay == null) continue;
                    nextSkillDay.EnableSpillover = false;
                    nextSkillDay.RecalculateDailyTasks();
                    nextSkillDay.EnableSpillover = true;
                }

                _isCalculatedWithinDay = false;
            }

            return SkillDayStaffHelper.CombineList(result);
        }

	    private bool skillShouldBeCalculatedWithinDay()
	    {
		    return _skill.SkillType.ForecastSource == ForecastSource.InboundTelephony ||_skill.SkillType.ForecastSource==ForecastSource.Retail || _skill.SkillType.ForecastSource == ForecastSource.Chat;
	    }

	    public ISkillDay FindNextOpenDay(IWorkload workload, DateOnly dateTime)
        {
            Dictionary<DateOnly,IWorkloadDay> foundList;
            if (!_workloadDaysCache.TryGetValue(workload,out foundList))
            {
                return null;
            }

			dateTime = dateTime.AddDays(1);
			while (foundList.TryGetValue(dateTime, out IWorkloadDay day))
			{
				if (day.OpenForWork.IsOpen)
					return (ISkillDay)day.Parent;
				dateTime = dateTime.AddDays(1);
			}
			return null;
		}

        public ISkillDay FindNextOpenDay(ISkillDay skillDay)
        {

            return _skillDays.FirstOrDefault(
                s => s.CurrentDate > skillDay.CurrentDate && s.SkillStaffPeriodCollection.Length > 0);
        }

        private IEnumerable<ITemplateTaskPeriod> GetClosedTaskPeriodsBeforeDateTime(IWorkload workload,DateOnly dateLimit,DateTime timeLimit)
        {
            var taskPeriodList = new List<ITemplateTaskPeriod>();
            var result = new List<ITemplateTaskPeriod>();
            
            Dictionary<DateOnly, IWorkloadDay> foundList;
            if (!_workloadDaysCache.TryGetValue(workload, out foundList))
            {
                return result;
            }

            var lowerDateLimit = dateLimit.AddDays(-8);
			for (var date = lowerDateLimit; date <= dateLimit; date = date.AddDays(1))
			{
				if (foundList.TryGetValue(date, out IWorkloadDay day))
					taskPeriodList.AddRange(day.TaskPeriodList.OrderBy(t => t.Period.StartDateTime));
			}

            taskPeriodList.Reverse();
            result.AddRange(taskPeriodList.Where(templateTaskPeriod => templateTaskPeriod.Period.StartDateTime < timeLimit).TakeWhile(templateTaskPeriod => ((IWorkloadDayBase) templateTaskPeriod.Parent).IsOnlyIncoming(templateTaskPeriod)));
            return result;
        }

        public static DateOnlyPeriod GetPeriodToLoad(DateOnlyPeriod visiblePeriod)
        {
			// some skills, like back office skills, email skills have a longer period when the load is distributed
			// to handle that, we load some days before and after
            return new DateOnlyPeriod(visiblePeriod.StartDate.AddDays(-7),visiblePeriod.EndDate.AddDays(1));
        }

        public virtual void CheckRestrictions()
        {
            _skillDays.ForEach(s => s.CheckRestrictions());
        }

        public virtual IEnumerable<ISkillStaffPeriod> GetSkillStaffPeriodsForDayCalculation(ISkillDay skillDay)
        {
            var foundSkillStaffPeriods = new List<ISkillStaffPeriod>();
            if (_skill.Equals(skillDay.Skill))
            {
                if (ShouldHandleAllTasksWithinSkillStaffPeriod(skillDay))
                    return skillDay.SkillStaffPeriodCollection;

                if (_skillStaffPeriods.Count == 0)
				{
					_skillStaffPeriods = _skillDays.SelectMany(currentSkillDay => currentSkillDay.CompleteSkillStaffPeriodCollection)
						.ToDictionary(p => p.Period.StartDateTime);
				}

                var skillResolution = _skill.DefaultResolution;
                var endDateTime = skillDay.CurrentDate.AddDays(10).Date;
                for (var currentDateTime = TimeZoneHelper.ConvertToUtc(skillDay.CurrentDate.Date, _skill.TimeZone); currentDateTime < endDateTime; currentDateTime = currentDateTime.AddMinutes(skillResolution))
                {
					if (!_skillStaffPeriods.TryGetValue(currentDateTime, out var foundSkillStaffPeriod))
                        break;
                    if (!foundSkillStaffPeriod.IsAvailable) continue;

                    foundSkillStaffPeriods.Add(foundSkillStaffPeriod);
                }
            }
            return foundSkillStaffPeriods;
        }

        private bool ShouldHandleAllTasksWithinSkillStaffPeriod(ISkillDay skillDay)
        {
            return
                skillDay.SkillStaffPeriodCollection.All(
                    s => s.Payload.ServiceAgreementData.ServiceLevel.Seconds <= _skill.DefaultResolution*60);
        }

        public virtual void ClearSkillStaffPeriods()
        {
            _skillStaffPeriods.Clear();
        }

        public virtual void DistributeStaff()
        {
            ResetSkillStaffPeriods();
	        var skillDayList = _skillDays.ToList();
					for (int i = skillDayList.Count - 1; i >= 0; i--)
            {
							skillDayList[i].RecalculateStaff();
            }
        }

        protected void ResetSkillStaffPeriods()
        {
            foreach (var skillStaffPeriod in _skillStaffPeriods)
            {
                ((SkillStaff)skillStaffPeriod.Value.Payload).IsCalculated = false;
            }
        }

        public virtual Percent GetPercentageForInterval(ISkill skill, DateTimePeriod period)
        {
            return new Percent(1);
        }

        /// <summary>
        /// Clones to scenario.
        /// </summary>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-12-10
        /// </remarks>
        public virtual ISkillDayCalculator CloneToScenario(IScenario scenario)
        {
            var newSkillDays = _skillDays.Select(skillDay => skillDay.NoneEntityClone(scenario)).ToList();
            var skillDayCalculator = new SkillDayCalculator(_skill, newSkillDays, _visiblePeriod);
            return skillDayCalculator;
        }
    }
}