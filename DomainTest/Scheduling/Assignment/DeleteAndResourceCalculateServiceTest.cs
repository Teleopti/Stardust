﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class DeleteAndResourceCalculateServiceTest
	{
		private MockRepository _mocks;
		private IDeleteAndResourceCalculateService _target;
		private IResourceOptimizationHelper _resourceOptimizationHelper;
		private IDeleteSchedulePartService _deleteSchedulePartService;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IList<IScheduleDay> _list;
		private IScheduleDay _part1;
		private IScheduleDay _part2;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_resourceOptimizationHelper = _mocks.StrictMock<IResourceOptimizationHelper>();
			_deleteSchedulePartService = _mocks.StrictMock<IDeleteSchedulePartService>();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_target = new DeleteAndResourceCalculateService(_deleteSchedulePartService, _resourceOptimizationHelper);
			_part1 = _mocks.StrictMock<IScheduleDay>();
			_part2 = _mocks.StrictMock<IScheduleDay>();
			_list = new List<IScheduleDay> { _part1, _part2 };
		}

		[Test]
		public void ShouldDeleteAndCalculate()
		{
			
		}

	[Test]
		public void ShouldResourceCalculateCorrectDatesIfAsked()
		{
			_list = new List<IScheduleDay> { _part1, _part2 };
			IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod1 = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
			IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod2 = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
			using (_mocks.Record())
			{

				//Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary).Repeat.AtLeastOnce();
				//Expect.Call(_part1.Person).Return(_person).Repeat.AtLeastOnce();
				//Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange1).Repeat.AtLeastOnce();
				//Expect.Call(_part2.Person).Return(_person).Repeat.AtLeastOnce();
				//Expect.Call(_scheduleRange1.ReFetch(_part1)).Return(_part3).Repeat.AtLeastOnce();
				//Expect.Call(_scheduleRange1.ReFetch(_part2)).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(_deleteSchedulePartService.Delete(_list, _rollbackService)).Return(_list);
				Expect.Call(_part1.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod1);
				Expect.Call(_part2.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod2);
				Expect.Call(dateOnlyAsDateTimePeriod1.DateOnly).Return(new DateOnly(2012, 1, 1));
				Expect.Call(dateOnlyAsDateTimePeriod2.DateOnly).Return(new DateOnly(2012, 1, 10));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2012, 1, 1), true, true, new List<IScheduleDay>{_part1}, new List<IScheduleDay>()));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2012, 1, 1).AddDays(1), true, true, new List<IScheduleDay>{_part1}, new List<IScheduleDay>()));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2012, 1, 10), true, true, new List<IScheduleDay>{_part2}, new List<IScheduleDay>()));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2012, 1, 10).AddDays(1), true, true, new List<IScheduleDay>{_part2}, new List<IScheduleDay>()));
			}

			IList<IScheduleDay> ret;

			using (_mocks.Playback())
			{
				ret = _target.DeleteWithResourceCalculation(_list, _rollbackService);
			}

			Assert.AreEqual(2, ret.Count);
		}
	}
}