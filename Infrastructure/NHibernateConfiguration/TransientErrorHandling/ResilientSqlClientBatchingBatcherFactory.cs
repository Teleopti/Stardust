using NHibernate;
using NHibernate.AdoNet;
using NHibernate.Engine;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling
{
	/// <summary>
	/// An <see cref="IBatcherFactory"/> implementation that creates
	/// <see cref="ResilientSqlClientBatchingBatcher"/> instances.
	/// </summary>
	public class ResilientSqlClientBatchingBatcherFactory : IBatcherFactory
	{
		/// <summary>
		/// Creates the batcher.
		/// </summary>
		/// <param name="connectionManager">The connection manager</param>
		/// <param name="interceptor">The interceptor</param>
		/// <returns>The <see cref="ResilientSqlClientBatchingBatcher"/> instance</returns>
		public virtual IBatcher CreateBatcher(ConnectionManager connectionManager, IInterceptor interceptor)
		{
			return new ResilientSqlClientBatchingBatcher(connectionManager, interceptor);
		}
	}
}