using System.Collections;
using System.Threading;
using System.Web;
using NHibernate;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class UnitOfWorkContext
	{
		private const string itemsKey = "UnitOfWorkContext";
		private static readonly ThreadLocal<Hashtable> threadSessions = new ThreadLocal<Hashtable>(() => new Hashtable());
		private readonly ISessionFactory _factory;

		public UnitOfWorkContext(ISessionFactory factory)
		{
			_factory = factory;
		}

		public void Set(IUnitOfWork unitOfWork)
		{
			sessions()[_factory] = unitOfWork;
		}

		public IUnitOfWork Get()
		{
			return (IUnitOfWork) sessions()[_factory];
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