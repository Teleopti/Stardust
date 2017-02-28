using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[TestFixture]
	public class AbsenceRequestQueueStrategyHandlerTest : ISetup
	{
		public FakeQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public AbsenceRequestQueueStrategyHandler Target;
		public IMutateNow Now;
		public FakeEventPublisher Publisher;
		public FakeBusinessUnitRepository FakeBusinessUnitRepository;
		public FakePersonRepository FakePersonRepository;
		private DateTime _now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<AbsenceRequestStrategyProcessor>().For<IAbsenceRequestStrategyProcessor>();
			var mutableNow = new MutableNow();
			mutableNow.Is("2016-03-01 10:00");
			system.UseTestDouble(mutableNow).For<INow>();
			_now = new DateTime(2016, 03, 01, 10, 0, 0, DateTimeKind.Utc);
		}

		[Test]
		public void ShouldPublishMultiEvent()
		{
			var person = new Person().WithName(new Name("Reko", "kille"));
			person.SetId(SystemUser.Id);
			FakePersonRepository.Add(person);
			FakeBusinessUnitRepository.Add(new Domain.Common.BusinessUnit("BU"));

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 9,38, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});
			
			Target.Handle(new TenantMinuteTickEvent());

			Publisher.PublishedEvents.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldResendTimedOutRequests()
		{
			var person = new Person().WithName(new Name("Reko", "kille"));
			person.SetId(SystemUser.Id);
			FakePersonRepository.Add(person);
			FakeBusinessUnitRepository.Add(new Domain.Common.BusinessUnit("BU"));

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-100),
				Sent = _now.AddMinutes(-95),
				PersonRequest = Guid.NewGuid()
			});

			Target.Handle(new TenantMinuteTickEvent());

			Publisher.PublishedEvents.Count().Should().Be.EqualTo(1);

		}

		[Test]
		public void ShouldRemove00000RequestsForCurrentPeriod()
		{
			var person = new Person().WithName(new Name("Reko", "kille"));
			person.SetId(SystemUser.Id);
			FakePersonRepository.Add(person);
			FakeBusinessUnitRepository.Add(new Domain.Common.BusinessUnit("BU"));

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-100),
				Sent = null,
				PersonRequest = Guid.NewGuid()
			});
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-100),
				Sent = null,
				PersonRequest = Guid.Empty
			});
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 6, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 6, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-100),
				Sent = null,
				PersonRequest = Guid.Empty
			});

			Target.Handle(new TenantMinuteTickEvent());

			var req = QueuedAbsenceRequestRepository.LoadAll();
			req.Count.Should().Be.EqualTo(2);
			req.Count(x => x.PersonRequest == Guid.Empty && x.StartDateTime == new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc)).Should().Be.EqualTo(0);
		}
	}
}
