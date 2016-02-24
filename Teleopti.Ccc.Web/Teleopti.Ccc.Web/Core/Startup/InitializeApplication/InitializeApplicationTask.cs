using System.Configuration;
using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	[TaskPriority(5)]
	public class InitializeApplicationTask : IBootstrapperTask
	{
		private readonly IInitializeApplication _initializeApplication;
		private readonly ISettings _settings;
		private readonly IPhysicalApplicationPath _physicalApplicationPath;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;

		public InitializeApplicationTask(IInitializeApplication initializeApplication, 
												ISettings settings, 
												IPhysicalApplicationPath physicalApplicationPath, 
												ITenantUnitOfWork tenantUnitOfWork)
		{
			_initializeApplication = initializeApplication;
			_settings = settings;
			_physicalApplicationPath = physicalApplicationPath;
			_tenantUnitOfWork = tenantUnitOfWork;
		}

		public Task Execute(IAppBuilder application)
		{
			var passwordPolicyPath = System.IO.Path.Combine(_physicalApplicationPath.Get(), _settings.ConfigurationFilesPath());
			using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
				_initializeApplication.Start(new State(), new LoadPasswordPolicyService(passwordPolicyPath),
					ConfigurationManager.AppSettings.ToDictionary());
			return null;
		}
	}
}