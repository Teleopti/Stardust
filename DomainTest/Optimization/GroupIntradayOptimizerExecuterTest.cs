using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class GroupIntradayOptimizerExecuterTest
	{
		private MockRepository _mock;
		private IGroupIntradayOptimizerExecuter _target;
		private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private IDeleteAndResourceCalculateService _deleteService;
		private ISchedulingOptionsCreator _schedulingOptionsCreator;
		private IOptimizationPreferences _optimizerPreferences;
		private IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
		private IGroupMatrixHelper _groupMatrixHelper;
		private IGroupSchedulingService _groupSchedulingService;
		private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
		private IList<IScheduleDay> _daysToSave;
		private IList<IScheduleDay> _daysToDelete;
		private IList<IScheduleMatrixPro> _allMatrixes;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IScheduleDay _scheduleDay;
		private ISchedulingOptions _schedulingOptions;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
		private IMainShift _mainShift;
		private IPersonAssignment _personAssignment;
		private IPerson _person;
		private IResourceOptimizationHelper _resourceOptimizationHelper;
		private IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitByRestrictionDecider;
			
		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_schedulePartModifyAndRollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
            _deleteService = _mock.StrictMock<IDeleteAndResourceCalculateService>();
			_schedulingOptionsCreator = _mock.StrictMock<ISchedulingOptionsCreator>();
			_optimizerPreferences = new OptimizationPreferences();
			_mainShiftOptimizeActivitySpecificationSetter = _mock.StrictMock<IMainShiftOptimizeActivitySpecificationSetter>();
			_groupMatrixHelper = _mock.StrictMock<IGroupMatrixHelper>();
			_groupSchedulingService = _mock.StrictMock<IGroupSchedulingService>();
			_resourceOptimizationHelper = _mock.StrictMock<IResourceOptimizationHelper>();
			_groupPersonBuilderForOptimization = _mock.StrictMock<IGroupPersonBuilderForOptimization>();
			_target = new GroupIntradayOptimizerExecuter(_schedulePartModifyAndRollbackService, _deleteService,
			                                             _schedulingOptionsCreator, _optimizerPreferences,
			                                             _mainShiftOptimizeActivitySpecificationSetter, _groupMatrixHelper,
			                                             _groupSchedulingService, _groupPersonBuilderForOptimization,
														 _resourceOptimizationHelper);
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_daysToDelete = new List<IScheduleDay> { _scheduleDay };
			_daysToSave = new List<IScheduleDay> { _scheduleDay };
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_allMatrixes = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			_schedulingOptions = new SchedulingOptions();
			_dateOnlyAsDateTimePeriod = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
			_mainShift = _mock.StrictMock<IMainShift>();
			_personAssignment = _mock.StrictMock<IPersonAssignment>();
			_person = new Person();
			
			_optimizationOverLimitByRestrictionDecider = _mock.StrictMock<IOptimizationOverLimitByRestrictionDecider>();

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldRollbackIfToManyRestrictionsBroken()
		{
			DateOnly date = new DateOnly(2012, 1, 1);
			using (_mock.Record())
			{
				commonMocks();
				Expect.Call(_scheduleDay.IsScheduled()).Return(true);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(date);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay.AssignmentHighZOrder()).Return(_personAssignment);
				Expect.Call(_personAssignment.ToMainShift()).Return(_mainShift);
				Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions, _optimizerPreferences, _mainShift, date));
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(date, _person, _groupSchedulingService,
																	_schedulePartModifyAndRollbackService, _schedulingOptions,
																	_groupPersonBuilderForOptimization, _allMatrixes)).Return(true);
				Expect.Call(_optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(false);
				Expect.Call(_optimizationOverLimitByRestrictionDecider.OverLimit()).Return(new List<DateOnly>{new DateOnly()});
				Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(date, true, true));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(date.AddDays(1), true, true));
			}

			bool result;

			using (_mock.Playback())
			{
				result = _target.Execute(_daysToDelete, _daysToSave, _allMatrixes, _optimizationOverLimitByRestrictionDecider);
			}

			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldRollbackIfMovedToManyDays()
		{
			using (_mock.Record())
			{
				commonMocks();
				Expect.Call(_scheduleDay.IsScheduled()).Return(true);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 1));
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay.AssignmentHighZOrder()).Return(_personAssignment);
				Expect.Call(_personAssignment.ToMainShift()).Return(_mainShift);
				Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions, _optimizerPreferences, _mainShift, new DateOnly(2012, 1, 1)));
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(new DateOnly(2012, 1, 1), _person, _groupSchedulingService,
																	_schedulePartModifyAndRollbackService, _schedulingOptions,
																	_groupPersonBuilderForOptimization, _allMatrixes)).Return(true);
				Expect.Call(_optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(true);
				Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());
				Expect.Call(() =>_resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2012, 1, 1), true, true));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2012, 1, 2), true, true));
			}

			bool result;

			using (_mock.Playback())
			{
				result = _target.Execute(_daysToDelete, _daysToSave, _allMatrixes, _optimizationOverLimitByRestrictionDecider);
			}

			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnFalseIfNotSuccess()
		{
			using (_mock.Record())
			{
				commonMocks();
				Expect.Call(_scheduleDay.IsScheduled()).Return(true);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 1));
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay.AssignmentHighZOrder()).Return(_personAssignment);
				Expect.Call(_personAssignment.ToMainShift()).Return(_mainShift);
				Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions, _optimizerPreferences, _mainShift, new DateOnly(2012, 1, 1)));
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(new DateOnly(2012, 1, 1), _person, _groupSchedulingService,
				                                                    _schedulePartModifyAndRollbackService, _schedulingOptions,
				                                                    _groupPersonBuilderForOptimization, _allMatrixes)).Return(false);
			}

			bool result;

			using (_mock.Playback())
			{
				result = _target.Execute(_daysToDelete, _daysToSave, _allMatrixes, _optimizationOverLimitByRestrictionDecider);
			}

			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnTrueIfSuccess()
		{
			using (_mock.Record())
			{
				commonMocks();
				Expect.Call(_scheduleDay.IsScheduled()).Return(true);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 1));
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay.AssignmentHighZOrder()).Return(_personAssignment);
				Expect.Call(_personAssignment.ToMainShift()).Return(_mainShift);
				Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions, _optimizerPreferences, _mainShift, new DateOnly(2012, 1, 1)));
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(new DateOnly(2012, 1, 1), _person, _groupSchedulingService,
																	_schedulePartModifyAndRollbackService, _schedulingOptions,
																	_groupPersonBuilderForOptimization, _allMatrixes)).Return(true);
				Expect.Call(_optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(false);
				Expect.Call(_optimizationOverLimitByRestrictionDecider.OverLimit()).Return(new List<DateOnly>());
			}

			bool result;

			using (_mock.Playback())
			{
				result = _target.Execute(_daysToDelete, _daysToSave, _allMatrixes, _optimizationOverLimitByRestrictionDecider);
			}

			Assert.IsTrue(result);
		}

		private void commonMocks()
		{
			Expect.Call(() =>_schedulePartModifyAndRollbackService.ClearModificationCollection());
			Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
			Expect.Call(() => _deleteService.DeleteWithResourceCalculation(_daysToDelete, _schedulePartModifyAndRollbackService, true));
			Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences)).Return(_schedulingOptions);
		}

	}
}