using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SkillAreaRepository : Repository<SkillArea>, ISkillAreaRepository
	{
		public SkillAreaRepository(ICurrentUnitOfWork currentUnitOfWork):base(currentUnitOfWork)
		{
		}
	}
}