using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class DenormalizeNotificationCommandHandlerTest
    {
		private IServiceBusEventPublisher _busSender;
        private MockRepository _mock;
        private DenormalizeNotificationCommandHandler _target;
        private DenormalizeNotificationCommandDto _denormalizeNotificationCommandDto;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
			_busSender = _mock.StrictMock<IServiceBusEventPublisher>();
            _target = new DenormalizeNotificationCommandHandler(_busSender);
            _denormalizeNotificationCommandDto = new DenormalizeNotificationCommandDto();
        }

        [Test]
        public void ShouldProcessDenormalizeNotificationCommand()
        {
            using (_mock.Record())
            {
                Expect.Call(_busSender.EnsureBus()).Return(true);
                Expect.Call(()=>_busSender.Publish(new ProcessDenormalizeQueue())).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.Handle(_denormalizeNotificationCommandDto);
            }
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowExceptionIfServiceBusNotEnabled()
        {
            using (_mock.Record())
            {
                Expect.Call(_busSender.EnsureBus()).Return(false);
                Expect.Call(() => _busSender.Publish(new ProcessDenormalizeQueue())).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.Handle(_denormalizeNotificationCommandDto);
            }
        }
    }
}
