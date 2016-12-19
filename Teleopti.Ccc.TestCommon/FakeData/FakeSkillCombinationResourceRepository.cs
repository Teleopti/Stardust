using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Intraday;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeSkillCombinationResourceRepository : ISkillCombinationResourceRepository
	{
		private List<SkillCombinationResource> _combinationResources = new List<SkillCombinationResource>();
		private readonly Dictionary<IEnumerable<Guid>, Guid> _skillCombination = new Dictionary<IEnumerable<Guid>, Guid>();


		public void PersistSkillCombination(IEnumerable<IEnumerable<Guid>> allSkillCombination)
		{
			allSkillCombination.ForEach(skillCombination =>
			{
				var combinationId = Guid.NewGuid();
				_skillCombination.Add(skillCombination, combinationId);
			});
		}

		public Guid? LoadSkillCombination(IEnumerable<Guid> skillCombination)
		{
			Guid combinationId;
			if (_skillCombination.TryGetValue(skillCombination, out combinationId))
				return combinationId;
			return null;
		}

		public void PersistSkillCombinationResource(IEnumerable<SkillCombinationResource> skillCombinationResources)
		{
			_combinationResources = skillCombinationResources.ToList();
		}

		public IEnumerable<SkillCombinationResource> LoadSkillCombinationResources(DateTimePeriod period)
		{
			return
				_combinationResources.Where(x => x.StartDateTime >= period.StartDateTime && x.StartDateTime < period.EndDateTime);
		}

	}
}