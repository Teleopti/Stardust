using NHibernate;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy
{
	public static class SessionExtension
	{
		public static void FlushAndClear(this ISession session)
		{
			session.Flush();
			session.Clear();
		}
	}
}