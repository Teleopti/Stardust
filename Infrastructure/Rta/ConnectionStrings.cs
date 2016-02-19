using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class ConnectionStrings : IConnectionStrings
	{
		private readonly ICurrentUnitOfWorkFactory _application;
		private readonly ICurrentAnalyticsUnitOfWorkFactory _analytics;

		public ConnectionStrings(ICurrentUnitOfWorkFactory application, ICurrentAnalyticsUnitOfWorkFactory analytics)
		{
			_application = application;
			_analytics = analytics;
		}

		public string Application()
		{
			return _application.Current().ConnectionString;
		}

		public string Analytics()
		{
			return _analytics.Current().ConnectionString;
		}
	}
}