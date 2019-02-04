using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Used to represent skill data
    /// </summary>
    /// <remarks>
    /// Created by: micke
    /// Created date: 18.12.2007
    /// </remarks>
	public class SkillDay : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, ISkillDay, IPeriodized
    {
        private DateOnly _currentDate;
        private ISkill _skill;
        private TimeSpan _totalAverageAfterTaskTime;
        private TimeSpan _totalAverageTaskTime;
        private TimeSpan _totalStatisticAverageTaskTime;
        private TimeSpan _totalStatisticAverageAfterTaskTime;
        private bool _turnOffInternalRecalc;
        private bool _initialized;
        private bool _isDirty;
        private double _totalTasks;
        private double _totalStatisticCalculatedTasks;
        private double _totalStatisticAnsweredTasks;
        private double _totalStatisticAbandonedTasks;
        private IScenario _scenario;
        private ISet<ISkillStaffPeriod> _skillStaffPeriodCollection = new HashSet<ISkillStaffPeriod>();
        private ITemplateReference _templateReference = new SkillDayTemplateReference();
        private ISkillDayCalculator _skillDayCalculator;

        private ISet<IWorkloadDay> _workloadDayCollection = new HashSet<IWorkloadDay>();
        private readonly IList<ITaskOwner> _parents = new List<ITaskOwner>();
        private ISet<ISkillDataPeriod> _skillDataPeriodCollection = new HashSet<ISkillDataPeriod>();
        private bool _enableSpillover = true;
        private ISet<ISkillStaffPeriod> _availableSkillStaffPeriods = new HashSet<ISkillStaffPeriod>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillDay"/> class.
        /// </summary>
        /// <param name="skillDayDate">The skill day date.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="workloadDayCollection">The workload day collection.</param>
        /// <param name="skillDataPeriodCollection">The skill data period collection.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 18.12.2007
        /// </remarks>
		public SkillDay(DateOnly skillDayDate, ISkill skill, IScenario scenario,
            IEnumerable<IWorkloadDay> workloadDayCollection, IEnumerable<ISkillDataPeriod> skillDataPeriodCollection)
            : this()
        {
            InParameter.NotNull(nameof(scenario), scenario);
            InParameter.NotNull(nameof(skill), skill);

            _currentDate = skillDayDate;
            _skill = skill;
            _scenario = scenario;
            ((SkillDayTemplateReference) _templateReference).Skill = _skill;

            verifyAndAttachWorkloadDays(workloadDayCollection);
            verifyAndAttachSkillDataPeriods(skillDataPeriodCollection);

            setupSkillDay();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillDay"/> class for NHibernate.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-08
        /// </remarks>
        public SkillDay()
        {
        }

        public virtual void ClearParents()
        {
            _parents.Clear();
        }

        /// <summary>
        /// Gets the skill day date.
        /// </summary>
        /// <value>The skill day date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-08
        /// </remarks>
        public virtual DateOnly CurrentDate => _currentDate;

	    /// <summary>
        /// Gets the Scenario
        /// </summary>
        public virtual IScenario Scenario
        {
            get { return _scenario; }
            protected set { _scenario = value; }
        }

        /// <summary>
        /// Gets the skill.
        /// </summary>
        /// <value>The skill.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 18.12.2007
        /// </remarks>
        public virtual ISkill Skill => _skill;

	    /// <summary>
        /// Gets the skill data period collection.
        /// </summary>
        /// <value>The skill data period collection.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-30
        /// </remarks>
        public virtual ReadOnlyCollection<ISkillDataPeriod> SkillDataPeriodCollection => new ReadOnlyCollection<ISkillDataPeriod>(_skillDataPeriodCollection.ToArray());

	    /// <summary>
        /// Gets or sets the total tasks.
        /// </summary>
        /// <value>The total tasks.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/27/2007
        /// </remarks>
        public virtual double TotalTasks
        {
            get
            {
                if (!_initialized) initialize();
                return _totalTasks;
            }
        }

	    public virtual double? OverrideTasks => null;

	    public virtual TimeSpan? OverrideAverageTaskTime { get; set; }
	    public virtual TimeSpan? OverrideAverageAfterTaskTime { get; set; }
		
	    /// <summary>
        /// Gets the workload day collection.
        /// </summary>
        /// <value>The workload day collection.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        public virtual ReadOnlyCollection<IWorkloadDay> WorkloadDayCollection => new ReadOnlyCollection<IWorkloadDay>(_workloadDayCollection.ToArray());

	    /// <summary>
        /// Gets the forecasted incoming demand for the skill day.
        /// </summary>
        /// <value>The forecasted incoming demand.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-08
        /// </remarks>
        public virtual TimeSpan ForecastedIncomingDemand
        {
            get
            {
                if (!_initialized) initialize();
                if (SkillStaffPeriodCollection.Length == 0) return TimeSpan.Zero;

                return TimeSpan.FromMinutes(SkillStaffPeriodCollection.Sum(s => s.ForecastedIncomingDemand().TotalMinutes));
            }
        }

        /// <summary>
        /// Gets the forecasted hours with shrinkage.
        /// </summary>
        /// <value>The forecasted hours with shrinkage.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-08
        /// </remarks>
        public virtual TimeSpan ForecastedIncomingDemandWithShrinkage
        {
            get
            {
                if (!_initialized) initialize();
                return TimeSpan.FromMinutes(SkillStaffPeriodCollection.Sum(s => s.ForecastedIncomingDemandWithShrinkage().TotalMinutes));
            }
        }

        /// <summary>
        /// Gets the forecasted distributed demand.
        /// </summary>
        /// <value>The forecasted distributed demand.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-05
        /// </remarks>
        public virtual TimeSpan ForecastedDistributedDemand
        {
            get
            {
                if (!_initialized) initialize();
                return TimeSpan.FromMinutes(SkillStaffPeriodCollection.Sum(s => s.ForecastedDistributedDemand * s.Period.ElapsedTime().TotalMinutes));
            }
        }

        /// <summary>
        /// Gets the forecasted distributed demand with shrinkage.
        /// </summary>
        /// <value>The forecasted distributed demand with shrinkage.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-05
        /// </remarks>
        public virtual TimeSpan ForecastedDistributedDemandWithShrinkage
        {
            get
            {
                if (!_initialized) initialize();
                return TimeSpan.FromMinutes(SkillStaffPeriodCollection.Sum(s => s.ForecastedDistributedDemandWithShrinkage * s.Period.ElapsedTime().TotalMinutes));
            }
        }

        #region Methods

        //Public methods

        /// <summary>
        /// Sets the parent.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public virtual void AddParent(ITaskOwner parent)
        {
            if (!_parents.Contains(parent))
                _parents.Add(parent);
        }

        /// <summary>
        /// Adds the workload day.
        /// Temporary function to add a workload day to the skill day collection
        /// </summary>
        /// <param name="workloadDay">The workload day.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public virtual void AddWorkloadDay(IWorkloadDay workloadDay)
        {
            InParameter.NotNull(nameof(workloadDay), workloadDay);
            workloadDay.ClearParents();
            workloadDay.SetParent(this);
            verifyAndAttachWorkloadDays(new List<IWorkloadDay> { workloadDay });
        }

        /// <summary>
        /// Applies the template on a skillday.
        /// </summary>
        /// <param name="skillDayTemplate">The skill day template.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-04
        /// </remarks>
        public virtual void ApplyTemplate(ISkillDayTemplate skillDayTemplate)
        {
            SplitSkillDataPeriods(new List<ISkillDataPeriod>(_skillDataPeriodCollection));

            DateTime utcCurrentDate = TimeZoneHelper.ConvertToUtc(_currentDate.Date, _skill.TimeZone);
            DateTime utcBaseDate = TimeZoneHelper.ConvertToUtc(SkillDayTemplate.BaseDate.Date, _skill.TimeZone);

            TimeSpan diff = utcCurrentDate.Subtract(utcBaseDate);

            bool isFirstOrLastDayOfDaylightSavingTime = isFirstDayOfDaylightSavingTime(CurrentDate) || isLastDayOfDaylightSavingTime(CurrentDate);

            foreach (ITemplateSkillDataPeriod templateSkillDataPeriod in skillDayTemplate.TemplateSkillDataPeriodCollection.OrderBy(t => t.Period.StartDateTime))
            {
                DateTimePeriod targetPeriod = templateSkillDataPeriod.Period.MovePeriod(diff);
                if (isFirstOrLastDayOfDaylightSavingTime)
                {
                    var localTimeToAdd = templateSkillDataPeriod.Period.StartDateTime.Subtract(utcBaseDate);
                    var localDateTime = CurrentDate.Date.Add(localTimeToAdd);
                    if (_skill.TimeZone.IsInvalidTime(localDateTime))
                    {
                        diff = diff.Add(TimeSpan.FromHours(-1));
                        isFirstOrLastDayOfDaylightSavingTime = false;
                    }
                    if (_skill.TimeZone.IsAmbiguousTime(localDateTime))
                    {
                        diff = diff.Add(TimeSpan.FromHours(1));
                        isFirstOrLastDayOfDaylightSavingTime = false;
                    }
                }
                var skillDataPeriods =
                    _skillDataPeriodCollection.Where(t => targetPeriod.Contains(t.Period.StartDateTime)).ToArray();
                int taskPeriodCount = skillDataPeriods.Length;
                if (taskPeriodCount > 1)
                {
                    MergeSkillDataPeriods(skillDataPeriods);
                    skillDataPeriods = _skillDataPeriodCollection.Where(t => targetPeriod.Contains(t.Period.StartDateTime)).ToArray();
                    taskPeriodCount = skillDataPeriods.Length;
                }
                if (taskPeriodCount == 0) continue;

                ISkillDataPeriod newSkillDataPeriod = skillDataPeriods[0];
                newSkillDataPeriod.SetParent(null);
                newSkillDataPeriod.ServiceAgreement = new ServiceAgreement(
                    templateSkillDataPeriod.ServiceAgreement.ServiceLevel,
                    templateSkillDataPeriod.ServiceAgreement.MinOccupancy,
                    templateSkillDataPeriod.ServiceAgreement.MaxOccupancy);
                newSkillDataPeriod.SkillPersonData = templateSkillDataPeriod.SkillPersonData;
                newSkillDataPeriod.SetParent(this);
                newSkillDataPeriod.Shrinkage = templateSkillDataPeriod.Shrinkage;
                newSkillDataPeriod.Efficiency = templateSkillDataPeriod.Efficiency;
                newSkillDataPeriod.ManualAgents = templateSkillDataPeriod.ManualAgents;
            }

            verifyAndAttachWorkloadDays(new List<IWorkloadDay>());

            Guid templateGuid = skillDayTemplate.Id.GetValueOrDefault();
            _templateReference = new SkillDayTemplateReference(templateGuid, skillDayTemplate.VersionNumber, skillDayTemplate.Name, skillDayTemplate.DayOfWeek, _skill)
                                 	{UpdatedDate = skillDayTemplate.UpdatedDate};
			initialize();
        }

        private bool isFirstDayOfDaylightSavingTime(DateOnly dateTime)
        {
            var date = dateTime.Date.AddHours(12);
            if (_skill.TimeZone.IsDaylightSavingTime(date))
            {
                if (!_skill.TimeZone.IsDaylightSavingTime(date.AddDays(-1)))
                {
                    return true;
                }
            }
            return false;
        }

        private bool isLastDayOfDaylightSavingTime(DateOnly dateTime)
        {
            if (_skill.TimeZone.IsDaylightSavingTime(dateTime.Date))
            {
                if (!_skill.TimeZone.IsDaylightSavingTime(dateTime.AddDays(1).Date))
                {
                    return true;
                }
            }
            return false;
        }

        private void aggregateTaskPeriods()
        {
            if (isChildSkill()) return;

            var taskPeriodCollection = skillTaskPeriodCollection();
            SkillDayStaffHelper.CombineTaskPeriodsAndServiceLevelPeriods(taskPeriodCollection,
                                                                         _skillDataPeriodCollection,
                                                                         _skillStaffPeriodCollection);

            if (SkillStaffPeriodCollection.Length==0)
            {
                if (_skill.SkillType.ForecastSource != ForecastSource.InboundTelephony &&
					_skill.SkillType.ForecastSource != ForecastSource.Retail)
                {
                    var nextOpenSkillDay = _skillDayCalculator.FindNextOpenDay(this);
	                nextOpenSkillDay?.RecalculateDailyTasks();
                }
            }
        }

        /// <summary>
        /// Creates from template.
        /// </summary>
        /// <param name="skillDayDate">The skill day date.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="skillDayTemplate">The skill day template.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-15
        /// </remarks>
        public virtual void CreateFromTemplate(DateOnly skillDayDate, ISkill skill, IScenario scenario, ISkillDayTemplate skillDayTemplate)
        {
            InParameter.NotNull(nameof(scenario), scenario);
            InParameter.NotNull(nameof(skill), skill);

            _currentDate = skillDayDate;
            _skill = skill;
            _scenario = scenario;
            setupSkillDay();
            ApplyTemplate(skillDayTemplate);
        }

        /// <summary>
        /// Merges the skill data periods.
        /// Only skill data periods owned by this skill day will be merged!
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        public virtual void MergeSkillDataPeriods(IList<ISkillDataPeriod> list)
        {
            ISkillDataPeriod newSkillDataPeriod = SkillDataPeriod.Merge(list, this);
            list.ForEach(i =>
	            {
		            _skillDataPeriodCollection.Remove(i);
	            });
            _skillDataPeriodCollection.Add(newSkillDataPeriod);
            aggregateTaskPeriods();
            recalculateStaff();
        }

        /// <summary>
        /// Occurs when [staff recalculated].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-06
        /// </remarks>
        public virtual event EventHandler<EventArgs> StaffRecalculated;
		
    	/// <summary>
        /// Recalculate staff. Use when periods/openhours have not been changed
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-25
        /// </remarks>
        public virtual void RecalculateStaff()
        {
            if (!_initialized)
            {
	            initialize();
            }
            else
            {
				recalculateStaff();
			}   
        }

        private void recalculateStaff()
        {
            var forthcomingSkillStaffPeriods = _skillDayCalculator.GetSkillStaffPeriodsForDayCalculation(this).ToArray();
			if (forthcomingSkillStaffPeriods.Length == 0)
			{
				triggerRecalculateEvent();
				return;
			}
            foreach (ISkillStaffPeriod skillStaffPeriod in SkillStaffPeriodCollection)
            {
                skillStaffPeriod.CalculateStaff(forthcomingSkillStaffPeriods);
            }
            triggerRecalculateEvent();
        }

        /// <summary>
        /// Combined task period collection.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-14
        /// </remarks>
        public virtual IList<ITemplateTaskPeriod> SkillTaskPeriodCollection()
        {
            return new List<ITemplateTaskPeriod>(skillTaskPeriodCollection());
        }

        private IEnumerable<ITemplateTaskPeriod> skillTaskPeriodCollection()
        {
            if (typeof(ChildSkill).IsInstanceOfType(_skill)) return new List<ITemplateTaskPeriod>();
            if (_skillDayCalculator == null) throw new InvalidOperationException("The skill day calculator must be set before this operation.");
            return _skillDayCalculator.CalculateTaskPeriods(this, _enableSpillover);
        }

        /// <summary>
        /// Splits the skill data periods.
        /// Only skill data periods owned by this skill day will be splitted!
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        public virtual void SplitSkillDataPeriods(IList<ISkillDataPeriod> list)
        {
            foreach (ISkillDataPeriod skillDataPeriod in list)
            {
                if (_skillDataPeriodCollection.Contains(skillDataPeriod))
                {
                    _skillDataPeriodCollection.Remove(skillDataPeriod);
                    TimeSpan resolutionAsTimeSpan = TimeSpan.FromMinutes(_skill.DefaultResolution);
                    for (DateTime t = skillDataPeriod.Period.StartDateTime; t < skillDataPeriod.Period.EndDateTime; t = t.Add(resolutionAsTimeSpan))
                    {
                        SkillDataPeriod newSkillDataPeriod = new SkillDataPeriod(
                            new ServiceAgreement(
                                new ServiceLevel(
                                    skillDataPeriod.ServiceLevelPercent,
                                    skillDataPeriod.ServiceLevelSeconds),
                                    skillDataPeriod.MinOccupancy,
                                    skillDataPeriod.MaxOccupancy),
                            new SkillPersonData(
                                skillDataPeriod.MinimumPersons,
                                skillDataPeriod.MaximumPersons),
                            new DateTimePeriod(t, t.Add(resolutionAsTimeSpan)));

                        newSkillDataPeriod.Shrinkage = skillDataPeriod.Shrinkage;
                        newSkillDataPeriod.Efficiency = skillDataPeriod.Efficiency;
                        newSkillDataPeriod.ManualAgents = skillDataPeriod.ManualAgents;
                        newSkillDataPeriod.SetParent(this);
                        _skillDataPeriodCollection.Add(newSkillDataPeriod);
                    }
                }
            }
            aggregateTaskPeriods();
            recalculateStaff();
        }

        private void recalculateDailyTasks()
        {
            if (!_turnOffInternalRecalc)
            {
                _turnOffInternalRecalc = true;
                _totalTasks = SkillStaffPeriodCollection.Sum(s => s.Payload.TaskData.Tasks);
                _turnOffInternalRecalc = false;

                //Inform parent about my changed value!
                onTasksChanged();
            }
            else
            {
                foreach (var parent in _parents)
                {
                    parent.SetDirty();
                }
                _isDirty = true;
            }
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-08
        /// </remarks>
        private void initialize()
        {
            _initialized = true;
            _skillDayCalculator.ClearSkillStaffPeriods();
            aggregateTaskPeriods();
            recalculateDailyTasks();
            recalculateDailyAverageTimes();
            recalculateDailyStatisticTasks();
            recalculateDailyAverageStatisticTimes();
            recalculateStaff();
        }

        private void onAverageTimesChanged()
        {
            foreach (ITaskOwner parent in _parents)
            {
                parent.RecalculateDailyTasks();
                parent.RecalculateDailyAverageTimes();
                parent.RecalculateDailyCampaignTasks();
                parent.RecalculateDailyAverageCampaignTimes();
            }
        }

        private void onTasksChanged()
        {
            foreach (ITaskOwner parent in _parents)
            {
                parent.RecalculateDailyTasks();
                parent.RecalculateDailyAverageTimes();
                parent.RecalculateDailyCampaignTasks();
                parent.RecalculateDailyAverageCampaignTimes();
            }
        }

        private void verifyAndAttachSkillDataPeriods(IEnumerable<ISkillDataPeriod> skillDataPeriodCollection)
        {
            if (skillDataPeriodCollection == null) return;
            foreach (ISkillDataPeriod skillDataPeriod in skillDataPeriodCollection)
            {
                skillDataPeriod.SetParent(this);
                _skillDataPeriodCollection.Add(skillDataPeriod);
            }
        }

        private void verifyAndAttachWorkloadDays(IEnumerable<IWorkloadDay> workloadDayCollection)
        {
            foreach (IWorkloadDay day in workloadDayCollection)
            {
                if (!day.Workload.Skill.Equals(_skill))
                    throw new ArgumentException("Incorrect skill found", nameof(workloadDayCollection));

                if (day.CurrentDate != _currentDate)
                    throw new ArgumentException("Incorrect date found", nameof(workloadDayCollection));

                day.AddParent(this);
                day.SetParent(this);
                _workloadDayCollection.Add(day);
                day.Initialize();
            }
        }

        private IEnumerable<IWorkloadDay> workloadDayOpenCollection()
        {
            return _workloadDayCollection.Where(wd => wd.OpenForWork.IsOpen).ToList();
        }


        /// <summary>
        /// Returns a ReadOnlyCollection with a projection of all  openhours for this skillday
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-04-23
        /// </remarks>
        public virtual IEnumerable<TimePeriod> OpenHours()
        {
            var totalOpenHours =
                from w in workloadDayOpenCollection()
                from o in w.OpenHourList
                select o;

            return TimePeriod.Combine(totalOpenHours.ToList());
        }

        public virtual void SetCalculatedStaffCollection(INewSkillStaffPeriodValues newSkillStaffPeriodValues)
        {
            newSkillStaffPeriodValues.SetValues(_skillStaffPeriodCollection);
            newSkillStaffPeriodValues.RunWhenBatchCompleted(calculateChildSkillDay);
        }

        private void calculateChildSkillDay(object state)
        {
            _availableSkillStaffPeriods.Clear();
            _initialized = true;
            _totalTasks = SkillStaffPeriodCollection.Sum(s => s.Payload.TaskData.Tasks);
            if (_totalTasks > 0d)
            {
                _totalAverageTaskTime = TimeSpan.FromTicks((long)
                    (SkillStaffPeriodCollection.Sum(s =>
                        s.Payload.TaskData.AverageTaskTime.Ticks * s.Payload.TaskData.Tasks) / _totalTasks));
                _totalAverageAfterTaskTime = TimeSpan.FromTicks((long)
                    (SkillStaffPeriodCollection.Sum(s =>
                        s.Payload.TaskData.AverageAfterTaskTime.Ticks * s.Payload.TaskData.Tasks) / _totalTasks));
            }
            else if (SkillStaffPeriodCollection.Length > 0)
            {
                _totalAverageTaskTime = TimeSpan.FromTicks((long)
                    (SkillStaffPeriodCollection.Average(s => s.Payload.TaskData.AverageTaskTime.Ticks)));
                _totalAverageAfterTaskTime = TimeSpan.FromTicks((long)
                    (SkillStaffPeriodCollection.Average(s => s.Payload.TaskData.AverageAfterTaskTime.Ticks)));
            }
            else
            {
                _totalAverageTaskTime = TimeSpan.FromSeconds(0);
                _totalAverageAfterTaskTime = TimeSpan.FromSeconds(0);
            }
            recalculateStaff();
            onTasksChanged();
            onAverageTimesChanged();
        }

        #endregion Methods

        #region ITaskOwner Members

        /// <summary>
        /// Gets or sets the average after task time.
        /// </summary>
        /// <value>The average after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        public virtual TimeSpan AverageAfterTaskTime
        {
            get
            {
                return TotalAverageAfterTaskTime;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets the average task time.
        /// </summary>
        /// <value>The average task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        public virtual TimeSpan AverageTaskTime
        {
            get
            {
                return TotalAverageTaskTime;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the calculated staff collection.
        /// </summary>
        /// <value>The calculated staff collection.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-22
        /// </remarks>
        public virtual ISkillStaffPeriod[] SkillStaffPeriodCollection
        {
            get
            {
                if (!_initialized) initialize();
                return createAvailableSkillStaffPeriodCollection().ToArray();
            }
        }

        private ISet<ISkillStaffPeriod> createAvailableSkillStaffPeriodCollection()
        {
            if (_availableSkillStaffPeriods.Count==0)
                _skillStaffPeriodCollection.ForEach(p => { if (p.IsAvailable) _availableSkillStaffPeriods.Add(p); });
            return _availableSkillStaffPeriods;
        }

        /// <summary>
        /// Recalcs the dayly average times.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public virtual void RecalculateDailyAverageTimes()
        {
            recalculateDailyAverageTimes();
        }

        private void recalculateDailyAverageTimes()
        {
            if (!_turnOffInternalRecalc)
            {
                _turnOffInternalRecalc = true;

                if (_totalTasks > 0d)
                {
                    _totalAverageTaskTime = TimeSpan.FromTicks((long)
                        (SkillStaffPeriodCollection.Sum(t => t.Payload.TaskData.AverageTaskTime.Ticks * t.Payload.TaskData.Tasks) / _totalTasks));
                    _totalAverageAfterTaskTime = TimeSpan.FromTicks((long)
                        (SkillStaffPeriodCollection.Sum(t => t.Payload.TaskData.AverageAfterTaskTime.Ticks * t.Payload.TaskData.Tasks) / _totalTasks));
                }
                else if (SkillStaffPeriodCollection.Length > 0)
                {
                    _totalAverageTaskTime = TimeSpan.FromTicks((long)
                        (SkillStaffPeriodCollection.Average(t => t.Payload.TaskData.AverageTaskTime.Ticks)));
                    _totalAverageAfterTaskTime = TimeSpan.FromTicks((long)
                        (SkillStaffPeriodCollection.Average(t => t.Payload.TaskData.AverageAfterTaskTime.Ticks)));
                }
                else
                {
                    _totalAverageTaskTime = TimeSpan.FromSeconds(0);
                    _totalAverageAfterTaskTime = TimeSpan.FromSeconds(0);
                }

                _turnOffInternalRecalc = false;

                //Inform parent about my changed values!
                onAverageTimesChanged();
                //OnAverageAfterTaskTimeChanged(); --> This does the same thing as the above!
            }
            else
            {
                _isDirty = true;
            }
        }

        /// <summary>
        /// Recalcs the daily tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public virtual void RecalculateDailyTasks()
        {
            if (!_initialized) initialize();
            if (!_turnOffInternalRecalc)
            {
                _availableSkillStaffPeriods.Clear();
                aggregateTaskPeriods();
                _availableSkillStaffPeriods.Clear();
                recalculateStaff();
                recalculateDailyTasks();
            }
            else
            {
                _isDirty = true;
            }
        }

        /// <summary>
        /// Resets the task owner.
        /// </summary>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-26
        /// </remarks>
        public virtual void ResetTaskOwner()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ITaskOwner Members	   

	    /// <summary>
        /// Gets the total average after task time.
        /// </summary>
        /// <value>The total average after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public virtual TimeSpan TotalAverageAfterTaskTime
        {
            get
            {
                if (!_initialized) initialize();
                return _totalAverageAfterTaskTime;
            }
        }

        /// <summary>
        /// Gets the total average task time.
        /// </summary>
        /// <value>The total average task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public virtual TimeSpan TotalAverageTaskTime
        {
            get
            {
                if (!_initialized) initialize();
                return _totalAverageTaskTime;
            }
        }

        /// <summary>
        /// Gets or sets the tasks.
        /// </summary>
        /// <value>The tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public virtual double Tasks
        {
            get
            {
                return TotalTasks;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets the campaign tasks.
        /// </summary>
        /// <value>The campaign tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public virtual Percent CampaignTasks
        {
            get
            {
                if (!_initialized) initialize();
                return new Percent(0);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets the campaign task time.
        /// </summary>
        /// <value>The campaign task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public virtual Percent CampaignTaskTime
        {
            get
            {
                if (!_initialized) initialize();
                return new Percent(0);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets the campaign after task time.
        /// </summary>
        /// <value>The campaign after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public virtual Percent CampaignAfterTaskTime
        {
            get
            {
                if (!_initialized) initialize();
                return new Percent(0);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Recalcs the daily campaign tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public virtual void RecalculateDailyCampaignTasks()
        {
        }

        /// <summary>
        /// Recalcs the daily average campaign times.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public virtual void RecalculateDailyAverageCampaignTimes()
        {
        }

        /// <summary>
        /// Locks this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-22
        /// </remarks>
        public virtual void Lock()
        {
            _turnOffInternalRecalc = true;
            foreach (ITaskOwner parent in _parents)
            {
                parent.Lock();
            }
        }

        /// <summary>
        /// Releases this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-22
        /// </remarks>
        public virtual void Release()
        {
            _turnOffInternalRecalc = false;
            if (_isDirty)
            {
                aggregateTaskPeriods();
                _availableSkillStaffPeriods.Clear();
                recalculateStaff();
                recalculateDailyTasks();
                recalculateDailyAverageTimes();
                recalculateDailyStatisticTasks();
                recalculateDailyAverageStatisticTimes();
                _isDirty = false;
            }
            foreach (ITaskOwner parent in _parents)
            {
                parent.Release();
            }
        }

        private void triggerRecalculateEvent()
		{
			StaffRecalculated?.Invoke(this, EventArgs.Empty);
		}

        /// <summary>
        /// Sets the entity as dirty.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-23
        /// </remarks>
        public virtual void SetDirty()
        {
            _isDirty = true;
        }

        public virtual OpenForWork OpenForWork
        {
            get
            {
                var isOpen = SkillStaffPeriodCollection.Length > 0;
	            return new OpenForWork(isOpen, isOpen);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is locked.
        /// </summary>
        /// <value><c>true</c> if this instance is locked; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public virtual bool IsLocked => _turnOffInternalRecalc;

		/// <summary>
        /// Gets the name of the template.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-14
        /// </remarks>
        public virtual ITemplateReference TemplateReference => _templateReference;

		/// <summary>
        /// Updates the name of the template.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-14
        /// </remarks>
        public virtual void ClearTemplateName()
        {
            // this really means that when the day is updated, you have broken the reference to the original template
            _templateReference = new SkillDayTemplateReference();
        }

        /// <summary>
        /// Removes the parent.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public virtual void RemoveParent(ITaskOwner parent)
        {
            if (_parents.Contains(parent))
                _parents.Remove(parent);
        }

        /// <summary>
        /// Gets the total statistic calculated tasks.
        /// </summary>
        /// <value>The total statistic calculated tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public virtual double TotalStatisticCalculatedTasks
        {
            get
            {
                if (!_initialized) initialize();
                return _totalStatisticCalculatedTasks;
            }
        }

        /// <summary>
        /// Gets the total statistic answered tasks.
        /// </summary>
        /// <value>The total statistic answered tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public virtual double TotalStatisticAnsweredTasks
        {
            get
            {
                if (!_initialized) initialize();
                return _totalStatisticAnsweredTasks;
            }
        }

        /// <summary>
        /// Gets the total statistic abandoned tasks.
        /// </summary>
        /// <value>The total statistic abandoned tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public virtual double TotalStatisticAbandonedTasks
        {
            get
            {
                if (!_initialized) initialize();
                return _totalStatisticAbandonedTasks;
            }
        }

        /// <summary>
        /// Gets the total statistic average task time.
        /// </summary>
        /// <value>The total statistic average task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public virtual TimeSpan TotalStatisticAverageTaskTime
        {
            get
            {
                if (!_initialized) initialize();
                return _totalStatisticAverageTaskTime;
            }
        }

        /// <summary>
        /// Gets the total statistic average after task time.
        /// </summary>
        /// <value>The total statistic average after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public virtual TimeSpan TotalStatisticAverageAfterTaskTime
        {
            get
            {
                if (!_initialized) initialize();
                return _totalStatisticAverageAfterTaskTime;
            }
        }

        /// <summary>
        /// Recalculates the daily average statistic times.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public virtual void RecalculateDailyAverageStatisticTimes()
        {
            if (!_initialized) initialize();
            recalculateDailyAverageStatisticTimes();
        }

        private void recalculateDailyAverageStatisticTimes()
        {
            if (!_turnOffInternalRecalc)
            {
                double sumTasks = _workloadDayCollection.Sum(t => t.TotalStatisticAnsweredTasks);
                if (sumTasks > 0d)
                {
                    _totalStatisticAverageTaskTime = TimeSpan.FromTicks((long)
                            (_workloadDayCollection.Sum(t => t.TotalStatisticAverageTaskTime.Ticks * t.TotalStatisticAnsweredTasks) / sumTasks));
                    _totalStatisticAverageAfterTaskTime = TimeSpan.FromTicks((long)
                        (_workloadDayCollection.Sum(t => t.TotalStatisticAverageAfterTaskTime.Ticks * t.TotalStatisticAnsweredTasks) / sumTasks));
                }
                else
                {
                    if (_workloadDayCollection.Count > 0)
                    {
                        _totalStatisticAverageTaskTime = TimeSpan.FromTicks((long)
                                (_workloadDayCollection.Average(t => t.TotalStatisticAverageTaskTime.Ticks)));
                        _totalStatisticAverageAfterTaskTime = TimeSpan.FromTicks((long)
                            (_workloadDayCollection.Average(t => t.TotalStatisticAverageAfterTaskTime.Ticks)));
                    }
                    else
                    {
                        _totalStatisticAverageTaskTime = TimeSpan.FromSeconds(0);
                        _totalStatisticAverageAfterTaskTime = TimeSpan.FromSeconds(0);
                    }
                }
            }
            else
            {
                _isDirty = true;
            }
        }

        /// <summary>
        /// Recalculates the daily statistic tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public virtual void RecalculateDailyStatisticTasks()
        {
            if (!_initialized) initialize();
            recalculateDailyStatisticTasks();
        }

        private void recalculateDailyStatisticTasks()
        {
            if (!_turnOffInternalRecalc)
            {
                _totalStatisticCalculatedTasks = _workloadDayCollection.Sum(t => t.TotalStatisticCalculatedTasks);
                _totalStatisticAnsweredTasks = _workloadDayCollection.Sum(t => t.TotalStatisticAnsweredTasks);
                _totalStatisticAbandonedTasks = _workloadDayCollection.Sum(t => t.TotalStatisticAbandonedTasks);
            }
            else
            {
                _isDirty = true;
            }
        }

        #endregion

        #region IRestrictionChecker<SkillDay> Members

        /// <summary>
        /// Checks the restrictions.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        public virtual void CheckRestrictions()
        {
            RestrictionSet.CheckEntity(this);
        }

        /// <summary>
        /// Gets the restriction set.
        /// </summary>
        /// <value>The restriction set.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        public virtual IRestrictionSet<ISkillDay> RestrictionSet
        {
            get { return SkillDayRestrictionSet.CurrentRestrictionSet; }
        }

        /// <summary>
        /// Gets or sets the skill day calculator.
        /// </summary>
        /// <value>The skill day calculator.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-02
        /// </remarks>
        public virtual ISkillDayCalculator SkillDayCalculator
        {
            get { return _skillDayCalculator; }
            set { _skillDayCalculator = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [enable spill over].
        /// </summary>
        /// <value><c>true</c> if [enable spill over]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-04
        /// </remarks>
        public virtual bool EnableSpillover
        {
            get { return _enableSpillover; }
            set { _enableSpillover = value; }
        }

        /// <summary>
        /// Gets the complete skill staff period collection.
        /// </summary>
        /// <value>The complete skill staff period collection.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-08
        /// </remarks>
        public virtual ISkillStaffPeriod[] CompleteSkillStaffPeriodCollection => _skillStaffPeriodCollection.ToArray();

		#endregion

        public virtual void SetupSkillDay()
        {
            setupSkillDay();
            foreach (IWorkloadDay day in _workloadDayCollection)
            {
                day.Initialize();
            }
        }

        private void setupSkillDay()
        {
            var interval = TimeSpan.FromMinutes(_skill.DefaultResolution);
            var startTimeLocal = CurrentDate.Date.Add(_skill.MidnightBreakOffset);
            var startDay = DaylightSavingTimeHelper.GetUtcStartTimeOfOneDay(startTimeLocal, _skill.TimeZone);
            var nextDay = DaylightSavingTimeHelper.GetUtcEndTimeOfOneDay(startTimeLocal, _skill.TimeZone);

            if (_skillDataPeriodCollection.Count == 0)
            {
                var period = new DateTimePeriod(startDay, startDay.Add(interval));
                while (period.StartDateTime < nextDay)
                {
                    var skillDataPeriod = new SkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(),
                                                              period);
                    skillDataPeriod.SetParent(this);
                    _skillDataPeriodCollection.Add(skillDataPeriod);
                    period = period.MovePeriod(interval);
                }
            }
            if (_skillStaffPeriodCollection.Count == 0)
            {

                var period = new DateTimePeriod(startDay, startDay.Add(interval));
                while (period.StartDateTime < nextDay)
                {
                    var skillStaffPeriod = new SkillStaffPeriod(period, new Task(), new ServiceAgreement());
                    skillStaffPeriod.SetSkillDay(this);
                    skillStaffPeriod.IsAvailable = false;
                    _skillStaffPeriodCollection.Add(skillStaffPeriod);
                    period = period.MovePeriod(interval);
                }
                foreach (IWorkloadDay day in _workloadDayCollection)
                {
                    day.AddParent(this);
                }
            }
        }

        public virtual object Clone()
        {
            return EntityClone();
        }

        public virtual ISkillDay NoneEntityClone()
        {
            return NoneEntityClone(Scenario);
        }

        /// <summary>
        /// Clones the Entity to another Scenario
        /// </summary>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-12-10
        /// </remarks>
        public virtual ISkillDay NoneEntityClone(IScenario scenario)
        {
            SkillDay newSkillDay = (SkillDay)Clone();
	        CloneEvents(newSkillDay);
			newSkillDay.SetId(null);
            newSkillDay.Scenario = scenario;
            newSkillDay._workloadDayCollection = new HashSet<IWorkloadDay>();
            foreach (WorkloadDay workloadDay in _workloadDayCollection)
            {
                newSkillDay.AddWorkloadDay(workloadDay.NoneEntityClone());
            }

            newSkillDay._skillStaffPeriodCollection = new HashSet<ISkillStaffPeriod>(_skillStaffPeriodCollection.ToArray()); //ToDo: need a cloned version.
            
            newSkillDay._availableSkillStaffPeriods = new HashSet<ISkillStaffPeriod>(); //Will be recreated

            newSkillDay._skillDataPeriodCollection = new HashSet<ISkillDataPeriod>(_skillDataPeriodCollection.Select(skillDataPeriod =>
			{
				ISkillDataPeriod newSkillDataPeriod = skillDataPeriod.NoneEntityClone();
				newSkillDataPeriod.SetParent(newSkillDay);
				return newSkillDataPeriod;
			}));
            
            return newSkillDay;
        }

        public virtual void SetNewSkillDataPeriodCollection(IList<ISkillDataPeriod> newSkillDataPeriods)
        {
            _skillDataPeriodCollection.Clear();
            foreach (ISkillDataPeriod skillDataPeriod in newSkillDataPeriods)
            {
                _skillDataPeriodCollection.Add(skillDataPeriod);
            }
        }

        public virtual void ResetSkillStaffPeriods(ISkillDataPeriod skillDataPeriod)
        {
            if (isChildSkill()) return;
            var skillStaffPeriods = SkillStaffPeriodCollection.Where(s => skillDataPeriod.Period.Contains(s.Period.StartDateTime));
            foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriods)
            {
                SkillStaff payload = (SkillStaff) skillStaffPeriod.Payload;
                payload.ServiceAgreementData = skillDataPeriod.ServiceAgreement;
                payload.Shrinkage = skillDataPeriod.Shrinkage;
                payload.Efficiency = skillDataPeriod.Efficiency;
                payload.ManualAgents = skillDataPeriod.ManualAgents;
	            payload.SkillPersonData = skillDataPeriod.SkillPersonData;
                payload.IsCalculated = false;
            }
        }

        private bool isChildSkill()
        {
            return _skill is IChildSkill;
        }

        public virtual ISkillDay EntityClone()
        {
            SkillDay newSkillDay = (SkillDay) MemberwiseClone();
            return newSkillDay;
        }

        public virtual ISkillStaffPeriodView[] SkillStaffPeriodViewCollection(TimeSpan periodLength, bool useShrinkage = false)
        {
            TimeSpan myPeriodLength = TimeSpan.Zero;
            if(SkillStaffPeriodCollection.Length > 0)
            {
                myPeriodLength = SkillStaffPeriodCollection[0].Period.ElapsedTime();
            }
	        if (myPeriodLength < periodLength) return new ISkillStaffPeriodView[0];
	        foreach (var skillStaffPeriod in SkillStaffPeriodCollection)
	        {
		        skillStaffPeriod.Payload.UseShrinkage = useShrinkage;
	        }
	        return SkillStaffPeriodCollection.SelectMany(period => period.Split(periodLength)).ToArray();
        }

		public virtual void OpenAllSkillStaffPeriods(int maxSeats)
		{
			foreach (var skillStaffPeriod in _skillStaffPeriodCollection)
			{
				skillStaffPeriod.IsAvailable = true;
				skillStaffPeriod.Payload.MaxSeats = maxSeats;
			}
		}

		public virtual DateTimePeriod Period => TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
			_currentDate.Date.Add(_skill.MidnightBreakOffset),
			_currentDate.AddDays(1).Date.Add(_skill.MidnightBreakOffset), _skill.TimeZone);

		public override void NotifyTransactionComplete(DomainUpdateType operation)
	    {
		    base.NotifyTransactionComplete(operation);

		    switch (operation)
		    {
			    case DomainUpdateType.Insert:
			    case DomainUpdateType.Update:
			    case DomainUpdateType.Delete:
				    AddEvent(new SkillDayChangedEvent
				    {
					    SkillDayId = Id.GetValueOrDefault()
				    });
				    break;
		    }
	    }
    }
}
