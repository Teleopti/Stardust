using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SkillGroupManagement;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SkillGroupRepository : Repository<SkillGroup>, ISkillGroupRepository
	{
		public SkillGroupRepository(ICurrentUnitOfWork currentUnitOfWork):base(currentUnitOfWork, null, null)
		{
		}
	}
}