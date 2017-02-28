﻿using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class DenormalizeScheduleCommandHandlerTest
    {
		private IMessagePopulatingServiceBusSender _busSender;
        private MockRepository _mock;
        private DenormalizeScheduleCommandHandler _target;
        private DenormalizeScheduleCommandDto _denormalizeScheduleCommandDto;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
			_busSender = _mock.StrictMock<IMessagePopulatingServiceBusSender>();
            _target = new DenormalizeScheduleCommandHandler(_busSender);
            _denormalizeScheduleCommandDto = new DenormalizeScheduleCommandDto();
        }

        [Test]
        public void ShouldProcessDenormalizeScheduleCommand()
        {
            using (_mock.Record())
            {
                Expect.Call(() => _busSender.Send(new ScheduleChangedEvent(), true)).IgnoreArguments();
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
                Expect.Call(() => _busSender.Send(new ScheduleChangedEvent(), true)).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.Handle(_denormalizeScheduleCommandDto);
            }
        }
    }
}
