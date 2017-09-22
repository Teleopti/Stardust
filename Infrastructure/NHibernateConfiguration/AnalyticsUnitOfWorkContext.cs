using System.Collections;
using System.Threading;
using System.Web;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class AnalyticsUnitOfWorkContext
	{
		private const string itemsKey = "AnalyticsUnitOfWorkContext";
		private static readonly ThreadLocal<Hashtable> threadSessions = new ThreadLocal<Hashtable>(() => new Hashtable());
		private readonly string _tenant;

		public AnalyticsUnitOfWorkContext(string tenant)
		{
			_tenant = tenant;
		}

		public void Set(IUnitOfWork unitOfWork)
		{
			sessions()[_tenant] = unitOfWork;
		}

		public IUnitOfWork Get()
		{
			return (IUnitOfWork)sessions()[_tenant];
		}

		public void Clear()
		{
			Set(null);
		}

		private static Hashtable sessions()
		{
			var httpContext = HttpContext.Current;
			return httpContext == null ? threadSessions.Value : getWebSessions(httpContext);
		}

		private static Hashtable getWebSessions(HttpContext httpContext)
		{
			var items = httpContext.Items;
			var sessions = items[itemsKey] as Hashtable;
			if (sessions != null)
				return sessions;
			sessions = new Hashtable();
			items[itemsKey] = sessions;
			return sessions;
		}
	}
}