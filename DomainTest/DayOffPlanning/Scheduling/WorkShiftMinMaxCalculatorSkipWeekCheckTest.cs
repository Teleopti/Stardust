using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning.Scheduling
{
	[TestFixture]
	public class WorkShiftMinMaxCalculatorSkipWeekCheckTest
	{

		private WorkShiftMinMaxCalculatorSkipWeekCheck _target;
		private MockRepository _mocks;
		private IScheduleMatrixPro _matrix;
		private IPerson _person;
		private IPersonPeriod _personPeriodOne;
		private IPersonPeriod _personPeriodTwo;
		private IPersonContract _personContractOne;
		private IPersonContract _personContractTwo;
		private IContract _contractOne;
		private IContract _contractTwo;
		private IVirtualSchedulePeriod _schedulePeriod;
		private DateOnly _dateToScheck;


		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_person = _mocks.Stub<IPerson>();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_personPeriodOne = _mocks.Stub<IPersonPeriod>();
			_personPeriodTwo = _mocks.Stub<IPersonPeriod>();
			_personContractOne = _mocks.Stub<IPersonContract>();
			_personContractTwo = _mocks.Stub<IPersonContract>();
			_contractOne = _mocks.Stub<IContract>();
			_contractTwo = _mocks.Stub<IContract>();
			_schedulePeriod = _mocks.Stub<IVirtualSchedulePeriod>();
			_person.FirstDayOfWeek = DayOfWeek.Monday;

			_target = new WorkShiftMinMaxCalculatorSkipWeekCheck();
		}

		[Test]
		public void WeekShouldNotSkipWhenOnePersonAndOneSchedulePeriod()
		{
			_dateToScheck = new DateOnly(new DateTime(2014, 10, 30));
			_personPeriodOne.StartDate = new DateOnly(new DateTime(2014, 10, 1));
			var schedulePeriodDatePeriod = new DateOnlyPeriod(new DateOnly(2014, 10, 1), new DateOnly(2014, 12, 1));

			using (_mocks.Record())
			{
				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_person.Period(_dateToScheck)).Return(_personPeriodOne);
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(schedulePeriodDatePeriod);
			}
			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.SkipWeekCheck(_matrix, _dateToScheck));
			}
		}

		[Test]
		public void WeekShouldNotSkipWhenTwoValidSchedulePeriods()
		{
			_dateToScheck = new DateOnly(new DateTime(2014, 10, 30));
			_personPeriodOne.StartDate = new DateOnly(new DateTime(2014, 10, 1));
			var schedulePeriodPrevious = _mocks.Stub<IVirtualSchedulePeriod>();
			var schedulePeriodDatePeriodPrevious = new DateOnlyPeriod(new DateOnly(2014, 10, 1), new DateOnly(2014, 10, 31));
			var schedulePeriodDatePeriod = new DateOnlyPeriod(new DateOnly(2014, 11, 1), new DateOnly(2014, 11, 30));

			using (_mocks.Record())
			{
				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_person.Period(_dateToScheck)).Return(_personPeriodOne);
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(schedulePeriodDatePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(schedulePeriodDatePeriod);
				Expect.Call(_person.VirtualSchedulePeriod(new DateOnly(2014, 10, 31)))
					.Return(schedulePeriodPrevious);
				Expect.Call(schedulePeriodPrevious.DateOnlyPeriod).Return(schedulePeriodDatePeriodPrevious);
				Expect.Call(schedulePeriodPrevious.IsValid).Return(true);
			}
			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.SkipWeekCheck(_matrix, _dateToScheck));
			}
		}

		[Test]
		public void WeekShouldSkipWhenPreviousSchedulePeriodIsInvalid()
		{
			_dateToScheck = new DateOnly(new DateTime(2014, 10, 30));
			_personPeriodOne.StartDate = new DateOnly(new DateTime(2014, 10, 1));
			var schedulePeriodPrevious = _mocks.Stub<IVirtualSchedulePeriod>();
			var schedulePeriodDatePeriodPrevious = new DateOnlyPeriod(new DateOnly(2014, 10, 1), new DateOnly(2014, 10, 31));
			var schedulePeriodDatePeriod = new DateOnlyPeriod(new DateOnly(2014, 11, 1), new DateOnly(2014, 11, 30));

			using (_mocks.Record())
			{
				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_person.Period(_dateToScheck)).Return(_personPeriodOne);
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(schedulePeriodDatePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(schedulePeriodDatePeriod);
				Expect.Call(_person.VirtualSchedulePeriod(new DateOnly(2014, 10, 31)))
					.Return(schedulePeriodPrevious);
				Expect.Call(schedulePeriodPrevious.DateOnlyPeriod).Return(schedulePeriodDatePeriodPrevious);
				Expect.Call(schedulePeriodPrevious.IsValid).Return(false);
			}
			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.SkipWeekCheck(_matrix, _dateToScheck));
			}
		}

		[Test]
		public void WeekShouldSkipWhenNextSchedulePeriodIsInvalid()
		{
			_dateToScheck = new DateOnly(new DateTime(2014, 10, 30));
			_personPeriodOne.StartDate = new DateOnly(new DateTime(2014, 10, 1));
			var schedulePeriodNext = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var schedulePeriodDatePeriod = new DateOnlyPeriod(new DateOnly(2014, 10, 1), new DateOnly(2014, 10, 31));

			using (_mocks.Record())
			{
				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_person.Period(_dateToScheck)).Return(_personPeriodOne);
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(schedulePeriodDatePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(schedulePeriodDatePeriod);
				Expect.Call(_person.VirtualSchedulePeriod(new DateOnly(2014, 11, 1)))
					.Return(schedulePeriodNext);
				Expect.Call(schedulePeriodNext.IsValid).Return(false);
			}
			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.SkipWeekCheck(_matrix, _dateToScheck));
			}
		}

		[Test]
		public void WeekShouldNotSkipWhenMaxTimePerWeekNotChanges()
		{
			_dateToScheck = new DateOnly(new DateTime(2014, 10, 29));
			_personPeriodOne.StartDate = new DateOnly(new DateTime(2014, 10, 1));
			var schedulePeriodDatePeriod = new DateOnlyPeriod(new DateOnly(2014, 10, 1), new DateOnly(2014, 12, 1));

			const int maxHours = 40;

			var maxTimePerWeekOne = TimeSpan.FromHours(maxHours);
			var workTimeDirectiveOne = new WorkTimeDirective(TimeSpan.Zero, maxTimePerWeekOne, TimeSpan.Zero, TimeSpan.Zero);
			var maxTimePerWeekTwo = TimeSpan.FromHours(maxHours);
			var workTimeDirectiveTwo = new WorkTimeDirective(TimeSpan.Zero, maxTimePerWeekTwo, TimeSpan.Zero, TimeSpan.Zero);

			_contractOne.WorkTimeDirective = workTimeDirectiveOne;
			_personContractOne.Contract = _contractOne;
			_personPeriodOne.PersonContract = _personContractOne;
			_personPeriodOne.StartDate = new DateOnly(new DateTime(2014, 10, 1));

			_contractTwo.WorkTimeDirective = workTimeDirectiveTwo;
			_personContractTwo.Contract = _contractTwo;
			_personPeriodTwo.PersonContract = _personContractTwo;
			_personPeriodTwo.StartDate = new DateOnly(new DateTime(2014, 11, 1));


			using (_mocks.Record())
			{
				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_person.Period(_dateToScheck)).Return(_personPeriodOne);
				Expect.Call(_person.NextPeriod(_personPeriodOne)).Return(_personPeriodTwo);

				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(schedulePeriodDatePeriod);
			}
			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.SkipWeekCheck(_matrix, _dateToScheck));
			}
		}

		[Test]
		public void WeekShouldSkipWhenStartPersonPeriodAndMaxTimePerWeekChanges()
		{
			_dateToScheck = new DateOnly(new DateTime(2014, 10, 29));

			const int maxHoursOne = 40;
			const int maxHourTwo = 48;

			var maxTimePerWeekOne = TimeSpan.FromHours(maxHoursOne);
			var workTimeDirectiveOne = new WorkTimeDirective(TimeSpan.Zero, maxTimePerWeekOne, TimeSpan.Zero, TimeSpan.Zero);
			var maxTimePerWeekTwo = TimeSpan.FromHours(maxHourTwo);
			var workTimeDirectiveTwo = new WorkTimeDirective(TimeSpan.Zero, maxTimePerWeekTwo, TimeSpan.Zero, TimeSpan.Zero);

			_contractOne.WorkTimeDirective = workTimeDirectiveOne;
			_personContractOne.Contract = _contractOne;
			_personPeriodOne.PersonContract = _personContractOne;
			_personPeriodOne.StartDate = new DateOnly(new DateTime(2014, 10, 1));

			_contractTwo.WorkTimeDirective = workTimeDirectiveTwo;
			_personContractTwo.Contract = _contractTwo;
			_personPeriodTwo.PersonContract = _personContractTwo;
			_personPeriodTwo.StartDate = new DateOnly(new DateTime(2014, 11, 1));


			using (_mocks.Record())
			{
				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_personPeriodOne.EndDate()).Return(new DateOnly(new DateTime(2014, 10, 30)));
				Expect.Call(_person.Period(_dateToScheck)).Return(_personPeriodOne);
				Expect.Call(_person.NextPeriod(_personPeriodOne)).Return(_personPeriodTwo);
			}
			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.SkipWeekCheck(_matrix, _dateToScheck));
			}
		}

		[Test]
		public void WeekShouldSkipWhenEndPersonPeriodAndMaxTimePerWeekChanges()
		{
			_dateToScheck = new DateOnly(new DateTime(2014, 11, 02));

			const int maxHoursOne = 40;
			const int maxHourTwo = 48;

			var maxTimePerWeekOne = TimeSpan.FromHours(maxHoursOne);
			var workTimeDirectiveOne = new WorkTimeDirective(TimeSpan.Zero, maxTimePerWeekOne, TimeSpan.Zero, TimeSpan.Zero);
			var maxTimePerWeekTwo = TimeSpan.FromHours(maxHourTwo);
			var workTimeDirectiveTwo = new WorkTimeDirective(TimeSpan.Zero, maxTimePerWeekTwo, TimeSpan.Zero, TimeSpan.Zero);

			_contractOne.WorkTimeDirective = workTimeDirectiveOne;
			_personContractOne.Contract = _contractOne;
			_personPeriodOne.PersonContract = _personContractOne;
			_personPeriodOne.StartDate = new DateOnly(new DateTime(2014, 10, 1));

			_contractTwo.WorkTimeDirective = workTimeDirectiveTwo;
			_personContractTwo.Contract = _contractTwo;
			_personPeriodTwo.PersonContract = _personContractTwo;
			_personPeriodTwo.StartDate = new DateOnly(new DateTime(2014, 11, 1));


			using (_mocks.Record())
			{
				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_person.Period(_dateToScheck)).Return(_personPeriodTwo);
				Expect.Call(_person.PreviousPeriod(_personPeriodTwo)).Return(_personPeriodOne);
			}
			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.SkipWeekCheck(_matrix, _dateToScheck));
			}
		}
	}
}
