using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class GroupMoveTimeOptimizationExecuterTest
    {
        private IGroupMoveTimeOptimizationExecuter _target;
        private MockRepository _mock;
        private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private IDeleteSchedulePartService _deleteService;
        private ISchedulingOptionsCreator _schedulingOptionsCreator;
        private OptimizationPreferences _optimizerPreferences;
        private IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
        private IGroupMatrixHelper _groupMatrixHelper;
        private IGroupSchedulingService _groupSchedulingService;
        private IResourceOptimizationHelper _resourceOptimizationHelper;
        private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
        private IList<IScheduleDay  > _daysToDelete;
        private IList<IScheduleDay> _daysToSave;
        private IList<IScheduleMatrixPro  > _allMatrixes;
        private IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitByRestrictionDecider;
        private IScheduleDay  _scheduleDay1;
        private ISchedulingOptions _schedulingOptions;
        private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
        private IPersonAssignment _personAssignment;
        private IMainShift _mainShift;
        private IPerson _person;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IScheduleDay _scheduleDay2;


        [ SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _schedulePartModifyAndRollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_deleteService = _mock.StrictMock<IDeleteSchedulePartService>();
			_schedulingOptionsCreator = _mock.StrictMock<ISchedulingOptionsCreator>();
			_optimizerPreferences = new OptimizationPreferences();
			_mainShiftOptimizeActivitySpecificationSetter = _mock.StrictMock<IMainShiftOptimizeActivitySpecificationSetter>();
			_groupMatrixHelper = _mock.StrictMock<IGroupMatrixHelper>();
			_groupSchedulingService = _mock.StrictMock<IGroupSchedulingService>();
			_resourceOptimizationHelper = _mock.StrictMock<IResourceOptimizationHelper>();
			_groupPersonBuilderForOptimization = _mock.StrictMock<IGroupPersonBuilderForOptimization>();
            _scheduleDay1 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mock.StrictMock<IScheduleDay>();
            _schedulingOptions = _mock.StrictMock<ISchedulingOptions>();
            _dateOnlyAsDateTimePeriod = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
            _personAssignment = _mock.StrictMock<IPersonAssignment>();
            _mainShift = _mock.StrictMock<IMainShift>();
            _person = _mock.StrictMock<IPerson>();
            _optimizationOverLimitByRestrictionDecider = _mock.StrictMock<IOptimizationOverLimitByRestrictionDecider>();

            _daysToDelete = new List<IScheduleDay> { _scheduleDay1,_scheduleDay2 };
            _daysToSave = new List<IScheduleDay> { _scheduleDay1, _scheduleDay2 };
            _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _allMatrixes = new List<IScheduleMatrixPro> { _scheduleMatrixPro };

            _target = new GroupMoveTimeOptimizationExecuter(_schedulePartModifyAndRollbackService, _deleteService,
                                                            _schedulingOptionsCreator, _optimizerPreferences,
                                                            _mainShiftOptimizeActivitySpecificationSetter,
                                                            _groupMatrixHelper,
                                                            _groupSchedulingService, _groupPersonBuilderForOptimization,
                                                            _resourceOptimizationHelper);
        }

        
        [Test]
        public void ShouldReturnTrueIfSuccess()
        {
            using (_mock.Record())
            {
                commonMocks();
                Expect.Call(_scheduleDay1.IsScheduled()).Return(true);
                Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_scheduleDay1.Person).Return(_person);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce() ;
                Expect.Call(_scheduleDay1.AssignmentHighZOrder()).Return(_personAssignment);
                Expect.Call(_scheduleDay2.IsScheduled()).Return(true);
                Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_scheduleDay2.Person).Return(_person);
                Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay2.AssignmentHighZOrder()).Return(_personAssignment);
                Expect.Call(_personAssignment.MainShift).Return(_mainShift).Repeat.Twice();
                Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions, _optimizerPreferences, _mainShift, new DateOnly(2012, 1, 1))).Repeat.Twice() ;
                Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 1)).Repeat.Twice();
                Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(new DateOnly(2012, 1, 1), _person, _groupSchedulingService,
                                                                    _schedulePartModifyAndRollbackService, _schedulingOptions,
                                                                    _groupPersonBuilderForOptimization, _allMatrixes)).Return(true).Repeat.Twice();
                Expect.Call(_optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(false).Repeat.Twice();
                Expect.Call(_optimizationOverLimitByRestrictionDecider.OverLimit()).Return(new List<DateOnly>()).Repeat.Twice();
                Expect.Call(_schedulingOptions.WorkShiftLengthHintOption).PropertyBehavior().Return(WorkShiftLengthHintOption.Long).Repeat.Twice();
                
            }

            bool result;

            using (_mock.Playback())
            {
                result = _target.Execute(_daysToDelete, _daysToSave, _allMatrixes, _optimizationOverLimitByRestrictionDecider);
            }

            Assert.IsTrue(result);
        }

        [Test]
        public void ShouldReturnFalseIfNotSuccess()
        {
            using (_mock.Record())
            {
                commonMocks();
                Expect.Call(_scheduleDay1.IsScheduled()).Return(true);
                Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay1.AssignmentHighZOrder()).Return(_personAssignment);
                Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 1));
                Expect.Call(_personAssignment.MainShift).Return(_mainShift);
                Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions, _optimizerPreferences, _mainShift, new DateOnly(2012, 1, 1)));
                Expect.Call(_scheduleDay1.Person).Return(_person);
                Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(new DateOnly(2012, 1, 1), _person, _groupSchedulingService,
                                                                    _schedulePartModifyAndRollbackService, _schedulingOptions,
                                                                    _groupPersonBuilderForOptimization, _allMatrixes)).Return(false);
                Expect.Call(_schedulingOptions.WorkShiftLengthHintOption).PropertyBehavior().Return(WorkShiftLengthHintOption.Long);
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
                Expect.Call(_scheduleDay1.IsScheduled()).Return(true);
                Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay1.AssignmentHighZOrder()).Return(_personAssignment);
                Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 1));
                Expect.Call(_personAssignment.MainShift).Return(_mainShift);
                Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions, _optimizerPreferences, _mainShift, new DateOnly(2012, 1, 1)));
                Expect.Call(_scheduleDay1.Person).Return(_person);
                Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(new DateOnly(2012, 1, 1), _person, _groupSchedulingService,
                                                                    _schedulePartModifyAndRollbackService, _schedulingOptions,
                                                                    _groupPersonBuilderForOptimization, _allMatrixes)).Return(true);
                Expect.Call(_optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(true);
                Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2012, 1, 1), true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2012, 1, 2), true, true));
                Expect.Call(_schedulingOptions.WorkShiftLengthHintOption).PropertyBehavior().Return(WorkShiftLengthHintOption.Long);
            }

            bool result;

            using (_mock.Playback())
            {
                result = _target.Execute(_daysToDelete, _daysToSave, _allMatrixes, _optimizationOverLimitByRestrictionDecider);
            }

            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldRollbackIfToManyRestrictionsBroken()
        {
            DateOnly date = new DateOnly(2012, 1, 1);
            using (_mock.Record())
            {
                commonMocks();
                Expect.Call(_scheduleDay1.IsScheduled()).Return(true);
                Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay1.AssignmentHighZOrder()).Return(_personAssignment);
                Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 1));
                Expect.Call(_personAssignment.MainShift).Return(_mainShift);
                Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions, _optimizerPreferences, _mainShift, date));
                Expect.Call(_scheduleDay1.Person).Return(_person);
                Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(date, _person, _groupSchedulingService,
                                                                    _schedulePartModifyAndRollbackService, _schedulingOptions,
                                                                    _groupPersonBuilderForOptimization, _allMatrixes)).Return(true);
                Expect.Call(_optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(false);
                Expect.Call(_optimizationOverLimitByRestrictionDecider.OverLimit()).Return(new List<DateOnly> { new DateOnly() });
                Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(date, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(date.AddDays(1), true, true));
                Expect.Call(_schedulingOptions.WorkShiftLengthHintOption).PropertyBehavior().Return(WorkShiftLengthHintOption.Long);
            }

            bool result;

            using (_mock.Playback())
            {
                result = _target.Execute(_daysToDelete, _daysToSave, _allMatrixes, _optimizationOverLimitByRestrictionDecider);
            }

            Assert.IsFalse(result);
        }

        private void commonMocks()
        {
            Expect.Call(() => _schedulePartModifyAndRollbackService.ClearModificationCollection());
            Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
            Expect.Call(() => _deleteService.Delete(_daysToDelete, _schedulePartModifyAndRollbackService));
            Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences)).Return(_schedulingOptions);
        }

    }

   
}
