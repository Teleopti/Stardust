using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.WinCode.Scheduling;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class AgentOvertimeAvailabilityPresenterTest
	{
		private AgentOvertimeAvailabilityPresenter _presenter;
		private IAgentOvertimeAvailabilityView _view;
		private MockRepository _mock;
		private IPerson _person;
		private DateOnly _dateOnly;
		private IScheduleDay _scheduleDay;
		private IAgentOvertimeAvailabilityAddCommand _addCommand;
		private IAgentOvertimeAvailabilityRemoveCommand _removeCommand;
		private IOvertimeAvailability _overtimeAvailabilityDay;
		private IAgentOvertimeAvailabilityEditCommand _editCommand;
		private IOvertimeAvailabilityCreator _dayCreator;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_view = _mock.StrictMock<IAgentOvertimeAvailabilityView>();
			_person = new Person();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_dateOnly = new DateOnly(2013, 1, 1);
			_overtimeAvailabilityDay = new OvertimeAvailability(_person, _dateOnly, TimeSpan.FromHours(8), TimeSpan.FromHours(18));
			_presenter = new AgentOvertimeAvailabilityPresenter(_view, _scheduleDay);
			_addCommand = _mock.StrictMock<IAgentOvertimeAvailabilityAddCommand>();
			_removeCommand = _mock.StrictMock<IAgentOvertimeAvailabilityRemoveCommand>();
			_editCommand = _mock.StrictMock<IAgentOvertimeAvailabilityEditCommand>();
			_dayCreator = _mock.StrictMock<IOvertimeAvailabilityCreator>();
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
			var overtimeAvailabilityDay = new OvertimeAvailability(_person, _dateOnly, TimeSpan.FromHours(8), TimeSpan.FromHours(20));
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { overtimeAvailabilityDay }));
				Expect.Call(() => _view.Update(TimeSpan.FromHours(8),  TimeSpan.FromHours(20)));
			}

			using (_mock.Playback())
			{
				_presenter.UpdateView();
			}
		}

		[Test]
		public void ShouldAddOvertimeAvailabilityDay()
		{
			using (_mock.Record())
			{
				Expect.Call(() => _addCommand.Execute());
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _overtimeAvailabilityDay }));
				Expect.Call(() => _view.Update(_overtimeAvailabilityDay.StartTime, _overtimeAvailabilityDay.EndTime));
			}

			using (_mock.Playback())
			{
				_presenter.Add(_addCommand);	
			}
		}

		[Test]
		public void ShouldRemoveOvertimeAvailabilityDay()
		{
			using (_mock.Record())
			{
				Expect.Call(() => _removeCommand.Execute());
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>()));
				Expect.Call(() => _view.Update(null, null));
			}

			using (_mock.Playback())
			{
				_presenter.Remove(_removeCommand);
			}
		}

		[Test]
		public void ShouldEditOvertimeAvailabilityDay()
		{
			using (_mock.Record())
			{
				Expect.Call(() => _editCommand.Execute());
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _overtimeAvailabilityDay }));
				Expect.Call(() => _view.Update(_overtimeAvailabilityDay.StartTime, _overtimeAvailabilityDay.EndTime));
			}

			using (_mock.Playback())
			{
				_presenter.Edit(_editCommand);
			}
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenNullCommandEdit()
		{
			_presenter.Edit(null);	
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenNullCommandAdd()
		{
			_presenter.Add(null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenNullCommandRemove()
		{
			_presenter.Remove(null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenNullDayCreator()
		{
			_presenter.CommandToExecute(TimeSpan.FromHours(1), TimeSpan.FromHours(2), null);
		}

		[Test]
		public void ShouldRemoveWhenExistingAndNoStartEndTime()
		{
			using (_mock.Record())
			{
				bool startError;
				bool endError;

				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _overtimeAvailabilityDay }));
				Expect.Call(_dayCreator.CanCreate(null, null, out startError, out endError)).OutRef(true, true).Return(false);
			}

			using (_mock.Playback())
			{
				var toExecute = _presenter.CommandToExecute(null, null, _dayCreator);
				Assert.AreEqual(AgentOvertimeAvailabilityExecuteCommand.Remove, toExecute);
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
				Assert.AreEqual(AgentOvertimeAvailabilityExecuteCommand.Add, toExecute);
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
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _overtimeAvailabilityDay }));
				Expect.Call(_dayCreator.CanCreate(startTime, endTime, out startError, out endError)).Return(true);
			}

			using (_mock.Playback())
			{
				var toExecute = _presenter.CommandToExecute(startTime, endTime, _dayCreator);
				Assert.AreEqual(AgentOvertimeAvailabilityExecuteCommand.Edit, toExecute);
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
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _overtimeAvailabilityDay }));
				Expect.Call(_dayCreator.CanCreate(startTime, endTime, out startError, out endError)).OutRef(true, false).Return(false);
			}

			using (_mock.Playback())
			{
				var toExecute = _presenter.CommandToExecute(startTime, endTime, _dayCreator);
				Assert.AreEqual(AgentOvertimeAvailabilityExecuteCommand.None, toExecute);
			}	
		}
	}
}
