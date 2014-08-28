using System.Threading.Tasks;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.MessageBroker.Client;

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

		public Task Execute()
		{
			return Task.Factory.StartNew(() => _signalSender.StartBrokerService());
		}
	}
}