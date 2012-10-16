using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class ScheduleServiceTest
    {
        private MockRepository _mocks;
        private IWorkShiftFinderService _workShiftFinder;
        private IScheduleMatrixListCreator _scheduleMatrixListCreator;
        private IShiftCategoryLimitationChecker _shiftCatLimitChecker;
        private ISchedulePartModifyAndRollbackService _modifyRollback;
        private ScheduleService _target;
        private IScheduleDay _part;
        private ISchedulingOptions _options;
        private IEffectiveRestriction _effectiveRestriction;
        private IPerson _person;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IWorkflowControlSet _workflowControlSet;
        private IEffectiveRestrictionCreator _restrictionCreator;
		private IResourceCalculateDelayer _resourceCalculateDelayer;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _workShiftFinder = _mocks.StrictMock<IWorkShiftFinderService>();
            _scheduleMatrixListCreator = _mocks.StrictMock<IScheduleMatrixListCreator>();
            _shiftCatLimitChecker = _mocks.StrictMock<IShiftCategoryLimitationChecker>();
            _modifyRollback = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
            _restrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
            _options = new SchedulingOptions();
            _target = new ScheduleService(_workShiftFinder, _scheduleMatrixListCreator, _shiftCatLimitChecker, _modifyRollback, _restrictionCreator);

            _part = _mocks.StrictMock<IScheduleDay>();
            
            _effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            _person = new Person();
            _workflowControlSet = _mocks.StrictMock<IWorkflowControlSet>();
            _person.WorkflowControlSet = _workflowControlSet;
            _scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
        }
    

        [Test]
        public void ShouldReturnTrueIfAlreadyScheduled()
        {
            Expect.Call(_part.IsScheduled()).Return(true);
            _mocks.ReplayAll();
			Assert.That(_target.SchedulePersonOnDay(_part, _options, true, _effectiveRestriction, _resourceCalculateDelayer), Is.True);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfEffectiveRestrictionIsNull()
        {
            IEffectiveRestriction effectiveRestriction = null;
            Expect.Call(_part.IsScheduled()).Return(false);
            Expect.Call(_part.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2011, 4, 18),
                                                                                    (
                                                                                        TimeZoneInfo.
                                                                                            FindSystemTimeZoneById("Utc"))));
            Expect.Call(_part.Person).Return(_person);
            _mocks.ReplayAll();
			Assert.That(_target.SchedulePersonOnDay(_part, _options, true, effectiveRestriction, _resourceCalculateDelayer), Is.False);
            Assert.That(_target.FinderResults.Count, Is.GreaterThan(0));
            _target.ClearFinderResults();
            Assert.That(_target.FinderResults.Count, Is.EqualTo(0));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfNotAvailable()
        {
            Expect.Call(_part.IsScheduled()).Return(false);
            Expect.Call(_part.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2011, 4, 18),
                                                                                    (
                                                                                        TimeZoneInfo.
                                                                                            FindSystemTimeZoneById("Utc"))));
            Expect.Call(_part.Person).Return(_person);
            Expect.Call(_effectiveRestriction.NotAvailable).Return(true);
            _mocks.ReplayAll();
			Assert.That(_target.SchedulePersonOnDay(_part, _options, true, _effectiveRestriction, _resourceCalculateDelayer), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfMatrixCountIsZero()
        {
            Expect.Call(_part.IsScheduled()).Return(false);
            Expect.Call(_part.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2011, 4, 18),
                                                                                    (
                                                                                        TimeZoneInfo.
                                                                                            FindSystemTimeZoneById("Utc"))));
            Expect.Call(_part.Person).Return(_person);
            Expect.Call(_effectiveRestriction.NotAvailable).Return(false);
            Expect.Call(
                () => _shiftCatLimitChecker.SetBlockedShiftCategories(_options, _person, (new DateOnly(2011, 4, 18))));
            Expect.Call(_scheduleMatrixListCreator.CreateMatrixListFromScheduleParts(new List<IScheduleDay> {_part})).Return(new List<IScheduleMatrixPro>());
            _mocks.ReplayAll();
			Assert.That(_target.SchedulePersonOnDay(_part, _options, true, _effectiveRestriction, _resourceCalculateDelayer), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfNotFindShift()
        {
            Expect.Call(_part.IsScheduled()).Return(false);
            Expect.Call(_part.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2011, 4, 18),
                                                                                    (
                                                                                        TimeZoneInfo.
                                                                                            FindSystemTimeZoneById("Utc"))));
            Expect.Call(_part.Person).Return(_person);
            Expect.Call(_effectiveRestriction.NotAvailable).Return(false);
            Expect.Call(
                () => _shiftCatLimitChecker.SetBlockedShiftCategories(_options, _person, (new DateOnly(2011, 4, 18))));
            Expect.Call(_scheduleMatrixListCreator.CreateMatrixListFromScheduleParts(new List<IScheduleDay> { _part })).Return(new List<IScheduleMatrixPro>{_scheduleMatrixPro});
			Expect.Call(_workShiftFinder.FindBestShift(_part, _options, _scheduleMatrixPro, _effectiveRestriction, null)).Return(null).IgnoreArguments();
            Expect.Call(_workShiftFinder.FinderResult).Return(new WorkShiftFinderResult(_person,
                                                                                        new DateOnly(2011, 4, 18))).Repeat.Times(3);
            _mocks.ReplayAll();
			Assert.That(_target.SchedulePersonOnDay(_part, _options, true, _effectiveRestriction, _resourceCalculateDelayer), Is.False);
            Assert.That(_target.FinderResults.Count,Is.GreaterThan(0));
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldResetSchedulingOptionsShiftCategoryIfUsingSpecificCategory()
        {
            var resultHolder = _mocks.StrictMock<IWorkShiftCalculationResultHolder>();
            var projCashe = _mocks.StrictMock<IShiftProjectionCache>();
            var mainShift = _mocks.StrictMock<IMainShift>();
            var start = new DateTime(2011, 1, 18, 0, 0, 0, DateTimeKind.Utc);
            var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Hej");
			var useCategory = new PossibleStartEndCategory { ShiftCategory = shiftCategory };
            var period = new DateTimePeriod(start, start.AddDays(1));
            Expect.Call(_part.IsScheduled()).Return(false);
            Expect.Call(_part.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2011, 4, 18),
                                                                                    (
                                                                                        TimeZoneInfo.
                                                                                            FindSystemTimeZoneById("Utc"))));
            Expect.Call(_part.Person).Return(_person);
            Expect.Call(_effectiveRestriction.NotAvailable).Return(false);
            Expect.Call(
                () => _shiftCatLimitChecker.SetBlockedShiftCategories(_options, _person, (new DateOnly(2011, 4, 18))));
            Expect.Call(_scheduleMatrixListCreator.CreateMatrixListFromScheduleParts(new List<IScheduleDay> { _part })).Return(new List<IScheduleMatrixPro> { _scheduleMatrixPro });

			Expect.Call(_workShiftFinder.FindBestShift(_part, _options, _scheduleMatrixPro, _effectiveRestriction, null)).Return(resultHolder).IgnoreArguments();
				Expect.Call(_workShiftFinder.FinderResult).Return(new WorkShiftFinderResult(_person,
																													  new DateOnly(2011, 4, 18)));
            Expect.Call(resultHolder.ShiftProjection).Return(projCashe).Repeat.Twice();
            Expect.Call(projCashe.TheMainShift).Return(mainShift);
            Expect.Call(mainShift.EntityClone()).Return(mainShift);
            Expect.Call(() => _part.AddMainShift(mainShift));
            Expect.Call(() => _modifyRollback.Modify(_part));
            Expect.Call(projCashe.WorkShiftProjectionPeriod).Return(period);
            Expect.Call(_restrictionCreator.GetEffectiveRestriction(null, _options)).IgnoreArguments().Return(_effectiveRestriction);
        	Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(new DateOnly(2011, 04, 18), period)).Return(false);
			

            _mocks.ReplayAll();
            Assert.IsNull(_options.ShiftCategory);
			Assert.That(_target.SchedulePersonOnDay(_part, _options, true,  _resourceCalculateDelayer, useCategory), Is.True);
            Assert.IsNull(_options.ShiftCategory);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnTrueIfFindShift()
        {

            var resultHolder = _mocks.StrictMock<IWorkShiftCalculationResultHolder>();
            var projCashe = _mocks.StrictMock<IShiftProjectionCache>();
            var mainShift = _mocks.StrictMock<IMainShift>();
            var start = new DateTime(2011, 1, 18, 0, 0, 0, DateTimeKind.Utc);
            var period = new DateTimePeriod(start, start.AddDays(1));
            Expect.Call(_part.IsScheduled()).Return(false);
            Expect.Call(_part.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2011, 4, 18),
                                                                                    (
                                                                                        TimeZoneInfo.
                                                                                            FindSystemTimeZoneById("Utc"))));
            Expect.Call(_part.Person).Return(_person);
            Expect.Call(_effectiveRestriction.NotAvailable).Return(false);
            Expect.Call(
                () => _shiftCatLimitChecker.SetBlockedShiftCategories(_options, _person, (new DateOnly(2011, 4, 18))));
            Expect.Call(_scheduleMatrixListCreator.CreateMatrixListFromScheduleParts(new List<IScheduleDay> { _part })).Return(new List<IScheduleMatrixPro> { _scheduleMatrixPro });

			Expect.Call(_workShiftFinder.FindBestShift(_part, _options, _scheduleMatrixPro, _effectiveRestriction, null)).Return(resultHolder).IgnoreArguments();
				Expect.Call(_workShiftFinder.FinderResult).Return(new WorkShiftFinderResult(_person,
																														new DateOnly(2011, 4, 18)));
            Expect.Call(resultHolder.ShiftProjection).Return(projCashe).Repeat.Twice();
            Expect.Call(projCashe.TheMainShift).Return(mainShift);
            Expect.Call(mainShift.EntityClone()).Return(mainShift);
            Expect.Call(() =>_part.AddMainShift(mainShift));
            Expect.Call(() => _modifyRollback.Modify(_part));
            Expect.Call(projCashe.WorkShiftProjectionPeriod).Return(period);
			Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(new DateOnly(2011, 04, 18), period)).Return(false);

            _mocks.ReplayAll();
			Assert.That(_target.SchedulePersonOnDay(_part, _options, true, _effectiveRestriction, _resourceCalculateDelayer), Is.True);
            _mocks.VerifyAll();
        }
    }
}