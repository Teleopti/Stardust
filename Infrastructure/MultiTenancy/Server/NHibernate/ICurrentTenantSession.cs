using System;
using NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public interface ICurrentTenantSession : IDisposable
	{
		ISession CurrentSession();
	}
}