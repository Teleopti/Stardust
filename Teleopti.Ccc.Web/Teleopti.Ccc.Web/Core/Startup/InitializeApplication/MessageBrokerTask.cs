using System;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	[TaskPriority(6)]
	public class MessageBrokerTask : IBootstrapperTask
	{
		private readonly IMessageBroker _messageBroker;
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly ISettings _settings;

		public MessageBrokerTask(IMessageBroker messageBroker, ICurrentHttpContext currentHttpContext, ISettings settings)
		{
			_messageBroker = messageBroker;
			_currentHttpContext = currentHttpContext;
			_settings = settings;
		}

		public Task Execute()
		{
			_messageBroker.ConnectionString = ConnectionString();
			return Task.Factory.StartNew(() => _messageBroker.StartMessageBroker());
		}

		private string ConnectionString()
		{
			var appSetting = _settings.MessageBroker();
			if (!string.IsNullOrEmpty(appSetting))
				return appSetting;
			if (_currentHttpContext.Current() != null)
				return ApplicationRootUrl();
			throw new Exception("No connection string (url) found for message broker client!");
		}

		private string ApplicationRootUrl()
		{
			var url = _currentHttpContext.Current().Request.Url;
			var urlString = new StringBuilder();
			urlString.Append(url.Scheme);
			urlString.Append("://");
			urlString.Append(url.Authority);
			urlString.Append(_currentHttpContext.Current().Request.ApplicationPath);
			return urlString.ToString();
		}
	}
}