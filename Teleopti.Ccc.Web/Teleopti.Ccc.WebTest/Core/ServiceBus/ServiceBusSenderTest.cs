﻿using System;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core.ServiceBus;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.WebTest.Core.ServiceBus
{
	[TestFixture]
	public class ServiceBusSenderTest
	{
		[Test]
		public void ShouldEnsureBus()
		{
			using (var sender = new ServiceBusSender(new CurrentIdentity()))
			{
				sender.EnsureBus();
				sender.FieldValue<IContainer>("_customHost").Should().Not.Be.Null();
				sender.FieldValue<bool>("_isRunning").Should().Be.False(); //due to miss config file
			}
		}
		
		[Test]
		public void ShouldSend()
		{
            var currentIdentity = MockRepository.GenerateMock<ICurrentIdentity>();
            var identity = MockRepository.GenerateMock<ITeleoptiIdentity>();
		    currentIdentity.Stub(x => x.Current()).Return(identity);
		    identity.Stub(x => x.BusinessUnit).Return(MockRepository.GenerateMock<IBusinessUnit>());
		    var dataSource = MockRepository.GenerateMock<IDataSource>();
		    identity.Stub(x => x.DataSource).Return(dataSource);
		    dataSource.Stub(x => x.Application).Return(MockRepository.GenerateMock<IUnitOfWorkFactory>());
			using (var sender = new TestServiceBusSender(currentIdentity))
			{
				var bus = MockRepository.GenerateMock<IOnewayBus>();
				sender.Resolveable = bus;
				sender.EnsureBus();
				var message = new TestMessage();
				sender.Send(message);
				bus.AssertWasCalled(x => x.Send(message));
			}
		}

		public class TestMessage : RaptorDomainMessage
		{
			public override Guid Identity { get { return Guid.Empty; } }
		}

		public class TestServiceBusSender : ServiceBusSender
		{
		    public TestServiceBusSender(ICurrentIdentity currentIdentity) : base(currentIdentity)
		    {
		    }

		    public object Resolveable { get; set; }

			protected override T Resolve<T>()
			{
				if (Resolveable is T)
					return (T)Resolveable;
				return default(T);
			}
		}
	}
}