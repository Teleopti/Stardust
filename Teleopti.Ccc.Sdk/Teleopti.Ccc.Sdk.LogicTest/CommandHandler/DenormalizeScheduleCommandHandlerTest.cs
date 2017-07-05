using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class DenormalizeScheduleCommandHandlerTest
    {
		private IEventPopulatingPublisher _busSender;
        private MockRepository _mock;
        private DenormalizeScheduleCommandHandler _target;
        private DenormalizeScheduleCommandDto _denormalizeScheduleCommandDto;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
			_busSender = _mock.StrictMock<IEventPopulatingPublisher>();
            _target = new DenormalizeScheduleCommandHandler(_busSender);
            _denormalizeScheduleCommandDto = new DenormalizeScheduleCommandDto();
        }

        [Test]
        public void ShouldProcessDenormalizeScheduleCommand()
        {
            using (_mock.Record())
            {
                Expect.Call(() => _busSender.Publish(new ScheduleChangedEvent())).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.Handle(_denormalizeScheduleCommandDto);
            }
        }

        [Test]
        public void ShouldThrowExceptionIfServiceBusNotEnabled()
        {
            using (_mock.Record())
            {
                Expect.Call(() => _busSender.Publish(new ScheduleChangedEvent())).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.Handle(_denormalizeScheduleCommandDto);
            }
        }
    }
}
