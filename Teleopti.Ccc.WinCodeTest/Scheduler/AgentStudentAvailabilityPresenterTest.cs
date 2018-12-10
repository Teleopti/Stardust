using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class AgentStudentAvailabilityPresenterTest
	{
		private AgentStudentAvailabilityPresenter _presenter;
		private IAgentStudentAvailabilityView _view;
		private MockRepository _mock;
		private IPerson _person;
		private DateOnly _dateOnly;
		private IStudentAvailabilityRestriction _studentAvailabilityRestriction;
		private IList<IStudentAvailabilityRestriction> _studentAvailabilityRestrictions;
		private IScheduleDay _scheduleDay;
		private IExecutableCommand _command;
		private IStudentAvailabilityDay _studentAvailabilityDay;
		private IAgentStudentAvailabilityDayCreator _dayCreator;
	    private ISchedulingResultStateHolder _schedulingResultStateHolder;

	    [SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_view = _mock.StrictMock<IAgentStudentAvailabilityView>();
			_person = new Person();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
            _schedulingResultStateHolder = _mock.DynamicMock<ISchedulingResultStateHolder>();
			_dateOnly = new DateOnly(2013, 1, 1);
			_studentAvailabilityRestriction = new StudentAvailabilityRestriction();
			_studentAvailabilityRestriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), null);
			_studentAvailabilityRestriction.EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(10));
			_studentAvailabilityRestrictions = new List<IStudentAvailabilityRestriction>{_studentAvailabilityRestriction};
			_studentAvailabilityDay = new StudentAvailabilityDay(_person, _dateOnly, _studentAvailabilityRestrictions);
			_presenter = new AgentStudentAvailabilityPresenter(_view, _scheduleDay, _schedulingResultStateHolder, new DoNothingScheduleDayChangeCallBack());
			_command = _mock.StrictMock<IAgentStudentAvailabilityCommand>();
			_dayCreator = _mock.StrictMock<IAgentStudentAvailabilityDayCreator>();
		}

		[Test]
		public void ShouldCreatePresenter()
		{
			Assert.AreEqual(_presenter.View, _view);
			Assert.AreEqual(_presenter.ScheduleDay, _scheduleDay);
		}

		[Test]
		public void ShouldUpdateView()
		{
			var studentAvailabilityRestriction = new StudentAvailabilityRestriction();
			studentAvailabilityRestriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), null);
			studentAvailabilityRestriction.EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(25));
			var studentAvailabilityRestrictions = new List<IStudentAvailabilityRestriction> { studentAvailabilityRestriction };
			var studentAvailabilityDay = new StudentAvailabilityDay(_person, _dateOnly, studentAvailabilityRestrictions);
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { studentAvailabilityDay }));
				Expect.Call(() => _view.Update(studentAvailabilityRestriction.StartTimeLimitation.StartTime, studentAvailabilityRestriction.EndTimeLimitation.EndTime));
			}

			using (_mock.Playback())
			{
				_presenter.UpdateView();
			}
		}

		[Test]
		public void ShouldRunStudentAvailabilityDayCommand()
		{
			using (_mock.Record())
			{
				Expect.Call(() => _command.Execute());
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _studentAvailabilityDay }));
				Expect.Call(() => _view.Update(_studentAvailabilityRestriction.StartTimeLimitation.StartTime, _studentAvailabilityRestriction.EndTimeLimitation.EndTime));
			}

			using (_mock.Playback())
			{
				_presenter.RunCommand(_command);	
			}
		}

		[Test]
		public void ShouldThrowExceptionWhenNullCommand()
		{
		    Assert.Throws<ArgumentNullException>(() => _presenter.RunCommand(null));
		}

		[Test]
		public void ShouldThrowExceptionWhenNullDayCreator()
		{
			Assert.Throws<ArgumentNullException>(() => _presenter.CommandToExecute(TimeSpan.FromHours(1), TimeSpan.FromHours(2), null));
		}

		[Test]
		public void ShouldRemoveWhenExistingAndNoStartEndTime()
		{
			using (_mock.Record())
			{
				bool startError;
				bool endError;

				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _studentAvailabilityDay }));
				Expect.Call(_dayCreator.CanCreate(null, null, out startError, out endError)).OutRef(true, true).Return(false);
			}

			using (_mock.Playback())
			{
				var toExecute = _presenter.CommandToExecute(null, null, _dayCreator);
				Assert.IsInstanceOf<AgentStudentAvailabilityRemoveCommand>(toExecute);
			}
		}

		[Test]
		public void ShouldAddWhenNoExisting()
		{
			var startTime = TimeSpan.FromHours(1);
			var endTime = TimeSpan.FromHours(2);

			using (_mock.Record())
			{
				bool startError;
				bool endError;
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> ()));
				Expect.Call(_dayCreator.CanCreate(startTime, endTime, out startError, out endError)).Return(true);
			}

			using (_mock.Playback())
			{
				var toExecute = _presenter.CommandToExecute(startTime, endTime, _dayCreator);
                Assert.IsInstanceOf<AgentStudentAvailabilityAddCommand>(toExecute);
			}	
		}

		[Test]
		public void ShouldEditWhenExisting()
		{
			var startTime = TimeSpan.FromHours(1);
			var endTime = TimeSpan.FromHours(2);

			using (_mock.Record())
			{
				bool startError;
				bool endError;
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _studentAvailabilityDay }));
				Expect.Call(_dayCreator.CanCreate(startTime, endTime, out startError, out endError)).Return(true);
			}

			using (_mock.Playback())
			{
				var toExecute = _presenter.CommandToExecute(startTime, endTime, _dayCreator);
                Assert.IsInstanceOf<AgentStudentAvailabilityEditCommand>(toExecute);
			}		
		}

		[Test]
		public void ShouldNoneWhenNotValidTimes()
		{
			var startTime = TimeSpan.FromHours(1);
			var endTime = TimeSpan.FromHours(1);

			using (_mock.Record())
			{
				bool startError;
				bool endError;
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _studentAvailabilityDay }));
				Expect.Call(_dayCreator.CanCreate(startTime, endTime, out startError, out endError)).OutRef(true, false).Return(false);
			}

			using (_mock.Playback())
			{
				var toExecute = _presenter.CommandToExecute(startTime, endTime, _dayCreator);
                Assert.IsNull(toExecute);
			}	
		}
	}
}
