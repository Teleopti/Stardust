﻿using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class GroupDayOffOptimizationResourceHelperTest
	{
		private GroupDayOffOptimizationResourceHelper _target;
		private IResourceOptimizationHelper _resourceOptimizationHelper;
		private MockRepository _mocks;
		private IList<IScheduleDay> _orgDays;
		private IList<IScheduleDay> _modifiedDays;
		private IScheduleDay _modifiedScheduleDay;
		private IScheduleDay _orgScheduleDay;
		private DateOnly _date;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
	
		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_resourceOptimizationHelper = _mocks.StrictMock<IResourceOptimizationHelper>();
			_target = new GroupDayOffOptimizationResourceHelper(_resourceOptimizationHelper);
			_modifiedScheduleDay = _mocks.StrictMock<IScheduleDay>();
			_orgScheduleDay = _mocks.StrictMock<IScheduleDay>();
			_modifiedDays = new List<IScheduleDay> { _modifiedScheduleDay };
			_orgDays = new List<IScheduleDay> { _orgScheduleDay };
			_date = new DateOnly(2012, 1, 1);
			_dateOnlyAsDateTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();	
		}

		[Test]
		public void ShouldResourceCalculateContainersToRemoveWhenModifiedIsMainShift()
		{

			IList<IScheduleDay> toRemove = new List<IScheduleDay> { _orgScheduleDay };
			IList<IScheduleDay> toAdd = new List<IScheduleDay> { _modifiedScheduleDay };
			
			using(_mocks.Record())
			{
				Expect.Call(_modifiedScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_orgScheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_modifiedScheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_date).Repeat.Twice();
				Expect.Call(_orgScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_date, false, false, toRemove, toAdd));
			}

			using(_mocks.Playback())
			{
				_target.ResourceCalculateContainersToRemove(_orgDays, _modifiedDays );
			}	
		}

		[Test]
		public void ShouldResourceCalculateContainersToRemoveWhenModifiedIsDayOff()
		{
			IList<IScheduleDay> toRemove = new List<IScheduleDay> { _orgScheduleDay };
			IList<IScheduleDay> toAdd = new List<IScheduleDay>();

			using (_mocks.Record())
			{
				Expect.Call(_modifiedScheduleDay.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(_orgScheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_modifiedScheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_date).Repeat.Twice();
				Expect.Call(_orgScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_date, false, false, toRemove, toAdd));
			}

			using (_mocks.Playback())
			{
				_target.ResourceCalculateContainersToRemove(_orgDays, _modifiedDays);
			}	
		}
	}
}
