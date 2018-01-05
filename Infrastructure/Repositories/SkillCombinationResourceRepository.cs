using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	// When removing Staffing_BPOExchangeImport_45202 toggle:
	// 1) Move LoadSkillCombinationResources() implementation from this class to SkillCombinationResourceRepositoryBase
	// 2) Remove this class and SkillCombinationResourceRepositoryOld
	// 3) Rename SkillCombinationResourceRepositoryBase to SkillCombinationResourceRepository and remove abstract keyword
	// 4) Change in IntradayWebModule Ioc registration to always use new SkillCombinationResourceRepository class
	[RemoveMeWithToggle(Toggles.Staffing_BPOExchangeImport_45202)]
	public class SkillCombinationResourceRepository : SkillCombinationResourceRepositoryBase
	{
		public SkillCombinationResourceRepository(INow now, ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, IStardustJobFeedback stardustJobFeedback) 
			: base(now, currentUnitOfWork, currentBusinessUnit, stardustJobFeedback)
		{
		}
		
		public override IEnumerable<SkillCombinationResource> LoadSkillCombinationResources(DateTimePeriod period, bool useBpoExchange = true)
		{
			return useBpoExchange ? skillCombinationResourcesWithBpo(period):skillCombinationResourcesWithoutBpo(period);
		}
	}
}