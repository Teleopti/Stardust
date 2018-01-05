using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	[RemoveMeWithToggle(Toggles.Staffing_BPOExchangeImport_45202)]
	public class SkillCombinationResourceRepositoryOld : SkillCombinationResourceRepositoryBase
	{
		public SkillCombinationResourceRepositoryOld(INow now, ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, IStardustJobFeedback stardustJobFeedback) 
			: base(now, currentUnitOfWork, currentBusinessUnit, stardustJobFeedback)
		{
		}
		
		public override IEnumerable<SkillCombinationResource> LoadSkillCombinationResources(DateTimePeriod period, bool useBpoExchange = true)
		{
			return skillCombinationResourcesWithoutBpo(period);
		}
	}
}