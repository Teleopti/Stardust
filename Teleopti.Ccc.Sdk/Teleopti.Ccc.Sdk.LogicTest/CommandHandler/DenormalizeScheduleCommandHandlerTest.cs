﻿using System.ServiceModel;
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
    public class DenormalizeScheduleCommandHandlerTest
    {
        private IServiceBusSender _busSender;
        private MockRepository _mock;
        private DenormalizeScheduleCommandHandler _target;
        private DenormalizeScheduleCommandDto _denormalizeScheduleCommandDto;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _busSender = _mock.StrictMock<IServiceBusSender>();
            _target = new DenormalizeScheduleCommandHandler(_busSender);
            _denormalizeScheduleCommandDto = new DenormalizeScheduleCommandDto();
        }

        [Test]
        public void ShouldProcessDenormalizeScheduleCommand()
        {
            using (_mock.Record())
            {
                Expect.Call(_busSender.EnsureBus()).Return(true);
                Expect.Call(() => _busSender.Send(new ScheduleChanged())).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.Handle(_denormalizeScheduleCommandDto);
            }
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowExceptionIfServiceBusNotEnabled()
        {
            using (_mock.Record())
            {
                Expect.Call(_busSender.EnsureBus()).Return(false);
                Expect.Call(() => _busSender.Send(new ScheduleChanged())).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.Handle(_denormalizeScheduleCommandDto);
            }
        }
    }
}
