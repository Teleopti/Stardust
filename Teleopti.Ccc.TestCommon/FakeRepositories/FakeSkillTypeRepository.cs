using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeSkillTypeRepository : ISkillTypeRepository
	{
		private readonly List<ISkillType> _skillTypes = new List<ISkillType>();

		public FakeSkillTypeRepository()
		{

		}

		public FakeSkillTypeRepository(ISkillType skillType)
		{
			_skillTypes.Add(skillType);
		}

		public void Add(ISkillType root)
		{
			_skillTypes.Add(root);
		}

		public void Remove(ISkillType root)
		{
			_skillTypes.Remove(root);
		}

		public ISkillType Get(Guid id)
		{
			return _skillTypes.FirstOrDefault(x => x.Id.GetValueOrDefault() == id);
		}

		public IEnumerable<ISkillType> LoadAll()
		{
			return _skillTypes;
		}

		public ISkillType Load(Guid id)
		{
			return _skillTypes.First(x => x.Id.GetValueOrDefault() == id);
		}

		public IUnitOfWork UnitOfWork { get; }
	}
}