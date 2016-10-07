using System.Linq;
using Hangfire;
using Owin;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireServerStarter
	{
		private readonly HangfireStarter _starter;
		private readonly IConfigReader _config;

		public HangfireServerStarter(
			HangfireStarter starter, 
			IConfigReader config)
		{
			_starter = starter;
			_config = config;
		}

		const int setThisToOneAndErikWillHuntYouDownAndKillYouSlowlyAndPainfully = 4;
		// But really.. using one worker will:
		// - reduce performance of RTA too much
		// - will be a one-way street where the code will never handle the concurrency
		// - still wont handle the fact that some customers have more than one server running, 
		//   so you will still have more than one worker

		public void Start(IAppBuilder app)
		{
			_starter.Start(_config.ConnectionString("Hangfire"));

			app.UseHangfireServer(new BackgroundJobServerOptions
			{
				WorkerCount = setThisToOneAndErikWillHuntYouDownAndKillYouSlowlyAndPainfully,
				Queues = Queues.OrderOfPriority().ToArray()
			});
		}
	}
}