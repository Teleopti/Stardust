using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	internal static class UnitOfWorkExtensions
	{
		internal static ISession Session(this ICurrentUnitOfWork unitOfWork)
		{
			return unitOfWork.Current().Session();
		}

		internal static ISession Session(this IUnitOfWorkFactory unitOfWorkFactory)
		{
			return unitOfWorkFactory.CurrentUnitOfWork().Session();
		}

		internal static IStatelessSession Session(this IStatelessUnitOfWork statelessUnitOfWork)
		{
			return ((NHibernateStatelessUnitOfWork)statelessUnitOfWork).Session;
		}

		internal static ISession Session(this IUnitOfWork unitOfWork)
		{
			switch (unitOfWork)
			{
				case NHibernateUnitOfWork application:
					return application.Session;
				case AnalyticsUnitOfWork analytics:
					return analytics.Session;
			}
			return null;
		}
	}
}