using System;
using System.Collections;
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
			get { return sessions()[_factory] as ISession; }
			set { sessions()[_factory] = value; }
		}

		private static Hashtable sessions()
		{
			var httpContext = ReflectiveHttpContext.HttpContextCurrentGetter();
			return httpContext != null ? getWebSessions(httpContext) : getThreadSessions();
		}

		private static Hashtable getWebSessions(object httpContext)
		{
			var items = ReflectiveHttpContext.HttpContextItemsGetter(httpContext);
			var sessions = items[itemsKey] as Hashtable;
			if (sessions != null) return sessions;
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
