using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Meetings.Overview
{
    [TestFixture]
    public class ExportMeetingsProviderTest
    {
        private MockRepository _mocks;
        private ExportMeetingsProvider _target;
        private IMeetingRepository _meetingRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IMeetingOverviewViewModel _model;
        private IUnitOfWork _unitOfWork;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _meetingRepository = _mocks.StrictMock<IMeetingRepository>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _model = _mocks.StrictMock<IMeetingOverviewViewModel>();
            _target = new ExportMeetingsProvider(_meetingRepository, _unitOfWorkFactory, _model);
            _unitOfWork = _mocks.StrictMock<IUnitOfWork>();
        }

        [Test]
        public void ShouldGetMeetingsFromRepositoryAndReturnTheOnesWithDatesInPeriod()
        {
            var start = new DateOnly(2011, 5, 9);
            var end = new DateOnly(2011, 5, 15);
            var fromScenario = _mocks.StrictMock<IScenario>();
            var dateOnlyPeriod = new DateOnlyPeriod(start, end);
            var dateTimePeriod =
                dateOnlyPeriod.ToDateTimePeriod(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
            var meeting = _mocks.StrictMock<IMeeting>();
            var meetings = new List<IMeeting> { meeting };
            var meetingId = Guid.NewGuid();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
            Expect.Call(_model.CurrentScenario).Return(fromScenario);
            Expect.Call(_meetingRepository.Find(dateTimePeriod, fromScenario)).Return(meetings);
            Expect.Call(meeting.Id).Return(meetingId).Repeat.Any();
			var meetingPersons = new List<IMeetingPerson>
			{
				new MeetingPerson(
					PersonFactory.CreatePersonWithGuid("FN", "LN"), true)
			};
			Expect.Call(meeting.MeetingPersons).Return(meetingPersons).Repeat.AtLeastOnce();
            Expect.Call(() => _unitOfWork.Dispose());
            _mocks.ReplayAll();
             var ret = _target.GetMeetings(dateOnlyPeriod);
            Assert.That(ret.Count,Is.EqualTo(1));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(ret.First().MeetingPersons.First()));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldGetMeetingsFromRepositoryAndReturnNoneIfNoOneInPeriod()
        {
            var start = new DateOnly(2011, 5, 9);
            var end = new DateOnly(2011, 5, 15);
            var fromScenario = _mocks.StrictMock<IScenario>();
            var dateOnlyPeriod = new DateOnlyPeriod(start, end);
            var dateTimePeriod =
                dateOnlyPeriod.ToDateTimePeriod(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
            var meeting = _mocks.StrictMock<IMeeting>();
            var meetings = new List<IMeeting> { meeting };
            var meetingId = Guid.NewGuid();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
            Expect.Call(_model.CurrentScenario).Return(fromScenario);
            Expect.Call(_meetingRepository.Find(dateTimePeriod, fromScenario)).Return(meetings);
            Expect.Call(meeting.Id).Return(meetingId).Repeat.Any();
			var meetingPersons = new List<IMeetingPerson>
			{
				new MeetingPerson(
					PersonFactory.CreatePersonWithGuid("FN", "LN"), true)
			};
			Expect.Call(meeting.MeetingPersons).Return(meetingPersons).Repeat.AtLeastOnce();
			Expect.Call(() => _unitOfWork.Dispose());
            _mocks.ReplayAll();
            var ret = _target.GetMeetings(dateOnlyPeriod);
            Assert.That(ret.Count, Is.EqualTo(1));
            _mocks.VerifyAll();
        }
    }
}