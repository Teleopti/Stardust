using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SkillMapRepository :Repository<ISkillMap_DEV>
	{
		public SkillMapRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
			: base(unitOfWork)
#pragma warning restore 618
		{
		}
	}
}