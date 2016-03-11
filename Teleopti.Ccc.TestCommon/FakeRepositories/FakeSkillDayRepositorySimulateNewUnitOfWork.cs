using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeSkillDayRepositorySimulateNewUnitOfWork : ISkillDayRepository
	{
		private readonly List<Func<ISkillDay>> _skillDays = new List<Func<ISkillDay>>();

		public void Has(IList<Func<ISkillDay>> skillDays)
		{
			_skillDays.AddRange(skillDays);
		}

		public void Add(ISkillDay root)
		{
			throw new NotImplementedException();
		}

		public void Remove(ISkillDay root)
		{
			throw new NotImplementedException();
		}

		public ISkillDay Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<ISkillDay> LoadAll()
		{
			throw new NotImplementedException();
		}

		public ISkillDay Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<ISkillDay> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public ICollection<ISkillDay> FindRange(DateOnlyPeriod period, ISkill skill, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public ICollection<ISkillDay> GetAllSkillDays(DateOnlyPeriod period, ICollection<ISkillDay> skillDays, ISkill skill, IScenario scenario,
			Action<IEnumerable<ISkillDay>> optionalAction)
		{
			throw new NotImplementedException();
		}

		public void Delete(DateOnlyPeriod dateTimePeriod, ISkill skill, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public DateOnly FindLastSkillDayDate(IWorkload workload, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public ISkillDay FindLatestUpdated(ISkill skill, IScenario scenario, bool withLongterm)
		{
			throw new NotImplementedException();
		}

		public ICollection<ISkillDay> FindReadOnlyRange(DateOnlyPeriod period, IList<ISkill> skills, IScenario scenario)
		{
			return _skillDays
				.Select(skillDayFunc => skillDayFunc())
				.Where(skillDay => skillDay.Scenario.Equals(scenario) && 
													 period.Contains(skillDay.CurrentDate) && 
													 skills.Contains(skillDay.Skill))
				.ToList();
		}

		public IEnumerable<SkillTaskDetailsModel> GetSkillsTasksDetails(DateTimePeriod period, IList<ISkill> skills, IScenario scenario)
		{
			throw new NotImplementedException();
		}
	}
}