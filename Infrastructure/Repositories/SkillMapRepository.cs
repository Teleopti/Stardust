using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SkillMapRepository :Repository<ISkillMap_DEV>
	{
		public SkillMapRepository(IUnitOfWork unitOfWork)
			: base(unitOfWork)
		{
		}
	}
}