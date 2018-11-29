using System;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;


namespace Teleopti.Ccc.WinCodeTest.Meetings.Commands
{
    [TestFixture]
    public class ExportMeetingCommandTest
    {
        private MockRepository _mocks;
        private ExportMeetingCommand _target;
        private IExportMeetingView _exportMeetingView;
        private IMeetingRepository _meetingRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IExportMeetingsProvider _exportMeetingsProvider;
        private IEventAggregator _eventAggregator;
        private IScenario _toScenario;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _exportMeetingView = _mocks.StrictMock<IExportMeetingView>();
            _meetingRepository = _mocks.StrictMock<IMeetingRepository>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _exportMeetingsProvider = _mocks.StrictMock<IExportMeetingsProvider>();
            _eventAggregator = new EventAggregator();
            _target = new ExportMeetingCommand(_exportMeetingView, _meetingRepository, _unitOfWorkFactory,
                                               _exportMeetingsProvider, _eventAggregator);
            _toScenario = _mocks.StrictMock<IScenario>();
        }

        [Test]
        public void CanExecuteShouldBeFalseIfScenarioIsNullIncorrect()
        {
            Expect.Call(_exportMeetingView.SelectedScenario).Return(null);
            _mocks.ReplayAll();
            _target.CanExecute().Should().Be.False();
            _mocks.VerifyAll();
        }

        [Test]
        public void CanExecuteShouldBeFalseIfDatesAreIncorrect()
        {
            Expect.Call(_exportMeetingView.SelectedScenario).Return(_toScenario);
            Expect.Call(_exportMeetingView.SelectedDates).Return(new List<DateOnlyPeriod>());
            _mocks.ReplayAll();
            _target.CanExecute().Should().Be.False();
            _mocks.VerifyAll();
        }

        [Test]
        public void CanExecuteShouldBeTrueIfScenarioAndDatesAreCorrect()
        {
            Expect.Call(_exportMeetingView.SelectedScenario).Return(_toScenario);
            Expect.Call(_exportMeetingView.SelectedDates).Return(new List<DateOnlyPeriod>{new DateOnlyPeriod()});
            _mocks.ReplayAll();
            _target.CanExecute().Should().Be.True();
            _mocks.VerifyAll();
        }

        [Test]
        public void ExecuteShouldNotRunIfNotCanExecute()
        {
            Expect.Call(_exportMeetingView.SelectedScenario).Return(_toScenario);
            Expect.Call(_exportMeetingView.SelectedDates).Return(new List<DateOnlyPeriod>());
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();
        }

        [Test]
        public void ExecuteShouldBeDeletingAllEarlierConflictingExportsAndExport()
        {
            var start = new DateOnly(2011, 5, 9);
            var end = new DateOnly(2011, 5, 15);
            var id = Guid.NewGuid();
            var dateOnlyPeriod = new DateOnlyPeriod(start,end);
            var meeting = _mocks.StrictMock<IMeeting>();
            var meetings = new List<IMeeting> {meeting};
			var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();

        	using (_mocks.Record())
        	{
        		Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
        		Expect.Call(_exportMeetingView.SelectedDates).Return(new List<DateOnlyPeriod> {dateOnlyPeriod}).Repeat.Twice();
        		Expect.Call(_exportMeetingView.SelectedScenario).Return(_toScenario).Repeat.Twice();
        		Expect.Call(_exportMeetingsProvider.GetMeetings(dateOnlyPeriod)).Return(meetings);
        		Expect.Call(_meetingRepository.FindMeetingsWithTheseOriginals(meetings, _toScenario)).Return(meetings);
        		Expect.Call(() => _meetingRepository.Remove(meeting));
        		Expect.Call(meeting.NoneEntityClone()).Return(meeting);
        		Expect.Call(() => meeting.SetScenario(_toScenario));
        		Expect.Call(meeting.OriginalMeetingId).Return(id);
        		Expect.Call(() => meeting.OriginalMeetingId = id);
        		Expect.Call(() => _meetingRepository.Add(meeting));
        		Expect.Call(() => unitOfWork.PersistAll());
        		Expect.Call(meeting.Snapshot);
        	}
        	using (_mocks.Playback())
			{
				_target.Execute();
        	}
        }
    }
}