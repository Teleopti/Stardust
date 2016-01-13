using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using NHibernate;
using NHibernate.Context;
using NHibernate.Engine;
using Teleopti.Interfaces.Infrastructure;

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
			get { return getSessions()[_factory] as ISession; }
			set { getSessions()[_factory] = value; }
		}

		private Hashtable getSessions()
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

	[IsNotDeadCode("Used from NH file in web app.")]
	public class HybridWebSessionContext : CurrentSessionContext
	{
		private const string _itemsKey = "HybridWebSessionContext";
		[ThreadStatic]
		private static ISession _threadSession;

		// This constructor should be kept, otherwise NHibernate will fail to create an instance of this class.
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "factory")]
		public HybridWebSessionContext(ISessionFactoryImplementor factory)
		{
		}

		protected override ISession Session
		{
			get
			{
				var currentContext = ReflectiveHttpContext.HttpContextCurrentGetter();
				if (currentContext != null)
				{
					var items = ReflectiveHttpContext.HttpContextItemsGetter(currentContext);
					var session = items[_itemsKey] as ISession;
					if (session != null)
					{
						return session;
					}
				}

				return _threadSession;
			}
			set
			{
				var currentContext = ReflectiveHttpContext.HttpContextCurrentGetter();
				if (currentContext != null)
				{
					var items = ReflectiveHttpContext.HttpContextItemsGetter(currentContext);
					items[_itemsKey] = value;
					return;
				}

				_threadSession = value;
			}
		}
	}
}
