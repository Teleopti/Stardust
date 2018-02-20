using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class ThrowOnNestedUnitOfWork : INestedUnitOfWorkStrategy
	{
		public void Strategize(ApplicationUnitOfWorkContext context)
		{
			if (context.Get() != null)
				throw new NestedUnitOfWorkException();
		}
	}
}