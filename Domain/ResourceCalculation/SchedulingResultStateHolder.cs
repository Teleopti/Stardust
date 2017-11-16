using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
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
		public IEnumerable<BpoResource> BpoResources { get; set; }
		public bool TeamLeaderMode { get; set; }
		public bool UseMinWeekWorkTime { get; set; }

		public SchedulingResultStateHolder()
		{
			LoadedAgents = new List<IPerson>();
			_visibleSkills = new Lazy<ISkill[]>(visibleSkills);
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

		private readonly ConcurrentDictionary<IBudgetDay, double> addedAbsenceMinutesDictionary = new ConcurrentDictionary<IBudgetDay, double>();
		public double AddedAbsenceMinutesDuringCurrentRequestHandlingCycle(IBudgetDay budgetDay)
		{
			double value;
			if (!addedAbsenceMinutesDictionary.TryGetValue(budgetDay, out value))
				return 0;
			return value;
		}

		public void AddAbsenceMinutesDuringCurrentRequestHandlingCycle(IBudgetDay budgetDay, double minutes)
		{
			addedAbsenceMinutesDictionary.AddOrUpdate(budgetDay, minutes,(b, m) => m+minutes);
		}

		public void SubtractAbsenceMinutesDuringCurrentRequestHandlingCycle(IBudgetDay budgetDay, double minutes)
		{
			addedAbsenceMinutesDictionary.AddOrUpdate(budgetDay, 0, (b, m) => Math.Max(0, m - minutes));
		}

		private readonly ConcurrentDictionary<IBudgetDay, int> addedAbsenceHeadCountDictionary = new ConcurrentDictionary<IBudgetDay, int>();
		public int AddedAbsenceHeadCountDuringCurrentRequestHandlingCycle(IBudgetDay budgetDay)
		{
			int value;
			if (!addedAbsenceHeadCountDictionary.TryGetValue(budgetDay, out value))
				return 0;
			return value;
		}

		public void AddAbsenceHeadCountDuringCurrentRequestHandlingCycle(IBudgetDay budgetDay)
		{
			addedAbsenceHeadCountDictionary.AddOrUpdate(budgetDay,1, (b, m) => m + 1);
		}

		public void SubtractAbsenceHeadCountDuringCurrentRequestHandlingCycle(IBudgetDay budgetDay)
		{
			addedAbsenceHeadCountDictionary.AddOrUpdate(budgetDay, 0, (b, m) => Math.Max(0, m - 1));
		}

		public IEnumerable<ISkillDay> AllSkillDays()
		{
			return _skillDays.ToSkillDayEnumerable();
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
			LoadedAgents = new List<IPerson>();
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

			return NewBusinessRuleCollection.MinimumAndPersonAccount(this, AllPersonAccounts);
		}
	}
}
