﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
		private IMatrixDataListInSteadyState _matrixDataListInSteadyState;
		private IMatrixDataListCreator _matrixDataListCreator;
		private IScheduleMatrixPro _matrix1;
		//private IScheduleMatrixPro _matrix2;
		private IList<IScheduleMatrixPro> _matrixList;
		private IVirtualSchedulePeriod _virtualSchedulePeriod;
		private ISchedulingOptions _schedulingOptions;
		private IList<IMatrixData> _matrixDataList;
		private IMatrixData _matrixData1;
		private IMatrixData _matrixData2;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IMatrixDatasWithToFewDaysOff _matrixDatasWithToFewDaysOff;
		private IScheduleDayPro _scheduleDayPro;
		private IScheduleDay _scheduleDay;
		private ReadOnlyCollection<IScheduleDayData> _scheduleDayDataCollection;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_bestSpotForAddingDayOffFinder = _mocks.StrictMock<IBestSpotForAddingDayOffFinder>();
			_matrixDataListInSteadyState = _mocks.StrictMock<IMatrixDataListInSteadyState>();
			_matrixDataListCreator = _mocks.StrictMock<IMatrixDataListCreator>();
			_matrixDatasWithToFewDaysOff = _mocks.StrictMock<IMatrixDatasWithToFewDaysOff>();
			_target = new MissingDaysOffScheduler(_bestSpotForAddingDayOffFinder, _matrixDataListInSteadyState,
			                                      _matrixDataListCreator, _matrixDatasWithToFewDaysOff);
			_matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			//_matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrixList = new List<IScheduleMatrixPro> {_matrix1};
			_virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_schedulingOptions = new SchedulingOptions();
			_matrixData1 = _mocks.StrictMock<IMatrixData>();
			_matrixData2 = _mocks.StrictMock<IMatrixData>();
			_matrixDataList = new List<IMatrixData> {_matrixData1, _matrixData2};
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_scheduleDayDataCollection = new ReadOnlyCollection<IScheduleDayData>(new List<IScheduleDayData>());
		}



		[Test]
		public void ShouldJumpOutWhenCorrectNumberOfDaysOff()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrixDataListCreator.Create(_matrixList, _schedulingOptions)).Return(_matrixDataList);
				Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(_matrixDataList)).Return(true);
				Expect.Call(_matrixDatasWithToFewDaysOff.FindMatrixesWithToFewDaysOff(_matrixDataList)).Return(
					new List<IMatrixData>());
			}

			using (_mocks.Playback())
			{
				bool result = _target.Execute(_matrixList, _schedulingOptions, _rollbackService);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldAddOnAllMatrixDatasIfInSteadyState()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrixDataListCreator.Create(_matrixList, _schedulingOptions)).Return(_matrixDataList);
				Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(_matrixDataList)).Return(true);
				Expect.Call(_matrixDatasWithToFewDaysOff.FindMatrixesWithToFewDaysOff(_matrixDataList)).Return(
					_matrixDataList);
				Expect.Call(_matrixDataList[0].ScheduleDayDataCollection).Return(_scheduleDayDataCollection);
				Expect.Call(_bestSpotForAddingDayOffFinder.Find(_scheduleDayDataCollection)).Return(
					DateOnly.MinValue);

				Expect.Call(_matrixData1.Matrix).Return(_matrix1);
				Expect.Call(_matrix1.GetScheduleDayByKey(DateOnly.MinValue)).Return(_scheduleDayPro);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
				Expect.Call(() => _rollbackService.Modify(_scheduleDay));

				Expect.Call(_matrixData2.Matrix).Return(_matrix1);
				Expect.Call(_matrix1.GetScheduleDayByKey(DateOnly.MinValue)).Return(_scheduleDayPro);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
				Expect.Call(() => _rollbackService.Modify(_scheduleDay));

				Expect.Call(_matrixData1.Matrix).Return(_matrix1);
				Expect.Call(_matrixData2.Matrix).Return(_matrix1);

				Expect.Call(_matrixDataListCreator.Create(new List<IScheduleMatrixPro>(_matrixList), _schedulingOptions)).IgnoreArguments().Return(_matrixDataList);
				Expect.Call(_matrixDatasWithToFewDaysOff.FindMatrixesWithToFewDaysOff(_matrixDataList)).Return(
					new List<IMatrixData>());
			}

			using (_mocks.Playback())
			{
				bool result = _target.Execute(_matrixList, _schedulingOptions, _rollbackService);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldAddOnFirstMatrixDataIfNotInSteadyState()
		{
			
		}

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