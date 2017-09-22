using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SkillAreaRepository : Repository<SkillArea>, ISkillAreaRepository
	{
		public SkillAreaRepository(ICurrentUnitOfWork currentUnitOfWork):base(currentUnitOfWork)
		{
		}
	}
}