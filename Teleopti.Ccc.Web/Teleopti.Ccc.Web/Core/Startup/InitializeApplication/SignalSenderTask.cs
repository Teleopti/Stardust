

using System.Threading.Tasks;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	[TaskPriority(14)]
	public class SignalSenderTask : IBootstrapperTask
	{
		private readonly IMessageSender _signalSender;

		public SignalSenderTask(IMessageSender signalSender)
		{
			_signalSender = signalSender;
		}

		public Task Execute()
		{
			return Task.Factory.StartNew(() => _signalSender.StartBrokerService());
		}
	}
}