using System.Collections;
using System.Threading;
using System.Web;
using NHibernate;
using NHibernate.Context;
using NHibernate.Engine;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class TenantSessionContext : CurrentSessionContext
	{
		private const string itemsKey = "TeleoptiSessionContext";
		private static readonly ThreadLocal<ISession> _session = new ThreadLocal<ISession>();

		public TenantSessionContext(ISessionFactoryImplementor factory)
		{
		}

		protected override ISession Session
		{
			get
			{
				if (HttpContext.Current != null)
					return (ISession)HttpContext.Current.Items[itemsKey];
				return _session.Value;
			}
			set
			{
				if (HttpContext.Current != null)
				{
					HttpContext.Current.Items[itemsKey] = value;
					return;
				}
				_session.Value = value;
			}
		}
	}
}
