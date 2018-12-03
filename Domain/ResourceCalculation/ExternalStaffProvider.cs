using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ExternalStaffProvider
	{
		private readonly ISkillCombinationResourceReader _skillCombinationResourceReader;
		private readonly SkillCombinationToBpoResourceMapper _skillCombinationToBpoResourceMapper;

		public ExternalStaffProvider(ISkillCombinationResourceReader skillCombinationResourceReader, 
			SkillCombinationToBpoResourceMapper skillCombinationToBpoResourceMapper)
		{
			_skillCombinationResourceReader = skillCombinationResourceReader;
			_skillCombinationToBpoResourceMapper = skillCombinationToBpoResourceMapper;
		}
		
		public IEnumerable<ExternalStaff> Fetch(IEnumerable<ISkill> skills, DateTimePeriod period)
		{
			var skillCombinationResources = _skillCombinationResourceReader.Execute(period);
			return _skillCombinationToBpoResourceMapper.Execute(skillCombinationResources, skills);
		}
	}
}