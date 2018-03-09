using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class FindExternalApplicationAccessByHash : IFindExternalApplicationAccessByHash
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public FindExternalApplicationAccessByHash(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public ExternalApplicationAccess Find(string hash)
		{
			return _currentTenantSession.CurrentSession()
				.GetNamedQuery("findByHash")
				.SetString("hash", hash)
				.UniqueResult<ExternalApplicationAccess>();
		}
	}

	public interface IFindExternalApplicationAccessByHash
	{
		ExternalApplicationAccess Find(string hash);
	}
}