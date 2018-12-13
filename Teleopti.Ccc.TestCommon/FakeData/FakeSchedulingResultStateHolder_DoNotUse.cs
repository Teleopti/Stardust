using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeSchedulingResultStateHolder_DoNotUse : ISchedulingResultStateHolder
	{
		private readonly List<ISkill> _skills = new List<ISkill>();

		public IDictionary<IPerson, IPersonAccountCollection> AllPersonAccounts { get; set; }
		public bool SkipResourceCalculation { get; set; }
		public ICollection<IPerson> LoadedAgents { get; set; }
		public IDictionary<ISkill, IEnumerable<ISkillDay>> SkillDays { get; set; }

		public IScheduleDictionary Schedules { get; set; }
		public ISet<ISkill>  Skills { get { return new HashSet<ISkill>(_skills); } }

		public IList<ISkill> VisibleSkills { get; private set; }
		public IEnumerable<ISkillDay> SkillDaysOnDateOnly(IEnumerable<DateOnly> theDateList)
		{
			throw new NotImplementedException();
		}

		private ISkillStaffPeriodHolder _skillStaffPeriodHolder;
		public ISkillStaffPeriodHolder SkillStaffPeriodHolder
		{
			get { return _skillStaffPeriodHolder; }
		}

		public void SetSkillStaffPeriodHolder(ISkillStaffPeriodHolder skillStaffPeriodHolder)
		{
			_skillStaffPeriodHolder = skillStaffPeriodHolder;
		}
		public bool TeamLeaderMode { get; set; }
		public bool UseMaximumWorkday { get; set; }
		public bool UseValidation { get; set; }
		public INewBusinessRuleCollection GetRulesToRun()
		{
			throw new NotImplementedException();
		}

		public ISkillDay SkillDayOnSkillAndDateOnly(ISkill skill, DateOnly dateOnly)
		{
			throw new NotImplementedException();
		}

		public ISeniorityWorkDayRanks SeniorityWorkDayRanks { get; set; }

		public IEnumerable<ExternalStaff> ExternalStaff
		{
			get { yield break; }
			set {  }
		}

		public void AddSkills(params ISkill[] skills)
		{
			_skills.AddRange(skills);
		}

		public void ClearSkills()
		{
			_skills.Clear();
		}

		public void RemoveSkill(ISkill skill)
		{
			_skills.Remove(skill);
		}

		public bool GuessResourceCalculationHasBeenMade()
		{
			return false;
		}

		public ResourceCalculationData ToResourceOptimizationData(bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			return new ResourceCalculationData(this, considerShortBreaks, doIntraIntervalCalculation);
		}

		public int MinimumSkillIntervalLength()
		{
			throw new NotImplementedException();
		}
	}
}