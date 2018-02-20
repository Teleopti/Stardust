using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface INestedUnitOfWorkStrategy
	{
		void Strategize(ApplicationUnitOfWorkContext context);
	}
}