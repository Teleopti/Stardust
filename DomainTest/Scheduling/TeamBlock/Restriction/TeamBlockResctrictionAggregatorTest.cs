using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Restriction
{
	[TestFixture]
	public class TeamBlockResctrictionAggregatorTest
	{
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IScheduleDayEquator _scheduleDayEquator;
		private TeamBlockRestrictionAggregator _target;
		private MockRepository _mocks;
		private IAssignmentPeriodRule _nightlyRestRule;
		private ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private DateOnly _dateOnly;
		private IPerson _person1;
		private IPerson _person2;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private IScheduleMatrixPro _scheduleMatrixPro2;
		private Group _group;
		private DateOnlyPeriod _blockPeriod;
		private TeamBlockInfo _teamBlockInfo;
		private SchedulingOptions _schedulingOptions;
		private IShiftProjectionCache _shift;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_scheduleDayEquator = _mocks.StrictMock<IScheduleDayEquator>();
			_nightlyRestRule = _mocks.StrictMock<IAssignmentPeriodRule>();
			_teamBlockSchedulingOptions = _mocks.StrictMock<ITeamBlockSchedulingOptions>();
			_target = new TeamBlockRestrictionAggregator(_effectiveRestrictionCreator, _schedulingResultStateHolder,
			                                             _scheduleDayEquator, _nightlyRestRule, _teamBlockSchedulingOptions);
			_dateOnly = new DateOnly(2013, 11, 12);
			_person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("bill"), _dateOnly);
			_person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("ball"), _dateOnly);
			_scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPro2 = _mocks.StrictMock<IScheduleMatrixPro>();
			_group = new Group(new List<IPerson> { _person1, _person2 }, "Hej");
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro1, _scheduleMatrixPro2 };
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			ITeamInfo teamInfo = new TeamInfo(_group, groupMatrixes);
			_blockPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
			_teamBlockInfo = new TeamBlockInfo(teamInfo, new BlockInfo(_blockPeriod));
			_schedulingOptions = new SchedulingOptions();
			_shift = _mocks.StrictMock<IShiftProjectionCache>();
		}

		[Test]
		public void ShouldAggregateExistedEffectiveRestriction()
		{
			var effectiveRestrictionForDayOne = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
													 new EndTimeLimitation(null, TimeSpan.FromHours(17)),
													 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			
			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(null, TimeSpan.FromHours(17)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var schedulePeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var schedulePeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var range1 = _mocks.DynamicMock<IScheduleRange>();
			var range2 = _mocks.DynamicMock<IScheduleRange>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var dateTimePeriod = new DateTimePeriod(new DateTime(2013, 11, 12, 0, 0, 0, DateTimeKind.Utc),
													new DateTime(2013, 11, 13, 0, 0, 0, DateTimeKind.Utc));

			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(schedulePeriod1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro1.Person).Return(_person1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.Person).Return(_person2).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.SchedulePeriod).Return(schedulePeriod2).Repeat.AtLeastOnce();
				Expect.Call(schedulePeriod1.DateOnlyPeriod).Return(_blockPeriod).Repeat.AtLeastOnce();
				Expect.Call(schedulePeriod2.DateOnlyPeriod).Return(_blockPeriod).Repeat.AtLeastOnce();

				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_group.GroupMembers, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestrictionForDayOne);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson>{_person1},_dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestrictionForDayOne);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameActivity(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameActivityInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_scheduleMatrixPro1.ActiveScheduleRange).Return(range1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.ActiveScheduleRange).Return(range2).Repeat.AtLeastOnce();
				Expect.Call(range1.ScheduledDay(_dateOnly)).Return(scheduleDay1);
				Expect.Call(range2.ScheduledDay(_dateOnly)).Return(scheduleDay2);
				Expect.Call(scheduleDay1.IsScheduled()).Return(false);
				Expect.Call(scheduleDay2.IsScheduled()).Return(false);
				Expect.Call(_nightlyRestRule.LongestDateTimePeriodForAssignment(range1, _dateOnly)).Return(dateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_nightlyRestRule.LongestDateTimePeriodForAssignment(range2, _dateOnly)).Return(dateTimePeriod).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_dateOnly, _person1, _teamBlockInfo, _schedulingOptions, null);
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void ShouldAggregateWithSameStartTimeRestriction()
		{
			var effectiveRestrictionForDayOne = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			
			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(9), TimeSpan.FromHours(9)),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var schedulePeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var schedulePeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var range1 = _mocks.DynamicMock<IScheduleRange>();
			var range2 = _mocks.DynamicMock<IScheduleRange>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDayPro1 = _mocks.DynamicMock<IScheduleDayPro>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var dateTimePeriod = new DateTimePeriod(new DateTime(2013, 11, 12, 0, 0, 0, DateTimeKind.Utc),
													new DateTime(2013, 11, 13, 0, 0, 0, DateTimeKind.Utc));
			var projectionService1 = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
			var period1 = new DateTimePeriod(new DateTime(2013, 11, 12, 9, 0, 0, DateTimeKind.Utc),
										   new DateTime(2013, 11, 12, 17, 30, 0, DateTimeKind.Utc));
			
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(schedulePeriod1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro1.Person).Return(_person1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.Person).Return(_person2).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.SchedulePeriod).Return(schedulePeriod2).Repeat.AtLeastOnce();
				Expect.Call(schedulePeriod1.DateOnlyPeriod).Return(_blockPeriod).Repeat.AtLeastOnce();
				Expect.Call(schedulePeriod2.DateOnlyPeriod).Return(_blockPeriod).Repeat.AtLeastOnce();
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_group.GroupMembers, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestrictionForDayOne);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { _person1 }, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestrictionForDayOne);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameActivity(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameActivityInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_scheduleMatrixPro1.ActiveScheduleRange).Return(range1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.ActiveScheduleRange).Return(range2).Repeat.AtLeastOnce();
				Expect.Call(range1.ScheduledDay(_dateOnly)).Return(scheduleDay1);
				Expect.Call(range2.ScheduledDay(_dateOnly)).Return(scheduleDay2);
				Expect.Call(scheduleDay1.IsScheduled()).Return(true);
				Expect.Call(scheduleDay2.IsScheduled()).Return(false);
				Expect.Call(_nightlyRestRule.LongestDateTimePeriodForAssignment(range2, _dateOnly)).Return(dateTimePeriod);
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnly)).Return(scheduleDayPro1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.GetScheduleDayByKey(_dateOnly)).Return(null).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay1.ProjectionService()).Return(projectionService1).Repeat.AtLeastOnce();
				Expect.Call(projectionService1.CreateProjection()).Return(visualLayerCollection1).Repeat.AtLeastOnce();
				Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(scheduleDay1).Repeat.AtLeastOnce();
				Expect.Call(visualLayerCollection1.Period()).Return(period1).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_dateOnly, _person1, _teamBlockInfo, _schedulingOptions, null);
				Assert.That(result.StartTimeLimitation, Is.EqualTo(expectedResult.StartTimeLimitation));
			}
		}
		
		[Test]
		public void ShouldAggregateWithSameEndTimeRestriction()
		{
			var effectiveRestrictionForDayOne = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			
			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(),
										 new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(17)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var schedulePeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var schedulePeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var range1 = _mocks.DynamicMock<IScheduleRange>();
			var range2 = _mocks.DynamicMock<IScheduleRange>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDayPro1 = _mocks.DynamicMock<IScheduleDayPro>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var dateTimePeriod = new DateTimePeriod(new DateTime(2013, 11, 12, 0, 0, 0, DateTimeKind.Utc),
													new DateTime(2013, 11, 13, 0, 0, 0, DateTimeKind.Utc));
			var projectionService1 = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
			var period1 = new DateTimePeriod(new DateTime(2013, 11, 12, 9, 0, 0, DateTimeKind.Utc),
										   new DateTime(2013, 11, 12, 17, 0, 0, DateTimeKind.Utc));
			
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(schedulePeriod1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro1.Person).Return(_person1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.Person).Return(_person2).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.SchedulePeriod).Return(schedulePeriod2).Repeat.AtLeastOnce();
				Expect.Call(schedulePeriod1.DateOnlyPeriod).Return(_blockPeriod).Repeat.AtLeastOnce();
				Expect.Call(schedulePeriod2.DateOnlyPeriod).Return(_blockPeriod).Repeat.AtLeastOnce();

				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_group.GroupMembers, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestrictionForDayOne);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { _person1 }, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestrictionForDayOne);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameActivity(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameActivityInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_scheduleMatrixPro1.ActiveScheduleRange).Return(range1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.ActiveScheduleRange).Return(range2).Repeat.AtLeastOnce();
				Expect.Call(range1.ScheduledDay(_dateOnly)).Return(scheduleDay1);
				Expect.Call(range2.ScheduledDay(_dateOnly)).Return(scheduleDay2);
				Expect.Call(scheduleDay1.IsScheduled()).Return(true);
				Expect.Call(scheduleDay2.IsScheduled()).Return(false);
				Expect.Call(_nightlyRestRule.LongestDateTimePeriodForAssignment(range2, _dateOnly)).Return(dateTimePeriod);
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnly)).Return(scheduleDayPro1);
				Expect.Call(_scheduleMatrixPro2.GetScheduleDayByKey(_dateOnly)).Return(null);
				Expect.Call(scheduleDay1.ProjectionService()).Return(projectionService1);
				Expect.Call(projectionService1.CreateProjection()).Return(visualLayerCollection1);
				Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(scheduleDay1);
				Expect.Call(visualLayerCollection1.Period()).Return(period1);

				Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
			}

			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_dateOnly, _person1, _teamBlockInfo, _schedulingOptions, null);
				Assert.That(result.EndTimeLimitation, Is.EqualTo(expectedResult.EndTimeLimitation));
			}
		}

		[Test]
		public void ShouldAggregateWithSameShiftRestriction()
		{
			var effectiveRestrictionForDayOne = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			
			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(),
										 new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(17)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var schedulePeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var schedulePeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var range1 = _mocks.DynamicMock<IScheduleRange>();
			var range2 = _mocks.DynamicMock<IScheduleRange>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDayPro1 = _mocks.DynamicMock<IScheduleDayPro>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var dateTimePeriod = new DateTimePeriod(new DateTime(2013, 11, 12, 0, 0, 0, DateTimeKind.Utc),
													new DateTime(2013, 11, 13, 0, 0, 0, DateTimeKind.Utc));
			var period1 = new DateTimePeriod(new DateTime(2013, 11, 12, 9, 0, 0, DateTimeKind.Utc),
										   new DateTime(2013, 11, 12, 17, 0, 0, DateTimeKind.Utc));
			var activity = new Activity("bo");
			var mainShift = EditableShiftFactory.CreateEditorShift(activity, period1, new ShiftCategory("cat"));
			expectedResult.CommonMainShift = mainShift;

			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(schedulePeriod1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro1.Person).Return(_person1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.Person).Return(_person2).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.SchedulePeriod).Return(schedulePeriod2).Repeat.AtLeastOnce();
				Expect.Call(schedulePeriod1.DateOnlyPeriod).Return(_blockPeriod).Repeat.AtLeastOnce();
				Expect.Call(schedulePeriod2.DateOnlyPeriod).Return(_blockPeriod).Repeat.AtLeastOnce();

				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_group.GroupMembers, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestrictionForDayOne);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { _person1 }, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestrictionForDayOne);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameActivity(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameActivityInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_scheduleMatrixPro1.ActiveScheduleRange).Return(range1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.ActiveScheduleRange).Return(range2).Repeat.AtLeastOnce();
				Expect.Call(range1.ScheduledDay(_dateOnly)).Return(scheduleDay1);
				Expect.Call(range2.ScheduledDay(_dateOnly)).Return(scheduleDay2);
				Expect.Call(scheduleDay1.IsScheduled()).Return(true);
				Expect.Call(scheduleDay2.IsScheduled()).Return(false);
				Expect.Call(_nightlyRestRule.LongestDateTimePeriodForAssignment(range2, _dateOnly)).Return(dateTimePeriod);
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnly)).Return(scheduleDayPro1);
				Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(scheduleDay1);
				Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(scheduleDay1.GetEditorShift()).Return(mainShift);
			}

			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_dateOnly, _person1, _teamBlockInfo, _schedulingOptions, null);
				Assert.That(result.CommonMainShift, Is.EqualTo(expectedResult.CommonMainShift));
			}
		}

		[Test]
		public void ShouldAggregateWithSameShiftCategoryRestriction()
		{
			var effectiveRestrictionForDayOne = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			
			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(),
										 new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(17)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var schedulePeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var schedulePeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var range1 = _mocks.DynamicMock<IScheduleRange>();
			var range2 = _mocks.DynamicMock<IScheduleRange>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDayPro1 = _mocks.DynamicMock<IScheduleDayPro>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var dateTimePeriod = new DateTimePeriod(new DateTime(2013, 11, 12, 0, 0, 0, DateTimeKind.Utc),
													new DateTime(2013, 11, 13, 0, 0, 0, DateTimeKind.Utc));
			var shiftCat = new ShiftCategory("cat");
			expectedResult.ShiftCategory = shiftCat;
			var personAssignment1 = _mocks.StrictMock<IPersonAssignment>();
			
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(schedulePeriod1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro1.Person).Return(_person1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.Person).Return(_person2).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.SchedulePeriod).Return(schedulePeriod2).Repeat.AtLeastOnce();
				Expect.Call(schedulePeriod1.DateOnlyPeriod).Return(_blockPeriod).Repeat.AtLeastOnce();
				Expect.Call(schedulePeriod2.DateOnlyPeriod).Return(_blockPeriod).Repeat.AtLeastOnce();

				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_group.GroupMembers, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestrictionForDayOne);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { _person1 }, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestrictionForDayOne);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameActivity(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameActivityInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_scheduleMatrixPro1.ActiveScheduleRange).Return(range1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.ActiveScheduleRange).Return(range2).Repeat.AtLeastOnce();
				Expect.Call(range1.ScheduledDay(_dateOnly)).Return(scheduleDay1);
				Expect.Call(range2.ScheduledDay(_dateOnly)).Return(scheduleDay2);
				Expect.Call(scheduleDay1.IsScheduled()).Return(true);
				Expect.Call(scheduleDay2.IsScheduled()).Return(false);
				Expect.Call(_nightlyRestRule.LongestDateTimePeriodForAssignment(range2, _dateOnly)).Return(dateTimePeriod);
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnly)).Return(scheduleDayPro1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.GetScheduleDayByKey(_dateOnly)).Return(null).Repeat.AtLeastOnce();
				Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(scheduleDay1).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay1.PersonAssignment()).Return(personAssignment1).Repeat.AtLeastOnce();
				Expect.Call(personAssignment1.ShiftCategory).Return(shiftCat).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_dateOnly, _person1, _teamBlockInfo, _schedulingOptions, null);
				Assert.That(result.ShiftCategory, Is.EqualTo(expectedResult.ShiftCategory));
			}
		}

		[Test]
		public void ShouldAggregateFromRoleModel()
		{
			var effectiveRestrictionForDayOne = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(10), null),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(10)),
										 new EndTimeLimitation(TimeSpan.FromHours(16), TimeSpan.FromHours(16)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var shiftCat = new ShiftCategory("cat");
			expectedResult.ShiftCategory = shiftCat;
			var workShift = _mocks.StrictMock<IWorkShift>();
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var schedulePeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var schedulePeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var range1 = _mocks.DynamicMock<IScheduleRange>();
			var range2 = _mocks.DynamicMock<IScheduleRange>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDayPro1 = _mocks.DynamicMock<IScheduleDayPro>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var dateTimePeriod = new DateTimePeriod(new DateTime(2013, 11, 12, 0, 0, 0, DateTimeKind.Utc),
													new DateTime(2013, 11, 13, 0, 0, 0, DateTimeKind.Utc));
			var projectionService1 = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
			var period1 = new DateTimePeriod(new DateTime(2013, 11, 12, 10, 0, 0, DateTimeKind.Utc),
										   new DateTime(2013, 11, 12, 16, 0, 0, DateTimeKind.Utc));
			var personAssignment1 = _mocks.StrictMock<IPersonAssignment>();
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(schedulePeriod1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro1.Person).Return(_person1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.Person).Return(_person2).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.SchedulePeriod).Return(schedulePeriod2).Repeat.AtLeastOnce();
				Expect.Call(schedulePeriod1.DateOnlyPeriod).Return(_blockPeriod).Repeat.AtLeastOnce();
				Expect.Call(schedulePeriod2.DateOnlyPeriod).Return(_blockPeriod).Repeat.AtLeastOnce();

				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_group.GroupMembers, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestrictionForDayOne);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { _person1 }, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestrictionForDayOne);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(false).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameActivity(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameActivityInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_scheduleMatrixPro1.ActiveScheduleRange).Return(range1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.ActiveScheduleRange).Return(range2).Repeat.AtLeastOnce();
				Expect.Call(range1.ScheduledDay(_dateOnly)).Return(scheduleDay1);
				Expect.Call(range2.ScheduledDay(_dateOnly)).Return(scheduleDay2);
				Expect.Call(scheduleDay1.IsScheduled()).Return(true);
				Expect.Call(scheduleDay2.IsScheduled()).Return(false);
				Expect.Call(_nightlyRestRule.LongestDateTimePeriodForAssignment(range2, _dateOnly)).Return(dateTimePeriod);
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnly)).Return(scheduleDayPro1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.GetScheduleDayByKey(_dateOnly)).Return(null).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay1.ProjectionService()).Return(projectionService1).Repeat.AtLeastOnce();
				Expect.Call(projectionService1.CreateProjection()).Return(visualLayerCollection1).Repeat.AtLeastOnce();
				Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(scheduleDay1).Repeat.AtLeastOnce();
				Expect.Call(visualLayerCollection1.Period()).Return(period1).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay1.PersonAssignment()).Return(personAssignment1);
				Expect.Call(personAssignment1.ShiftCategory).Return(shiftCat);
				Expect.Call(_shift.WorkShiftStartTime).Return(TimeSpan.FromHours(10));
				Expect.Call(_shift.WorkShiftEndTime).Return(TimeSpan.FromHours(16));
				Expect.Call(_shift.TheWorkShift).Return(workShift);
				Expect.Call(workShift.ShiftCategory).Return(shiftCat);
			}

			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_dateOnly, _person1, _teamBlockInfo, _schedulingOptions, _shift);
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}
	}
}
