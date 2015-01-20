using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate
{
	public interface ITennantSessionFactory
	{
		IDisposable StartTransaction();
		void EndTransaction();
	}
}