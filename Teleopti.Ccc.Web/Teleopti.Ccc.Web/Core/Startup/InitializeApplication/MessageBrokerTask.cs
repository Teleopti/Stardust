﻿using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	[TaskPriority(5)]
	public class MessageBrokerTask : IBootstrapperTask
	{
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly ISettings _settings;

		public MessageBrokerTask(IMessageBrokerComposite messageBroker, ICurrentHttpContext currentHttpContext, ISettings settings)
		{
			_messageBroker = messageBroker;
			_currentHttpContext = currentHttpContext;
			_settings = settings;
		}

		public Task Execute()
		{
			_messageBroker.ServerUrl = ConnectionString();
			bool useLongPolling;
			return bool.TryParse(_settings.MessageBrokerLongPolling(), out useLongPolling)
				? Task.Factory.StartNew(() => _messageBroker.StartBrokerService(useLongPolling))
				: Task.Factory.StartNew(() => _messageBroker.StartBrokerService(true));
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