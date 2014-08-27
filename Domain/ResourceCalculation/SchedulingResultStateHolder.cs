using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SchedulingResultStateHolder : ISchedulingResultStateHolder
	{
        private IDictionary<ISkill, IList<ISkillDay>> _skillDays;
        private readonly IList<ISkill> _skills = new List<ISkill>();
        private SkillStaffPeriodHolder _skillStaffPeriodHolder;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ICollection<IPerson> PersonsInOrganization { get; set; }
        public IScheduleDictionary Schedules { get; set; }
        public bool SkipResourceCalculation { get; set; }
		public bool UseValidation { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IDictionary<IPerson, IPersonAccountCollection> AllPersonAccounts { get; set; }

		public bool TeamLeaderMode { get; set; }
        public bool UseMinWeekWorkTime { get; set; }

        public SchedulingResultStateHolder()
        {
            PersonsInOrganization = new List<IPerson>();
        }
        
        public IEnumerable<IShiftCategory> ShiftCategories { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public IList<IOptionalColumn> OptionalColumns { get; set; } 

        public ISkillStaffPeriodHolder SkillStaffPeriodHolder
        {
            get
            {
                if (_skillStaffPeriodHolder == null)
                    _skillStaffPeriodHolder = new SkillStaffPeriodHolder(_skillDays);

                return _skillStaffPeriodHolder;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IDictionary<ISkill, IList<ISkillDay>> SkillDays
        {
            get { return _skillDays; }
            set
            {
                _skillDays = value;
                _skillStaffPeriodHolder = null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public SchedulingResultStateHolder(ICollection<IPerson> personsInOrganization, IScheduleDictionary schedules, IDictionary<ISkill, IList<ISkillDay>> skillDays)
            : this()
        {
            PersonsInOrganization = personsInOrganization;
            Schedules = schedules;
            _skillDays = skillDays;
        }

        /// <summary>
        /// Gets the skills.
        /// </summary>
        /// <value>The skills.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-01-10
        /// </remarks>
        public IList<ISkill> Skills
        {
            get { return _skills; }
        }

        /// <summary>
        /// Gets the visible skills.
        /// </summary>
        /// <value>The visible skills.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-07
        /// </remarks>
        public IList<ISkill> VisibleSkills
        {
            get
            {
                return (from s in _skills
                        where
                        (s.WorkloadCollection.Any() ||
                        s is IChildSkill || s.SkillType.ForecastSource == ForecastSource.MaxSeatSkill
						|| s.SkillType.ForecastSource == ForecastSource.NonBlendSkill) &&
                        !(s is IMultisiteSkill)
                        select s).ToList();
            }
        }

		public IList<ISkill> NonVirtualSkills
		{
			get
			{
				IList<ISkill> ret = new List<ISkill>();
				foreach (var visibleSkill in VisibleSkills)
				{
					if (visibleSkill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill &&
					    visibleSkill.SkillType.ForecastSource != ForecastSource.NonBlendSkill)
						ret.Add(visibleSkill);
				}

				return ret;
			}
		}

		public IList<ISkillDay> SkillDaysOnDateOnly(IList<DateOnly> theDateList)
        {
            IList<ISkillDay> ret = new List<ISkillDay>();
            foreach (KeyValuePair<ISkill, IList<ISkillDay>> pair in SkillDays)
            {
                foreach (ISkillDay skillDay in pair.Value)
                {
                    if (theDateList.Contains(skillDay.CurrentDate))
                        ret.Add(skillDay);
                }
            }

            return ret;
        }

        ///<summary>
        ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///</summary>
        /// <remarks>
        /// So far only managed code. No need implementing destructor.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Virtual dispose method
        /// </summary>
        /// <param name="disposing">
        /// If set to <c>true</c>, explicitly called.
        /// If set to <c>false</c>, implicitly called from finalizer.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseManagedResources();
            }
            ReleaseUnmanagedResources();
        }

        /// <summary>
        /// Releases the unmanaged resources.
        /// </summary>
        protected virtual void ReleaseUnmanagedResources()
        {
        }

        /// <summary>
        /// Releases the managed resources.
        /// </summary>
        protected virtual void ReleaseManagedResources()
        {
            _skillStaffPeriodHolder = null;
            _skillDays = null;
            _skills.Clear();
            PersonsInOrganization = new List<IPerson>();
            AllPersonAccounts = null;
            Schedules = null;
        }

		public INewBusinessRuleCollection GetRulesToRun()
		{
		    if (UseValidation)
		    {
                var rules = NewBusinessRuleCollection.All(this);
                if(UseMinWeekWorkTime) rules.ActivateMinWeekWorkTimeRule();
                return rules;
		    }
				
			return NewBusinessRuleCollection.MinimumAndPersonAccount(this);
		}
    }
}
