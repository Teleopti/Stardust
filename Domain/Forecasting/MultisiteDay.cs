using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Used to represent multi site skill day data
    /// </summary>
    /// <remarks>
    /// Created by: micke
    /// Created date: 18.12.2007
    /// </remarks>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class MultisiteDay : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IMultisiteDay
    {
        private ISet<IMultisitePeriod> _multisitePeriodCollection = new HashSet<IMultisitePeriod>();
        private TemplateReference _templateReference = new MultisiteDayTemplateReference();
        private DateOnly _multisiteDayDate;
        private IMultisiteSkill _skill;
        private IScenario _scenario;
        private IList<ISkillDay> _childSkillDays = new List<ISkillDay>();
        private ISkillDay _multisiteSkillDay;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultisiteDay"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-08
        /// </remarks>
        public MultisiteDay() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultisiteDay"/> class.
        /// </summary>
        /// <param name="multisiteDayDate">The multisite day date.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-12
        /// </remarks>
        public MultisiteDay(DateOnly multisiteDayDate, IMultisiteSkill skill, IScenario scenario)
            :this()
        {
            InParameter.NotNull(nameof(scenario), scenario);
            InParameter.NotNull(nameof(skill), skill);
            
            _multisiteDayDate = multisiteDayDate;
            _skill = skill;
            _scenario = scenario;
            ((MultisiteDayTemplateReference)_templateReference).MultisiteSkill = _skill;
        }

        /// <summary>
        /// Creates from template.
        /// </summary>
        /// <param name="multisiteDayDate">The multisite day date.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="multisiteDayTemplate">The multisite day template.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        public virtual void CreateFromTemplate(DateOnly multisiteDayDate, IMultisiteSkill skill, IScenario scenario, IMultisiteDayTemplate multisiteDayTemplate)
        {
            InParameter.NotNull(nameof(scenario), scenario);
            InParameter.NotNull(nameof(skill), skill);

            _multisiteDayDate = multisiteDayDate;
            _skill = skill;
            _scenario = scenario;
            ((MultisiteDayTemplateReference) _templateReference).MultisiteSkill = _skill;

            ApplyTemplate(multisiteDayTemplate);
        }

        /// <summary>
        /// Applies the template.
        /// </summary>
        /// <param name="templateDay">The template day.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        public virtual void ApplyTemplate(IMultisiteDayTemplate templateDay)
        {
            InParameter.NotNull(nameof(templateDay), templateDay);

            //Copy the TemplateMultisitePeriod to the MultisiteDay
            TimeZoneInfo raptorTimeZoneInfo = _skill.TimeZone;
            IList<IMultisitePeriod> multisitePeriods = new List<IMultisitePeriod>();
            foreach (ITemplateMultisitePeriod multisitePeriod in templateDay.TemplateMultisitePeriodCollection)
            {
	            var localStartDateTime = TimeZoneHelper.ConvertFromUtc(multisitePeriod.Period.StartDateTime, raptorTimeZoneInfo);
				var localEndDateTime = TimeZoneHelper.ConvertFromUtc(multisitePeriod.Period.EndDateTime, raptorTimeZoneInfo);
				var start = TimeZoneHelper.ConvertToUtc(_multisiteDayDate.Date.Add(localStartDateTime.TimeOfDay), raptorTimeZoneInfo);
	            var end =
		            TimeZoneHelper.ConvertToUtc(
						_multisiteDayDate.Date.Add(localEndDateTime.Date > localStartDateTime.Date
				            ? new TimeSpan(1, 0, 0, 0)
							: localEndDateTime.TimeOfDay)
			            , raptorTimeZoneInfo);

				if(end<=start)
					continue;

                IMultisitePeriod newMultisitePeriod = new MultisitePeriod(
                    new DateTimePeriod(start,end),
                    new Dictionary<IChildSkill, Percent>(multisitePeriod.Distribution));
                multisitePeriods.Add(newMultisitePeriod);
            }
            _multisitePeriodCollection.Clear();
            VerifyAndAttachMultisitePeriods(multisitePeriods);

            Guid templateGuid = templateDay.Id.GetValueOrDefault();
            _templateReference = new MultisiteDayTemplateReference(templateGuid, templateDay.VersionNumber, templateDay.Name, templateDay.DayOfWeek, _skill)
                                 	{UpdatedDate = templateDay.UpdatedDate};

			RedistributeChilds();
        }
		
        /// <summary>
        /// Gets the skill data period collection.
        /// </summary>
        /// <value>The skill data period collection.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-30
        /// </remarks>
        public virtual ReadOnlyCollection<IMultisitePeriod> MultisitePeriodCollection => new ReadOnlyCollection<IMultisitePeriod>(_multisitePeriodCollection.ToArray());

	    private void VerifyAndAttachMultisitePeriods(IEnumerable<IMultisitePeriod> multisitePeriodCollection)
        {
            foreach (IMultisitePeriod multisitePeriod in multisitePeriodCollection)
            {
	            if (_multisitePeriodCollection.Any(t => t.Period.StartDateTime == multisitePeriod.Period.StartDateTime))
		            throw new InvalidOperationException("The multisite periods must have unique start times");
	            multisitePeriod.SetParent(this);
                _multisitePeriodCollection.Add(multisitePeriod);
            }
        }

        /// <summary>
        /// Sets the multisite period collection.
        /// </summary>
        /// <param name="periods">The periods.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-15
        /// </remarks>
        public virtual void SetMultisitePeriodCollection(IList<IMultisitePeriod> periods)
        {
            if(periods!=null)
            {
                _multisitePeriodCollection.Clear();
                VerifyAndAttachMultisitePeriods(periods);
            }
        }

        /// <summary>
        /// Gets the multisite day date.
        /// </summary>
        /// <value>The multisite day date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        public virtual DateOnly MultisiteDayDate => _multisiteDayDate;

	    /// <summary>
        /// Gets the skill.
        /// </summary>
        /// <value>The skill.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        public virtual IMultisiteSkill Skill => _skill;

	    /// <summary>
        /// Gets the scenario.
        /// </summary>
        /// <value>The scenario.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        public virtual IScenario Scenario => _scenario;

	    /// <summary>
        /// Gets the template reference.
        /// </summary>
        /// <value>The template reference.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-09
        /// </remarks>
        public virtual ITemplateReference TemplateReference => _templateReference;

	    /// <summary>
        /// Updates the name of the template.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-14
        /// </remarks>
        public virtual void UpdateTemplateName()
        {
            _templateReference = new MultisiteDayTemplateReference();
        }

        /// <summary>
        /// Splits the multisite periods.
        /// Only template multisite periods owned by this multisite day will be splitted!
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        public virtual void SplitMultisitePeriods(IList<IMultisitePeriod> list)
        {
            foreach (IMultisitePeriod multisitePeriod in list)
            {
                if (_multisitePeriodCollection.Contains(multisitePeriod))
                {
                    _multisitePeriodCollection.Remove(multisitePeriod);
                    TimeSpan resolutionAsTimeSpan = TimeSpan.FromMinutes(_skill.DefaultResolution);
                    for (DateTime t = multisitePeriod.Period.StartDateTime; t < multisitePeriod.Period.EndDateTime; t = t.Add(resolutionAsTimeSpan))
                    {
                        IMultisitePeriod newMultisitePeriod = new MultisitePeriod(
                            new DateTimePeriod(t, t.Add(resolutionAsTimeSpan)),
                            new Dictionary<IChildSkill, Percent>(multisitePeriod.Distribution));
                        newMultisitePeriod.SetParent(this);
                        _multisitePeriodCollection.Add(newMultisitePeriod);
                    }
                }
            }
        }

        /// <summary>
        /// Merges the multisite periods.
        /// Only multisite periods owned by this multisite day will be merged!
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        public virtual void MergeMultisitePeriods(IList<IMultisitePeriod> list)
        {
            list = list.OrderBy(s => s.Period.StartDateTime).ToList();
            IMultisitePeriod newMultisitePeriod = new MultisitePeriod(
                            new DateTimePeriod(list[0].Period.StartDateTime, list.Last().Period.EndDateTime),
                            new Dictionary<IChildSkill, Percent>(list[0].Distribution));
            newMultisitePeriod.SetParent(this);
            list.ForEach(i => _multisitePeriodCollection.Remove(i));
            _multisitePeriodCollection.Add(newMultisitePeriod);
        }

        #region IRestrictionChecker<IMultisiteDay> Members

        /// <summary>
        /// Gets the restriction set.
        /// </summary>
        /// <value>The restriction set.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        public virtual IRestrictionSet<IMultisiteDay> RestrictionSet
        {
            get { return MultisiteDayRestrictionSet.CurrentRestrictionSet; }
        }

        #endregion

        #region IRestrictionChecker Members

        /// <summary>
        /// Checks the restrictions.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        public virtual void CheckRestrictions()
        {
            RestrictionSet.CheckEntity(this);
        }

        #endregion

        /// <summary>
        /// Sets the child skill days.
        /// </summary>
        /// <param name="childSkillDays">The child skill days.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-29
        /// </remarks>
        public virtual void SetChildSkillDays(IEnumerable<ISkillDay> childSkillDays)
        {
            _childSkillDays = new List<ISkillDay>(childSkillDays);
        }

        /// <summary>
        /// Gets the child skill days.
        /// </summary>
        /// <value>The child skill days.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-29
        /// </remarks>
        public virtual IList<ISkillDay> ChildSkillDays => _childSkillDays;

	    /// <summary>
        /// Gets or sets the multisite skill day.
        /// </summary>
        /// <value>The multisite skill day.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-29
        /// </remarks>
        public virtual ISkillDay MultisiteSkillDay
        {
            get { return _multisiteSkillDay; }
            set {
                if (_multisiteSkillDay != null) _multisiteSkillDay.StaffRecalculated -= _multisiteSkillDay_StaffRecalculated;
                _multisiteSkillDay = value;
                if (_multisiteSkillDay != null) _multisiteSkillDay.StaffRecalculated += _multisiteSkillDay_StaffRecalculated;
            }
        }
		
        private void _multisiteSkillDay_StaffRecalculated(object sender, EventArgs e)
        {
            RedistributeChilds();
        }

        /// <summary>
        /// Redistributes the childs.
        /// The skill day must already gone through CalculateStaff method!
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-29
        /// </remarks>
        public virtual void RedistributeChilds()
        {
            if (_multisiteSkillDay == null) return;
            if (_childSkillDays.Count == 0) return;

            IList<ISkillStaffPeriod> splittedPeriods = new List<ISkillStaffPeriod>(
                _multisiteSkillDay.SkillStaffPeriodCollection);

            splittedPeriods = SkillDayStaffHelper.CombineSkillStaffPeriodsAndMultisitePeriods(
                splittedPeriods, _multisitePeriodCollection);

            var matchedContents = from m in _multisitePeriodCollection
                                  from s in splittedPeriods
                                  where m.Period.StartDateTime < s.Period.EndDateTime &&
                                        m.Period.EndDateTime > s.Period.StartDateTime
                                  orderby s.Period.StartDateTime
                                  select new {SkillStaffPeriod = s, MultisitePeriod = m};

            IDictionary<ISkillDay, IList<ISkillStaffPeriod>> skillDayContent =
                new Dictionary<ISkillDay, IList<ISkillStaffPeriod>>();
            foreach (var content in matchedContents)
            {
                foreach (ISkillDay skillDay in _childSkillDays)
                {
                    ISkillDataPeriod period = null;
                    foreach (var skillDataPeriod in skillDay.SkillDataPeriodCollection.OrderBy(p => p.Period.StartDateTime))
                    {
                        if (skillDataPeriod.Period.StartDateTime>content.SkillStaffPeriod.Period.EndDateTime) break;
                        if (skillDataPeriod.Period.EndDateTime<=content.SkillStaffPeriod.Period.StartDateTime) continue;

                        period = skillDataPeriod;
                        break;
                    }
                    if (period==null) continue;

					if (!content.MultisitePeriod.Distribution.TryGetValue((IChildSkill) skillDay.Skill, out var factor)) continue;
                    
                    double tasks = content.SkillStaffPeriod.Payload.TaskData.Tasks*factor.Value;

					if (!skillDayContent.TryGetValue(skillDay, out var skillDayStaffPeriods))
                    {
                        skillDayStaffPeriods = new List<ISkillStaffPeriod>();
                        skillDayContent.Add(skillDay, skillDayStaffPeriods);
                    }

                    ISkillStaffPeriod skillStaffPeriod = new SkillStaffPeriod(content.SkillStaffPeriod.Period,
                                                                              new Task(
                                                                                  tasks,
                                                                                  content.SkillStaffPeriod.
                                                                                      Payload.
                                                                                      TaskData.AverageTaskTime,
                                                                                  content.SkillStaffPeriod.
                                                                                      Payload.
                                                                                      TaskData.
                                                                                      AverageAfterTaskTime),
                                                                              new ServiceAgreement(
                                                                                  content.SkillStaffPeriod.
                                                                                      Payload.
                                                                                      ServiceAgreementData.
                                                                                      ServiceLevel,
                                                                                  new Percent(
                                                                                      content.SkillStaffPeriod.
                                                                                          Payload.
                                                                                          CalculatedOccupancy),
                                                                                  new Percent(
                                                                                      content.SkillStaffPeriod.
                                                                                          Payload.
                                                                                          CalculatedOccupancy)));

					var multiSiteStaffPeriod =
						_multisiteSkillDay.SkillStaffPeriodCollection.FirstOrDefault(x =>
							x.Period == content.SkillStaffPeriod.Period);
					var manualAgents = multiSiteStaffPeriod?.Payload.ManualAgents;
					if (manualAgents.HasValue)
					{
						skillStaffPeriod.Payload.ManualAgents = manualAgents * factor.Value;
					}

					skillStaffPeriod.Payload.Shrinkage = content.SkillStaffPeriod.Payload.Shrinkage;
                    skillStaffPeriod.Payload.Efficiency = content.SkillStaffPeriod.Payload.Efficiency;
                    skillStaffPeriod.Payload.SkillPersonData = period.SkillPersonData;
                    skillStaffPeriod.IsAvailable = true;
                    skillDayStaffPeriods.Add(skillStaffPeriod);
                }

            }

            _multisiteSkillDay.SkillDayCalculator.ClearSkillStaffPeriods();
            var batchContents = new List<INewSkillStaffPeriodValues>();
            skillDayContent.ForEach(i =>
                                        {
                                            var newSkillStaffPeriodValues = new NewSkillStaffPeriodValues(i.Value);
                                            i.Key.SetCalculatedStaffCollection(newSkillStaffPeriodValues);
                                            batchContents.Add(newSkillStaffPeriodValues);
                                        });
            batchContents.ForEach(b => b.BatchCompleted());

        	triggerValueChangedEvent();
        }

        public virtual IMultisiteDay NoneEntityClone(IScenario scenario)
        {
            MultisiteDay retObj = (MultisiteDay) NoneEntityClone();
	        CloneEvents(retObj);
			retObj._scenario = scenario;
            return retObj;
        }

		public virtual event EventHandler<EventArgs> ValueChanged;

		private void triggerValueChangedEvent()
		{
			ValueChanged?.Invoke(this, EventArgs.Empty);
		}

    	public virtual object Clone()
        {
            return NoneEntityClone();
        }

        public virtual IMultisiteDay NoneEntityClone()
        {
            MultisiteDay retobj = (MultisiteDay)MemberwiseClone();
	        CloneEvents(retobj);
			retobj.SetId(null);
            retobj._multisitePeriodCollection = new HashSet<IMultisitePeriod>();
            foreach (IMultisitePeriod multisitePeriod in _multisitePeriodCollection)
            {
                IMultisitePeriod clonedPeriod = multisitePeriod.NoneEntityClone();
                clonedPeriod.SetParent(retobj);
                retobj._multisitePeriodCollection.Add(clonedPeriod);
            }
            retobj._childSkillDays = new List<ISkillDay>();
            foreach (ISkillDay childSkillDay in _childSkillDays)
            {
                retobj._childSkillDays.Add(childSkillDay);
            }
            return retobj;
        }

        public virtual IMultisiteDay EntityClone()
        {
            MultisiteDay retobj = (MultisiteDay)MemberwiseClone();
	        CloneEvents(retobj);
			retobj._multisitePeriodCollection = new HashSet<IMultisitePeriod>();
            foreach (IMultisitePeriod multisitePeriod in _multisitePeriodCollection)
            {
                IMultisitePeriod clonedPeriod = multisitePeriod.EntityClone();
                clonedPeriod.SetParent(retobj);
                retobj._multisitePeriodCollection.Add(clonedPeriod);
            }
            retobj._childSkillDays = new List<ISkillDay>();
            foreach (ISkillDay childSkillDay in _childSkillDays)
            {
                retobj._childSkillDays.Add(childSkillDay);
            }
            return retobj;
        }
    }
}
