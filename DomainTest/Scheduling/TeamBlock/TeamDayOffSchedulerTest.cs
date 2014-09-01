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
		private IHasContractDayOffDefinition _hasContractDayOffDefinition;
		private IMatrixDataListCreator _matrixDataListCreator;
		private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private SchedulingOptions _schedulingOptions;
		private IVirtualSchedulePeriod _schedulePeriod;
		private IScheduleDayPro _scheduleDayPro;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IList<IPerson> _selectedPersons;
		private IPerson _person1;
		private IMatrixData _matrixData1;
		private IList<IMatrixData> _matrixDataList;
		private IList<IScheduleMatrixPro> _matrixList;
		private List<IScheduleDayPro> _scheduleDayProList;
		private Group _group;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleDay _scheduleDay;
		private EffectiveRestriction _effectiveRestriction;
		private IScheduleDayAvailableForDayOffSpecification _scheduleDayAvailableForDayOffSpecification;
		private IPersonAssignment _personAssignment;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
		private IPrincipalAuthorization _principalAuthorization;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulingOptions = new SchedulingOptions();
			_dayOffsInPeriodCalculator = _mocks.StrictMock<IDayOffsInPeriodCalculator>();
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_schedulePartModifyAndRollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_hasContractDayOffDefinition = _mocks.StrictMock<IHasContractDayOffDefinition>();
			_matrixDataListCreator = _mocks.StrictMock<IMatrixDataListCreator>();
			_groupPersonBuilderForOptimization = _mocks.StrictMock<IGroupPersonBuilderForOptimization>();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_scheduleDayAvailableForDayOffSpecification = _mocks.StrictMock<IScheduleDayAvailableForDayOffSpecification>();
			_target = new TeamDayOffScheduler(_dayOffsInPeriodCalculator, _effectiveRestrictionCreator,
			                                  _hasContractDayOffDefinition, _matrixDataListCreator,
			                                  _schedulingResultStateHolder, _scheduleDayAvailableForDayOffSpecification);
			_person1 = PersonFactory.CreatePerson();
			_selectedPersons = new List<IPerson>{_person1};
			_matrixData1 = _mocks.StrictMock<IMatrixData>();
			_matrixDataList = new List<IMatrixData> { _matrixData1 };
			_matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			_scheduleDayProList = new List<IScheduleDayPro> {_scheduleDayPro};
			_group = new Group(_selectedPersons, "");
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
			                                                 new EndTimeLimitation(),
			                                                 new WorkTimeLimitation()
			                                                 , null, null, null,
			                                                 new List<IActivityRestriction>());

			_personAssignment = _mocks.StrictMock<IPersonAssignment>();
			_dateOnlyAsDateTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
			_principalAuthorization = _mocks.StrictMock<IPrincipalAuthorization>();
		}

		[Test]
		public void ShouldAddRestrictionDayOffForTeamMember()
		{
			_effectiveRestriction.DayOffTemplate = new DayOffTemplate(new Description("DayOff"));

			using (_mocks.Record())
			{
				commonMocks();

				//first foreach
				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2013, 2, 1));
				getMatrixesAndRestrictionMocks();
				addDaysOffForTeamMocks();

				//second foreach
				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2013, 2, 1));
				getMatrixesAndRestrictionMocks();

				//addContractDaysOff
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.IsValid).Return(true);
				int target;
				IList<IScheduleDay> currentScheduleDayList;
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target,
																			 out currentScheduleDayList)).Return(true)
				  .OutRef(1, new List<IScheduleDay>{_scheduleDay});
				
			}

			using (_mocks.Playback())
			{
				_target.DayOffScheduling(_matrixList, _selectedPersons, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization);
			}
		}

		[Test]
		public void ShouldNotAddMoreContractDaysOffIfMemberHasTarget()
		{
			_effectiveRestriction.DayOffTemplate = new DayOffTemplate(new Description("DayOff"));

			using (_mocks.Record())
			{
				commonMocks();

				//first foreach
				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2013, 2, 1));
				getMatrixesAndRestrictionMocks();
				addDaysOffForTeamMocks();

				//second foreach
				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2013, 2, 1));
				getMatrixesAndRestrictionMocks();

				//addContractDaysOff
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.IsValid).Return(true);
				int target;
				IList<IScheduleDay> currentScheduleDayList;
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target,
																			 out currentScheduleDayList)).Return(true)
				  .OutRef(1, new List<IScheduleDay> { _scheduleDay });

			}

			using (_mocks.Playback())
			{
				_target.DayOffScheduling(_matrixList, _selectedPersons, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization);
			}
		}

		[Test]
		public void ShouldAddContractDayOffForTeamMemberIfActualIsBelowTarget()
		{
			using (_mocks.Record())
			{
				commonMocks();

				//first foreach
				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2013, 2, 1));
				getMatrixesAndRestrictionMocks();

				//second foreach
				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2013, 2, 1));
				getMatrixesAndRestrictionMocks();

				//addContractDaysOff
				addContractDaysOffMocksBelowTarget(false, true);
			}

			using (_mocks.Playback())
			{
				using (new CustomAuthorizationContext(_principalAuthorization))
				{
					_target.DayOffScheduling(_matrixList, _selectedPersons, _schedulePartModifyAndRollbackService, _schedulingOptions,
					                         _groupPersonBuilderForOptimization);
				}
			}
		}


		[Test]
		public void ShouldSkipModifyWhenNoPermission()
		{
			using (_mocks.Record())
			{
				commonMocks();

				//first foreach
				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2013, 2, 1));
				getMatrixesAndRestrictionMocks();

				//second foreach
				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2013, 2, 1));
				getMatrixesAndRestrictionMocks();

				//addContractDaysOff
				addContractDaysOffMocksBelowTarget(false, false);
			}

			using (_mocks.Playback())
			{
				using (new CustomAuthorizationContext(_principalAuthorization))
				{
					_target.DayOffScheduling(_matrixList, _selectedPersons, _schedulePartModifyAndRollbackService, _schedulingOptions,
											 _groupPersonBuilderForOptimization);
				}
			}
		}
		
		[Test]
		public void ShouldCancelAddRestrictionDayOffForTeamMember()
		{
			_effectiveRestriction.DayOffTemplate = new DayOffTemplate(new Description("DayOff"));

			using (_mocks.Record())
			{
				commonMocks();

				//first foreach
				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2013, 2, 1));
				getMatrixesAndRestrictionMocks();
				addDaysOffForTeamMocks();
			}

			using (_mocks.Playback())
			{
				_target.DayScheduled += targetDayScheduled;
				_target.DayOffScheduling(_matrixList, _selectedPersons, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization);
				_target.DayScheduled -= targetDayScheduled;
			}
		}

		[Test]
		public void ShouldCancelAddContractDayOffForTeamMember()
		{
			using (_mocks.Record())
			{
				commonMocks();

				//first foreach
				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2013, 2, 1));
				getMatrixesAndRestrictionMocks();

				//second foreach
				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2013, 2, 1));
				getMatrixesAndRestrictionMocks();

				//addContractDaysOff
				addContractDaysOffMocksBelowTarget(true, true);
			}

			using (_mocks.Playback())
			{
				using (new CustomAuthorizationContext(_principalAuthorization))
				{
					_target.DayScheduled += targetDayScheduled;
					_target.DayOffScheduling(_matrixList, _selectedPersons, _schedulePartModifyAndRollbackService, _schedulingOptions,
					                         _groupPersonBuilderForOptimization);
					_target.DayScheduled -= targetDayScheduled;
				}
			}
		}

		[Test]
		public void ShouldNotAddRestrictionDayOffForTeamMemberIfInConflictWithSchedulingOptions()
		{
			_effectiveRestriction.DayOffTemplate = new DayOffTemplate(new Description("DayOff"));
			_schedulingOptions.PreferencesDaysOnly = true;

			using (_mocks.Record())
			{
				commonMocks();

				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2013, 2, 1));
				getMatrixesAndRestrictionMocks();
				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2013, 2, 1));
				getMatrixesAndRestrictionMocks();
				addContractDaysOffMocksBelowTarget(false, true);
			}

			using (_mocks.Playback())
			{
				using (new CustomAuthorizationContext(_principalAuthorization))
				{
					_target.DayOffScheduling(_matrixList, _selectedPersons, _schedulePartModifyAndRollbackService, _schedulingOptions,
					                         _groupPersonBuilderForOptimization);
				}
			}
		}

		private void addContractDaysOffMocksBelowTarget(bool cancel, bool havePermission)
		{
			var dayOffOnPeriod = _mocks.StrictMock<IDayOffOnPeriod>();
			var dayOffPeriods = new List<IDayOffOnPeriod> { dayOffOnPeriod };
			var dateOnly = new DateOnly(2013, 2, 1);
			int target;
			IList<IScheduleDay> currentScheduleDayList;
			Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod);
			Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out target,
																			 out currentScheduleDayList)).Return(false)
				  .OutRef(1, new List<IScheduleDay>()).Repeat.AtLeastOnce();

			
			Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
			Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
			Expect.Call(_dayOffsInPeriodCalculator.WeekPeriodsSortedOnDayOff(_scheduleMatrixPro)).Return(dayOffPeriods).Repeat.AtLeastOnce();
			Expect.Call(dayOffOnPeriod.FindBestSpotForDayOff(_hasContractDayOffDefinition, _scheduleDayAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions)).Return(_scheduleDay);
			if(!cancel && havePermission)
				Expect.Call(dayOffOnPeriod.FindBestSpotForDayOff(_hasContractDayOffDefinition, _scheduleDayAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions)).Return(null);
			Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
			Expect.Call(_personAssignment.FunctionPath).Return("functionPath");
			Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
			Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(dateOnly);
			Expect.Call(_scheduleDay.Person).Return(_person1);
			Expect.Call(_principalAuthorization.IsPermitted("functionPath", dateOnly, _person1)).Return(havePermission);
			Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));

			if (havePermission)
				Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay));
			
		}

		private void addDaysOffForTeamMocks()
		{
			Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(new DateOnly(2013, 2, 1))).Return(_scheduleDayPro);
			Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
			Expect.Call(_scheduleDay.IsScheduled()).Return(false);
			Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_effectiveRestriction.DayOffTemplate));
			Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay));
		}

		private void commonMocks()
		{
			Expect.Call(_matrixDataListCreator.Create(_matrixList, _schedulingOptions)).Return(_matrixDataList);
			Expect.Call(_matrixData1.Matrix).Return(_scheduleMatrixPro);
			Expect.Call(_scheduleMatrixPro.Person).Return(_person1);

			Expect.Call(_matrixData1.Matrix).Return(_scheduleMatrixPro);
			Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(_scheduleDayProList));
			Expect.Call(_scheduleMatrixPro.Person).Return(_person1);
		}

		private void getMatrixesAndRestrictionMocks()
		{
			Expect.Call(_groupPersonBuilderForOptimization.BuildGroup(_person1, new DateOnly(2013, 2, 1)))
			      .Return(_group);
			Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
			Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestrictionForSinglePerson (_person1, new DateOnly(2013, 2, 1),
			                                                                 _schedulingOptions, _scheduleDictionary))
			      .Return(_effectiveRestriction);
			Expect.Call(_scheduleMatrixPro.Person).Return(_person1);
			Expect.Call(_scheduleMatrixPro.Person).Return(_person1);

			Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod);
			Expect.Call(_schedulePeriod.IsValid).Return(true);
			Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(2013, 2, 1, 2013, 2, 1));
		}

		void targetDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			e.Cancel = true;
		}
	}
}
