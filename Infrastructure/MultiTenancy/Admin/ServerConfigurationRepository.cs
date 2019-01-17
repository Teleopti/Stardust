using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Admin
{
	public class ServerConfigurationRepository : IServerConfigurationRepository
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public ServerConfigurationRepository(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}
		public void Update(string key, string value)
		{
			var updated = _currentTenantSession.CurrentSession()
				.CreateSQLQuery("UPDATE [Tenant].[ServerConfiguration] SET [Value] = :value WHERE [Key] = :key")
				.SetParameter("key", key)
				.SetParameter("value", value)
				.ExecuteUpdate();
			if (updated == 0)
				_currentTenantSession.CurrentSession()
					.CreateSQLQuery("INSERT INTO [Tenant].[ServerConfiguration] ([Key], [Value]) VALUES (:key, :value)")
					.SetParameter("key", key)
					.SetParameter("value", value)
					.ExecuteUpdate();
		}

		public string Get(string key)
		{
			return _currentTenantSession.CurrentSession()
				.CreateSQLQuery("SELECT [Value] FROM [Tenant].[ServerConfiguration] WHERE [Key] = :key")
				.SetParameter("key", key)
				.UniqueResult<string>();
		}

		public IEnumerable<ServerConfiguration> AllConfigurations()
		{
			return _currentTenantSession.CurrentSession()
				.CreateSQLQuery("SELECT [Key], [Value], [Description] FROM [Tenant].[ServerConfiguration]")
				.SetResultTransformer(Transformers.AliasToBean<ServerConfiguration>())
				.List<ServerConfiguration>();
		}
	}
}