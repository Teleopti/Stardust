using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class EmptyDaysInBlockOutsideSelectedHandlerTest
	{
		private MockRepository _mocks;
		private EmptyDaysInBlockOutsideSelectedHandler _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new EmptyDaysInBlockOutsideSelectedHandler();
		}

		[Test]
		public void ShouldRemoveDaysOutsideIfWholeBlockEmpty()
		{
			var matrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			var scheduleDay = _mocks.StrictMock<IScheduleDayPro>();
			var part = _mocks.StrictMock<IScheduleDay>();
			var blockDates = new List<DateOnly>
			                 	{
			                 		new DateOnly(2012, 4, 17),
			                 		new DateOnly(2012, 4, 18),
			                 		new DateOnly(2012, 4, 19),
			                 		new DateOnly(2012, 4, 20),
			                 		new DateOnly(2012, 4, 21)
			                 	};

#pragma warning disable 612,618
			Expect.Call(matrixPro.SelectedPeriod).Return(new DateOnlyPeriod(new DateOnly(2012, 4, 20), new DateOnly(2012, 5, 20)));
#pragma warning restore 612,618
			Expect.Call(matrixPro.GetScheduleDayByKey(new DateOnly())).IgnoreArguments().Return(scheduleDay).Repeat.AtLeastOnce();
			Expect.Call(scheduleDay.DaySchedulePart()).Return(part).Repeat.AtLeastOnce();
			Expect.Call(part.SignificantPart()).Return(SchedulePartView.None).Repeat.AtLeastOnce();
			_mocks.ReplayAll();
			var datesLeft = _target.CheckDates(blockDates, matrixPro);
			Assert.That(datesLeft.Count,Is.EqualTo(2));
			Assert.That(datesLeft[0],Is.EqualTo(new DateOnly(2012,4,20)));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotRemoveAnyIfOneDayIsScheduled()
		{
			var matrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			var scheduleDay = _mocks.StrictMock<IScheduleDayPro>();
			var part = _mocks.StrictMock<IScheduleDay>();
			var blockDates = new List<DateOnly>
			                 	{
			                 		new DateOnly(2012, 4, 17),
			                 		new DateOnly(2012, 4, 18),
			                 		new DateOnly(2012, 4, 19),
			                 		new DateOnly(2012, 4, 20),
			                 		new DateOnly(2012, 4, 21)
			                 	};

#pragma warning disable 612,618
			Expect.Call(matrixPro.SelectedPeriod).Return(new DateOnlyPeriod(new DateOnly(2012, 4, 20), new DateOnly(2012, 5, 20)));
#pragma warning restore 612,618
			Expect.Call(matrixPro.GetScheduleDayByKey(new DateOnly(2012, 4, 17))).IgnoreArguments().Return(scheduleDay).Repeat.AtLeastOnce();
			Expect.Call(scheduleDay.DaySchedulePart()).Return(part).Repeat.AtLeastOnce();
			Expect.Call(part.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
			_mocks.ReplayAll();
			var datesLeft = _target.CheckDates(blockDates, matrixPro);
			Assert.That(datesLeft.Count, Is.EqualTo(5));
			Assert.That(datesLeft[0], Is.EqualTo(new DateOnly(2012, 4, 17)));
			_mocks.VerifyAll();
		}
		
	}

}