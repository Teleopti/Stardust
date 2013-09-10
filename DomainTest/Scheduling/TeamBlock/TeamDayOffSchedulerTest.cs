using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamDayOffSchedulerTest
	{
		private ITeamDayOffScheduler _target;
		private IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private IScheduleDayAvailableForDayOffSpecification _scheduleDayAvailableForDayOffSpecification;
		private IHasContractDayOffDefinition _hasContractDayOffDefinition;
		private IMatrixDataListInSteadyState _matrixDataListInSteadyState;
		private IMatrixDataListCreator _matrixDataListCreator;
		private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private SchedulingOptions _schedulingOptions;
		private IVirtualSchedulePeriod _schedulePeriod;
		private IScheduleDayPro _scheduleDayPro;
		private IScheduleMatrixPro _scheduleMatrixPro;

		[SetUp]
		public void Setup()
		{
			_schedulingOptions = new SchedulingOptions();
			_dayOffsInPeriodCalculator = MockRepository.GenerateMock<IDayOffsInPeriodCalculator>();

			_effectiveRestrictionCreator = MockRepository.GenerateMock<IEffectiveRestrictionCreator>();
			_schedulePartModifyAndRollbackService = MockRepository.GenerateMock<ISchedulePartModifyAndRollbackService>();

			_scheduleDayAvailableForDayOffSpecification = MockRepository.GenerateMock<IScheduleDayAvailableForDayOffSpecification>();
			_hasContractDayOffDefinition = MockRepository.GenerateMock<IHasContractDayOffDefinition>();

			_matrixDataListInSteadyState = MockRepository.GenerateMock<IMatrixDataListInSteadyState>();
			_matrixDataListCreator = MockRepository.GenerateMock<IMatrixDataListCreator>();

			_groupPersonBuilderForOptimization = MockRepository.GenerateMock<IGroupPersonBuilderForOptimization>();

			_schedulingResultStateHolder = MockRepository.GenerateMock<ISchedulingResultStateHolder>();
			_schedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			_schedulingOptions.DayOffTemplate = new DayOffTemplate(new Description("DayOff"));
			_schedulingOptions.GroupOnGroupPageForTeamBlockPer = new GroupPageLight
				{
					Key = "Root",
					Name = "BU"
				};
			_scheduleDayPro = MockRepository.GenerateMock<IScheduleDayPro>();
			_scheduleMatrixPro = MockRepository.GenerateMock<IScheduleMatrixPro>();
			_schedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			_target = new TeamDayOffScheduler(_dayOffsInPeriodCalculator, _effectiveRestrictionCreator,
											  _schedulePartModifyAndRollbackService, _scheduleDayAvailableForDayOffSpecification,
											  _hasContractDayOffDefinition, _matrixDataListInSteadyState, _matrixDataListCreator,
											  _schedulingResultStateHolder);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"),
		 Test]
		public void ShouldAddPreferenceDayOffForTeam()
		{
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro, MockRepository.GenerateMock<IScheduleMatrixPro>() };
			var person = PersonFactory.CreatePerson("Bill");
			var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {_scheduleDayPro});
			var date = new DateOnly(2013, 2, 1);
			var matrixData1 = MockRepository.GenerateMock<IMatrixData>();
			var matrixDataList = new List<IMatrixData> {matrixData1};
			var groupPerson = MockRepository.GenerateMock<IGroupPerson>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
			                                                    new EndTimeLimitation(),
			                                                    new WorkTimeLimitation()
			                                                    , null, null, null,
			                                                    new List<IActivityRestriction>())
				{
					DayOffTemplate = _schedulingOptions.DayOffTemplate
				};

			_scheduleMatrixPro.Stub(x => x.Person).Return(person);
			_scheduleDayPro.Stub(x => x.Day).Return(date);
			_scheduleMatrixPro.Stub(x => x.UnlockedDays).Return(scheduleDayProList);
			_matrixDataListCreator.Stub(x => x.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
			_matrixDataListInSteadyState.Stub(x => x.IsListInSteadyState(matrixDataList)).Return(true);
			matrixData1.Stub(x => x.Matrix).Return(_scheduleMatrixPro);
			_groupPersonBuilderForOptimization.Stub(x => x.BuildGroupPerson(person, date)).Return(groupPerson);
			groupPerson.Stub(x => x.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> {person}));
			_scheduleDayPro.Stub(x => x.DaySchedulePart()).Return(scheduleDay);
			_scheduleMatrixPro.Stub(x => x.SchedulePeriod).Return(_schedulePeriod);
			_effectiveRestrictionCreator.Stub(
				x => x.GetEffectiveRestriction(new List<IPerson> {person}, date, _schedulingOptions, null))
			                            .Return(effectiveRestriction);
			_scheduleMatrixPro.Stub(x => x.GetScheduleDayByKey(date)).Return(_scheduleDayPro);

			_target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions,
			                         _groupPersonBuilderForOptimization);

			scheduleDay.AssertWasCalled(x => x.CreateAndAddDayOff(effectiveRestriction.DayOffTemplate), o => o.Repeat.Once());
		}

		/*
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldAddContractDayOffForTeam()
		{
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var person = PersonFactory.CreatePerson("Bill");
			var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
			var date = new DateOnly(2013, 2, 1);
			var matrixData1 = MockRepository.GenerateMock<IMatrixData>();
			var matrixDataList = new List<IMatrixData> { matrixData1 };
			var groupPerson = MockRepository.GenerateMock<IGroupPerson>();
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
			                                                    new EndTimeLimitation(),
			                                                    new WorkTimeLimitation()
			                                                    , null, null, null,
			                                                    new List<IActivityRestriction>());

			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro.Person).Return(person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro.Day).Return(date).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_matrixDataListCreator.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
				Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(matrixDataList)).Return(true);
				Expect.Call(matrixData1.Matrix).Return(_scheduleMatrixPro).Repeat.AtLeastOnce();
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(person, date)).Return(groupPerson).Repeat.Twice();
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.Twice();
				Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { person })).Repeat.Twice();
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { person }, date, _schedulingOptions, scheduleDictionary)).Return(effectiveRestriction).Repeat.Twice();
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(date)).Return(_scheduleDayPro).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(scheduleDay).Repeat.AtLeastOnce();
				Expect.Call(() => scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate)).Repeat.AtLeastOnce();
				Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(scheduleDay)).Repeat.AtLeastOnce();
				int target;
			    IList<IScheduleDay> currentScheduleDayList;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target, out currentScheduleDayList)).Return(true)
					  .OutRef(1, new List<IScheduleDay>());
				Expect.Call(_scheduleDayAvailableForDayOffSpecification.IsSatisfiedBy(scheduleDay)).Return(true);
				Expect.Call(_hasContractDayOffDefinition.IsDayOff(scheduleDay)).Return(true);
			}

			using (_mocks.Playback())
			{
				_target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization);
			}
		}

		

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldAddPreferenceDayOffIndividually()
		{
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
			var matrixData1 = MockRepository.GenerateMock<IMatrixData>();
			var matrixDataList = new List<IMatrixData> { matrixData1 };
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																new EndTimeLimitation(),
																new WorkTimeLimitation()
																, null, null, null,
																new List<IActivityRestriction>())
			{
				DayOffTemplate = _schedulingOptions.DayOffTemplate
			};

			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.IsValid).Return(false).Repeat.AtLeastOnce();
				Expect.Call(_matrixDataListCreator.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
				Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(matrixDataList)).Return(false);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay, _schedulingOptions)).Return(effectiveRestriction);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(scheduleDay).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay.IsScheduled()).Return(false).Repeat.AtLeastOnce();
				Expect.Call(() => scheduleDay.CreateAndAddDayOff(effectiveRestriction.DayOffTemplate)).Repeat.AtLeastOnce();
				Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(scheduleDay)).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				_target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization);
			}
		}
		
		

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldAddContractDayOffIndividually()
		{
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
			var matrixData1 = MockRepository.GenerateMock<IMatrixData>();
			var matrixDataList = new List<IMatrixData> { matrixData1 };
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
			                                                    new EndTimeLimitation(),
			                                                    new WorkTimeLimitation()
			                                                    , null, null, null,
			                                                    new List<IActivityRestriction>());

			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_matrixDataListCreator.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
				Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(matrixDataList)).Return(false);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay, _schedulingOptions)).Return(effectiveRestriction).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(scheduleDay).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay.IsScheduled()).Return(false).Repeat.AtLeastOnce();
				Expect.Call(() => scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate)).Repeat.AtLeastOnce();
				Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(scheduleDay)).Repeat.AtLeastOnce();
				int target;
                IList<IScheduleDay> currentScheduleDayList;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target, out currentScheduleDayList)).Return(true)
					  .OutRef(1, new List<IScheduleDay>());
				Expect.Call(_scheduleDayAvailableForDayOffSpecification.IsSatisfiedBy(scheduleDay)).Return(true);
				Expect.Call(_hasContractDayOffDefinition.IsDayOff(scheduleDay)).Return(true);
			}

			using (_mocks.Playback())
			{
				_target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization);
			}
		}
	
		

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldStopAddingPreferenceDayOffIfCanceled()
		{
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var person = PersonFactory.CreatePerson("Bill");
			var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
			var date = new DateOnly(2013, 2, 1);
			var matrixData1 = MockRepository.GenerateMock<IMatrixData>();
			var matrixDataList = new List<IMatrixData> { matrixData1 };
			var groupPerson = MockRepository.GenerateMock<IGroupPerson>();
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																new EndTimeLimitation(),
																new WorkTimeLimitation()
																, null, null, null,
																new List<IActivityRestriction>())
			{
				DayOffTemplate = _schedulingOptions.DayOffTemplate
			};

			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro.Person).Return(person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro.Day).Return(date).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_matrixDataListCreator.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
				Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(matrixDataList)).Return(true);
				Expect.Call(matrixData1.Matrix).Return(_scheduleMatrixPro).Repeat.AtLeastOnce();
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(person, date)).Return(groupPerson).Repeat.Twice();
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.Twice();
				Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { person })).Repeat.Twice();
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { person }, date, _schedulingOptions, scheduleDictionary)).Return(effectiveRestriction).Repeat.Twice();
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(date)).Return(_scheduleDayPro).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(scheduleDay).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay.IsScheduled()).Return(false).Repeat.AtLeastOnce();
				Expect.Call(() => scheduleDay.CreateAndAddDayOff(effectiveRestriction.DayOffTemplate)).Repeat.AtLeastOnce();
				Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(scheduleDay)).Repeat.AtLeastOnce();
				int target;
                IList<IScheduleDay> currentScheduleDayList;
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target, out currentScheduleDayList )).Return(true)
					  .OutRef(1, new List<IScheduleDay>());
				Expect.Call(_scheduleDayAvailableForDayOffSpecification.IsSatisfiedBy(scheduleDay)).Return(true);
				Expect.Call(_hasContractDayOffDefinition.IsDayOff(scheduleDay)).Return(true);
			}

			using (_mocks.Playback())
			{
				_target.DayScheduled += targetDayScheduled;
				_target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization);
				_target.DayScheduled -= targetDayScheduled;
			}
		}

		void targetDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			e.Cancel = true;
		}

        [Test]
        public void DoNotContinueIfMatrixListIsNull()
        {
            _target.DayOffScheduling(null, _schedulePartModifyAndRollbackService, _schedulingOptions, null);
        }

        [Test]
        public void DoNotContinueIfGroupPersonBuilderForOptimizationIsNull()
        {
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            _target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService,_schedulingOptions , null);
        }

        [Test]
        public void ShouldContinueIfCurrentScheduleDayHasValue()
        {
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
            var matrixData1 = MockRepository.GenerateMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
            
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_matrixDataListCreator.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
                Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(matrixDataList)).Return(false);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay.IsScheduled()).Return(true ).Repeat.AtLeastOnce();

                int target;
                IList<IScheduleDay> currentScheduleDayList;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target, out currentScheduleDayList)).Return(true )
                      .OutRef(1, new List<IScheduleDay>{ scheduleDay });
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization);
            }
        }

        [Test]
        public void ShouldContinueIfTargetIsEqualToCurrentDayOff()
        {
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
            var matrixData1 = MockRepository.GenerateMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
           

            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_matrixDataListCreator.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
                Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(matrixDataList)).Return(false);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay.IsScheduled()).Return(true).Repeat.AtLeastOnce();
                int target;
                IList<IScheduleDay> currentScheduleDayList;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target, out currentScheduleDayList)).Return(true)
                      .OutRef(0, new List<IScheduleDay> ());
                
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization);
            }
        }

        [Test]
        public void ShouldContinueIfScheduleDayAvailableForDayOffSpecificationNotSatisfied()
        {
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
            var matrixData1 = MockRepository.GenerateMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
            

            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_matrixDataListCreator.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
                Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(matrixDataList)).Return(false);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay.IsScheduled()).Return(true).Repeat.AtLeastOnce();
                int target;
                IList<IScheduleDay> currentScheduleDayList;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target, out currentScheduleDayList)).Return(true)
                      .OutRef(1, new List<IScheduleDay>());
                Expect.Call(_scheduleDayAvailableForDayOffSpecification.IsSatisfiedBy(scheduleDay)).Return(false);
                
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization);
            }
        }

        [Test]
        public void ShouldContinueIfNotContractDayOff()
        {
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
            var matrixData1 = MockRepository.GenerateMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
           

            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_matrixDataListCreator.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
                Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(matrixDataList)).Return(false);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay.IsScheduled()).Return(true).Repeat.AtLeastOnce();

                int target;
                IList<IScheduleDay> currentScheduleDayList;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target, out currentScheduleDayList)).Return(true)
                      .OutRef(1, new List<IScheduleDay>());
                Expect.Call(_scheduleDayAvailableForDayOffSpecification.IsSatisfiedBy(scheduleDay)).Return(true  );
                Expect.Call(_hasContractDayOffDefinition.IsDayOff(scheduleDay)).Return(false );
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization);
            }
        }

        [Test]
        public void ShouldContinueIfEffectiveRestrictionNotAllowDayOff()
        {
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
            var matrixData1 = MockRepository.GenerateMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
            var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
                                                                new EndTimeLimitation(),
                                                                new WorkTimeLimitation()
                                                                , null, null, null,
                                                                new List<IActivityRestriction>());

            effectiveRestriction.NotAllowedForDayOffs = true;

            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_matrixDataListCreator.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
                Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(matrixDataList)).Return(false);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay, _schedulingOptions)).Return(effectiveRestriction).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay.IsScheduled()).Return(true).Repeat.AtLeastOnce();

                int target;
                IList<IScheduleDay> currentScheduleDayList;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target, out currentScheduleDayList)).Return(true)
                      .OutRef(1, new List<IScheduleDay>());
                Expect.Call(_scheduleDayAvailableForDayOffSpecification.IsSatisfiedBy(scheduleDay)).Return(true);
                Expect.Call(_hasContractDayOffDefinition.IsDayOff(scheduleDay)).Return(true);
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization);
            }
        }

        [Test]
        public void ShouldThrowExceptionWhenRollbackIsNull()
        {
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
            var matrixData1 = MockRepository.GenerateMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
           
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_matrixDataListCreator.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
                Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(matrixDataList)).Return(false);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay.IsScheduled()).Return(true ).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                Assert.Throws<ArgumentNullException>(() => _target.DayOffScheduling(matrixList, null, _schedulingOptions, _groupPersonBuilderForOptimization)); 
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldThrowExceptionForTeamWhenRollbackIsNull()
        {
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var person = PersonFactory.CreatePerson("Bill");
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
            var date = new DateOnly(2013, 2, 1);
            var matrixData1 = MockRepository.GenerateMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var groupPerson = MockRepository.GenerateMock<IGroupPerson>();
            var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
            var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
                                                                new EndTimeLimitation(),
                                                                new WorkTimeLimitation()
                                                                , null, null, null,
                                                                new List<IActivityRestriction>());

            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.Day).Return(date).Repeat.AtLeastOnce();
                Expect.Call(_matrixDataListCreator.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
                Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(matrixDataList)).Return(true);
                Expect.Call(matrixData1.Matrix).Return(_scheduleMatrixPro).Repeat.AtLeastOnce();
                Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(person, date)).Return(groupPerson).Repeat.Twice();
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.Twice();
                Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { person })).Repeat.Twice();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { person }, date, _schedulingOptions, scheduleDictionary)).Return(effectiveRestriction).Repeat.Twice();
                
            }

            using (_mocks.Playback())
            {
                Assert.Throws<ArgumentNullException>(() => _target.DayOffScheduling(matrixList, null, _schedulingOptions, _groupPersonBuilderForOptimization));
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldReturnForTeamWhenNotAllowedForDayOffsIsTrue()
        {
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var person = PersonFactory.CreatePerson("Bill");
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
            var date = new DateOnly(2013, 2, 1);
            var matrixData1 = MockRepository.GenerateMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var groupPerson = MockRepository.GenerateMock<IGroupPerson>();
            var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
            var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
                                                                new EndTimeLimitation(),
                                                                new WorkTimeLimitation()
                                                                , null, null, null,
                                                                new List<IActivityRestriction>());
            effectiveRestriction.NotAllowedForDayOffs = true;

            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.Day).Return(date).Repeat.AtLeastOnce();
                Expect.Call(_matrixDataListCreator.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
                Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(matrixDataList)).Return(true);
                Expect.Call(matrixData1.Matrix).Return(_scheduleMatrixPro).Repeat.AtLeastOnce();
                Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(person, date)).Return(groupPerson).Repeat.Twice();
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.Twice();
                Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { person })).Repeat.Twice();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { person }, date, _schedulingOptions, scheduleDictionary)).Return(effectiveRestriction).Repeat.Twice();
                
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldContinueForTeamWhenCurrentDayOffIsPopulated()
        {
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var person = PersonFactory.CreatePerson("Bill");
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
            var date = new DateOnly(2013, 2, 1);
            var matrixData1 = MockRepository.GenerateMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var groupPerson = MockRepository.GenerateMock<IGroupPerson>();
            var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
            var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
            var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
                                                                new EndTimeLimitation(),
                                                                new WorkTimeLimitation()
                                                                , null, null, null,
                                                                new List<IActivityRestriction>());


            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.Day).Return(date).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_matrixDataListCreator.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
                Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(matrixDataList)).Return(true);
                Expect.Call(matrixData1.Matrix).Return(_scheduleMatrixPro).Repeat.AtLeastOnce();
                Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(person, date)).Return(groupPerson).Repeat.Twice();
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.Twice();
                Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { person })).Repeat.Twice();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { person }, date, _schedulingOptions, scheduleDictionary)).Return(effectiveRestriction).Repeat.Twice();
                int target;
                IList<IScheduleDay> currentScheduleDayList;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target, out currentScheduleDayList)).Return(true)
                      .OutRef(1, new List<IScheduleDay>{scheduleDay });
                
            }

            using (_mocks.Playback())
            {
                 _target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldContinueForTeamWhenCurrentEqualsTarget()
        {
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var person = PersonFactory.CreatePerson("Bill");
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
            var date = new DateOnly(2013, 2, 1);
            var matrixData1 = MockRepository.GenerateMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var groupPerson = MockRepository.GenerateMock<IGroupPerson>();
            var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
            var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
                                                                new EndTimeLimitation(),
                                                                new WorkTimeLimitation()
                                                                , null, null, null,
                                                                new List<IActivityRestriction>());


            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.Day).Return(date).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_matrixDataListCreator.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
                Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(matrixDataList)).Return(true);
                Expect.Call(matrixData1.Matrix).Return(_scheduleMatrixPro).Repeat.AtLeastOnce();
                Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(person, date)).Return(groupPerson).Repeat.Twice();
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.Twice();
                Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { person })).Repeat.Twice();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { person }, date, _schedulingOptions, scheduleDictionary)).Return(effectiveRestriction).Repeat.Twice();
                int target;
                IList<IScheduleDay> currentScheduleDayList;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target, out currentScheduleDayList)).Return(true)
                      .OutRef(0, new List<IScheduleDay> ());
               
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldContinueForTeamIfScheduleDayAvailableForDayOffSpecificationNotSatisfied()
        {
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var person = PersonFactory.CreatePerson("Bill");
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
            var date = new DateOnly(2013, 2, 1);
            var matrixData1 = MockRepository.GenerateMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var groupPerson = MockRepository.GenerateMock<IGroupPerson>();
            var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
            var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
            var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
                                                                new EndTimeLimitation(),
                                                                new WorkTimeLimitation()
                                                                , null, null, null,
                                                                new List<IActivityRestriction>());


            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.Day).Return(date).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_matrixDataListCreator.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
                Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(matrixDataList)).Return(true);
                Expect.Call(matrixData1.Matrix).Return(_scheduleMatrixPro).Repeat.AtLeastOnce();
                Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(person, date)).Return(groupPerson).Repeat.Twice();
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.Twice();
                Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { person })).Repeat.Twice();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { person }, date, _schedulingOptions, scheduleDictionary)).Return(effectiveRestriction).Repeat.Twice();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(date)).Return(_scheduleDayPro).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(scheduleDay).Repeat.AtLeastOnce();
                int target;
                IList<IScheduleDay> currentScheduleDayList;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target, out currentScheduleDayList)).Return(true)
                      .OutRef(1, new List<IScheduleDay>());
                Expect.Call(_scheduleDayAvailableForDayOffSpecification.IsSatisfiedBy(scheduleDay)).Return(false );
                
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldContinueForTeamDayOffIsFromContract()
        {
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var person = PersonFactory.CreatePerson("Bill");
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
            var date = new DateOnly(2013, 2, 1);
            var matrixData1 = MockRepository.GenerateMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var groupPerson = MockRepository.GenerateMock<IGroupPerson>();
            var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
            var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
            var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
                                                                new EndTimeLimitation(),
                                                                new WorkTimeLimitation()
                                                                , null, null, null,
                                                                new List<IActivityRestriction>());


            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.Day).Return(date).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_matrixDataListCreator.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
                Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(matrixDataList)).Return(true);
                Expect.Call(matrixData1.Matrix).Return(_scheduleMatrixPro).Repeat.AtLeastOnce();
                Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(person, date)).Return(groupPerson).Repeat.Twice();
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.Twice();
                Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { person })).Repeat.Twice();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { person }, date, _schedulingOptions, scheduleDictionary)).Return(effectiveRestriction).Repeat.Twice();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(date)).Return(_scheduleDayPro).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(scheduleDay).Repeat.AtLeastOnce();
                int target;
                IList<IScheduleDay> currentScheduleDayList;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target, out currentScheduleDayList)).Return(true)
                      .OutRef(1, new List<IScheduleDay>());
                Expect.Call(_scheduleDayAvailableForDayOffSpecification.IsSatisfiedBy(scheduleDay)).Return(true);
                Expect.Call(_hasContractDayOffDefinition.IsDayOff(scheduleDay)).Return(false );
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization);
            }
        }
		 * */
	}
}
