using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SkillGroupManagement;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeSkillGroupRepository : ISkillGroupRepository
	{
		private readonly IList<SkillGroup> _skillAreas = new List<SkillGroup>();

		public void Has(SkillGroup skillGroup)
		{
			_skillAreas.Add(skillGroup);
		}

		public void Add(SkillGroup root)
		{
			_skillAreas.Add(root);
		}

		public void Remove(SkillGroup root)
		{
			_skillAreas.Remove(root);
		}

		public SkillGroup Get(Guid id)
		{
			return _skillAreas.Single(x => x.Id == id);
		}

		public IList<SkillGroup> LoadAll()
		{
			return _skillAreas;
		}

		public SkillGroup Load(Guid id)
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