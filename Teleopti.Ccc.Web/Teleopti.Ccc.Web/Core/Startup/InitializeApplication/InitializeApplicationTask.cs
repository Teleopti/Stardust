using System.Threading.Tasks;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	[TaskPriority(4)]
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

		public Task Execute()
		{
			var nhibConfPath = System.IO.Path.Combine(_physicalApplicationPath.Get(), _settings.nhibConfPath());
			_initializeApplication.Start(new WebState(), nhibConfPath, new LoadPasswordPolicyService(_physicalApplicationPath.Get()), new ConfigurationManagerWrapper(), false);
			return null;
		}
	}
}