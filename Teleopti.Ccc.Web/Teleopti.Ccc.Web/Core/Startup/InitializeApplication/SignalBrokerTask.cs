using System.Configuration;
using System.Web;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	[TaskPriority(3)]//before InitializeApplicationTask please
	public class SignalBrokerTask : IBootstrapperTask
	{
		private readonly IMessageBroker _messageBroker;

		public SignalBrokerTask(IMessageBroker messageBroker)
		{
			_messageBroker = messageBroker;
		}

		public void Execute()
		{
			if (HttpContext.Current == null)
				return;
			if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["MessageBroker"]))
				return;

			_messageBroker.ConnectionString = HttpContext.Current.Request.Url.ToString();
		}
	}
}