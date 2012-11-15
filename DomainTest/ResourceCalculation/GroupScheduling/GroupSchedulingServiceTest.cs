﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation.GroupScheduling
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
    public class GroupSchedulingServiceTest
    {
    	private readonly DateOnly _date1 = new DateOnly(2010, 10, 11);
    	private readonly DateOnly _date2 = new DateOnly(2010, 10, 12);

        private GroupSchedulingService _target;
        private MockRepository _mock;
        private IBestBlockShiftCategoryFinder _bestBlockShiftCategoryFinder;
		private IGroupPersonsBuilder _groupPersonsBuilder;
    	private IScheduleDictionary _scheduleDictionary;
    	private IScheduleDay _scheduleDay;
        private ISchedulePartModifyAndRollbackService _rollbackService;
    	private IShiftCategory _shiftCategory;
    	private IPerson _person1;
		private IPerson _person2;
    	private ReadOnlyCollection<IPerson> _persons;
    	private IGroupPerson _groupPerson;
    	private List<IGroupPerson> _groupPersons;
    	private IResourceOptimizationHelper _resourceOptimizationHelper;
		private WorkShiftFinderResultHolder _schedulingResults;
		private IScheduleService _scheduleService;
        private ISchedulingResultStateHolder _stateHolder;
        private IList<IPerson> _selectedPersons;
        private BackgroundWorker _bgWorker;
        private ISchedulingOptions _schedulingOptions;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IScheduleDayPro _scheduleDayPro;
    	private IPossibleStartEndCategory _useCategory;
    	private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
    	private IEffectiveRestriction _effectiveRestriction;
    	private IVirtualSchedulePeriod _schedulePeriod;
        private IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
  		private IDeleteSchedulePartService _deleteSchedulePartService;
        private IShiftCategoryLimitationChecker _shiftCategoryLimitationChecker;
    	private ITeamSteadyStateMainShiftScheduler _teamSteadyStateMainShiftScheduler;
    	private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
    	private IFairnessValueResult _fairnessValueResult;
    	private ITeamSteadyStateHolder _teamSteadyStateHolder;
			
		[SetUp]
        public void Setup()
    	{
			_mock = new MockRepository();
			_resourceCalculateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
			_shiftCategory = new ShiftCategory("kat");
    		_person1 = _mock.StrictMock<IPerson>();
			_person2 = _mock.StrictMock<IPerson>();
			_persons = new ReadOnlyCollection<IPerson>(new List<IPerson> { _person1, _person2 });
			_groupPerson = _mock.StrictMock<IGroupPerson>();
			_groupPersons = new List<IGroupPerson> { _groupPerson };
			_useCategory = new PossibleStartEndCategory { ShiftCategory = _shiftCategory };
    		_resourceOptimizationHelper = _mock.StrictMock<IResourceOptimizationHelper>();
    		_bestBlockShiftCategoryFinder = _mock.StrictMock<IBestBlockShiftCategoryFinder>();
			_groupPersonsBuilder = _mock.StrictMock<IGroupPersonsBuilder>();
            _stateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
    		_scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
			_rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
    		_scheduleDay= _mock.StrictMock<IScheduleDay>();
			_schedulingResults = new WorkShiftFinderResultHolder();
    	    _scheduleService = _mock.StrictMock<IScheduleService>();
            _selectedPersons = new List<IPerson>();
            _bgWorker = new BackgroundWorker();
            _schedulingOptions = new SchedulingOptions();
    		_effectiveRestrictionCreator = _mock.StrictMock<IEffectiveRestrictionCreator>();
    		_effectiveRestriction = _mock.DynamicMock<IEffectiveRestriction>();
    		_schedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
    	    _workShiftMinMaxCalculator = _mock.StrictMock<IWorkShiftMinMaxCalculator>();
            _deleteSchedulePartService = _mock.StrictMock<IDeleteSchedulePartService>();
            _shiftCategoryLimitationChecker = _mock.StrictMock<IShiftCategoryLimitationChecker>();
    	    _target = new GroupSchedulingService(_groupPersonsBuilder,
													_bestBlockShiftCategoryFinder,
                                                    _stateHolder,
													_scheduleService, 
													_rollbackService, 
													_resourceOptimizationHelper,
													_schedulingResults,
													_effectiveRestrictionCreator,
                                                    _workShiftMinMaxCalculator,
                                                    _deleteSchedulePartService,
                                                    _shiftCategoryLimitationChecker);

            _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();

			_teamSteadyStateMainShiftScheduler = _mock.StrictMock<ITeamSteadyStateMainShiftScheduler>();
			_groupPersonBuilderForOptimization = _mock.StrictMock<IGroupPersonBuilderForOptimization>();
			_fairnessValueResult = _mock.StrictMock<IFairnessValueResult>();
			_teamSteadyStateHolder = _mock.StrictMock<ITeamSteadyStateHolder>();
    	}

        [TearDown]
        public void Teardown()
        {
            _bgWorker.Dispose();
        }
        [Test]
        public void VerifyCreate()
        {
            Assert.IsNotNull(_target);
        }

		[Test]
		public void ShouldUseSteadyStateMainShiftSchedulerWhenInSteadyState()
		{
			var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };

			using(_mock.Record())
			{
				Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_date1, _selectedPersons, true, _schedulingOptions)).Return(_groupPersons);
				Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_date2, _selectedPersons, true, _schedulingOptions)).Return(_groupPersons);
				Expect.Call(_teamSteadyStateMainShiftScheduler.ScheduleTeam(_date1, _groupPerson, _target, null, null,
				                                                            _groupPersonBuilderForOptimization, matrixProList,
				                                                            _scheduleDictionary)).IgnoreArguments().Return(true);

				Expect.Call(_teamSteadyStateMainShiftScheduler.ScheduleTeam(_date2, _groupPerson, _target, null, null,
																			_groupPersonBuilderForOptimization, matrixProList,
																			_scheduleDictionary)).IgnoreArguments().Return(true);

				Expect.Call(_stateHolder.Schedules).Return(null).Repeat.Twice();
				Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true).Repeat.Twice();
			}

			using(_mock.Playback())
			{
				_target.Execute(new DateOnlyPeriod(_date1, _date2), matrixProList, _schedulingOptions, _selectedPersons, _bgWorker, _teamSteadyStateHolder, _teamSteadyStateMainShiftScheduler, _groupPersonBuilderForOptimization);	
			}	
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldTryToScheduleWithoutSteadyStateWhenSteadyStateFails()
		{
			var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };

			using (_mock.Record())
			{
				Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_date1, _selectedPersons, true, _schedulingOptions)).Return(_groupPersons);
				Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_date2, _selectedPersons, true, _schedulingOptions)).Return(_groupPersons);
				Expect.Call(_teamSteadyStateMainShiftScheduler.ScheduleTeam(_date1, _groupPerson, _target, null, null,
																			_groupPersonBuilderForOptimization, matrixProList,
																			_scheduleDictionary)).IgnoreArguments().Return(true);

				Expect.Call(_teamSteadyStateMainShiftScheduler.ScheduleTeam(_date2, _groupPerson, _target, null, null,
																			_groupPersonBuilderForOptimization, matrixProList,
																			_scheduleDictionary)).IgnoreArguments().Return(false);

				Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary).Repeat.AtLeastOnce();
				//Expect.Call(_groupPerson.Id).Return(_guid).Repeat.AtLeastOnce();
				//Expect.Call(() => _rollbackService.Rollback());
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson>()));
				Expect.Call(_scheduleDictionary.AverageFairnessPoints(null)).IgnoreArguments().Return(_fairnessValueResult);
				Expect.Call(_groupPerson.CommonPossibleStartEndCategory).Return(new PossibleStartEndCategory());
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(null, new DateOnly(), null, null)).IgnoreArguments().Return(null);
                Expect.Call(
                   () => _shiftCategoryLimitationChecker.SetBlockedShiftCategories(_schedulingOptions, _person1, _date1)).IgnoreArguments().Repeat.AtLeastOnce();

				Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true).Repeat.Twice();
				Expect.Call(()=>_teamSteadyStateHolder.SetSteadyState(_groupPerson, false));

			}

			using (_mock.Playback())
			{
				_target.Execute(new DateOnlyPeriod(_date1, _date2), matrixProList, _schedulingOptions, _selectedPersons, _bgWorker, _teamSteadyStateHolder, _teamSteadyStateMainShiftScheduler, _groupPersonBuilderForOptimization);
			}	
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldDoSomethingIfSuccess()
        {
            var range1 = _mock.StrictMock<IScheduleRange>();
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
        	var useCategory = new PossibleStartEndCategory {ShiftCategory = _shiftCategory};

            using (_mock.Record())
            {
                commonMocks(false);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_persons, _date1, _schedulingOptions,
																			 _scheduleDictionary)).Return(
																				_effectiveRestriction);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_persons, _date2, _schedulingOptions,
																				 _scheduleDictionary)).Return(
																					_effectiveRestriction);
                Expect.Call(_scheduleDictionary[_person1]).Return(range1);
                Expect.Call(range1.ScheduledDay(_date1)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, useCategory, _rollbackService)).IgnoreArguments()
					.Return(true);
                Expect.Call(_scheduleDictionary[_person2]).Return(range1);
                Expect.Call(range1.ScheduledDay(_date1)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, useCategory, _rollbackService)).IgnoreArguments()
                    .Return(true);
                Expect.Call(_scheduleDictionary[_person1]).Return(range1);
                Expect.Call(range1.ScheduledDay(_date2)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, useCategory, _rollbackService)).IgnoreArguments()
                    .Return(true);
                Expect.Call(_scheduleDictionary[_person2]).Return(range1);
                Expect.Call(range1.ScheduledDay(_date2)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, useCategory, _rollbackService)).IgnoreArguments()
                    .Return(true);

                Expect.Call(_scheduleMatrixPro.Person).Return(_person1).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.Person).Return(_person1).Repeat.AtLeastOnce();
            	Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_date1, _date1)).Repeat.
            		AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
 				Expect.Call(
                   () => _shiftCategoryLimitationChecker.SetBlockedShiftCategories(_schedulingOptions, _person1, _date1)).IgnoreArguments().Repeat.AtLeastOnce();
            
				
            	Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(false).Repeat.Twice();
            }

            using (_mock.Playback())
            {
				_target.Execute(new DateOnlyPeriod(_date1, _date2), matrixProList, _schedulingOptions, _selectedPersons, _bgWorker, _teamSteadyStateHolder, _teamSteadyStateMainShiftScheduler, _groupPersonBuilderForOptimization);	
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldRollbackGroupIfAnyoneInGroupIsUnsuccessful()
        {
            var range1 = _mock.StrictMock<IScheduleRange>();
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });

            using (_mock.Record())
            {
                commonMocks(false);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_persons, _date1, _schedulingOptions,
																			 _scheduleDictionary)).Return(
																				_effectiveRestriction);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_persons, _date2, _schedulingOptions,
																				 _scheduleDictionary)).Return(
																					_effectiveRestriction);
                Expect.Call(_scheduleDictionary[_person1]).Return(range1);
                Expect.Call(range1.ScheduledDay(_date1)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);

				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, _useCategory, _rollbackService)).IgnoreArguments()
                    .Return(true);

                Expect.Call(_scheduleDictionary[_person2]).Return(range1);
                Expect.Call(range1.ScheduledDay(_date1)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, _useCategory, _rollbackService)).IgnoreArguments()
                    .Return(false);
                Expect.Call(() => _rollbackService.Rollback());
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_date1, true, true));

                Expect.Call(_scheduleDictionary[_person1]).Return(range1);
                Expect.Call(range1.ScheduledDay(_date2)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, _useCategory, _rollbackService)).IgnoreArguments()
                    .Return(true);

                Expect.Call(_scheduleDictionary[_person2]).Return(range1);
                Expect.Call(range1.ScheduledDay(_date2)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, _useCategory, _rollbackService)).IgnoreArguments()
                    .Return(true);

                Expect.Call(_scheduleMatrixPro.Person).Return(_person1).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.Person).Return(_person1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_date1, _date1)).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
 				Expect.Call(
                    () => _shiftCategoryLimitationChecker.SetBlockedShiftCategories(_schedulingOptions, _person1, _date1)).IgnoreArguments().Repeat.AtLeastOnce();
			
            	Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(false).Repeat.Twice();
            }

            using (_mock.Playback())
            {
				_target.Execute(new DateOnlyPeriod(_date1, _date2), matrixProList, _schedulingOptions, _selectedPersons, _bgWorker, _teamSteadyStateHolder, _teamSteadyStateMainShiftScheduler, _groupPersonBuilderForOptimization);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldJumpIfBestShiftCategoryIsNull()
        {
            var range1 = _mock.StrictMock<IScheduleRange>();
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });

            using (_mock.Record())
            {
                commonMocks(true);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_persons, _date2, _schedulingOptions,
																				 _scheduleDictionary)).Return(
																					_effectiveRestriction);
                Expect.Call(() => _rollbackService.Rollback());
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_date1, true, true));
                Expect.Call(_scheduleDictionary[_person1]).Return(range1);
                Expect.Call(range1.ScheduledDay(_date2)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, _useCategory, _rollbackService)).IgnoreArguments()
                    .Return(true);

                Expect.Call(_scheduleDictionary[_person2]).Return(range1);
                Expect.Call(range1.ScheduledDay(_date2)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, _useCategory, _rollbackService)).IgnoreArguments()
                    .Return(true);

                Expect.Call(_scheduleMatrixPro.Person).Return(_person1).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.Person).Return(_person1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
            	Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_date1, _date2)).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
  				Expect.Call(
                   () => _shiftCategoryLimitationChecker.SetBlockedShiftCategories(_schedulingOptions, _person1, _date1)).IgnoreArguments().Repeat.AtLeastOnce();
            	
            	Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(false).Repeat.Twice();

            }

            using (_mock.Playback())
            {
				_target.Execute(new DateOnlyPeriod(_date1, _date2), matrixProList, _schedulingOptions, _selectedPersons, _bgWorker, _teamSteadyStateHolder, _teamSteadyStateMainShiftScheduler, _groupPersonBuilderForOptimization);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldNotScheduleAlreadyAssignedAgent()
        {
            var range1 = _mock.StrictMock<IScheduleRange>();
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };;
            var dateOnlyPeriod = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();

            using (_mock.Record())
            {
                commonMocks(true);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_persons, _date2, _schedulingOptions,
																				 _scheduleDictionary)).Return(
																					_effectiveRestriction);
                Expect.Call(() => _rollbackService.Rollback());
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_date1, true, true));
                Expect.Call(_scheduleDictionary[_person1]).Return(range1);
                Expect.Call(range1.ScheduledDay(_date2)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(true);

                Expect.Call(_scheduleDictionary[_person2]).Return(range1);
                Expect.Call(range1.ScheduledDay(_date2)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(true);

                //Expect.Call(_scheduleDayPro.Day).Return(dateOnly).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyPeriod).Repeat.Any();
                //Expect.Call(dateOnlyPeriod.DateOnly).Return(dateOnly).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.Person).Return(_person1).Repeat.Any();
   				Expect.Call(
                   () => _shiftCategoryLimitationChecker.SetBlockedShiftCategories(_schedulingOptions, _person1, _date1)).IgnoreArguments().Repeat.AtLeastOnce();
            	
            	Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(false).Repeat.Twice();
            }

            using (_mock.Playback())
            {
				_target.Execute(new DateOnlyPeriod(_date1, _date2), matrixProList, _schedulingOptions, _selectedPersons, _bgWorker, _teamSteadyStateHolder, _teamSteadyStateMainShiftScheduler, _groupPersonBuilderForOptimization);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldNotScheduleLockedDay()
        {
            var range1 = _mock.StrictMock<IScheduleRange>();
            var matrixProList = new List<IScheduleMatrixPro>();
            
            using (_mock.Record())
            {
                commonMocks(true);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_persons, _date2, _schedulingOptions,
																				 _scheduleDictionary)).Return(
																					_effectiveRestriction);
                Expect.Call(() => _rollbackService.Rollback());
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_date1, true, true));
                Expect.Call(_scheduleDictionary[_person1]).Return(range1);
                Expect.Call(range1.ScheduledDay(_date2)).Return(_scheduleDay);

                Expect.Call(_scheduleDictionary[_person2]).Return(range1);
                Expect.Call(range1.ScheduledDay(_date2)).Return(_scheduleDay);
            	Expect.Call(_scheduleDay.IsScheduled()).Return(true).Repeat.AtLeastOnce();
 				Expect.Call(
                   () => _shiftCategoryLimitationChecker.SetBlockedShiftCategories(_schedulingOptions, _person1, _date1)).IgnoreArguments().Repeat.AtLeastOnce();
           		//Expect.Call(_groupPerson.Id).Return(_guid).Repeat.AtLeastOnce();
            	Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(false).Repeat.Twice();
            }

            using (_mock.Playback())
            {
				_target.Execute(new DateOnlyPeriod(_date1, _date2), matrixProList, _schedulingOptions, _selectedPersons, _bgWorker, _teamSteadyStateHolder, _teamSteadyStateMainShiftScheduler, _groupPersonBuilderForOptimization);
            }   
        }

    	private void commonMocks(bool failOnBestShiftCategoryForDays)
    	{
    		var returnPoss = new PossibleStartEndCategory {ShiftCategory = _shiftCategory};
			if (failOnBestShiftCategoryForDays)
				returnPoss = null;

			var result = new BestShiftCategoryResult(returnPoss, FailureCause.NoFailure);
			IBlockFinderResult result1 = new BlockFinderResult(null, new List<DateOnly> {_date1 },
    		                                                   new Dictionary<string, IWorkShiftFinderResult>());
			IBlockFinderResult result2 = new BlockFinderResult(null, new List<DateOnly> { _date2 },
															   new Dictionary<string, IWorkShiftFinderResult>());
            Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary).Repeat.Any();
			Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_date1, _selectedPersons, true, _schedulingOptions)).Return(_groupPersons);
			Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_date2, _selectedPersons, true, _schedulingOptions)).Return(_groupPersons);
			Expect.Call(_groupPerson.CommonPossibleStartEndCategory).Return(null).Repeat.AtLeastOnce();
    		Expect.Call(_groupPerson.GroupMembers).Return(_persons).Repeat.Twice();
    		Expect.Call(_scheduleDictionary.AverageFairnessPoints(_persons)).Repeat.Any();
    		Expect.Call(
				_bestBlockShiftCategoryFinder.BestShiftCategoryForDays(result1, _groupPerson, _schedulingOptions, null)).Return(result).IgnoreArguments();
    		Expect.Call(
    			_bestBlockShiftCategoryFinder.BestShiftCategoryForDays(result2, _groupPerson, _schedulingOptions, null)).Return(
					new BestShiftCategoryResult(new PossibleStartEndCategory{ShiftCategory = _shiftCategory}, FailureCause.NoFailure)).IgnoreArguments();
    		Expect.Call(() => _rollbackService.ClearModificationCollection()).Repeat.Twice();
    		Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_date1)).Return(_scheduleDayPro).Repeat.Any();
			Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_date2)).Return(_scheduleDayPro).Repeat.Any();

    	}

		[Test]
		public void ShouldNotScheduleOneDayOnePersonSteadyStateWhenGroupPersonIsNull()
		{
			var result = _target.ScheduleOneDayOnePersonSteadyState(new DateOnly(), _person1, _schedulingOptions, null, new List<IScheduleMatrixPro>());
			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldScheduleOneDayOnePersonSteadyState()
		{
			var range = _mock.StrictMock<IScheduleRange>();
	
			using(_mock.Record())
			{
				Expect.Call(_groupPerson.GroupMembers).Return(_persons);
				Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_persons, _date1, _schedulingOptions,_scheduleDictionary)).Return(_effectiveRestriction);
				Expect.Call(_scheduleDictionary[_person1]).Return(range);
				Expect.Call(range.ScheduledDay(_date1)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.IsScheduled()).Return(true);
			}

			using(_mock.Playback())
			{
				var result = _target.ScheduleOneDayOnePersonSteadyState(_date1, _person1, _schedulingOptions, _groupPerson, new List<IScheduleMatrixPro>());
				Assert.IsTrue(result);
			}	
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowIfMatrixIsNull()
		{
			_target.Execute(new DateOnlyPeriod(new DateOnly(), new DateOnly()), null, _schedulingOptions, _persons, _bgWorker, _teamSteadyStateHolder, _teamSteadyStateMainShiftScheduler, _groupPersonBuilderForOptimization);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowIfWorkerIsNull()
		{
			var matrixProList = new List<IScheduleMatrixPro>();
			_target.Execute(new DateOnlyPeriod(new DateOnly(), new DateOnly()), matrixProList, _schedulingOptions, _persons, null, _teamSteadyStateHolder, _teamSteadyStateMainShiftScheduler, _groupPersonBuilderForOptimization);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowIfMatrixIsNullOnSchedule()
		{
			_target.ScheduleOneDay(new DateOnly(), _schedulingOptions,_groupPerson, null);
		}
    }
}
