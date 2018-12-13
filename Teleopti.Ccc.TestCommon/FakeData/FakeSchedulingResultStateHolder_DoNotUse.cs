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
		public ISet<ISkill> Skills { get => new HashSet<ISkill>(_skills);set{}}

		public IList<ISkill> VisibleSkills { get; private set; }
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
		public ISeniorityWorkDayRanks SeniorityWorkDayRanks { get; set; }

		public IEnumerable<ExternalStaff> ExternalStaff
		{
			get { yield break; }
			set {  }
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