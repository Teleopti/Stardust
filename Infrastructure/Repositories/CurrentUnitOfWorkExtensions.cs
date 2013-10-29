using NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	internal static class CurrentUnitOfWorkExtensions
	{
		internal static ISession Session(this ICurrentUnitOfWork unitOfWork)
		{
			return ((NHibernateUnitOfWork) unitOfWork.Current()).Session;
		}

		internal static ISession Session(this IUnitOfWorkFactory unitOfWorkFactory)
		{
			return ((NHibernateUnitOfWork) unitOfWorkFactory.CurrentUnitOfWork()).Session;
		}
	}
}