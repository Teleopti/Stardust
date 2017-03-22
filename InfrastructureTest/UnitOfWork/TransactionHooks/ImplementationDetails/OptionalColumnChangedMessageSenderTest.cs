using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.TransactionHooks.ImplementationDetails
{
	[TestFixture]
	public class OptionalColumnChangedMessageSenderTest
	{
		private ITransactionHook _target;
		private MockRepository _mocks;
		private IEventPopulatingPublisher _serviceBusSender;
		private IBusinessUnit _businessUnit;

		[SetUp]
		public void Setup()
		{
			var currentBusinessUnit = new SpecificBusinessUnit(BusinessUnitFactory.CreateWithId("fakeBu"));
			_businessUnit = currentBusinessUnit.Current();
			_mocks = new MockRepository();
			_serviceBusSender = _mocks.DynamicMock<IEventPopulatingPublisher>();
			_target = new OptionalColumnCollectionChangedEventPublisher(_serviceBusSender, currentBusinessUnit);

		}

		[Test]
		public void ShouldPublishEventWhenOptionalColumnIsChanged()
		{
			var optionalColumn = new OptionalColumn("opt");
			var ids = new Guid[0];
			var message = new OptionalColumnCollectionChangedEvent(){ LogOnBusinessUnitId = _businessUnit.Id.Value };
			message.SetOptionalColumnIdCollection(ids);

			var roots = new IRootChangeInfo[1];
			roots[0] = new RootChangeInfo(optionalColumn, DomainUpdateType.Update);

			using (_mocks.Record())
			{
				Expect.Call(() => _serviceBusSender.Publish(message)).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				_target.AfterCompletion(roots);
			}
		}

		[Test]
		public void ShouldNotPublishEventIfNotOptionalColumnThatIsChanged()
		{
			var contract = new Contract("contract");
			var ids = new Guid[0];
			var message = new OptionalColumnCollectionChangedEvent() { LogOnBusinessUnitId = _businessUnit.Id.Value };
			message.SetOptionalColumnIdCollection(ids);

			var roots = new IRootChangeInfo[1];
			roots[0] = new RootChangeInfo(contract, DomainUpdateType.Update);

			using (_mocks.Record())
			{
				Expect.Call(() => _serviceBusSender.Publish(message)).IgnoreArguments().Repeat.Never();
			}
			using (_mocks.Playback())
			{
				_target.AfterCompletion(roots);
			}
		}
	}
}
