using System;
using System.Configuration;
using System.Text;
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

			_messageBroker.ConnectionString = ApplicationRootUrl();
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