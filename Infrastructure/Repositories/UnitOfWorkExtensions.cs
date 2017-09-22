using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

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

		internal static ISession Session(this IUnitOfWork unitOfWork)
		{
			var application = unitOfWork as NHibernateUnitOfWork;
			if (application != null)
				return application.Session;
			var analytics = unitOfWork as AnalyticsUnitOfWork;
			if (analytics != null)
				return analytics.Session;
			return null;
		}
	}
}