using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class ScheduleMatrixListPeriodFinderTest
	{
		private ScheduleMatrixListPeriodFinder _periodFinder;
		private IList<IScheduleMatrixPro> _matrixList;
		private MockRepository _mockRepository;
		private IScheduleMatrixPro _matrix1;
		private IScheduleMatrixPro _matrix2;
		private IScheduleDayPro _dayProLast1;
		private IScheduleDayPro _dayProLast2;
		private IScheduleDayPro _dayProFirst1;
		private IScheduleDayPro _dayProFirst2;
		private DateOnlyPeriod _dateOnlyPeriod1;
		private DateOnlyPeriod _dateOnlyPeriod2;
		private ReadOnlyCollection<IScheduleDayPro> _days1;
		private ReadOnlyCollection<IScheduleDayPro> _days2;

		[SetUp]
		public void Setup()
		{
			_mockRepository = new MockRepository();
			_matrix1 = _mockRepository.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mockRepository.StrictMock<IScheduleMatrixPro>();
			_dayProFirst1 = _mockRepository.StrictMock<IScheduleDayPro>();
			_dayProLast1 = _mockRepository.StrictMock<IScheduleDayPro>();
			_dayProFirst2 = _mockRepository.StrictMock<IScheduleDayPro>();
			_dayProLast2 = _mockRepository.StrictMock<IScheduleDayPro>();
			_matrixList = new List<IScheduleMatrixPro> {_matrix1, _matrix2};
			_periodFinder = new ScheduleMatrixListPeriodFinder(_matrixList);
			_dateOnlyPeriod1 = new DateOnlyPeriod(2011, 1, 1, 2011, 1, 10);
			_dateOnlyPeriod2 = new DateOnlyPeriod(2011, 1, 5, 2011, 1, 15);
			_days1 = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {_dayProFirst1, _dayProLast1});
			_days2 = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {_dayProFirst2, _dayProLast2});
		}

		[Test]
		public void ShouldReturnOuterWeekPeriod()
		{
			using(_mockRepository.Record())
			{
				Expect.Call(_matrix1.OuterWeeksPeriodDays).Return(_days1);
				Expect.Call(_matrix2.OuterWeeksPeriodDays).Return(_days2);
				Expect.Call(_dayProFirst1.Day).Return(_dateOnlyPeriod1.StartDate);
				Expect.Call(_dayProLast1.Day).Return(_dateOnlyPeriod1.EndDate);
				Expect.Call(_dayProFirst2.Day).Return(_dateOnlyPeriod2.StartDate);
				Expect.Call(_dayProLast2.Day).Return(_dateOnlyPeriod2.EndDate);
			}

			using(_mockRepository.Playback())
			{
				var period = _periodFinder.FindOuterWeekPeriod();
				Assert.AreEqual(_dateOnlyPeriod1.StartDate, period.StartDate);
				Assert.AreEqual(_dateOnlyPeriod2.EndDate, period.EndDate);
			}
		}
	}
}
