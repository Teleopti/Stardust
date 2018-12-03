using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Restriction;


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
		private IAgentOvertimeAvailabilityCommand _command;
		private IOvertimeAvailability _overtimeAvailabilityDay;
		private IOvertimeAvailabilityCreator _dayCreator;
		private IProjectionService _projectionService;
		private IVisualLayerCollection _visualLayerCollection;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_view = _mock.StrictMock<IAgentOvertimeAvailabilityView>();
			_person = PersonFactory.CreatePerson("bill");
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_dateOnly = new DateOnly(2013, 1, 1);
			_overtimeAvailabilityDay = new OvertimeAvailability(_person, _dateOnly, TimeSpan.FromHours(8), TimeSpan.FromHours(18));
			_presenter = new AgentOvertimeAvailabilityPresenter(_view, _scheduleDay, new SchedulingResultStateHolder(), new DoNothingScheduleDayChangeCallBack());
			_command = _mock.StrictMock<IAgentOvertimeAvailabilityCommand>();
			_dayCreator = _mock.StrictMock<IOvertimeAvailabilityCreator>();
			_projectionService = _mock.StrictMock<IProjectionService>();
			_visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();
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
				Expect.Call(() => _view.Update(TimeSpan.FromHours(8), TimeSpan.FromHours(20))).IgnoreArguments();
			}

			using (_mock.Playback())
			{
				_presenter.UpdateView();
			}
		}

		[Test]
		public void ShouldRunOvertimeAvailabilityDayCommand()
		{
			addPeriodAndContractToPerson();
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(null);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_dateOnly, TimeZoneInfo.Utc));
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(() => _command.Execute());
			}

			using (_mock.Playback())
			{
				_presenter.Initialize();
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

				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _overtimeAvailabilityDay }));
				Expect.Call(_dayCreator.CanCreate(null, null, out startError, out endError)).OutRef(true, true).Return(false);
			}

			using (_mock.Playback())
			{
				var toExecute = _presenter.CommandToExecute(null, null, _dayCreator);
				Assert.IsInstanceOf<AgentOvertimeAvailabilityRemoveCommand>(toExecute);
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
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>()));
				Expect.Call(_dayCreator.CanCreate(startTime, endTime, out startError, out endError)).Return(true);
			}

			using (_mock.Playback())
			{
				var toExecute = _presenter.CommandToExecute(startTime, endTime, _dayCreator);
				Assert.IsInstanceOf<AgentOvertimeAvailabilityAddCommand>(toExecute);
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
				Assert.IsInstanceOf<AgentOvertimeAvailabilityEditCommand>(toExecute);
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
				Assert.IsNull(toExecute);
			}
		}

		private void addPeriodAndContractToPerson()
		{
			var schedWeek = new ContractScheduleWeek();
			var period = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1999, 1, 1));
			_person.AddPersonPeriod(period);
			period.PersonContract.ContractSchedule.AddContractScheduleWeek(schedWeek);
			schedWeek.Add(DayOfWeek.Monday, true);
			schedWeek.Add(DayOfWeek.Tuesday, true);
			schedWeek.Add(DayOfWeek.Wednesday, true);
			schedWeek.Add(DayOfWeek.Thursday, true);
			schedWeek.Add(DayOfWeek.Friday, true);
			schedWeek.Add(DayOfWeek.Saturday, true);
			schedWeek.Add(DayOfWeek.Sunday, true);
		}
	}
}
