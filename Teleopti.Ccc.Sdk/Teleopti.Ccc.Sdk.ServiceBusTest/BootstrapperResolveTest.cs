using System;
using System.Collections.Generic;
using Autofac;
using NUnit.Framework;
using Rhino.ServiceBus;
using Rhino.ServiceBus.Impl;
using Rhino.ServiceBus.Internal;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Container;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
	public class BootstrapperResolveTest
	{
		[Test]
		public void ShouldResolveFromBootstrapper()
		{
			using (var container = new ContainerBuilder().Build())
			{
				createFakeBus(container, new Uri("dummy://test"));

				new ContainerConfiguration(container, new FalseToggleManager()).Configure(null);
				var fakeBus = new ConfigFileDefaultHost("testQueue.config", new BusBootStrapper(container));
				fakeBus.Start();
				fakeBus.Dispose();
			}
		}

		private void createFakeBus(IContainer container, Uri endpoint)
		{
			var builder = new ContainerBuilder();
			builder.RegisterInstance(new Endpoint {Uri = endpoint}).SingleInstance();
			builder.RegisterType<fakeTransport>().As<ITransport>();
			builder.RegisterType<fakeSubscriptionStorage>().As<ISubscriptionStorage>();

			builder.Update(container);
		}

		private class fakeTransport : ITransport
		{
			public fakeTransport(Endpoint endpoint)
			{
				Endpoint = endpoint;
			}

			public void Dispose()
			{
				
			}

			public void Start()
			{
				
			}

			public void Send(Endpoint destination, object[] msgs)
			{
				throw new NotImplementedException();
			}

			public void Send(Endpoint endpoint, DateTime processAgainAt, object[] msgs)
			{
				throw new NotImplementedException();
			}

			public void Reply(params object[] messages)
			{
				throw new NotImplementedException();
			}

			public Endpoint Endpoint { get; private set; }
			public int ThreadCount { get; private set; }
			public CurrentMessageInformation CurrentMessageInformation { get; private set; }
			public event Action<CurrentMessageInformation> MessageSent;
			public event Func<CurrentMessageInformation, bool> AdministrativeMessageArrived;
			public event Func<CurrentMessageInformation, bool> MessageArrived;
			public event Action<CurrentMessageInformation, Exception> MessageSerializationException;
			public event Action<CurrentMessageInformation, Exception> MessageProcessingFailure;
			public event Action<CurrentMessageInformation, Exception> MessageProcessingCompleted;
			public event Action<CurrentMessageInformation> BeforeMessageTransactionRollback;
			public event Action<CurrentMessageInformation> BeforeMessageTransactionCommit;
			public event Action<CurrentMessageInformation, Exception> AdministrativeMessageProcessingCompleted;
			public event Action Started;
		}

		private class fakeSubscriptionStorage : ISubscriptionStorage
		{
			public void Initialize()
			{
				
			}

			public IEnumerable<Uri> GetSubscriptionsFor(Type type)
			{
				throw new NotImplementedException();
			}

			public void AddLocalInstanceSubscription(IMessageConsumer consumer)
			{
				throw new NotImplementedException();
			}

			public void RemoveLocalInstanceSubscription(IMessageConsumer consumer)
			{
				throw new NotImplementedException();
			}

			public object[] GetInstanceSubscriptions(Type type)
			{
				throw new NotImplementedException();
			}

			public bool AddSubscription(string type, string endpoint)
			{
				throw new NotImplementedException();
			}

			public void RemoveSubscription(string type, string endpoint)
			{
				throw new NotImplementedException();
			}

			public event Action SubscriptionChanged;
		}
	}
}