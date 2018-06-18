using NHibernate;
using NHibernate.AdoNet;
using NHibernate.Engine;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	/// <summary>
	/// An <see cref="IBatcherFactory"/> implementation that creates
	/// <see cref="ReliableSqlClientBatchingBatcher"/> instances.
	/// </summary>
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic_76181)]
	public class ReliableSqlClientBatchingBatcherFactory : IBatcherFactory
	{
		/// <summary>
		/// Creates the batcher.
		/// </summary>
		/// <param name="connectionManager">The connection manager</param>
		/// <param name="interceptor">The interceptor</param>
		/// <returns>The <see cref="ReliableSqlClientBatchingBatcher"/> instance</returns>
		public virtual IBatcher CreateBatcher(ConnectionManager connectionManager, IInterceptor interceptor)
		{
			return new ReliableSqlClientBatchingBatcher(connectionManager, interceptor);
		}
	}
}