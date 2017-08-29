using System.Reflection;
using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.TestCommon
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