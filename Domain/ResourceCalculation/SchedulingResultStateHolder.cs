using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	/*
	*  PLEASE DONT ADD MORE STATE TO THIS TYPE!
	*/
	public class SchedulingResultStateHolder : ISchedulingResultStateHolder
	{
		private IDictionary<ISkill, IEnumerable<ISkillDay>> _skillDays;
		private readonly HashSet<ISkill> _skills = new HashSet<ISkill>();

		private Lazy<SkillStaffPeriodHolder> _skillStaffPeriodHolder =
			new Lazy<SkillStaffPeriodHolder>(
				() => new SkillStaffPeriodHolder(Enumerable.Empty<KeyValuePair<ISkill, IEnumerable<ISkillDay>>>()));
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

		public ICollection<IPerson> LoadedAgents { get; set; }
		public IScheduleDictionary Schedules { get; set; }
		public bool SkipResourceCalculation { get; set; }
		public bool UseValidation { get; set; }
		public IDictionary<IPerson, IPersonAccountCollection> AllPersonAccounts { get; set; }
		public IEnumerable<ExternalStaff> ExternalStaff { get; set; }
		public bool TeamLeaderMode { get; set; }
		public bool UseMaximumWorkday { get; set; }

		public SchedulingResultStateHolder()
		{
			LoadedAgents = new List<IPerson>();
			_visibleSkills = new Lazy<ISkill[]>(visibleSkills);
			ExternalStaff = new List<ExternalStaff>();
			AllPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>();
		}

		public SchedulingResultStateHolder(ICollection<IPerson> personsInOrganization, IScheduleDictionary schedules, IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays)
			: this()
		{
			LoadedAgents = personsInOrganization;
			Schedules = schedules;
			SkillDays = skillDays;
		}

		public ISkillStaffPeriodHolder SkillStaffPeriodHolder
		{
			get
			{
				return _skillStaffPeriodHolder.Value;
			}
		}

		public IDictionary<ISkill, IEnumerable<ISkillDay>> SkillDays
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

		public bool GuessResourceCalculationHasBeenMade()
		{
			return _skillStaffPeriodHolder.Value.GuessResourceCalculationHasBeenMade();
		}

		public ResourceCalculationData ToResourceOptimizationData(bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			return new ResourceCalculationData(this, considerShortBreaks, doIntraIntervalCalculation);
		}

		public int MinimumSkillIntervalLength()
		{
			return Skills.Any() ? Skills.Min(s => s.DefaultResolution) : 15;
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
				//THIS IS MOST PROBABLY WRONG when having cascading skills and you're in primary mode
				return _visibleSkills.Value;
			}
		}

		public IEnumerable<ISkillDay> SkillDaysOnDateOnly(IEnumerable<DateOnly> theDateList)
		{
			return SkillDays == null ? 
				Enumerable.Empty<ISkillDay>() : 
				SkillDays.FilterOnDates(theDateList);
		}

		public ISkillDay SkillDayOnSkillAndDateOnly(ISkill skill, DateOnly dateOnly)
		{
			IEnumerable<ISkillDay> foundSkillDays;
			if (SkillDays != null && SkillDays.TryGetValue(skill, out foundSkillDays))
			{
				return foundSkillDays.FirstOrDefault(s => s.CurrentDate == dateOnly);
			}

			return null;
		}

		public ISeniorityWorkDayRanks SeniorityWorkDayRanks { get; set; }

		public INewBusinessRuleCollection GetRulesToRun()
		{
			if (UseValidation)
			{
				var rules = NewBusinessRuleCollection.All(this);
				return rules;
			}

			return NewBusinessRuleCollection.MinimumAndPersonAccount(this, AllPersonAccounts);
		}
	}
}
