using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	[TaskPriority(5)]
	public class MessageBrokerTask : IBootstrapperTask
	{
		private readonly IMessageBroker _messageBroker;

		public MessageBrokerTask(IMessageBroker messageBroker)
		{
			_messageBroker = messageBroker;
		}

		public Task Execute()
		{
			_messageBroker.ConnectionString = ConnectionString();
			return Task.Factory.StartNew(() => _messageBroker.StartMessageBroker());
		}

		private string ConnectionString()
		{
			var appSetting = ConfigurationManager.AppSettings["MessageBroker"];
			if (!string.IsNullOrEmpty(appSetting))
				return appSetting;
			if (HttpContext.Current != null)
				return ApplicationRootUrl();
			throw new Exception("No connection string (url) found for message broker client!");
		}

		private string ApplicationRootUrl()
		{
			var url = HttpContext.Current.Request.Url;
			var urlString = new StringBuilder();
			urlString.Append(url.Scheme);
			urlString.Append("://");
			urlString.Append(url.Authority);
			urlString.Append(HttpContext.Current.Request.ApplicationPath);
			return urlString.ToString();
		}
	}
}