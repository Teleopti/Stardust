using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
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
        private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
        private IList<IScheduleDay  > _daysToDelete;
        private IList<KeyValuePair<DayReadyToMove, IScheduleDay>> _daysToSave;
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
    	private ITeamSteadyStateHolder _teamSteadyStateHolder;
		private IScheduleDictionary _scheduleDictionary;
		private ITeamSteadyStateMainShiftScheduler _teamSteadyStateMainShiftScheduler;
		private IGroupPerson _groupPerson;
		private IGroupMoveTimeOptimizationResourceHelper _groupMoveTimeOptimizationResourceHelper;


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
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
            _daysToSave = new List<KeyValuePair<DayReadyToMove, IScheduleDay>> {new KeyValuePair<DayReadyToMove, IScheduleDay>(DayReadyToMove.FirstDay , _scheduleDay1),new KeyValuePair<DayReadyToMove, IScheduleDay>(DayReadyToMove.SecondDay , _scheduleDay2 )};
            _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _allMatrixes = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			_teamSteadyStateMainShiftScheduler = _mock.StrictMock<ITeamSteadyStateMainShiftScheduler>();
			_teamSteadyStateHolder = _mock.StrictMock<ITeamSteadyStateHolder>();
			_scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
			_groupPerson = _mock.StrictMock<IGroupPerson>();
			_groupMoveTimeOptimizationResourceHelper = _mock.StrictMock<IGroupMoveTimeOptimizationResourceHelper>();

            _target = new GroupMoveTimeOptimizationExecuter(_schedulePartModifyAndRollbackService, _deleteService,
                                                            _schedulingOptionsCreator, _optimizerPreferences,
                                                            _mainShiftOptimizeActivitySpecificationSetter,
                                                            _groupMatrixHelper,
                                                            _groupSchedulingService, _groupPersonBuilderForOptimization,
															_groupMoveTimeOptimizationResourceHelper);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldReturnTrueIfSuccess()
        {
            using (_mock.Record())
            {
                commonMocks();
                ShouldReturnTrueIfSuccessExpectValues();
                
                Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions, _optimizerPreferences, _mainShift, new DateOnly(2012, 1, 1))).Repeat.Twice() ;
                Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 1)).Repeat.AtLeastOnce();
                Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(new DateOnly(2012, 1, 1), _person, _groupSchedulingService,
                                                                    _schedulePartModifyAndRollbackService, _schedulingOptions,
                                                                    _groupPersonBuilderForOptimization, _allMatrixes)).Return(true).Repeat.Twice();
                Expect.Call(_optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(false).Repeat.Twice();
                Expect.Call(_optimizationOverLimitByRestrictionDecider.OverLimit()).Return(new List<DateOnly>()).Repeat.Twice();
                Expect.Call(_schedulingOptions.WorkShiftLengthHintOption).PropertyBehavior().Return(WorkShiftLengthHintOption.Long).Repeat.Twice();
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, new DateOnly(2012, 1, 1))).Return(_groupPerson);
            	Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(false);
				Expect.Call(_scheduleDay1.Person).Return(_person);
                
            }

            bool result;

            using (_mock.Playback())
            {
				result = _target.Execute(_daysToDelete, _daysToSave, _allMatrixes, _optimizationOverLimitByRestrictionDecider, _teamSteadyStateMainShiftScheduler, _teamSteadyStateHolder, _scheduleDictionary);
            }

            Assert.IsTrue(result);	
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldUseTeamSteadyState()
		{
			using (_mock.Record())
			{
				commonMocks();

				Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay1.Person).Return(_person);
				Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay2.Person).Return(_person);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 1)).Repeat.AtLeastOnce();
				
				Expect.Call(_optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(false).Repeat.Twice();
				Expect.Call(_optimizationOverLimitByRestrictionDecider.OverLimit()).Return(new List<DateOnly>()).Repeat.Twice();
				Expect.Call(_schedulingOptions.WorkShiftLengthHintOption).PropertyBehavior().Return(WorkShiftLengthHintOption.Long).Repeat.Twice();
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, new DateOnly(2012, 1, 1))).Return(_groupPerson).Repeat.Twice();
				Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true).Repeat.Twice();
				Expect.Call(_teamSteadyStateMainShiftScheduler.ScheduleTeam(new DateOnly(2012, 1, 1), _groupPerson, _groupSchedulingService,
																			_schedulePartModifyAndRollbackService,
																			_schedulingOptions, _groupPersonBuilderForOptimization,
																			_allMatrixes, _scheduleDictionary)).Return(true).Repeat.Twice();

			}

			bool result;

			using (_mock.Playback())
			{
				result = _target.Execute(_daysToDelete, _daysToSave, _allMatrixes, _optimizationOverLimitByRestrictionDecider, _teamSteadyStateMainShiftScheduler, _teamSteadyStateHolder, _scheduleDictionary);
			}

			Assert.IsTrue(result);		
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldUseRegularTeamOptimizationIfSteadyStateFails()
		{
			using (_mock.Record())
			{
				commonMocks();
				ShouldReturnTrueIfSuccessExpectValues();
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 1)).Repeat.AtLeastOnce();
				Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions, _optimizerPreferences, _mainShift, new DateOnly(2012, 1, 1))).Repeat.Twice();
				Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(new DateOnly(2012, 1, 1), _person, _groupSchedulingService,
																	_schedulePartModifyAndRollbackService, _schedulingOptions,
																	_groupPersonBuilderForOptimization, _allMatrixes)).Return(true).Repeat.Twice();
				
				Expect.Call(_scheduleDay1.Person).Return(_person);
				Expect.Call(_optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(false).Repeat.Twice();
				Expect.Call(_optimizationOverLimitByRestrictionDecider.OverLimit()).Return(new List<DateOnly>()).Repeat.Twice();
				Expect.Call(_schedulingOptions.WorkShiftLengthHintOption).PropertyBehavior().Return(WorkShiftLengthHintOption.Long).Repeat.Twice();
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, new DateOnly(2012, 1, 1))).Return(_groupPerson);
				Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);
				Expect.Call(_teamSteadyStateMainShiftScheduler.ScheduleTeam(new DateOnly(2012, 1, 1), _groupPerson, _groupSchedulingService,
																			_schedulePartModifyAndRollbackService,
																			_schedulingOptions, _groupPersonBuilderForOptimization,
																			_allMatrixes, _scheduleDictionary)).Return(false);

			}

			bool result;

			using (_mock.Playback())
			{
				result = _target.Execute(_daysToDelete, _daysToSave, _allMatrixes, _optimizationOverLimitByRestrictionDecider, _teamSteadyStateMainShiftScheduler, _teamSteadyStateHolder, _scheduleDictionary);
			}

			Assert.IsTrue(result);		
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldUseRegularTeamOptimizationIfNotInSteadyState()
		{
			using (_mock.Record())
			{
				commonMocks();
				ShouldReturnTrueIfSuccessExpectValues();
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 1)).Repeat.AtLeastOnce();
				Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions, _optimizerPreferences, _mainShift, new DateOnly(2012, 1, 1))).Repeat.Twice();
				Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(new DateOnly(2012, 1, 1), _person, _groupSchedulingService,
																	_schedulePartModifyAndRollbackService, _schedulingOptions,
																	_groupPersonBuilderForOptimization, _allMatrixes)).Return(true).Repeat.Twice();

				Expect.Call(_scheduleDay1.Person).Return(_person);
				Expect.Call(_optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(false).Repeat.Twice();
				Expect.Call(_optimizationOverLimitByRestrictionDecider.OverLimit()).Return(new List<DateOnly>()).Repeat.Twice();
				Expect.Call(_schedulingOptions.WorkShiftLengthHintOption).PropertyBehavior().Return(WorkShiftLengthHintOption.Long).Repeat.Twice();
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, new DateOnly(2012, 1, 1))).Return(_groupPerson);
				Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(false);

			}

			bool result;

			using (_mock.Playback())
			{
				result = _target.Execute(_daysToDelete, _daysToSave, _allMatrixes, _optimizationOverLimitByRestrictionDecider, _teamSteadyStateMainShiftScheduler, _teamSteadyStateHolder, _scheduleDictionary);
			}

			Assert.IsTrue(result);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldRollbackAndUseRegularTeamOptimizationIfSteadyStateFailsOnMoveMaxDaysOverLimit()
		{
			using (_mock.Record())
			{
				commonMocks();
				ShouldReturnTrueIfSuccessExpectValues();
		
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 1)).Repeat.AtLeastOnce();
				Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions, _optimizerPreferences, _mainShift, new DateOnly(2012, 1, 1))).Repeat.Twice();
				Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(new DateOnly(2012, 1, 1), _person, _groupSchedulingService,
																	_schedulePartModifyAndRollbackService, _schedulingOptions,
																	_groupPersonBuilderForOptimization, _allMatrixes)).Return(true).Repeat.Twice();

				Expect.Call(_scheduleDay1.Person).Return(_person);
				Expect.Call(_optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(true);
				Expect.Call(_optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(false);
				Expect.Call(_optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(false);
				Expect.Call(_optimizationOverLimitByRestrictionDecider.OverLimit()).Return(new List<DateOnly>()).Repeat.Twice();
				Expect.Call(_schedulingOptions.WorkShiftLengthHintOption).PropertyBehavior().Return(WorkShiftLengthHintOption.Long).Repeat.Twice();
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, new DateOnly(2012, 1, 1))).Return(_groupPerson);
				Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);
				Expect.Call(_teamSteadyStateMainShiftScheduler.ScheduleTeam(new DateOnly(2012, 1, 1), _groupPerson, _groupSchedulingService,
																			_schedulePartModifyAndRollbackService,
																			_schedulingOptions, _groupPersonBuilderForOptimization,
																			_allMatrixes, _scheduleDictionary)).Return(true);

				//Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());
				Expect.Call(() => _groupMoveTimeOptimizationResourceHelper.Rollback(_schedulePartModifyAndRollbackService, _scheduleDictionary));

			}

			bool result;

			using (_mock.Playback())
			{
				result = _target.Execute(_daysToDelete, _daysToSave, _allMatrixes, _optimizationOverLimitByRestrictionDecider, _teamSteadyStateMainShiftScheduler, _teamSteadyStateHolder, _scheduleDictionary);
			}

			Assert.IsTrue(result);	
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldRollbackAndUseRegularTeamOptimizationIfSteadyStateFailsOnOverLimit()
		{
			using (_mock.Record())
			{
				commonMocks();
				ShouldReturnTrueIfSuccessExpectValues();

				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 1)).Repeat.AtLeastOnce();
				Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions, _optimizerPreferences, _mainShift, new DateOnly(2012, 1, 1))).Repeat.Twice();
				Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(new DateOnly(2012, 1, 1), _person, _groupSchedulingService,
																	_schedulePartModifyAndRollbackService, _schedulingOptions,
																	_groupPersonBuilderForOptimization, _allMatrixes)).Return(true).Repeat.Twice();

				Expect.Call(_scheduleDay1.Person).Return(_person);
				Expect.Call(_optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(false).Repeat.AtLeastOnce();
				Expect.Call(_optimizationOverLimitByRestrictionDecider.OverLimit()).Return(new List<DateOnly>{new DateOnly()});
				Expect.Call(_optimizationOverLimitByRestrictionDecider.OverLimit()).Return(new List<DateOnly>());
				Expect.Call(_optimizationOverLimitByRestrictionDecider.OverLimit()).Return(new List<DateOnly>());
				Expect.Call(_schedulingOptions.WorkShiftLengthHintOption).PropertyBehavior().Return(WorkShiftLengthHintOption.Long).Repeat.Twice();
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, new DateOnly(2012, 1, 1))).Return(_groupPerson);
				Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);
				Expect.Call(_teamSteadyStateMainShiftScheduler.ScheduleTeam(new DateOnly(2012, 1, 1), _groupPerson, _groupSchedulingService,
																			_schedulePartModifyAndRollbackService,
																			_schedulingOptions, _groupPersonBuilderForOptimization,
																			_allMatrixes, _scheduleDictionary)).Return(true);

				//Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());
				Expect.Call(() => _groupMoveTimeOptimizationResourceHelper.Rollback(_schedulePartModifyAndRollbackService, _scheduleDictionary));

			}

			bool result;

			using (_mock.Playback())
			{
				result = _target.Execute(_daysToDelete, _daysToSave, _allMatrixes, _optimizationOverLimitByRestrictionDecider, _teamSteadyStateMainShiftScheduler, _teamSteadyStateHolder, _scheduleDictionary);
			}

			Assert.IsTrue(result);
		}

        private void ShouldReturnTrueIfSuccessExpectValues()
        {
            Expect.Call(_scheduleDay1.IsScheduled()).Return(true);
            Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDay1.Person).Return(_person);
            Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDay1.AssignmentHighZOrder()).Return(_personAssignment);
            Expect.Call(_scheduleDay2.IsScheduled()).Return(true);
            Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDay2.Person).Return(_person);
            Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDay2.AssignmentHighZOrder()).Return(_personAssignment);
            Expect.Call(_personAssignment.ToMainShift()).Return(_mainShift).Repeat.Twice();
			//Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2012, 1, 1), true, true)).Repeat.AtLeastOnce();
			//Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2012, 1, 2), true, true)).Repeat.AtLeastOnce();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldReturnFalseIfNotSuccess()
        {
            using (_mock.Record())
            {
                commonMocks();
                ShouldReturnFalseIfNotSuccessExpectValues();
                Expect.Call(_personAssignment.ToMainShift()).Return(_mainShift);
                Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions, _optimizerPreferences, _mainShift, new DateOnly(2012, 1, 1)));
                Expect.Call(_scheduleDay1.Person).Return(_person).Repeat.Twice();
                Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(new DateOnly(2012, 1, 1), _person, _groupSchedulingService,
                                                                    _schedulePartModifyAndRollbackService, _schedulingOptions,
                                                                    _groupPersonBuilderForOptimization, _allMatrixes)).Return(false);
                Expect.Call(_schedulingOptions.WorkShiftLengthHintOption).PropertyBehavior().Return(WorkShiftLengthHintOption.Long);
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, new DateOnly(2012, 1, 1))).Return(_groupPerson);
            	Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(false);
            }

            bool result;

            using (_mock.Playback())
            {
				result = _target.Execute(_daysToDelete, _daysToSave, _allMatrixes, _optimizationOverLimitByRestrictionDecider, _teamSteadyStateMainShiftScheduler, _teamSteadyStateHolder, _scheduleDictionary);
            }

            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldGetSchedulingOptions()
        {
            using(_mock.Record())
            {
                Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(null)).IgnoreArguments().Return(
                    _schedulingOptions);
            }
            using(_mock.Playback())
            {
                Assert.That(_target.SchedulingOptions, Is.EqualTo(_schedulingOptions));
            }
        }

        private void ShouldReturnFalseIfNotSuccessExpectValues()
        {
            var date = new DateOnly(2012, 1, 1);
            Expect.Call(_scheduleDay1.IsScheduled()).Return(true);
            Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDay1.AssignmentHighZOrder()).Return(_personAssignment);
            Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
            //Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
            Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(date).Repeat.AtLeastOnce();
			//Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(date, true, true)).Repeat.AtLeastOnce();
			//Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(date.AddDays(1), true, true)).Repeat.AtLeastOnce();
        }

        [Test]
        public void ShouldRollbackIfMovedToManyDays()
        {
            using (_mock.Record())
            {
                commonMocks();
                ShouldRollbackIfMovedToManyDaysExpectValues();
                Expect.Call(_optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(true);
                //Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());
            	Expect.Call(() =>_groupMoveTimeOptimizationResourceHelper.Rollback(_schedulePartModifyAndRollbackService,_scheduleDictionary));
				//Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2012, 1, 1), true, true)).Repeat.AtLeastOnce();
				//Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2012, 1, 2), true, true)).Repeat.AtLeastOnce();
                Expect.Call(_schedulingOptions.WorkShiftLengthHintOption).PropertyBehavior().Return(WorkShiftLengthHintOption.Long);
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, new DateOnly(2012, 1, 1))).Return(_groupPerson);
            	Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(false);
				Expect.Call(_scheduleDay1.Person).Return(_person);
            }

            bool result;

            using (_mock.Playback())
            {
				result = _target.Execute(_daysToDelete, _daysToSave, _allMatrixes, _optimizationOverLimitByRestrictionDecider, _teamSteadyStateMainShiftScheduler, _teamSteadyStateHolder, _scheduleDictionary);
            }

            Assert.IsFalse(result);
        }

        private void ShouldRollbackIfMovedToManyDaysExpectValues()
        {
            Expect.Call(_scheduleDay1.IsScheduled()).Return(true);
            Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDay1.AssignmentHighZOrder()).Return(_personAssignment);
            //Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
            Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 1)).Repeat.AtLeastOnce();
            Expect.Call(_personAssignment.ToMainShift()).Return(_mainShift);
            Expect.Call(
                () =>
                _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions, _optimizerPreferences,
                                                                               _mainShift, new DateOnly(2012, 1, 1)));
            Expect.Call(_scheduleDay1.Person).Return(_person);
            Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(new DateOnly(2012, 1, 1), _person, _groupSchedulingService,
                                                                _schedulePartModifyAndRollbackService, _schedulingOptions,
                                                                _groupPersonBuilderForOptimization, _allMatrixes)).Return(true);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldRollbackIfToManyRestrictionsBroken()
        {
            DateOnly date = new DateOnly(2012, 1, 1);
            using (_mock.Record())
            {
                commonMocks();
                Expect.Call(_scheduleDay1.IsScheduled()).Return(true);
                Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay1.AssignmentHighZOrder()).Return(_personAssignment);
                Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
                //Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
                Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 1)).Repeat.AtLeastOnce();
                Expect.Call(_personAssignment.ToMainShift()).Return(_mainShift);
                Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions, _optimizerPreferences, _mainShift, date));
                Expect.Call(_scheduleDay1.Person).Return(_person).Repeat.Twice();
                Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(date, _person, _groupSchedulingService,
                                                                    _schedulePartModifyAndRollbackService, _schedulingOptions,
                                                                    _groupPersonBuilderForOptimization, _allMatrixes)).Return(true);
                Expect.Call(_optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(false);
                Expect.Call(_optimizationOverLimitByRestrictionDecider.OverLimit()).Return(new List<DateOnly> { new DateOnly() });
                //Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());
            	Expect.Call(() =>_groupMoveTimeOptimizationResourceHelper.Rollback(_schedulePartModifyAndRollbackService,_scheduleDictionary));
				//Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(date, true, true)).Repeat.AtLeastOnce();
				//Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(date.AddDays(1), true, true)).Repeat.AtLeastOnce();
                Expect.Call(_schedulingOptions.WorkShiftLengthHintOption).PropertyBehavior().Return(WorkShiftLengthHintOption.Long);
            	Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, new DateOnly(2012, 1, 1))).Return(_groupPerson);
            	Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(false);
            }

            bool result;

            using (_mock.Playback())
            {
				result = _target.Execute(_daysToDelete, _daysToSave, _allMatrixes, _optimizationOverLimitByRestrictionDecider, _teamSteadyStateMainShiftScheduler, _teamSteadyStateHolder, _scheduleDictionary);
            }

            Assert.IsFalse(result);
        }

        private void commonMocks()
        {
            Expect.Call(() => _schedulePartModifyAndRollbackService.ClearModificationCollection());
            Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
            Expect.Call(() => _deleteService.Delete(_daysToDelete, _schedulePartModifyAndRollbackService));
        	Expect.Call(() => _groupMoveTimeOptimizationResourceHelper.CalculateDeletedDays(_daysToDelete));
            Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences)).Return(_schedulingOptions);
        }

    }

   
}
