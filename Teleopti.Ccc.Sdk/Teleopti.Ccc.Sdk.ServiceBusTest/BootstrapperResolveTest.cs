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

			protected virtual void OnMessageSent(CurrentMessageInformation obj)
			{
				var handler = MessageSent;
				if (handler != null) handler(obj);
			}

			public event Func<CurrentMessageInformation, bool> AdministrativeMessageArrived;

			protected virtual void OnAdministrativeMessageArrived(CurrentMessageInformation arg)
			{
				var handler = AdministrativeMessageArrived;
				if (handler != null) handler(arg);
			}

			public event Func<CurrentMessageInformation, bool> MessageArrived;

			protected virtual void OnMessageArrived(CurrentMessageInformation arg)
			{
				var handler = MessageArrived;
				if (handler != null) handler(arg);
			}

			public event Action<CurrentMessageInformation, Exception> MessageSerializationException;

			protected virtual void OnMessageSerializationException(CurrentMessageInformation arg1, Exception arg2)
			{
				var handler = MessageSerializationException;
				if (handler != null) handler(arg1, arg2);
			}

			public event Action<CurrentMessageInformation, Exception> MessageProcessingFailure;

			protected virtual void OnMessageProcessingFailure(CurrentMessageInformation arg1, Exception arg2)
			{
				var handler = MessageProcessingFailure;
				if (handler != null) handler(arg1, arg2);
			}

			public event Action<CurrentMessageInformation, Exception> MessageProcessingCompleted;

			protected virtual void OnMessageProcessingCompleted(CurrentMessageInformation arg1, Exception arg2)
			{
				var handler = MessageProcessingCompleted;
				if (handler != null) handler(arg1, arg2);
			}

			public event Action<CurrentMessageInformation> BeforeMessageTransactionRollback;

			protected virtual void OnBeforeMessageTransactionRollback(CurrentMessageInformation obj)
			{
				var handler = BeforeMessageTransactionRollback;
				if (handler != null) handler(obj);
			}

			public event Action<CurrentMessageInformation> BeforeMessageTransactionCommit;

			protected virtual void OnBeforeMessageTransactionCommit(CurrentMessageInformation obj)
			{
				var handler = BeforeMessageTransactionCommit;
				if (handler != null) handler(obj);
			}

			public event Action<CurrentMessageInformation, Exception> AdministrativeMessageProcessingCompleted;

			protected virtual void OnAdministrativeMessageProcessingCompleted(CurrentMessageInformation arg1, Exception arg2)
			{
				var handler = AdministrativeMessageProcessingCompleted;
				if (handler != null) handler(arg1, arg2);
			}

			public event Action Started;

			protected virtual void OnStarted()
			{
				var handler = Started;
				if (handler != null) handler();
			}
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

			protected virtual void OnSubscriptionChanged()
			{
				var handler = SubscriptionChanged;
				if (handler != null) handler();
			}
		}
	}
}