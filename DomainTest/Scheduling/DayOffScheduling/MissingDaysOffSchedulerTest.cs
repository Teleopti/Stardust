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
		private IList<IScheduleMatrixPro> _matrixList;
		private ISchedulingOptions _schedulingOptions;
		private IList<IMatrixData> _matrixDataList;
		private IMatrixData _matrixData1;
		private IMatrixData _matrixData2;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IMatrixDataWithToFewDaysOff _matrixDataWithToFewDaysOff;
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
			_matrixDataWithToFewDaysOff = _mocks.StrictMock<IMatrixDataWithToFewDaysOff>();
			_target = new MissingDaysOffScheduler(_bestSpotForAddingDayOffFinder, _matrixDataListInSteadyState,
			                                      _matrixDataListCreator, _matrixDataWithToFewDaysOff);
			_matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrixList = new List<IScheduleMatrixPro> {_matrix1};
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
				Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(_matrixDataList)).Return(
					new List<IMatrixData>());
			}

			using (_mocks.Playback())
			{
				bool result = _target.Execute(_matrixList, _schedulingOptions, _rollbackService);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldAddOnAllMatrixDataIfInSteadyState()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrixDataListCreator.Create(_matrixList, _schedulingOptions)).Return(_matrixDataList);
				Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(_matrixDataList)).Return(true);
				Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(_matrixDataList)).Return(
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
				Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(_matrixDataList)).Return(
					new List<IMatrixData>());
			}

			using (_mocks.Playback())
			{
				bool result = _target.Execute(_matrixList, _schedulingOptions, _rollbackService);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldCancelAndReturnFalseIfCanceled()
		{
			_target.DayScheduled += _target_DayScheduled;
			using (_mocks.Record())
			{
				Expect.Call(_matrixDataListCreator.Create(_matrixList, _schedulingOptions)).Return(_matrixDataList);
				Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(_matrixDataList)).Return(true);
				Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(_matrixDataList)).Return(
					_matrixDataList);
				Expect.Call(_matrixDataList[0].ScheduleDayDataCollection).Return(_scheduleDayDataCollection);
				Expect.Call(_bestSpotForAddingDayOffFinder.Find(_scheduleDayDataCollection)).Return(
					DateOnly.MinValue);

				Expect.Call(_matrixData1.Matrix).Return(_matrix1);
				Expect.Call(_matrix1.GetScheduleDayByKey(DateOnly.MinValue)).Return(_scheduleDayPro);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
				Expect.Call(() => _rollbackService.Modify(_scheduleDay));
			}

			using (_mocks.Playback())
			{
				bool result = _target.Execute(_matrixList, _schedulingOptions, _rollbackService);
				Assert.IsFalse(result);
			}
			_target.DayScheduled -= _target_DayScheduled;
		}

		void _target_DayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			e.Cancel = true;
		}

		[Test]
		public void ShouldAddOnFirstMatrixDataIfNotInSteadyState()
		{
			IList<IScheduleMatrixPro> secondMatrixList = new List<IScheduleMatrixPro> { _matrix1  };
			IList<IMatrixData> secondMatrixDataList = new List<IMatrixData> {_matrixData2};

			using (_mocks.Record())
			{
				Expect.Call(_matrixDataListCreator.Create(_matrixList, _schedulingOptions)).Return(_matrixDataList);
				Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(_matrixDataList)).Return(false);
				Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(_matrixDataList)).Return(
					_matrixDataList);
				Expect.Call(_matrixDataList[0].ScheduleDayDataCollection).Return(_scheduleDayDataCollection);
				Expect.Call(_bestSpotForAddingDayOffFinder.Find(_scheduleDayDataCollection)).Return(
					DateOnly.MinValue);

				Expect.Call(_matrixData1.Matrix).Return(_matrix1);
				Expect.Call(_matrix1.GetScheduleDayByKey(DateOnly.MinValue)).Return(_scheduleDayPro);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
				Expect.Call(() => _rollbackService.Modify(_scheduleDay));

				Expect.Call(_matrixData1.Matrix).Return(_matrix1);
				Expect.Call(_matrixData2.Matrix).Return(_matrix1);
				Expect.Call(_matrixDataListCreator.Create(new List<IScheduleMatrixPro>(_matrixList), _schedulingOptions)).IgnoreArguments().Return(_matrixDataList);
				Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(_matrixDataList)).Return(
					secondMatrixDataList);

				Expect.Call(secondMatrixDataList[0].ScheduleDayDataCollection).Return(_scheduleDayDataCollection);
				Expect.Call(_bestSpotForAddingDayOffFinder.Find(_scheduleDayDataCollection)).Return(
					DateOnly.MinValue);
				Expect.Call(_matrixData2.Matrix).Return(_matrix1);
				Expect.Call(_matrix1.GetScheduleDayByKey(DateOnly.MinValue)).Return(_scheduleDayPro);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
				Expect.Call(() => _rollbackService.Modify(_scheduleDay));

				Expect.Call(_matrixData2.Matrix).Return(_matrix1);

				Expect.Call(_matrixDataListCreator.Create(new List<IScheduleMatrixPro>(secondMatrixList), _schedulingOptions)).IgnoreArguments().Return(secondMatrixDataList);
				Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(secondMatrixDataList)).Return(
					new List<IMatrixData>());
			}

			using (_mocks.Playback())
			{
				bool result = _target.Execute(_matrixList, _schedulingOptions, _rollbackService);
				Assert.IsTrue(result);
			}
		}
	}

	
}