using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeSchedulingResultStateHolder : ISchedulingResultStateHolder
	{
		private readonly List<ISkill> _skills = new List<ISkill>();

		public void Dispose()
		{
		}

		public IDictionary<IPerson, IPersonAccountCollection> AllPersonAccounts { get; set; }
		public bool SkipResourceCalculation { get; set; }
		public ICollection<IPerson> PersonsInOrganization { get; set; }
		public IDictionary<ISkill, IList<ISkillDay>> SkillDays { get; set; }

		public IScheduleDictionary Schedules { get; set; }
		public ISkill[] Skills { get { return _skills.ToArray(); } }
		public IList<ISkill> VisibleSkills { get; private set; }
		public IList<ISkillDay> SkillDaysOnDateOnly(IList<DateOnly> theDateList)
		{
			throw new NotImplementedException();
		}

		public ISkillStaffPeriodHolder SkillStaffPeriodHolder { get; private set; }
		public IEnumerable<IShiftCategory> ShiftCategories { get; set; }
		public bool TeamLeaderMode { get; set; }
		public bool UseValidation { get; set; }
		public INewBusinessRuleCollection GetRulesToRun()
		{
			throw new NotImplementedException();
		}

		public IList<IOptionalColumn> OptionalColumns { get; set; }
		public bool UseMinWeekWorkTime { get; set; }
		public ISkillDay SkillDayOnSkillAndDateOnly(ISkill skill, DateOnly dateOnly)
		{
			throw new NotImplementedException();
		}

		public ISeniorityWorkDayRanks SeniorityWorkDayRanks { get; set; }
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
	}
}