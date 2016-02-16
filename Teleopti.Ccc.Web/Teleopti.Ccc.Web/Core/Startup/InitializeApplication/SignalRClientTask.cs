using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	[TaskPriority(14)]
	public class SignalRClientTask : IBootstrapperTask
	{
		private readonly ISignalRClient _signalSender;

		public SignalRClientTask(ISignalRClient signalSender)
		{
			_signalSender = signalSender;
		}

		public Task Execute(IAppBuilder application)
		{
			return Task.Factory.StartNew(() => _signalSender.StartBrokerService());
		}
	}
}