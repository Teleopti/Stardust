using NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	internal static class UnitOfWorkExtensions
	{
		internal static ISession Session(this ICurrentUnitOfWork unitOfWork)
		{
			return ((NHibernateUnitOfWork) unitOfWork.Current()).Session;
		}

		internal static ISession Session(this IUnitOfWorkFactory unitOfWorkFactory)
		{
			return ((NHibernateUnitOfWork) unitOfWorkFactory.CurrentUnitOfWork()).Session;
		}

		internal static IStatelessSession Session(this IStatelessUnitOfWork statelessUnitOfWork)
		{
			return ((NHibernateStatelessUnitOfWork)statelessUnitOfWork).Session;
		}

		internal static ISession Session(this IUnitOfWork unitOfWok)
		{
			return ((NHibernateUnitOfWork) unitOfWok).Session;
		}
	}
}