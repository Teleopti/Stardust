using System.Collections;
using System.Threading;
using System.Web;
using NHibernate;
using NHibernate.Context;
using NHibernate.Engine;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	/// <summary>
	/// Puts current session on httpcontext if exists, if not on thread.
	/// A hashtable is needed so current sessions from different session factories are not "colliding".
	/// </summary>
	/// <remarks>
	/// If we later need a third way of storing "current session" (eg desktop scope?),
	/// break this type into multiple ones instead of adding yet another if...
	/// </remarks>
	public class TeleoptiSessionContext : CurrentSessionContext
	{
		private const string itemsKey = "TeleoptiSessionContext";
		private static readonly ThreadLocal<Hashtable> threadSessions = new ThreadLocal<Hashtable>(() => new Hashtable());
		private readonly ISessionFactoryImplementor _factory;

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
