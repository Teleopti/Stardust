using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class SkillDayCalculator : ISkillDayCalculator
    {
        private readonly IList<ISkillDay> _skillDays;
        private readonly ISkill _skill;
        private bool _isCalculatedWithinDay;
        private DateOnlyPeriod _visiblePeriod;
        private readonly Dictionary<IWorkload, SortedList<DateOnly, IWorkloadDay>> _workloadDaysCache;
        private readonly IDictionary<DateTime, ISkillStaffPeriod> _skillStaffPeriods = new Dictionary<DateTime, ISkillStaffPeriod>(new DateTimeAsLongEqualityComparer());

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public SkillDayCalculator(ISkill skill, IList<ISkillDay> skillDays, DateOnlyPeriod visiblePeriod)
        {
            _skillDays = skillDays;
            _skill = skill;
            _visiblePeriod = visiblePeriod;
            _skillDays.ForEach(s => s.SkillDayCalculator = this);
            _workloadDaysCache = new Dictionary<IWorkload,SortedList<DateOnly, IWorkloadDay>>();
            foreach (var skillDay in skillDays)
            {
                foreach (var workloadDay in skillDay.WorkloadDayCollection)
                {
                    SortedList<DateOnly, IWorkloadDay> workloadDayList;
                    if (!_workloadDaysCache.TryGetValue(workloadDay.Workload,out workloadDayList))
                    {
                        workloadDayList = new SortedList<DateOnly, IWorkloadDay>();
                        _workloadDaysCache.Add(workloadDay.Workload,workloadDayList);
                    }
                    workloadDayList.Add(workloadDay.CurrentDate, workloadDay);
                }
            }
        }

		public event EventHandler<CustomEventArgs<IUnsavedDaysInfo>> DaysUnsaved;

        public void InvokeDatesUnsaved(IUnsavedDaysInfo unsavedDays)
    	{
            if (unsavedDays == null) return;
    		var handler = DaysUnsaved;
            if (handler != null) handler(this, new CustomEventArgs<IUnsavedDaysInfo>(unsavedDays));
    	}

    	public ISkill Skill
        {
            get { return _skill; }
        }

        public IEnumerable<ISkillDay> SkillDays
        {
            get { return _skillDays; }
        }

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
        public bool IsCalculatedWithinDay
        {
            get { return _isCalculatedWithinDay; }
        }

        public DateOnlyPeriod VisiblePeriod
        {
            get { return _visiblePeriod; }
            set { _visiblePeriod = value; }
        }

        public IEnumerable<ISkillDay> VisibleSkillDays
        {
            get { return _skillDays.Where(s => _visiblePeriod.Contains(s.CurrentDate)); }
        }

        public IEnumerable<ISkillStaffPeriod> SkillStaffPeriods
        {
            get { return _skillStaffPeriods.Values; }
        }

        public IEnumerable<ITemplateTaskPeriod> CalculateTaskPeriods(ISkillDay skillDay, bool enableSpillover)
        {
            List<ITemplateTaskPeriod> result = new List<ITemplateTaskPeriod>();

            if (_skill.SkillType.ForecastSource == ForecastSource.InboundTelephony)
            {
                var unsortedOpenTaskPeriods = new List<ITemplateTaskPeriod>();
                foreach (var workloadDay in skillDay.WorkloadDayCollection)
                {
                    unsortedOpenTaskPeriods.AddRange(workloadDay.OpenTaskPeriodList);
                }
                result = unsortedOpenTaskPeriods;

                _isCalculatedWithinDay = true;
            }
            else
            {
                foreach (var workload in _skill.WorkloadCollection)
                {
                    SortedList<DateOnly,IWorkloadDay> foundList;
                    if (!_workloadDaysCache.TryGetValue(workload, out foundList))
                        continue;

                    IWorkloadDay workloadDay;
                    if (!foundList.TryGetValue(skillDay.CurrentDate,out workloadDay))
                        continue;

                    var unsortedOpenTaskPeriods = workloadDay.OpenTaskPeriodList.ToList();
                    if (unsortedOpenTaskPeriods.Count == 0) continue;

                    var openTaskPeriods = unsortedOpenTaskPeriods.OrderBy(t => t.Period.StartDateTime);
                    var firstOpenTaskPeriod = openTaskPeriods.First();
                    var closedTaskPeriodsBefore =
                        GetClosedTaskPeriodsBeforeDateTime(workload, workloadDay.CurrentDate, firstOpenTaskPeriod.Period.StartDateTime);

                    double numberOfTasks = 0;
                    foreach (var taskPeriod in closedTaskPeriodsBefore)
                    {
                        numberOfTasks += taskPeriod.TotalTasks;
                    }

                    firstOpenTaskPeriod.AggregatedTasks = numberOfTasks;
                    result.AddRange(openTaskPeriods);

                    if (enableSpillover)
                    {
                        ISkillDay nextSkillDay = FindNextOpenDay(workload, workloadDay.CurrentDate);
                        if (nextSkillDay != null)
                        {
                            nextSkillDay.EnableSpillover = false;
                            nextSkillDay.RecalculateDailyTasks();
                            nextSkillDay.EnableSpillover = true;
                        }
                    }
                }

                _isCalculatedWithinDay = false;
            }

            return SkillDayStaffHelper.CombineList(result);
        }

        public ISkillDay FindNextOpenDay(IWorkload workload, DateOnly dateTime)
        {
            SortedList<DateOnly,IWorkloadDay> foundList;
            if (!_workloadDaysCache.TryGetValue(workload,out foundList))
            {
                return null;
            }

            foreach (var workloadDay in foundList)
            {
                if (workloadDay.Key<=dateTime) continue;
                if (workloadDay.Value.OpenForWork.IsOpen) return (ISkillDay)workloadDay.Value.Parent;
            }

            return null;
        }

        public ISkillDay FindNextOpenDay(ISkillDay skillDay)
        {
            return _skillDays.FirstOrDefault(
                s => s.CurrentDate > skillDay.CurrentDate && s.SkillStaffPeriodCollection.Count > 0);
        }

        private IEnumerable<ITemplateTaskPeriod> GetClosedTaskPeriodsBeforeDateTime(IWorkload workload,DateOnly dateLimit,DateTime timeLimit)
        {
            var taskPeriodList = new List<ITemplateTaskPeriod>();
            var result = new List<ITemplateTaskPeriod>();
            
            SortedList<DateOnly, IWorkloadDay> foundList;
            if (!_workloadDaysCache.TryGetValue(workload, out foundList))
            {
                return result;
            }

            var lowerDateLimit = dateLimit.AddDays(-8);
            foreach (var workloadDay in foundList)
            {
                if (workloadDay.Key > dateLimit) break;
                if (workloadDay.Key < lowerDateLimit) continue;

                taskPeriodList.AddRange(workloadDay.Value.TaskPeriodList.OrderBy(t => t.Period.StartDateTime));
            }

            taskPeriodList.Reverse();
            foreach (var templateTaskPeriod in taskPeriodList)
            {
                if (templateTaskPeriod.Period.StartDateTime>=timeLimit) continue;
                if (!((IWorkloadDayBase) templateTaskPeriod.Parent).IsOnlyIncoming(templateTaskPeriod)) break;

                result.Add(templateTaskPeriod);
            }
            return result;
        }

        public static DateOnlyPeriod GetPeriodToLoad(DateOnlyPeriod visiblePeriod)
        {
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
                {
                    return skillDay.SkillStaffPeriodCollection;
                }

                if (_skillStaffPeriods.Count == 0)
                {
                    foreach (var currentSkillDay in _skillDays)
                    {
                        foreach (
                            ISkillStaffPeriod skillStaffPeriod in currentSkillDay.CompleteSkillStaffPeriodCollection)
                        {
                            _skillStaffPeriods.Add(skillStaffPeriod.Period.StartDateTime, skillStaffPeriod);
                        }
                    }
                }

                var skillResolution = _skill.DefaultResolution;
                var endDateTime = skillDay.CurrentDate.AddDays(10).Date;
                for (DateTime currentDateTime = TimeZoneHelper.ConvertToUtc(skillDay.CurrentDate.Date, _skill.TimeZone); currentDateTime < endDateTime; currentDateTime = currentDateTime.AddMinutes(skillResolution))
                {
                    ISkillStaffPeriod foundSkillStaffPeriod;
                    if (!_skillStaffPeriods.TryGetValue(currentDateTime,out foundSkillStaffPeriod))
                    {
                        break;
                    }
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
                    s => s.Payload.ServiceAgreementData.ServiceLevel!=null && s.Payload.ServiceAgreementData.ServiceLevel.Seconds <= _skill.DefaultResolution*60);
        }

        public virtual void ClearSkillStaffPeriods()
        {
            _skillStaffPeriods.Clear();
        }

        public virtual void DistributeStaff()
        {
            ResetSkillStaffPeriods();
            for (int i = _skillDays.Count-1; i >= 0; i--)
            {
                _skillDays[i].RecalculateStaff();
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
            IList<ISkillDay> newSkillDays = new List<ISkillDay>();
            foreach (ISkillDay skillDay in _skillDays)
            {
                ISkillDay newSkillDay = skillDay.NoneEntityClone(scenario);
                newSkillDays.Add(newSkillDay);
            }

            ISkillDayCalculator skillDayCalculator = new SkillDayCalculator(_skill, newSkillDays, _visiblePeriod);
            return skillDayCalculator;
        }
    }

    internal class DateTimeAsLongEqualityComparer : IEqualityComparer<DateTime>
    {
        public bool Equals(DateTime x, DateTime y)
        {
            return y.Ticks.Equals(x.Ticks);
        }

        public int GetHashCode(DateTime obj)
        {
            return obj.GetHashCode();
        }
    }
}