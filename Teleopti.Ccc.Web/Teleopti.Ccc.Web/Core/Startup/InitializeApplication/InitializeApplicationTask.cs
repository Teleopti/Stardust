using System.Configuration;
using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	[TaskPriority(5)]
	public class InitializeApplicationTask : IBootstrapperTask
	{
		private readonly IInitializeApplication _initializeApplication;
		private readonly ISettings _settings;
		private readonly IPhysicalApplicationPath _physicalApplicationPath;

		public InitializeApplicationTask(IInitializeApplication initializeApplication, ISettings settings, IPhysicalApplicationPath physicalApplicationPath)
		{
			_initializeApplication = initializeApplication;
			_settings = settings;
			_physicalApplicationPath = physicalApplicationPath;
		}

		public Task Execute(IAppBuilder application)
		{
			var passwordPolicyPath = System.IO.Path.Combine(_physicalApplicationPath.Get(), _settings.nhibConfPath());
			_initializeApplication.Start(new WebState(), new LoadPasswordPolicyService(passwordPolicyPath), ConfigurationManager.AppSettings.ToDictionary(), false);
			return null;
		}
	}
}