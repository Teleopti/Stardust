using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class DenormalizeScheduleCommandHandlerTest
    {
        private IServiceBusSender _busSender;
        private MockRepository _mock;
        private DenormalizeScheduleCommandHandler _target;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _busSender = _mock.StrictMock<IServiceBusSender>();
            _target = new DenormalizeScheduleCommandHandler(_busSender);
        }

        [Test]
        public void ShouldProcessDenormalizeScheduleCommand()
        {
            var denormalizeScheduleCommandDto = new DenormalizeScheduleCommandDto();

            using (_mock.Record())
            {
                Expect.Call(_busSender.EnsureBus()).Return(true);
                Expect.Call(() => _busSender.NotifyServiceBus(new DenormalizeScheduleProjection())).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.Handle(denormalizeScheduleCommandDto);
            }
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowExceptionIfServiceBusNotEnabled()
        {
            var denormalizeScheduleCommandDto = new DenormalizeScheduleCommandDto();

            using (_mock.Record())
            {
                Expect.Call(_busSender.EnsureBus()).Return(false);
                Expect.Call(() => _busSender.NotifyServiceBus(new DenormalizeScheduleProjection())).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.Handle(denormalizeScheduleCommandDto);
            }
        }
    }
}
