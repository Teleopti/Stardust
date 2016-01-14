using System;
using System.Collections;
using System.Web;
using NHibernate;
using NHibernate.Context;
using NHibernate.Engine;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class TeleoptiSessionContext : CurrentSessionContext
	{
		private readonly ISessionFactoryImplementor _factory;
		private const string itemsKey = "TeleoptiSessionContext";

		public TeleoptiSessionContext(ISessionFactoryImplementor factory)
		{
			_factory = factory;
		}

		protected override ISession Session
		{
			get { return (ISession)sessions()[_factory]; }
			set { sessions()[_factory] = value; }
		}

		private static Hashtable sessions()
		{
			var httpContext = HttpContext.Current;
			return httpContext == null ? getThreadSessions() : getWebSessions(httpContext);
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

		[ThreadStatic]
		private static Hashtable _threadSessions;

		private static Hashtable getThreadSessions()
		{
			return _threadSessions ?? (_threadSessions = new Hashtable());
		}
	}
}
