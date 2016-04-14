using System.Reflection;
using NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	public static class UnitOfWorkAndSession
	{
		public static ISession FetchSession(this IUnitOfWork unitOfWork)
		{
			var application = unitOfWork as NHibernateUnitOfWork;
			if (application != null)
				return (ISession) typeof (NHibernateUnitOfWork)
					.GetProperty("Session", BindingFlags.Instance | BindingFlags.NonPublic)
					.GetValue(unitOfWork, null);
			var analytics = unitOfWork as AnalyticsUnitOfWork;
			if (analytics != null)
				return (ISession) typeof (AnalyticsUnitOfWork)
					.GetProperty("Session", BindingFlags.Instance | BindingFlags.NonPublic)
					.GetValue(unitOfWork, null);
			return null;
		}
		
	}
}