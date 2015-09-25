using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class ScheduleDayServiceTest
    {
        private MockRepository _mocks;
        private IScheduleDayService _target;
		private IScheduleService _scheduleService;
        private IDeleteSchedulePartService _deleteSchedulePartService;
        private IResourceOptimizationHelper _resourceOptimizationHelper;
        private IScheduleDay _newSchedulePart;
        private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
    	private ISchedulingOptions _schedulingOptions;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _newSchedulePart = _mocks.StrictMock<IScheduleDay>();
            _resourceOptimizationHelper = _mocks.StrictMock<IResourceOptimizationHelper>();
            _deleteSchedulePartService = _mocks.StrictMock<IDeleteSchedulePartService>();
            _scheduleService = _mocks.StrictMock<IScheduleService>();
            _effectiveRestrictionCreator = _mocks.DynamicMock<IEffectiveRestrictionCreator>();
			_rollbackService = _mocks.DynamicMock<ISchedulePartModifyAndRollbackService>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
			_schedulingOptions = new SchedulingOptions();
			_target = new ScheduleDayService(_scheduleService, _deleteSchedulePartService, _resourceOptimizationHelper, _effectiveRestrictionCreator, _rollbackService, ()=> new SchedulerStateHolder(new SchedulingResultStateHolder(), new CommonStateHolder(null), new TimeZoneGuardWrapper()));
        }


        [Test]
        public void VerifyScheduleDay()
        {
            bool result;
            var firstDate = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var period = new DateTimePeriod(firstDate, firstDate);
            IEffectiveRestriction effectiveRestriction = null;
            using (_mocks.Record())
            {
                Expect.Call(_newSchedulePart.Period).Return(period).Repeat.Any();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(null, null)).IgnoreArguments()
                    .Return(effectiveRestriction);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_newSchedulePart, _schedulingOptions, effectiveRestriction, _resourceCalculateDelayer, _rollbackService)).IgnoreArguments()
                    .Return(true);
 
            }

            using (_mocks.Playback())
            {
				result = _target.ScheduleDay(_newSchedulePart, _schedulingOptions);
            }

            Assert.IsTrue(result);
        }
    }
}