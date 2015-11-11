using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	public class UnitOfWorkModuleTest
	{
		private ContainerBuilder builder;
		private IContainer container;

		[SetUp]
		public void Setup()
		{
			builder = new ContainerBuilder();
			builder.RegisterModule(CommonModule.ForTest());
			builder.RegisterType<TrueToggleManager>().As<IToggleManager>();
			container = builder.Build();
		}

		[Test]
		public void ShouldResolveLicenseActivatorProvider()
		{
			container.Resolve<ILicenseActivatorProvider>().Should().Be.OfType<LicenseActivatorProvider>();
		}

		[Test]
		public void ShouldResolveCurrentPersistCallbacks()
		{
			var allCallbacks = container.Resolve<ICurrentPersistCallbacks>();
			allCallbacks.Should().Be.OfType<CurrentPersistCallbacks>();
			validateAllCallbacks((CurrentPersistCallbacks)allCallbacks);
		}

		[Test]
		public void ShouldResolveMessageSendersScope()
		{
			var allCallbacks = container.Resolve<IMessageSendersScope>();
			allCallbacks.Should().Be.OfType<CurrentPersistCallbacks>();
			validateAllCallbacks((CurrentPersistCallbacks)allCallbacks);
		}

		private void validateAllCallbacks(CurrentPersistCallbacks allCallbacks)
		{
			var senderTypes = new[]
			{
				typeof (ScheduleChangedEventPublisher),
				typeof (EventsMessageSender),
				typeof (ScheduleChangedEventFromMeetingPublisher),
				typeof (GroupPageChangedBusMessageSender),
				typeof (PersonCollectionChangedEventPublisherForTeamOrSite),
				typeof (PersonCollectionChangedEventPublisher),
				typeof (PersonPeriodChangedBusMessagePublisher),
				typeof (ScheduleChangedMessageSender)
			};

			var senders = allCallbacks.Current().ToList();
			Assert.AreEqual(senderTypes.Length, senders.Count);

			var allSenderExists = senderTypes.All(type => senders.Any(x => x.GetType() == type));
			Assert.AreEqual(allSenderExists, true);
		}
	}
}