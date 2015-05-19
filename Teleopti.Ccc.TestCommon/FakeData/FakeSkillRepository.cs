using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeSkillRepository : ISkillRepository
	{
		public void Add(ISkill entity)
		{
			throw new NotImplementedException();
		}

		public void Remove(ISkill entity)
		{
			throw new NotImplementedException();
		}

		public ISkill Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<ISkill> LoadAll()
		{
			throw new NotImplementedException();
		}

		public ISkill Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
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