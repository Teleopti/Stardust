using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SchedulingResultStateHolder : ISchedulingResultStateHolder
	{
        private IDictionary<ISkill, IList<ISkillDay>> _skillDays;
		private readonly HashSet<ISkill> _skills = new HashSet<ISkill>();

		private Lazy<SkillStaffPeriodHolder> _skillStaffPeriodHolder =
			new Lazy<SkillStaffPeriodHolder>(
				() => new SkillStaffPeriodHolder(Enumerable.Empty<KeyValuePair<ISkill, IList<ISkillDay>>>()));
		private Lazy<ISkill[]> _visibleSkills;

		private ISkill[] visibleSkills()
		{
			return (from s in _skills
				where
					(s.WorkloadCollection.Any() ||
					 s is IChildSkill || s.SkillType.ForecastSource == ForecastSource.MaxSeatSkill
					 || s.SkillType.ForecastSource == ForecastSource.NonBlendSkill) &&
					!(s is IMultisiteSkill)
				select s).ToArray();
		}

		public ICollection<IPerson> PersonsInOrganization { get; set; }
        public IScheduleDictionary Schedules { get; set; }
        public bool SkipResourceCalculation { get; set; }
		public bool UseValidation { get; set; }
        public IDictionary<IPerson, IPersonAccountCollection> AllPersonAccounts { get; set; }

		public bool TeamLeaderMode { get; set; }
        public bool UseMinWeekWorkTime { get; set; }

        public SchedulingResultStateHolder()
        {
            PersonsInOrganization = new List<IPerson>();
			_visibleSkills = new Lazy<ISkill[]>(visibleSkills);
        }

		public SchedulingResultStateHolder(ICollection<IPerson> personsInOrganization, IScheduleDictionary schedules, IDictionary<ISkill, IList<ISkillDay>> skillDays)
			: this()
		{
			PersonsInOrganization = personsInOrganization;
			Schedules = schedules;
			SkillDays = skillDays;
		}

        public IEnumerable<IShiftCategory> ShiftCategories { get; set; }

		public IList<IOptionalColumn> OptionalColumns { get; set; } 

        public ISkillStaffPeriodHolder SkillStaffPeriodHolder
        {
            get {
	            return _skillStaffPeriodHolder.Value;
            }
        }

        public IDictionary<ISkill, IList<ISkillDay>> SkillDays
        {
            get { return _skillDays; }
            set
            {
                _skillDays = value;
				_skillStaffPeriodHolder = new Lazy<SkillStaffPeriodHolder>(() => new SkillStaffPeriodHolder(_skillDays));
            }
        }

        /// <summary>
        /// Gets the skills.
        /// </summary>
        /// <value>The skills.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-01-10
        /// </remarks>
        public ISkill[] Skills
        {
            get { return _skills.ToArray(); }
        }

		public void AddSkills(params ISkill[] skills)
		{
			skills.ForEach(s => _skills.Add(s));
			_visibleSkills = new Lazy<ISkill[]>(visibleSkills);
		}

		public void ClearSkills()
		{
			_skills.Clear();
			_visibleSkills = new Lazy<ISkill[]>(visibleSkills);
		}

		public void RemoveSkill(ISkill skill)
		{
			if (_skills.Remove(skill))
			{
				_visibleSkills = new Lazy<ISkill[]>(visibleSkills);
			}
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
                return _visibleSkills.Value;
            }
        }

		public IList<ISkillDay> SkillDaysOnDateOnly(IList<DateOnly> theDateList)
        {
            var ret = new List<ISkillDay>();
			if (SkillDays == null) return ret;

			var days = SkillDays.SelectMany(s => s.Value).ToLookup(k => k.CurrentDate);
			foreach (var dateOnly in theDateList)
			{
				ret.AddRange(days[dateOnly]);
			}

            return ret;
        }

		public ISkillDay SkillDayOnSkillAndDateOnly(ISkill skill, DateOnly dateOnly)
		{
			IList<ISkillDay> foundSkillDays;
			if (SkillDays != null && SkillDays.TryGetValue(skill, out foundSkillDays))
			{
				return foundSkillDays.FirstOrDefault(s => s.CurrentDate == dateOnly);
			}

			return null;
		}

		public ISeniorityWorkDayRanks SeniorityWorkDayRanks { get; set; }

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
	        _visibleSkills = null;
            PersonsInOrganization = new List<IPerson>();
            AllPersonAccounts = null;
            Schedules = null;
        }

		public INewBusinessRuleCollection GetRulesToRun()
		{
		    if (UseValidation)
		    {
                var rules = NewBusinessRuleCollection.All(this);
                return rules;
		    }
				
			return NewBusinessRuleCollection.MinimumAndPersonAccount(this);
		}
    }
}
