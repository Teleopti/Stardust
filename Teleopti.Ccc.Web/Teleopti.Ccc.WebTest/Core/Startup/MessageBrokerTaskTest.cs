﻿using System;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Core.Startup.InitializeApplication;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
	[TestFixture]
	public class MessageBrokerTaskTest
	{
		[Test]
		public void ShouldConnectToItselfIfNoAppSetting()
		{
			var currentHttpContext = CurrentHttpContext("http://my.url.com", "/app/");
			var settings = MockRepository.GenerateMock<ISettings>();
			var messageBroker = MockRepository.GenerateMock<IMessageBrokerComposite>();
			messageBroker.Stub(x => x.ServerUrl).PropertyBehavior();
			var target = new MessageBrokerTask(messageBroker, currentHttpContext, settings);

			target.Execute(null);

			messageBroker.ServerUrl.Should().Be("http://my.url.com/app/");
		}

		[Test]
		public void ShouldConnectToAppSetting()
		{
			var currentHttpContext = CurrentHttpContext("http://my.url.com", "/app/");
			var settings = MockRepository.GenerateMock<ISettings>();
			settings.Stub(x => x.MessageBroker()).Return("http://my.broker.com/path/");
			var messageBroker = MockRepository.GenerateMock<IMessageBrokerComposite>();
			messageBroker.Stub(x => x.ServerUrl).PropertyBehavior();
			var target = new MessageBrokerTask(messageBroker, currentHttpContext, settings);

			target.Execute(null);

			messageBroker.ServerUrl.Should().Be(@"http://my.broker.com/path/");
		}

		[Test]
		public async void ShouldStartMessageBroker()
		{
			var currentHttpContext = CurrentHttpContext("http://localhost", "/");
			var messageBroker = MockRepository.GenerateMock<IMessageBrokerComposite>();
			var settings = MockRepository.GenerateMock<ISettings>();
			settings.Stub(x => x.MessageBrokerLongPolling()).Return(true);
			var target = new MessageBrokerTask(messageBroker, currentHttpContext, settings);

			await target.Execute(null);

			messageBroker.AssertWasCalled(x => x.StartBrokerService(true));
		}

		private static ICurrentHttpContext CurrentHttpContext(string url, string applicationPath)
		{
			var httpRequest = MockRepository.GenerateStub<HttpRequestBase>();
			httpRequest.Stub(x => x.Url).Return(new Uri(url));
			httpRequest.Stub(x => x.ApplicationPath).Return(applicationPath);
			var httpContext = MockRepository.GenerateStub<HttpContextBase>();
			httpContext.Stub(x => x.Request).Return(httpRequest);
			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			currentHttpContext.Stub(x => x.Current()).Return(httpContext);
			return currentHttpContext;
		}
	}
}