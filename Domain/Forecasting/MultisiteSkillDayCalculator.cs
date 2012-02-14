using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class MultisiteSkillDayCalculator : SkillDayCalculator
    {
        private readonly IMultisiteSkill _multisiteSkill;
        private readonly IList<IMultisiteDay> _multisiteDays;
        private readonly IDictionary<IChildSkill, IList<ISkillDay>> _childSkillDays = new Dictionary<IChildSkill, IList<ISkillDay>>();
        private readonly IDictionary<IChildSkill, IDictionary<DateTime, ISkillStaffPeriod>> _childSkillStaffPeriods = new Dictionary<IChildSkill, IDictionary<DateTime, ISkillStaffPeriod>>();

        public MultisiteSkillDayCalculator(IMultisiteSkill multisiteSkill, IList<ISkillDay> multisiteSkillDays, IList<IMultisiteDay> multisiteDays, DateOnlyPeriod visiblePeriod)
            : base(multisiteSkill,multisiteSkillDays,visiblePeriod)
        {
            _multisiteSkill = multisiteSkill;
            _multisiteDays = multisiteDays;
        }

        public IMultisiteSkill MultisiteSkill
        {
            get { return _multisiteSkill; }
        }

        public ReadOnlyCollection<IMultisiteDay> MultisiteDays
        {
            get { return new ReadOnlyCollection<IMultisiteDay>(_multisiteDays); }
        }

        public ReadOnlyCollection<IMultisiteDay> VisibleMultisiteDays
        {
            get { return new ReadOnlyCollection<IMultisiteDay>(_multisiteDays.Where(m => VisiblePeriod.Contains(m.MultisiteDayDate)).ToList()); }
        }

        public IDictionary<IChildSkill, IDictionary<DateTime, ISkillStaffPeriod>> ChildSkillStaffPeriods
        {
            get { return new ReadOnlyDictionary<IChildSkill, IDictionary<DateTime, ISkillStaffPeriod>>(_childSkillStaffPeriods); }
        }

        public void SetChildSkillDays(IChildSkill childSkill, IList<ISkillDay> childSkillDays)
        {
            if (!_multisiteSkill.ChildSkills.Contains(childSkill)) throw new ArgumentException("The supplied child skill is not contained in this multisite skill.","childSkill");
            if (_childSkillDays.ContainsKey(childSkill)) _childSkillDays.Remove(childSkill);
            foreach (ISkillDay childSkillDay in childSkillDays)
            {
                childSkillDay.SkillDayCalculator = this;
            }
            _childSkillDays.Add(childSkill, childSkillDays);
        }

        public IList<ISkillDay> GetVisibleChildSkillDays(IChildSkill childSkill)
        {
            IList<ISkillDay> skillDaysForChildSkill;
            if (_childSkillDays.TryGetValue(childSkill, out skillDaysForChildSkill))
                return skillDaysForChildSkill.Where(s => VisiblePeriod.Contains(s.CurrentDate)).ToList();

            throw new ArgumentException("The supplied child skill has not been setup properly. Run SetChildSkillDays first.");
        }

        public void InitializeChildSkills()
        {
            foreach(IChildSkill childSkill in _multisiteSkill.ChildSkills)
            {
                if (!_childSkillDays.ContainsKey(childSkill)) throw new InvalidOperationException("All child skill days must be initialized before this call.");
            }
            foreach (IMultisiteDay multisiteDay in _multisiteDays)
            {
                var multisiteDayDate = multisiteDay.MultisiteDayDate;
                ISkillDay parentSkillDay = SkillDays.FirstOrDefault(ms => ms.CurrentDate == multisiteDayDate);

                if (parentSkillDay != null)
                {
                    multisiteDay.MultisiteSkillDay = parentSkillDay;
                    multisiteDay.SetChildSkillDays(from c in _childSkillDays.Values
                                                   from s in c
                                                   where multisiteDayDate == s.CurrentDate
                                                   select s);
                	multisiteDay.RedistributeChilds();
                }
            }
        }

        public override void CheckRestrictions()
        {
            base.CheckRestrictions();
            _multisiteDays.ForEach(m => m.CheckRestrictions());

            foreach (var skillDayList in _childSkillDays.Values)
            {
                skillDayList.ForEach(m => m.CheckRestrictions());
            }
        }

        public override IEnumerable<ISkillStaffPeriod> GetSkillStaffPeriodsForDayCalculation(ISkillDay skillDay)
        {
            var result = base.GetSkillStaffPeriodsForDayCalculation(skillDay).ToList();
            IChildSkill childSkill = skillDay.Skill as IChildSkill;
            if (childSkill != null)
            {
                if (_multisiteSkill.ChildSkills.Contains(childSkill))
                {
                    IDictionary<DateTime, ISkillStaffPeriod> foundSkillStaffPeriods;
                    if (!_childSkillStaffPeriods.TryGetValue(childSkill, out foundSkillStaffPeriods))
                    {
                        foundSkillStaffPeriods = new Dictionary<DateTime, ISkillStaffPeriod>(new DateTimeAsLongEqualityComparer());
                        foreach (var currentSkillDay in _childSkillDays[childSkill])
                        {
                            foreach (ISkillStaffPeriod skillStaffPeriod in currentSkillDay.CompleteSkillStaffPeriodCollection)
                            {
                                foundSkillStaffPeriods.Add(skillStaffPeriod.Period.StartDateTime, skillStaffPeriod);
                            }
                        }
                        _childSkillStaffPeriods.Add(childSkill, foundSkillStaffPeriods);
                    }

                    var skillResolution = _multisiteSkill.DefaultResolution;
                    var endDateTime = skillDay.CurrentDate.AddDays(10).Date;
                    for (var currentDateTime = TimeZoneHelper.ConvertToUtc(skillDay.CurrentDate.Date,_multisiteSkill.TimeZone); currentDateTime<endDateTime; currentDateTime = currentDateTime.AddMinutes(skillResolution))
                    {
                        ISkillStaffPeriod foundSkillStaffPeriod;
                        if (!foundSkillStaffPeriods.TryGetValue(currentDateTime, out foundSkillStaffPeriod))
                        {
                            break;
                        }
                        if (!foundSkillStaffPeriod.IsAvailable) continue;

                        result.Add(foundSkillStaffPeriod);
                    }
                }
            }
            return result;
        }

        public override void ClearSkillStaffPeriods()
        {
            base.ClearSkillStaffPeriods();
            _childSkillStaffPeriods.Clear();
        }

        public override Percent GetPercentageForInterval(ISkill skill, DateTimePeriod period)
        {
            if (_multisiteSkill.Equals(skill)) return new Percent(1);
            IChildSkill childSkill = skill as IChildSkill;
            if (childSkill==null) throw new ArgumentException("The supplied skill is not a child skill nor the parent multisite skill.");
            if (!_multisiteSkill.ChildSkills.Contains(childSkill)) throw new ArgumentException("The supplied child skill does not have the current multisite skill as parent.");

            DateTime localDateTime = TimeZoneHelper.ConvertFromUtc(period.StartDateTime, skill.TimeZone);
            IMultisiteDay currentDay = _multisiteDays.FirstOrDefault(m => m.MultisiteDayDate.Date == localDateTime.Date);
            if (currentDay == null) return new Percent();

            IMultisitePeriod currentPeriod =
                currentDay.MultisitePeriodCollection.First(p => p.Period.Contains(period.StartDateTime));

            return currentPeriod.Distribution[childSkill];
        }

        /// <summary>
        /// Summarizes the staffing for multisite skill days. Used when you have calculated resources and calculated logged on for sub skill to visualize staffing for complete multisite skill.
        /// </summary>
        /// <param name="multisiteSkill">The multisite skill.</param>
        /// <param name="skillDays">The skill days.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-23
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static void SummarizeStaffingForMultisiteSkillDays(IMultisiteSkill multisiteSkill, IDictionary<ISkill,IList<ISkillDay>> skillDays)
        {
            if (multisiteSkill == null) return;

            int intervalCount = (int)TimeSpan.FromDays(1).TotalMinutes / multisiteSkill.DefaultResolution;

            IDictionary<int, StaffingResource> calculatedResources = new Dictionary<int, StaffingResource>(intervalCount);
            foreach (ISkillDay skillDay in skillDays[multisiteSkill])
            {
                foreach (IChildSkill childSkill in multisiteSkill.ChildSkills)
                {
                    var currentDate = skillDay.CurrentDate;
                    ISkillDay childSkillDay =
                        skillDays[childSkill].FirstOrDefault(s => s.CurrentDate == currentDate);
                    if (childSkillDay==null) continue;

                    for (int childIndex = 0; childIndex < childSkillDay.CompleteSkillStaffPeriodCollection.Count; childIndex++)
                    {
                        StaffingResource staffingResource;
                        if (!calculatedResources.TryGetValue(childIndex,out staffingResource))
                        {
                            staffingResource = new StaffingResource();
                            calculatedResources.Add(childIndex,staffingResource);
                        }

                        staffingResource.CalculatedResource +=
                            childSkillDay.CompleteSkillStaffPeriodCollection[childIndex].Payload.CalculatedResource;
                        staffingResource.CalculatedLoggedOn +=
                            childSkillDay.CompleteSkillStaffPeriodCollection[childIndex].Payload.CalculatedLoggedOn;
                    }
                }
                for (int i = 0; i < calculatedResources.Count; i++)
                {
                    StaffingResource staffingResource;
                    if (calculatedResources.TryGetValue(i,out staffingResource))
                    {
                        skillDay.CompleteSkillStaffPeriodCollection[i].SetCalculatedResource65(
                            staffingResource.CalculatedResource);
                        skillDay.CompleteSkillStaffPeriodCollection[i].Payload.CalculatedLoggedOn =
                            staffingResource.CalculatedLoggedOn;
                    }
                }
                calculatedResources.Clear();
            }
        }

        public override ISkillDayCalculator CloneToScenario(IScenario scenario)
        {
            IList<ISkillDay> newSkillDays = new List<ISkillDay>();
            foreach (ISkillDay skillDay in SkillDays)
            {
                ISkillDay newSkillDay = skillDay.NoneEntityClone(scenario);
                newSkillDays.Add(newSkillDay);
            }
            IList<IMultisiteDay> newMultisiteDays = new List<IMultisiteDay>();
            foreach (IMultisiteDay multisiteDay in _multisiteDays)
            {
                IMultisiteDay newMultisiteDay = multisiteDay.NoneEntityClone(scenario);
                newMultisiteDays.Add(newMultisiteDay);
            }
            MultisiteSkillDayCalculator multisiteSkillDayCalculator = new MultisiteSkillDayCalculator(_multisiteSkill, newSkillDays, newMultisiteDays, VisiblePeriod);
            foreach (var childSkillDayPair in _childSkillDays)
            {
                IList<ISkillDay> newChildSkillDays = new List<ISkillDay>();
                foreach (var skillDay in childSkillDayPair.Value)
                {
                    ISkillDay newSkillDay = skillDay.NoneEntityClone(scenario);
                    newChildSkillDays.Add(newSkillDay);
                }
                multisiteSkillDayCalculator._childSkillDays.Add(childSkillDayPair.Key,newChildSkillDays);
            }
            multisiteSkillDayCalculator.InitializeChildSkills();
            return multisiteSkillDayCalculator;
        }

        private class StaffingResource
        {
            public double CalculatedResource { get; set; }

            public double CalculatedLoggedOn { get; set; }
        }
    }
}