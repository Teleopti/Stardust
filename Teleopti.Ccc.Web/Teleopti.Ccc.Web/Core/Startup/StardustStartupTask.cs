using System.Threading.Tasks;
using log4net;
using Owin;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(100)]
	public class StardustStartupTask : IBootstrapperTask
	{
		private readonly StardustServerStarter _starter;
		private static readonly ILog logger = LogManager.GetLogger(typeof(StardustServerStarter));

		public StardustStartupTask(StardustServerStarter starter)
		{
			_starter = starter;
		}

		public Task Execute(IAppBuilder application)
		{
			logger.Info($"StardustStartupTask.Execute()");
			_starter.Start(application);
			return Task.FromResult(true);
		}
	}
}