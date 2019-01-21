using System.Threading;
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
		
		public PurgeOldSignInAttempts(IServerConfigurationRepository serverConfigurationRepository,
			ICurrentTenantSession currentTenantSession, INow now)
		{
			_serverConfigurationRepository = serverConfigurationRepository;
			_currentTenantSession = currentTenantSession;
			_now = now;
		}

		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual void Purge()
		{
			var manager = _currentTenantSession as TenantUnitOfWorkManager;
			var preserveLogonAttemptsDays = 30;
			using (manager.EnsureUnitOfWorkIsStarted())
			{
				if (!int.TryParse(_serverConfigurationRepository.Get(ServerConfigurationKey.PreserveLogonAttemptsDays),
					out preserveLogonAttemptsDays))
					preserveLogonAttemptsDays = 30;
			}

			var rowsEffected = 0;
			do
			{
				using (manager.EnsureUnitOfWorkIsStarted())
				{
					rowsEffected = _currentTenantSession.CurrentSession()
						.CreateSQLQuery("DELETE top(1000) FROM Tenant.Security WHERE DateTimeUtc < :dateTime")
						.SetDateTime("dateTime", _now.UtcDateTime().AddDays(-preserveLogonAttemptsDays)).ExecuteUpdate();
					manager.CommitAndDisposeCurrent();
					//a small delay to not to have table deadlock
					Thread.Sleep(2000);
				}
			} while (rowsEffected > 0);
		}
	}

	public interface IPurgeOldSignInAttempts
	{
		void Purge();
	}
}