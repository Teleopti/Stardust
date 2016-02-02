using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeSkillAreaRepository : ISkillAreaRepository
	{
		private readonly IList<SkillArea> _skillAreas = new List<SkillArea>();

		public void Has(SkillArea skillArea)
		{
			_skillAreas.Add(skillArea);
		}

		public void Add(SkillArea root)
		{
			_skillAreas.Add(root);
		}

		public void Remove(SkillArea root)
		{
			throw new NotImplementedException();
		}

		public SkillArea Get(Guid id)
		{
			return _skillAreas.Single(x => x.Id == id);
		}

		public IList<SkillArea> LoadAll()
		{
			return _skillAreas;
		}

		public SkillArea Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<SkillArea> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
	}

	public class FakeLoadAllSkillInIntradays : ILoadAllSkillInIntradays
	{
		private readonly IList<SkillInIntraday> _skillInIntradays = new List<SkillInIntraday>();

		public void Has(SkillInIntraday existingSkillInIntraday)
		{
			_skillInIntradays.Add(existingSkillInIntraday);
		}

		public IEnumerable<SkillInIntraday> Skills()
		{
			return _skillInIntradays;
		}

		public void HasWithName(string name)
		{
			var skillInIntraday = new SkillInIntraday {Name = name};
			_skillInIntradays.Add(skillInIntraday);
		}
	}
}