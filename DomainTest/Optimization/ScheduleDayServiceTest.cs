using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class ScheduleDayServiceTest
    {
        private MockRepository _mocks;
        private IScheduleDayService _target;
		private IScheduleService _scheduleService;
        private IOptimizerOriginalPreferences _optimizerPreferences;
        private IDeleteSchedulePartService _deleteSchedulePartService;
        private IResourceOptimizationHelper _resourceOptimizationHelper;
        private IScheduleDay _schedulePart;
        private IScheduleDay _newSchedulePart;
        private IPerson _person;
        private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private ISchedulePartModifyAndRollbackService _rollbackService;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulePart = _mocks.StrictMock<IScheduleDay>();
            _newSchedulePart = _mocks.StrictMock<IScheduleDay>();
            _resourceOptimizationHelper = _mocks.StrictMock<IResourceOptimizationHelper>();
            _deleteSchedulePartService = _mocks.StrictMock<IDeleteSchedulePartService>();
            _optimizerPreferences = new OptimizerOriginalPreferences();
            _scheduleService = _mocks.StrictMock<IScheduleService>();
            _person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2009, 1, 1), new List<ISkill>());
            _effectiveRestrictionCreator = _mocks.DynamicMock<IEffectiveRestrictionCreator>();
			_rollbackService = _mocks.DynamicMock<ISchedulePartModifyAndRollbackService>();
			_target = new ScheduleDayService(_scheduleService, _optimizerPreferences.SchedulingOptions, _deleteSchedulePartService, _resourceOptimizationHelper, _effectiveRestrictionCreator, _rollbackService);
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
                Expect.Call(_scheduleService.SchedulePersonOnDay(_newSchedulePart, _optimizerPreferences.SchedulingOptions, true, effectiveRestriction))
                    .Return(true);
 
            }

            using (_mocks.Playback())
            {
                result = _target.ScheduleDay(_newSchedulePart);
            }

            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyRescheduleDay()
        {
            bool result;
            var firstDate = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var period = new DateTimePeriod(firstDate, firstDate);
            IList<IScheduleDay> newList = new List<IScheduleDay>{_newSchedulePart};
            IEffectiveRestriction effectiveRestriction = null;
            using (_mocks.Record())
            {
                Expect.Call(_schedulePart.Clone()).Return(_schedulePart).Repeat.AtLeastOnce();
                Expect.Call(_deleteSchedulePartService.Delete(new List<IScheduleDay> {_schedulePart},
															  new DeleteOption(), _rollbackService, null)).IgnoreArguments().Return(
                                                               newList).Repeat.Once();

                Expect.Call(_schedulePart.Period).Return(period).Repeat.Any();
                Expect.Call(_newSchedulePart.Period).Return(period).Repeat.Any();
                _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(period.LocalStartDateTime), true, true);
                LastCall.Repeat.Once();
                _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(period.LocalStartDateTime.AddDays(1)), true, true);
                LastCall.Repeat.Once();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(null, null)).IgnoreArguments().Return(
                    effectiveRestriction);
                Expect.Call(_scheduleService.SchedulePersonOnDay(_newSchedulePart, _optimizerPreferences.SchedulingOptions, true, effectiveRestriction)).Return(true);
            }

            using (_mocks.Playback())
            {
                result = _target.RescheduleDay(_schedulePart);
            }

            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyRescheduleDay2()
        {
            bool result;
            var firstDate = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var period = new DateTimePeriod(firstDate, firstDate);
            IList<IScheduleDay> newList = new List<IScheduleDay> { _newSchedulePart };
            _person.PersonPeriodCollection[0].PersonContract.Contract.EmploymentType = EmploymentType.HourlyStaff;
            IEffectiveRestriction effectiveRestriction = null;
            using (_mocks.Record())
            {
                Expect.Call(_schedulePart.Clone()).Return(_schedulePart).Repeat.AtLeastOnce();
                Expect.Call(_deleteSchedulePartService.Delete(new List<IScheduleDay> { _schedulePart },
															  new DeleteOption(), _rollbackService, null)).IgnoreArguments().Return(
                                                               newList).Repeat.Once();

                Expect.Call(_schedulePart.Period).Return(period).Repeat.Any();
                Expect.Call(_newSchedulePart.Period).Return(period).Repeat.Any();
                _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(period.LocalStartDateTime), true, true);
                LastCall.Repeat.AtLeastOnce();
                _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(period.LocalStartDateTime.AddDays(1)), true, true);
                LastCall.Repeat.Once();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(null, null)).IgnoreArguments()
                    .Return(effectiveRestriction);
                Expect.Call(_scheduleService.SchedulePersonOnDay(_newSchedulePart, _optimizerPreferences.SchedulingOptions, true, effectiveRestriction))
                    .Return(false);
            }

            using (_mocks.Playback())
            {
                result = _target.RescheduleDay(_schedulePart);
            }

            Assert.IsFalse(result);
        }
    }
}