using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Wfm.Administration.Core
{
	public class PurgeOldSignInAttempts : IPurgeOldSignInAttempts
	{
		private readonly IServerConfigurationRepository _serverConfigurationRepository;
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly INow _now;

		public PurgeOldSignInAttempts(IServerConfigurationRepository serverConfigurationRepository, ICurrentTenantSession currentTenantSession, INow now)
		{
			_serverConfigurationRepository = serverConfigurationRepository;
			_currentTenantSession = currentTenantSession;
			_now = now;
		}

		[TenantUnitOfWork]
		public virtual void Purge()
		{
			var manager = _currentTenantSession as TenantUnitOfWorkManager;

			using (manager.EnsureUnitOfWorkIsStarted())
			{
				if (!int.TryParse(_serverConfigurationRepository.Get("PreserveLogonAttemptsDays"),
					out var preserveLogonAttemptsDays))
					preserveLogonAttemptsDays = 30;

				_currentTenantSession.CurrentSession().CreateSQLQuery("DELETE FROM Tenant.Security WHERE DateTimeUtc < :dateTime")
					.SetDateTime("dateTime", _now.UtcDateTime().AddDays(-preserveLogonAttemptsDays)).ExecuteUpdate();
			}
		}
	}

	public class PurgeOldSignInAttemptsEmpty : IPurgeOldSignInAttempts
	{
		public void Purge()
		{

		}
	}

	public interface IPurgeOldSignInAttempts
	{
		void Purge();
	}
}