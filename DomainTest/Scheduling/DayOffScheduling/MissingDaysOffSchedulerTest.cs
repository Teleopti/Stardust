using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.DayOffScheduling
{
	[TestFixture]
	public class MissingDaysOffSchedulerTest
	{
		private MockRepository _mocks;
		private IMissingDaysOffScheduler _target;
		private IBestSpotForAddingDayOffFinder _bestSpotForAddingDayOffFinder;
		private IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		//private IScheduleMatrixPro _matrix1;
		//private IScheduleMatrixPro _matrix2;
		//private IList<IScheduleMatrixPro> _matrixList;
		//private IVirtualSchedulePeriod _virtualSchedulePeriod;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dayOffsInPeriodCalculator = _mocks.StrictMock<IDayOffsInPeriodCalculator>();
			_bestSpotForAddingDayOffFinder = _mocks.StrictMock<IBestSpotForAddingDayOffFinder>();
			_target = new MissingDaysOffScheduler(_dayOffsInPeriodCalculator, _bestSpotForAddingDayOffFinder);
		//    _matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
		//    _matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
		//    _matrixList = new List<IScheduleMatrixPro>{ _matrix1, _matrix2 };
		//    _virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
		}

		
		
		//[Test]
		//public void ShouldJumpOutWhenCorrectNumberOfDaysOff()
		//{
		//    using (_mocks.Record())
		//    {
		//        //allSolved();
		//    }

		//    using (_mocks.Playback())
		//    {
		//        bool result = _target.Execute(_matrixList);
		//        Assert.IsTrue(result);
		//    }
		//}

		//private void initialMocks()
		//{
		//    Expect.Call(_matrix1.SchedulePeriod).Return(_virtualSchedulePeriod);
		//    Expect.Call(_matrix2.SchedulePeriod).Return(_virtualSchedulePeriod);
		//    int targetDaysOff;
		//    int current;
		//    Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_virtualSchedulePeriod, out targetDaysOff,
		//                                                                     out current)).Return(false).Repeat.Twice();
		//}

		//private void allSolved()
		//{
		//    Expect.Call(_matrix1.SchedulePeriod).Return(_virtualSchedulePeriod);
		//    Expect.Call(_matrix2.SchedulePeriod).Return(_virtualSchedulePeriod);
		//    int targetDaysOff;
		//    int current;
		//    Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_virtualSchedulePeriod, out targetDaysOff,
		//                                                                     out current)).Return(true).Repeat.Twice();
		//}
	}

	
}