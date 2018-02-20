using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class SirLeakAlot : INestedUnitOfWorkStrategy
	{
		public void Strategize(ApplicationUnitOfWorkContext context)
		{
		}
	}
}