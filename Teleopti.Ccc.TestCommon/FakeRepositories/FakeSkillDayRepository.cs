using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeSkillDayRepository : ISkillDayRepository
	{
		private IList<ISkillDay> _skillDays = new List<ISkillDay>();

		public void Add(ISkillDay root)
		{
			_skillDays.Add(root);
		}

		public ISkillDay HasSkillDayWithDemand(ISkill skill, DateOnly dateOnly, TimeSpan demand, IScenario scenario)
		{
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemand(dateOnly, demand, scenario);
			_skillDays.Add(skillDay);
			return skillDay;
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

		public long CountAllEntities()
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

		public ICollection<ISkillDay> FindRange(DateOnlyPeriod period, IList<ISkill> skills, IScenario scenario)
		{
			return _skillDays
				.Where(skillDayInDb => 
						skillDayInDb.Scenario.Equals(scenario) && 
						period.Contains(skillDayInDb.CurrentDate) && 
						skills.Contains(skillDayInDb.Skill))
				.ToList();
		}
	}
}