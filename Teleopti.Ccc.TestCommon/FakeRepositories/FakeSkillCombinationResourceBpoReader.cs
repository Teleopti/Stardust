using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeSkillCombinationResourceBpoReader : ISkillCombinationResourceBpoReader
	{
		private readonly ICollection<SkillCombinationResource> _skillCombinationResources;

		public FakeSkillCombinationResourceBpoReader()
		{
			_skillCombinationResources = new List<SkillCombinationResource>();
		}
		
		public IEnumerable<SkillCombinationResource> Execute(DateTimePeriod period)
		{
			return _skillCombinationResources.Where(x =>
				x.StartDateTime < period.EndDateTime && x.EndDateTime > period.StartDateTime);
		}

		public void Has(double resource, DateTimePeriod period, params ISkill[] skills)
		{
			_skillCombinationResources.Add(new SkillCombinationResource
			{
				Resource = resource,
				StartDateTime = period.StartDateTime,
				EndDateTime = period.EndDateTime,
				SkillCombination = skills.Select(x => x.Id.Value)
			});
		}
	}
}