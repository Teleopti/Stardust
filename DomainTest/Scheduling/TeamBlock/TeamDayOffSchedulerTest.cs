using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamDayOffSchedulerTest
	{
		private ITeamDayOffScheduler _target;
		private MockRepository _mocks;
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
		private IPersonDayOff _personDayOff;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
		private IPrincipalAuthorization _principalAuthorization;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulingOptions = new SchedulingOptions();
			_dayOffsInPeriodCalculator = _mocks.StrictMock<IDayOffsInPeriodCalculator>();

			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_schedulePartModifyAndRollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();

			_scheduleDayAvailableForDayOffSpecification = _mocks.StrictMock<IScheduleDayAvailableForDayOffSpecification>();
			_hasContractDayOffDefinition = _mocks.StrictMock<IHasContractDayOffDefinition>();

			_matrixDataListInSteadyState = _mocks.StrictMock<IMatrixDataListInSteadyState>();
			_matrixDataListCreator = _mocks.StrictMock<IMatrixDataListCreator>();

			_groupPersonBuilderForOptimization = _mocks.StrictMock<IGroupPersonBuilderForOptimization>();

			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_schedulingOptions.DayOffTemplate = new DayOffTemplate(new Description("DayOff"));
			_schedulingOptions.GroupOnGroupPageForTeamBlockPer = new GroupPageLight
				{
					Key = "Root",
					Name = "BU"
				};
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_target = new TeamDayOffScheduler(_dayOffsInPeriodCalculator, _effectiveRestrictionCreator,
											  _schedulePartModifyAndRollbackService, _scheduleDayAvailableForDayOffSpecification,
											  _hasContractDayOffDefinition, _matrixDataListInSteadyState, _matrixDataListCreator,
											  _schedulingResultStateHolder);

			_personDayOff = _mocks.StrictMock<IPersonDayOff>();
			_dateOnlyAsDateTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
			_principalAuthorization = _mocks.StrictMock<IPrincipalAuthorization>();
			_person = PersonFactory.CreatePerson("Bill");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldAddPreferenceDayOffForTeam()
		{
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var person = PersonFactory.CreatePerson("Bill");
			var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
			var date = new DateOnly(2013, 2, 1);
			var matrixData1 = _mocks.StrictMock<IMatrixData>();
			var matrixDataList = new List<IMatrixData> {matrixData1};
			var groupPerson = _mocks.StrictMock<IGroupPerson>();
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
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
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro.Day).Return(date).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.IsValid).Return(false).Repeat.AtLeastOnce();
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
			}

			using (_mocks.Playback())
			{
				_target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization);
			}
		}

		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldAddContractDayOffForTeam()
		{
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var person = PersonFactory.CreatePerson("Bill");
			var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
			var date = new DateOnly(2013, 2, 1);
			var matrixData1 = _mocks.StrictMock<IMatrixData>();
			var matrixDataList = new List<IMatrixData> { matrixData1 };
			var groupPerson = _mocks.StrictMock<IGroupPerson>();
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
			                                                    new EndTimeLimitation(),
			                                                    new WorkTimeLimitation()
			                                                    , null, null, null,
			                                                    new List<IActivityRestriction>());

			var dayOffOnPeriod = _mocks.StrictMock<IDayOffOnPeriod>();
			var dayOffPeriods = new List<IDayOffOnPeriod> { dayOffOnPeriod };

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
				
				Expect.Call(_dayOffsInPeriodCalculator.WeekPeriodsSortedOnDayOff(_scheduleMatrixPro)).Return(dayOffPeriods).Repeat.AtLeastOnce();
				Expect.Call(dayOffOnPeriod.FindBestSpotForDayOff(_hasContractDayOffDefinition, _scheduleDayAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions)).Return(scheduleDay);
				Expect.Call(dayOffOnPeriod.FindBestSpotForDayOff(_hasContractDayOffDefinition, _scheduleDayAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions)).Return(null);
				Expect.Call(scheduleDay.PersonDayOffCollection()).Return(new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { _personDayOff }));
				Expect.Call(_personDayOff.FunctionPath).Return("functionPath");
				Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(date);
				Expect.Call(scheduleDay.Person).Return(person);
				Expect.Call(_principalAuthorization.IsPermitted("functionPath", date, person)).Return(true);

				Expect.Call(() => scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate)).Repeat.AtLeastOnce();
				Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(scheduleDay)).Repeat.AtLeastOnce();
				int target;
			    IList<IScheduleDay> currentScheduleDayList;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target, out currentScheduleDayList)).Return(true)
					  .OutRef(1, new List<IScheduleDay>()).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				using (new CustomAuthorizationContext(_principalAuthorization))
				{
					_target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions,
					                         _groupPersonBuilderForOptimization);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldSkipModifyContractDayOffForTeamWhenNoPermission()
		{
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var person = PersonFactory.CreatePerson("Bill");
			var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
			var date = new DateOnly(2013, 2, 1);
			var matrixData1 = _mocks.StrictMock<IMatrixData>();
			var matrixDataList = new List<IMatrixData> { matrixData1 };
			var groupPerson = _mocks.StrictMock<IGroupPerson>();
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																new EndTimeLimitation(),
																new WorkTimeLimitation()
																, null, null, null,
																new List<IActivityRestriction>());

			var dayOffOnPeriod = _mocks.StrictMock<IDayOffOnPeriod>();
			var dayOffPeriods = new List<IDayOffOnPeriod> { dayOffOnPeriod };

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

				Expect.Call(_dayOffsInPeriodCalculator.WeekPeriodsSortedOnDayOff(_scheduleMatrixPro)).Return(dayOffPeriods).Repeat.AtLeastOnce();
				Expect.Call(dayOffOnPeriod.FindBestSpotForDayOff(_hasContractDayOffDefinition, _scheduleDayAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions)).Return(scheduleDay);
				Expect.Call(scheduleDay.PersonDayOffCollection()).Return(new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { _personDayOff }));
				Expect.Call(_personDayOff.FunctionPath).Return("functionPath");
				Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(date);
				Expect.Call(scheduleDay.Person).Return(person);
				Expect.Call(_principalAuthorization.IsPermitted("functionPath", date, person)).Return(false);


				Expect.Call(() => scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate)).Repeat.AtLeastOnce();
				int target;
				IList<IScheduleDay> currentScheduleDayList;
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target, out currentScheduleDayList)).Return(true)
					  .OutRef(1, new List<IScheduleDay>()).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				using (new CustomAuthorizationContext(_principalAuthorization))
				{
					_target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions,
											 _groupPersonBuilderForOptimization);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldAddPreferenceDayOffIndividually()
		{
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
			var matrixData1 = _mocks.StrictMock<IMatrixData>();
			var matrixDataList = new List<IMatrixData> { matrixData1 };
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
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
			var matrixData1 = _mocks.StrictMock<IMatrixData>();
			var matrixDataList = new List<IMatrixData> { matrixData1 };
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
			                                                    new EndTimeLimitation(),
			                                                    new WorkTimeLimitation()
			                                                    , null, null, null,
			                                                    new List<IActivityRestriction>());

			var dayOffOnPeriod = _mocks.StrictMock<IDayOffOnPeriod>();
			var dayOffPeriods = new List<IDayOffOnPeriod> { dayOffOnPeriod };
			var date = new DateOnly(2013, 2, 1);

			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_matrixDataListCreator.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
				Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(matrixDataList)).Return(false);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay, _schedulingOptions)).Return(effectiveRestriction).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(scheduleDay).Repeat.AtLeastOnce();


				Expect.Call(_dayOffsInPeriodCalculator.WeekPeriodsSortedOnDayOff(_scheduleMatrixPro)).Return(dayOffPeriods).Repeat.AtLeastOnce();
				Expect.Call(dayOffOnPeriod.FindBestSpotForDayOff(_hasContractDayOffDefinition, _scheduleDayAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions)).Return(scheduleDay);
				Expect.Call(dayOffOnPeriod.FindBestSpotForDayOff(_hasContractDayOffDefinition, _scheduleDayAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions)).Return(null);
				Expect.Call(scheduleDay.PersonDayOffCollection()).Return(new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { _personDayOff }));
				Expect.Call(_personDayOff.FunctionPath).Return("functionPath");
				Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(date);
				Expect.Call(scheduleDay.Person).Return(_person);
				Expect.Call(_principalAuthorization.IsPermitted("functionPath", date, _person)).Return(true);

				Expect.Call(scheduleDay.IsScheduled()).Return(false).Repeat.AtLeastOnce();
				Expect.Call(() => scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate)).Repeat.AtLeastOnce();
				Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(scheduleDay)).Repeat.AtLeastOnce();
				int target;
                IList<IScheduleDay> currentScheduleDayList;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target, out currentScheduleDayList)).Return(true)
					  .OutRef(1, new List<IScheduleDay>()).Repeat.AtLeastOnce(); 
			}

			using (_mocks.Playback())
			{
				using (new CustomAuthorizationContext(_principalAuthorization))
				{
					_target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions,
					                         _groupPersonBuilderForOptimization);
				}
			}
		}
	
		

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldStopAddingPreferenceDayOffIfCanceled()
		{
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var person = PersonFactory.CreatePerson("Bill");
			var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
			var date = new DateOnly(2013, 2, 1);
			var matrixData1 = _mocks.StrictMock<IMatrixData>();
			var matrixDataList = new List<IMatrixData> { matrixData1 };
			var groupPerson = _mocks.StrictMock<IGroupPerson>();
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																new EndTimeLimitation(),
																new WorkTimeLimitation()
																, null, null, null,
																new List<IActivityRestriction>())
			{
				DayOffTemplate = _schedulingOptions.DayOffTemplate
			};

			var dayOffOnPeriod = _mocks.StrictMock<IDayOffOnPeriod>();
			var dayOffPeriods = new List<IDayOffOnPeriod> { dayOffOnPeriod };

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

				Expect.Call(_dayOffsInPeriodCalculator.WeekPeriodsSortedOnDayOff(_scheduleMatrixPro)).Return(dayOffPeriods).Repeat.AtLeastOnce();
				Expect.Call(dayOffOnPeriod.FindBestSpotForDayOff(_hasContractDayOffDefinition, _scheduleDayAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions)).Return(scheduleDay);
				Expect.Call(scheduleDay.PersonDayOffCollection()).Return(new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { _personDayOff }));
				Expect.Call(_personDayOff.FunctionPath).Return("functionPath");
				Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(date);
				Expect.Call(scheduleDay.Person).Return(person);
				Expect.Call(_principalAuthorization.IsPermitted("functionPath", date, person)).Return(true);

				Expect.Call(() => scheduleDay.CreateAndAddDayOff(effectiveRestriction.DayOffTemplate)).Repeat.AtLeastOnce();
				Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(scheduleDay)).Repeat.AtLeastOnce();
				int target;
                IList<IScheduleDay> currentScheduleDayList;
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target, out currentScheduleDayList )).Return(true)
					  .OutRef(1, new List<IScheduleDay>());
			}

			using (_mocks.Playback())
			{
				using (new CustomAuthorizationContext(_principalAuthorization))
				{
					_target.DayScheduled += targetDayScheduled;
					_target.DayOffScheduling(matrixList, _schedulePartModifyAndRollbackService, _schedulingOptions,
					                         _groupPersonBuilderForOptimization);
					_target.DayScheduled -= targetDayScheduled;
				}
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
            var matrixData1 = _mocks.StrictMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            
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
            var matrixData1 = _mocks.StrictMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
           

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
        public void ShouldContinueIfCantFindBestSpot()
        {
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
            var matrixData1 = _mocks.StrictMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var dayOffOnPeriod = _mocks.StrictMock<IDayOffOnPeriod>();
			var dayOffPeriods = new List<IDayOffOnPeriod> { dayOffOnPeriod };
			

            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_matrixDataListCreator.Create(matrixList, _schedulingOptions)).Return(matrixDataList);
                Expect.Call(_matrixDataListInSteadyState.IsListInSteadyState(matrixDataList)).Return(false);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay.IsScheduled()).Return(true).Repeat.AtLeastOnce();

				Expect.Call(_dayOffsInPeriodCalculator.WeekPeriodsSortedOnDayOff(_scheduleMatrixPro)).Return(dayOffPeriods).Repeat.AtLeastOnce();
				Expect.Call(dayOffOnPeriod.FindBestSpotForDayOff(_hasContractDayOffDefinition, _scheduleDayAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions)).Return(null);

                int target;
                IList<IScheduleDay> currentScheduleDayList;
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target, out currentScheduleDayList)).Return(false)
					 .OutRef(1, new List<IScheduleDay>());
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target, out currentScheduleDayList)).Return(true)
                      .OutRef(1, new List<IScheduleDay>());     
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
            var matrixData1 = _mocks.StrictMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
           
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
            var matrixData1 = _mocks.StrictMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var groupPerson = _mocks.StrictMock<IGroupPerson>();
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
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
            var matrixData1 = _mocks.StrictMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var groupPerson = _mocks.StrictMock<IGroupPerson>();
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
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
            var matrixData1 = _mocks.StrictMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var groupPerson = _mocks.StrictMock<IGroupPerson>();
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
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
            var matrixData1 = _mocks.StrictMock<IMatrixData>();
            var matrixDataList = new List<IMatrixData> { matrixData1 };
            var groupPerson = _mocks.StrictMock<IGroupPerson>();
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
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
	}
}
