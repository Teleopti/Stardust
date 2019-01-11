using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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

        public IMultisiteSkill MultisiteSkill => _multisiteSkill;

	    public IMultisiteDay[] MultisiteDays => _multisiteDays.ToArray();

	    public IMultisiteDay[] VisibleMultisiteDays => _multisiteDays.Where(m => VisiblePeriod.Contains(m.MultisiteDayDate)).ToArray();

		public IDictionary<IChildSkill, IDictionary<DateTime, ISkillStaffPeriod>> ChildSkillStaffPeriods => new ReadOnlyDictionary<IChildSkill, IDictionary<DateTime, ISkillStaffPeriod>>(_childSkillStaffPeriods);

	    public void SetChildSkillDays(IChildSkill childSkill, IList<ISkillDay> childSkillDays)
        {
            if (!_multisiteSkill.ChildSkills.Contains(childSkill)) throw new ArgumentException("The supplied child skill is not contained in this multisite skill.",nameof(childSkill));
            _childSkillDays.Remove(childSkill);
            foreach (var childSkillDay in childSkillDays)
            {
                childSkillDay.SkillDayCalculator = this;
            }
            _childSkillDays.Add(childSkill, childSkillDays);
        }

        public IList<ISkillDay> GetVisibleChildSkillDays(IChildSkill childSkill)
        {
			if (_childSkillDays.TryGetValue(childSkill, out var skillDaysForChildSkill))
                return skillDaysForChildSkill.Where(s => VisiblePeriod.Contains(s.CurrentDate)).ToList();

            throw new ArgumentException("The supplied child skill has not been setup properly. Run SetChildSkillDays first.");
        }

        public void InitializeChildSkills()
        {
            if (_multisiteSkill.ChildSkills.Any(childSkill => !_childSkillDays.ContainsKey(childSkill)))
            {
                throw new InvalidOperationException("All child skill days must be initialized before this call.");
            }
			var days = SkillDays.ToDictionary(ms => ms.CurrentDate);
			var childDays = _childSkillDays.Values.SelectMany(s => s).ToLookup(s => s.CurrentDate);
			foreach (var multisiteDay in _multisiteDays)
            {
                var multisiteDayDate = multisiteDay.MultisiteDayDate;
                if (!days.TryGetValue(multisiteDayDate,out ISkillDay parentSkillDay)) continue;
				
                multisiteDay.MultisiteSkillDay = parentSkillDay;
                multisiteDay.SetChildSkillDays(childDays[multisiteDayDate]);
                multisiteDay.RedistributeChilds();
            }
        }

        public override void CheckRestrictions()
        {
            base.CheckRestrictions();
            _multisiteDays.ForEach(m => m.CheckRestrictions());
            _childSkillDays.Values.SelectMany(s => s).ForEach(m => m.CheckRestrictions());
        }

        public override IEnumerable<ISkillStaffPeriod> GetSkillStaffPeriodsForDayCalculation(ISkillDay skillDay)
        {
            var result = base.GetSkillStaffPeriodsForDayCalculation(skillDay).ToList();
			if (skillDay.Skill is IChildSkill childSkill)
            {
                if (_multisiteSkill.ChildSkills.Contains(childSkill))
                {
					if (!_childSkillStaffPeriods.TryGetValue(childSkill, out var foundSkillStaffPeriods))
					{
						foundSkillStaffPeriods = _childSkillDays[childSkill].SelectMany(
								currentSkillDay => currentSkillDay.CompleteSkillStaffPeriodCollection)
							.ToDictionary(skillStaffPeriod => skillStaffPeriod.Period.StartDateTime);
                        _childSkillStaffPeriods.Add(childSkill, foundSkillStaffPeriods);
                    }

                    var skillResolution = _multisiteSkill.DefaultResolution;
                    var endDateTime = skillDay.CurrentDate.AddDays(10).Date;
                    for (var currentDateTime = TimeZoneHelper.ConvertToUtc(skillDay.CurrentDate.Date, _multisiteSkill.TimeZone);
                        currentDateTime < endDateTime; 
                        currentDateTime = currentDateTime.AddMinutes(skillResolution))
                    {
						if (!foundSkillStaffPeriods.TryGetValue(currentDateTime, out var foundSkillStaffPeriod))
                            break;
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
            if (!(skill is IChildSkill childSkill)) throw new ArgumentException("The supplied skill is not a child skill nor the parent multisite skill.");
            if (!_multisiteSkill.ChildSkills.Contains(childSkill)) throw new ArgumentException("The supplied child skill does not have the current multisite skill as parent.");

            var localDateTime = period.StartDateTimeLocal(skill.TimeZone);
            var currentDay = _multisiteDays.FirstOrDefault(m => m.MultisiteDayDate.Date == localDateTime.Date);
            if (currentDay == null) return new Percent();

            var currentPeriod = currentDay.MultisitePeriodCollection.First(p => p.Period.Contains(period.StartDateTime));

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
        public static void SummarizeStaffingForMultisiteSkillDays(IMultisiteSkill multisiteSkill, IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays)
        {
            if (multisiteSkill == null) return;

            var intervalCount = (int) TimeSpan.FromDays(1).TotalMinutes/multisiteSkill.DefaultResolution;

            var calculatedResources = new Dictionary<int, StaffingResource>(intervalCount);
            foreach (var skillDay in skillDays[multisiteSkill])
            {
                var day = skillDay;
                foreach (var childSkillDay in from childSkill
                                                  in multisiteSkill.ChildSkills
                                              let currentDate = day.CurrentDate
                                              select
                                                  skillDays[childSkill].FirstOrDefault(s => s.CurrentDate == currentDate)
                                              into childSkillDay
                                              where childSkillDay != null
                                              select childSkillDay)
				{
					var completeSkillStaffPeriodCollection = childSkillDay.CompleteSkillStaffPeriodCollection;
					for (var childIndex = 0;
                         childIndex < completeSkillStaffPeriodCollection.Length;
                         childIndex++)
                    {
						if (!calculatedResources.TryGetValue(childIndex, out var staffingResource))
                        {
                            staffingResource = new StaffingResource();
                            calculatedResources.Add(childIndex, staffingResource);
                        }

                        staffingResource.CalculatedResource +=
                            completeSkillStaffPeriodCollection[childIndex].Payload.CalculatedResource;
                        staffingResource.CalculatedLoggedOn +=
                            completeSkillStaffPeriodCollection[childIndex].Payload.CalculatedLoggedOn;
                    }
				}
				var skillStaffPeriodCollection = skillDay.CompleteSkillStaffPeriodCollection;
                for (var i = 0; i < calculatedResources.Count; i++)
                {
					if (!calculatedResources.TryGetValue(i, out var staffingResource)) continue;
					skillStaffPeriodCollection[i].SetCalculatedResource65(
                        staffingResource.CalculatedResource);
                    skillStaffPeriodCollection[i].Payload.CalculatedLoggedOn =
                        staffingResource.CalculatedLoggedOn;
                }
                calculatedResources.Clear();
            }
        }

        public override ISkillDayCalculator CloneToScenario(IScenario scenario)
        {
            var newSkillDays = SkillDays.Select(skillDay => skillDay.NoneEntityClone(scenario)).ToList();
            var newMultisiteDays = _multisiteDays.Select(multisiteDay => multisiteDay.NoneEntityClone(scenario)).ToList();
            var multisiteSkillDayCalculator = new MultisiteSkillDayCalculator(_multisiteSkill, newSkillDays, newMultisiteDays, VisiblePeriod);
            foreach (var childSkillDayPair in _childSkillDays)
            {
                var newChildSkillDays = childSkillDayPair.Value.Select(skillDay => skillDay.NoneEntityClone(scenario)).ToList();
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