using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class SafeRollbackAndResourceCalculationTest
	{
		private MockRepository _mocks;
		private ISafeRollbackAndResourceCalculation _target;
		private IResourceOptimizationHelper _resourceOptimizationHelper;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private ISchedulingOptions _schedulingOptions;
		private IList<IScheduleDay> _modifyedScheduleDays;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_resourceOptimizationHelper = _mocks.StrictMock<IResourceOptimizationHelper>();
			_target = new SafeRollbackAndResourceCalculation(_resourceOptimizationHelper);
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_schedulingOptions = new SchedulingOptions();
			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			_modifyedScheduleDays = new List<IScheduleDay>{_scheduleDay1, _scheduleDay2};
		}

		[Test]
		public void ShouldCalculateAllModifiedDaysAndTheDaysAfter()
		{

			using (_mocks.Record())
			{
				Expect.Call(_rollbackService.ModificationCollection).Return(_modifyedScheduleDays);
				Expect.Call(() => _rollbackService.Rollback());
				Expect.Call(_scheduleDay1.DateOnlyAsPeriod)
				      .Return(new DateOnlyAsDateTimePeriod(DateOnly.MinValue, TimeZoneInfo.Utc));
				Expect.Call(_scheduleDay2.DateOnlyAsPeriod)
					  .Return(new DateOnlyAsDateTimePeriod(DateOnly.MinValue.AddDays(10), TimeZoneInfo.Utc));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue, _schedulingOptions.ConsiderShortBreaks, false));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue.AddDays(1), _schedulingOptions.ConsiderShortBreaks, false));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue.AddDays(10), _schedulingOptions.ConsiderShortBreaks, false));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue.AddDays(11), _schedulingOptions.ConsiderShortBreaks, false));
			}

			using (_mocks.Playback())
			{
				_target.Execute(_rollbackService, _schedulingOptions);
			}
      
      
		}

	}
}