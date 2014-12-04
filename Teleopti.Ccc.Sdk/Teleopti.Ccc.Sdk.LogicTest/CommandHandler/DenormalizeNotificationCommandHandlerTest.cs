using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class DenormalizeNotificationCommandHandlerTest
    {
		private IMessagePopulatingServiceBusSender _busSender;
        private MockRepository _mock;
        private DenormalizeNotificationCommandHandler _target;
        private DenormalizeNotificationCommandDto _denormalizeNotificationCommandDto;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
			_busSender = _mock.StrictMock<IMessagePopulatingServiceBusSender>();
            _target = new DenormalizeNotificationCommandHandler(_busSender);
            _denormalizeNotificationCommandDto = new DenormalizeNotificationCommandDto();
        }

        [Test]
        public void ShouldProcessDenormalizeNotificationCommand()
        {
            using (_mock.Record())
            {
                Expect.Call(()=>_busSender.Send(new ProcessDenormalizeQueue(), true)).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.Handle(_denormalizeNotificationCommandDto);
            }
        }

        [Test]
        public void ShouldThrowExceptionIfServiceBusNotEnabled()
        {
            using (_mock.Record())
            {
                Expect.Call(() => _busSender.Send(new ProcessDenormalizeQueue(), true)).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.Handle(_denormalizeNotificationCommandDto);
            }
        }
    }
}
