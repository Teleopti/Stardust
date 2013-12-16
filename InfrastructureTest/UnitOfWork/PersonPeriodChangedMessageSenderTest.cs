using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class PersonPeriodChangedMessageSenderTest
	{
		private IMessageSender _target;
		private MockRepository _mocks;
		private IServiceBusSender _serviceBusSender;
		
		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_serviceBusSender = _mocks.DynamicMock<IServiceBusSender>();
			_target = new PersonPeriodChangedMessageSender(_serviceBusSender);
		}

        [Test]
        public void ShouldSaveRebuildReadModelForOthersToQueue()
        {
            var contract = new Contract("c");
            var ids = new[] {Guid.NewGuid()};

            var message = new PersonChangedMessage();
            message.SetPersonIdCollection(ids);
            
           var roots = new IRootChangeInfo[1];
            roots[0] = new RootChangeInfo(contract, DomainUpdateType.Update);

            using (_mocks.Record())
            {
				Expect.Call(_serviceBusSender.EnsureBus()).Return(true);
	            Expect.Call(() => _serviceBusSender.Send(message)).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
				_target.Execute(roots);
            }
        }

        [Test]
        public void ShouldNotRebuildReadModelForScenario()
        {
            var scenario = _mocks.DynamicMock<IScenario>();

            using (_mocks.Record())
            {
            }
            using (_mocks.Playback())
            {
				_target.Execute(new IRootChangeInfo[] { new RootChangeInfo(scenario, DomainUpdateType.Insert) });
            }
        }
	}
}