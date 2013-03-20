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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		internal static ISession Session(this IUnitOfWorkFactory unitOfWorkFactory)
		{
			return ((NHibernateUnitOfWork) unitOfWorkFactory.CurrentUnitOfWork()).Session;
		}
	}
}