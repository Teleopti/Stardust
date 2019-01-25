using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Analytics.Etl.Common.Service
{
	public interface IPmInfoProvider
	{
		string PmInstallation();
		string Cube();
	}

	public class PmInfoProvider : IPmInfoProvider
	{
		private readonly IServerConfigurationRepository _serverConfigurationRepository;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;

		public PmInfoProvider(IServerConfigurationRepository serverConfigurationRepository, ITenantUnitOfWork tenantUnitOfWork)
		{
			_serverConfigurationRepository = serverConfigurationRepository;
			_tenantUnitOfWork = tenantUnitOfWork;
		}

		public string PmInstallation()
		{
			using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				return _serverConfigurationRepository.Get(ServerConfigurationKey.PM_INSTALL);
			}
		}

		public string Cube()
		{
			using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var asServerName = _serverConfigurationRepository.Get(ServerConfigurationKey.AS_SERVER_NAME);
				var asDatabase = _serverConfigurationRepository.Get(ServerConfigurationKey.AS_DATABASE);
				return 
					$"Provider=MSOLAP.3;Integrated Security=SSPI;Persist Security Info=True;Data Source={asServerName};Initial Catalog={asDatabase};Client Cache Size=25;Auto Synch Period=10000";
			}
		}
	}
}