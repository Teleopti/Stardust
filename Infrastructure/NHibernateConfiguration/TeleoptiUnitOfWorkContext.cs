using System.Collections;
using System.Threading;
using System.Web;
using NHibernate;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class TeleoptiUnitOfWorkContext
	{
		private const string itemsKey = "TeleoptiUnitOfWorkContext";
		private static readonly ThreadLocal<Hashtable> threadSessions = new ThreadLocal<Hashtable>(() => new Hashtable());
		private readonly ISessionFactory _factory;

		public TeleoptiUnitOfWorkContext(ISessionFactory factory)
		{
			_factory = factory;
		}

		public IUnitOfWork UnitOfWork
		{
			get { return (IUnitOfWork) sessions()[_factory]; }
			set { sessions()[_factory] = value; }
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