using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeSkillRepository : ISkillRepository
	{
		private IList<ISkill> _skills;
 
		public FakeSkillRepository()
		{
			_skills = new List<ISkill>();
		}

		public ISkill Has(string skillName)
		{
			var skill = SkillFactory.CreateSkill(skillName);
			_skills.Add(skill);
			return skill;
		}

		public void Add(ISkill skill)
		{
			_skills.Add(skill);
		}

		public void Remove(ISkill skill)
		{
			_skills.Remove(_skills.First(x => x.Id == skill.Id));
		}

		public ISkill Get(Guid id)
		{
			return _skills.First(x => x.Id == id);
		}

		public IList<ISkill> LoadAll()
		{
			return _skills;
		}

		public ISkill Load(Guid id)
		{
			return _skills.First(x => x.Id == id);
		}

		public long CountAllEntities()
		{
			return _skills.Count;
		}

		public void AddRange(IEnumerable<ISkill> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public ICollection<ISkill> FindAllWithWorkloadAndQueues()
		{
			throw new NotImplementedException();
		}

		public ICollection<ISkill> FindAllWithoutMultisiteSkills()
		{
			throw new NotImplementedException();
		}

		public ICollection<ISkill> FindAllWithSkillDays(DateOnlyPeriod periodWithSkillDays)
		{
			return new Collection<ISkill> { SkillFactory.CreateSkillWithId("Direct Sales") };
		}

		public ISkill LoadSkill(ISkill skill)
		{
			throw new NotImplementedException();
		}

		public IMultisiteSkill LoadMultisiteSkill(ISkill skill)
		{
			throw new NotImplementedException();
		}

		public int MinimumResolution()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ISkill> FindSkillsWithAtLeastOneQueueSource()
		{
			throw new NotImplementedException();
		}
	}
}