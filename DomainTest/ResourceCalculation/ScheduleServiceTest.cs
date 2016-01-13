using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class ScheduleServiceTest
    {
        private MockRepository _mocks;
        private IWorkShiftFinderService _workShiftFinder;
		private IMatrixListFactory _matrixListFactory;
        private IShiftCategoryLimitationChecker _shiftCatLimitChecker;
        private ScheduleService _target;
        private IScheduleDay _part;
        private ISchedulingOptions _options;
        private IEffectiveRestriction _effectiveRestriction;
        private IPerson _person;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IWorkflowControlSet _workflowControlSet;
        private IEffectiveRestrictionCreator _restrictionCreator;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
    	private ISchedulePartModifyAndRollbackService _rollbackService;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _workShiftFinder = _mocks.StrictMock<IWorkShiftFinderService>();
			_matrixListFactory = _mocks.StrictMock<IMatrixListFactory>();
            _shiftCatLimitChecker = _mocks.StrictMock<IShiftCategoryLimitationChecker>();
            _restrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
            _options = new SchedulingOptions();
			_target = new ScheduleService(_workShiftFinder, _matrixListFactory, _shiftCatLimitChecker, _restrictionCreator);

            _part = _mocks.StrictMock<IScheduleDay>();
            
            _effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            _person = new Person();
            _workflowControlSet = _mocks.StrictMock<IWorkflowControlSet>();
            _person.WorkflowControlSet = _workflowControlSet;
            _scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
        	_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
        }
    

        [Test]
        public void ShouldReturnTrueIfAlreadyScheduled()
        {
            Expect.Call(_part.IsScheduled()).Return(true);
            _mocks.ReplayAll();
			Assert.That(_target.SchedulePersonOnDay(_part, _options, _effectiveRestriction, _resourceCalculateDelayer, _rollbackService), Is.True);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfEffectiveRestrictionIsNull()
        {
            Expect.Call(_part.IsScheduled()).Return(false);
            Expect.Call(_part.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2011, 4, 18),
                                                                                    (
                                                                                        TimeZoneInfo.
                                                                                            FindSystemTimeZoneById("Utc"))));
            Expect.Call(_part.Person).Return(_person);
            _mocks.ReplayAll();
			Assert.That(_target.SchedulePersonOnDay(_part, _options, null, _resourceCalculateDelayer, _rollbackService), Is.False);
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
			Assert.That(_target.SchedulePersonOnDay(_part, _options, _effectiveRestriction, _resourceCalculateDelayer, _rollbackService), Is.False);
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
			Expect.Call(_matrixListFactory.CreateMatrixListForSelection(new List<IScheduleDay> { _part })).Return(new List<IScheduleMatrixPro>());
            _mocks.ReplayAll();
			Assert.That(_target.SchedulePersonOnDay(_part, _options, _effectiveRestriction, _resourceCalculateDelayer, _rollbackService), Is.False);
            _mocks.VerifyAll();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
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
			Expect.Call(_matrixListFactory.CreateMatrixListForSelection(new List<IScheduleDay> { _part })).Return(new List<IScheduleMatrixPro> { _scheduleMatrixPro });
			Expect.Call(_workShiftFinder.FindBestShift(_part, _options, _scheduleMatrixPro, _effectiveRestriction)).Return(new WorkShiftFinderServiceResult(null, new WorkShiftFinderResult(_person,
																						new DateOnly(2011, 4, 18)))).IgnoreArguments();
            _mocks.ReplayAll();
			Assert.That(_target.SchedulePersonOnDay(_part, _options, _effectiveRestriction, _resourceCalculateDelayer, _rollbackService), Is.False);
            Assert.That(_target.FinderResults.Count,Is.GreaterThan(0));
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldResetSchedulingOptionsShiftCategoryIfUsingSpecificCategory()
        {
            var resultHolder = _mocks.StrictMock<IWorkShiftCalculationResultHolder>();
            var projCashe = _mocks.StrictMock<IShiftProjectionCache>();
			var mainShift = _mocks.StrictMock<IEditableShift>();
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
			Expect.Call(_matrixListFactory.CreateMatrixListForSelection(new List<IScheduleDay> { _part })).Return(new List<IScheduleMatrixPro> { _scheduleMatrixPro });

			Expect.Call(_workShiftFinder.FindBestShift(_part, _options, _scheduleMatrixPro, _effectiveRestriction)).Return(new WorkShiftFinderServiceResult(resultHolder, new WorkShiftFinderResult(_person,
																													  new DateOnly(2011, 4, 18)))).IgnoreArguments();
            Expect.Call(resultHolder.ShiftProjection).Return(projCashe).Repeat.Twice();
            Expect.Call(projCashe.TheMainShift).Return(mainShift);
            Expect.Call(() => _part.AddMainShift(mainShift));
            Expect.Call(() => _rollbackService.Modify(_part));
            Expect.Call(projCashe.WorkShiftProjectionPeriod).Return(period);
            Expect.Call(_restrictionCreator.GetEffectiveRestriction(null, _options)).IgnoreArguments().Return(_effectiveRestriction);
        	Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(new DateOnly(2011, 04, 18), period)).IgnoreArguments().Return(false);
			

            _mocks.ReplayAll();
            Assert.IsNull(_options.ShiftCategory);
			Assert.That(_target.SchedulePersonOnDay(_part, _options, _resourceCalculateDelayer, _rollbackService), Is.True);
            Assert.IsNull(_options.ShiftCategory);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnTrueIfFindShift()
        {

            var resultHolder = _mocks.StrictMock<IWorkShiftCalculationResultHolder>();
            var projCashe = _mocks.StrictMock<IShiftProjectionCache>();
			var mainShift = _mocks.StrictMock<IEditableShift>();
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
			Expect.Call(_matrixListFactory.CreateMatrixListForSelection(new List<IScheduleDay> { _part })).Return(new List<IScheduleMatrixPro> { _scheduleMatrixPro });

			Expect.Call(_workShiftFinder.FindBestShift(_part, _options, _scheduleMatrixPro, _effectiveRestriction)).Return(new WorkShiftFinderServiceResult(resultHolder, new WorkShiftFinderResult(_person, new DateOnly(2011, 4, 18)))).IgnoreArguments();
            Expect.Call(resultHolder.ShiftProjection).Return(projCashe).Repeat.Twice();
            Expect.Call(projCashe.TheMainShift).Return(mainShift);
            Expect.Call(() =>_part.AddMainShift(mainShift));
			Expect.Call(() => _rollbackService.Modify(_part));
            Expect.Call(projCashe.WorkShiftProjectionPeriod).Return(period);
			Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(new DateOnly(2011, 04, 18), period)).IgnoreArguments().Return(false);

            _mocks.ReplayAll();
			Assert.That(_target.SchedulePersonOnDay(_part, _options, _effectiveRestriction, _resourceCalculateDelayer, _rollbackService), Is.True);
            _mocks.VerifyAll();
        }
    }
}