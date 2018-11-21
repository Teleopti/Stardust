using System;
using NHibernate;
using Teleopti.Ccc.Domain;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	public static class SessionFactoryExtensions
	{
		public static IDisposable WithStats(this ISessionFactory sessionFactory)
		{
			sessionFactory.Statistics.Clear();
			sessionFactory.Statistics.IsStatisticsEnabled = true;
			return new GenericDisposable(() => sessionFactory.Statistics.IsStatisticsEnabled = false);
		}
	}
}