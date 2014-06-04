using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Restriction
{
	[TestFixture]
	public class BlockRestrictionAggregatorTest
	{
		private MockRepository _mocks;
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IScheduleDayEquator _scheduleDayEquator;
		private IAssignmentPeriodRule _nightlyRestRule;
		private ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private BlockRestrictionAggregator _target;
		private DateOnly _dateOnly;
		private IPerson _person1;
		private IPerson _person2;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private IScheduleMatrixPro _scheduleMatrixPro2;
		private TeamBlockInfo _teamBlockInfo;
		private SchedulingOptions _schedulingOptions;
		private IShiftProjectionCache _shift;
		private DateOnlyPeriod _blockPeriod;
		private GroupPerson _groupPerson;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_scheduleDayEquator = _mocks.StrictMock<IScheduleDayEquator>();
			_nightlyRestRule = _mocks.StrictMock<IAssignmentPeriodRule>();
			_teamBlockSchedulingOptions = new TeamBlockSchedulingOptions();
			_target = new BlockRestrictionAggregator(_effectiveRestrictionCreator, _schedulingResultStateHolder,
													 _scheduleDayEquator, _nightlyRestRule, _teamBlockSchedulingOptions);
			_dateOnly = new DateOnly(2013, 11, 12);
			_person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("bill"), _dateOnly);
			_person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("ball"), _dateOnly);
			_scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPro2 = _mocks.StrictMock<IScheduleMatrixPro>();
			_groupPerson = new GroupPerson(new List<IPerson> { _person1, _person2 }, _dateOnly, "Hej", Guid.Empty);
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro1, _scheduleMatrixPro2 };
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			ITeamInfo teamInfo = new TeamInfo(_groupPerson, groupMatrixes);
			_blockPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(1));
			_teamBlockInfo = new TeamBlockInfo(teamInfo, new BlockInfo(_blockPeriod));
			_schedulingOptions = new SchedulingOptions();
			_shift = _mocks.StrictMock<IShiftProjectionCache>();
		}

		[Test]
		public void ShouldAggregateExistedEffectiveRestriction()
		{
			var effectiveRestrictionForDayOne = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var effectiveRestrictionForDayTwo = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(7), null),
										 new EndTimeLimitation(null, TimeSpan.FromHours(17)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(null, TimeSpan.FromHours(17)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var schedulePeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var schedulePeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var range1 = _mocks.DynamicMock<IScheduleRange>();
			var scheduleDay1 = _mocks.DynamicMock<IScheduleDay>();
			var scheduleDay2 = _mocks.DynamicMock<IScheduleDay>();
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

				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { _person1 }, _dateOnly,
																				 _schedulingOptions, scheduleDictionary)).IgnoreArguments()
					  .Return(effectiveRestrictionForDayOne);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { _person1 }, _dateOnly.AddDays(1),
																				 _schedulingOptions, scheduleDictionary)).IgnoreArguments()
					  .Return(effectiveRestrictionForDayTwo);
				Expect.Call(_scheduleMatrixPro1.ActiveScheduleRange).Return(range1).Repeat.AtLeastOnce();
				Expect.Call(range1.ScheduledDay(_dateOnly)).Return(scheduleDay1);
				Expect.Call(range1.ScheduledDay(_dateOnly.AddDays(1))).Return(scheduleDay2);
				Expect.Call(scheduleDay1.IsScheduled()).Return(false);
				Expect.Call(scheduleDay2.IsScheduled()).Return(false);
				Expect.Call(_nightlyRestRule.LongestDateTimePeriodForAssignment(range1, _dateOnly)).Return(dateTimePeriod);
				Expect.Call(_nightlyRestRule.LongestDateTimePeriodForAssignment(range1, _dateOnly.AddDays(1))).Return(dateTimePeriod.MovePeriod(TimeSpan.FromDays(1)));
			}

			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_person1, _teamBlockInfo, _schedulingOptions, null);
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test, Ignore("Micke, need to talk to Asad")]
		public void ShouldAggregateWithSameStartTimeRestriction()
		{
			var effectiveRestrictionForDayOne = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var effectiveRestrictionForDayTwo = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(null, TimeSpan.FromHours(17)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(null, TimeSpan.FromHours(17)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var schedulePeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var schedulePeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var range1 = _mocks.DynamicMock<IScheduleRange>();
			var scheduleDay1 = _mocks.DynamicMock<IScheduleDay>();
			var scheduleDayPro1 = _mocks.DynamicMock<IScheduleDayPro>();
			var scheduleDay2 = _mocks.DynamicMock<IScheduleDay>();
			var scheduleDayPro2 = _mocks.DynamicMock<IScheduleDayPro>();
			var dateTimePeriod = new DateTimePeriod(new DateTime(2013, 11, 12, 0, 0, 0, DateTimeKind.Utc),
													new DateTime(2013, 11, 13, 0, 0, 0, DateTimeKind.Utc));
			var projectionService1 = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
			var projectionService2 = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection2 = _mocks.StrictMock<IVisualLayerCollection>();
			var period1 = new DateTimePeriod(new DateTime(2013, 11, 12, 9, 0, 0, DateTimeKind.Utc),
										   new DateTime(2013, 11, 12, 17, 30, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2013, 11, 13, 9, 0, 0, DateTimeKind.Utc),
										   new DateTime(2013, 11, 13, 17, 30, 0, DateTimeKind.Utc));

			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(schedulePeriod1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro1.Person).Return(_person1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.Person).Return(_person2).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.SchedulePeriod).Return(schedulePeriod2).Repeat.AtLeastOnce();
				Expect.Call(schedulePeriod1.DateOnlyPeriod).Return(_blockPeriod).Repeat.AtLeastOnce();
				Expect.Call(schedulePeriod2.DateOnlyPeriod).Return(_blockPeriod).Repeat.AtLeastOnce();

				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { _person1 }, _dateOnly,
																				 _schedulingOptions, scheduleDictionary)).IgnoreArguments()
					  .Return(effectiveRestrictionForDayOne);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { _person1 }, _dateOnly.AddDays(1),
																				 _schedulingOptions, scheduleDictionary)).IgnoreArguments()
					  .Return(effectiveRestrictionForDayTwo);
				Expect.Call(_scheduleMatrixPro1.ActiveScheduleRange).Return(range1).Repeat.AtLeastOnce();
				Expect.Call(range1.ScheduledDay(_dateOnly)).Return(scheduleDay1);
				Expect.Call(range1.ScheduledDay(_dateOnly.AddDays(1))).Return(scheduleDay2);
				Expect.Call(scheduleDay1.IsScheduled()).Return(false);
				Expect.Call(scheduleDay2.IsScheduled()).Return(false);
				Expect.Call(_nightlyRestRule.LongestDateTimePeriodForAssignment(range1, _dateOnly)).Return(dateTimePeriod);
				Expect.Call(_nightlyRestRule.LongestDateTimePeriodForAssignment(range1, _dateOnly.AddDays(1))).Return(dateTimePeriod.MovePeriod(TimeSpan.FromDays(1)));
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnly)).Return(scheduleDayPro1);
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnly.AddDays(1))).Return(scheduleDayPro2);
				Expect.Call(scheduleDay1.ProjectionService()).Return(projectionService1);
				Expect.Call(projectionService1.CreateProjection()).Return(visualLayerCollection1);
				Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(scheduleDay1);
				Expect.Call(visualLayerCollection1.Period()).Return(period1);
				Expect.Call(scheduleDay2.ProjectionService()).Return(projectionService2);
				Expect.Call(scheduleDayPro2.DaySchedulePart()).Return(scheduleDay2);
				Expect.Call(projectionService2.CreateProjection()).Return(visualLayerCollection2);
				Expect.Call(visualLayerCollection2.Period()).Return(period2);
				Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
			}

			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_person1, _teamBlockInfo, _schedulingOptions, null);
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void ShouldAggregateWithSameShiftRestriction()
		{
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.BlockSameShift = true;
			_schedulingOptions.BlockSameShiftCategory = false;
			var effectiveRestrictionForDayOne = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var effectiveRestrictionForDayTwo = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(null, TimeSpan.FromHours(17)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(null, TimeSpan.FromHours(17)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var schedulePeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var schedulePeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var range1 = _mocks.DynamicMock<IScheduleRange>();
			var scheduleDay1 = _mocks.DynamicMock<IScheduleDay>();
			var scheduleDayPro1 = _mocks.DynamicMock<IScheduleDayPro>();
			var scheduleDayPro2 = _mocks.DynamicMock<IScheduleDayPro>();
			var scheduleDay2 = _mocks.DynamicMock<IScheduleDay>();
			var dateTimePeriod = new DateTimePeriod(new DateTime(2013, 11, 12, 0, 0, 0, DateTimeKind.Utc),
													new DateTime(2013, 11, 13, 0, 0, 0, DateTimeKind.Utc));
			var period1 = new DateTimePeriod(new DateTime(2013, 11, 12, 9, 0, 0, DateTimeKind.Utc),
										   new DateTime(2013, 11, 12, 17, 30, 0, DateTimeKind.Utc));
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

				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { _person1 }, _dateOnly,
																				 _schedulingOptions, scheduleDictionary)).IgnoreArguments()
					  .Return(effectiveRestrictionForDayOne);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { _person1 }, _dateOnly.AddDays(1),
																				 _schedulingOptions, scheduleDictionary)).IgnoreArguments()
					  .Return(effectiveRestrictionForDayTwo);
				Expect.Call(_scheduleMatrixPro1.ActiveScheduleRange).Return(range1).Repeat.AtLeastOnce();
				Expect.Call(range1.ScheduledDay(_dateOnly)).Return(scheduleDay1);
				Expect.Call(range1.ScheduledDay(_dateOnly.AddDays(1))).Return(scheduleDay2);
				Expect.Call(_nightlyRestRule.LongestDateTimePeriodForAssignment(range1, _dateOnly)).Return(dateTimePeriod);
				Expect.Call(_nightlyRestRule.LongestDateTimePeriodForAssignment(range1, _dateOnly.AddDays(1))).Return(dateTimePeriod.MovePeriod(TimeSpan.FromDays(1)));
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnly)).Return(scheduleDayPro1);
				Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(scheduleDay1);
				Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(scheduleDay1.GetEditorShift()).Return(mainShift);
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnly.AddDays(1))).Return(scheduleDayPro2);
				Expect.Call(scheduleDayPro2.DaySchedulePart()).Return(scheduleDay2);
				Expect.Call(scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(scheduleDay2.GetEditorShift()).Return(mainShift);
				Expect.Call(_scheduleDayEquator.MainShiftBasicEquals(mainShift, mainShift)).Return(true);
			}

			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_person1, _teamBlockInfo, _schedulingOptions, null);
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void ShouldAggregateWithSameShiftCategoryRestriction()
		{
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.BlockSameShiftCategory = true;

			var effectiveRestrictionForDayOne = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var effectiveRestrictionForDayTwo = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(null, TimeSpan.FromHours(17)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(null, TimeSpan.FromHours(17)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var shiftCat = new ShiftCategory("cat");
			effectiveRestrictionForDayOne.ShiftCategory = shiftCat;
			expectedResult.ShiftCategory = shiftCat;
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var schedulePeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var schedulePeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var range1 = _mocks.DynamicMock<IScheduleRange>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDayPro1 = _mocks.DynamicMock<IScheduleDayPro>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var dateTimePeriod = new DateTimePeriod(new DateTime(2013, 11, 12, 0, 0, 0, DateTimeKind.Utc),
													new DateTime(2013, 11, 13, 0, 0, 0, DateTimeKind.Utc));
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

				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { _person1 }, _dateOnly,
																				 _schedulingOptions, scheduleDictionary)).IgnoreArguments()
					  .Return(effectiveRestrictionForDayOne);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { _person1 }, _dateOnly.AddDays(1),
																				 _schedulingOptions, scheduleDictionary)).IgnoreArguments()
					  .Return(effectiveRestrictionForDayTwo);
				Expect.Call(_scheduleMatrixPro1.ActiveScheduleRange).Return(range1).Repeat.AtLeastOnce();
				Expect.Call(range1.ScheduledDay(_dateOnly)).Return(scheduleDay1);
				Expect.Call(range1.ScheduledDay(_dateOnly.AddDays(1))).Return(scheduleDay2);
				Expect.Call(_nightlyRestRule.LongestDateTimePeriodForAssignment(range1, _dateOnly.AddDays(1))).Return(dateTimePeriod.MovePeriod(TimeSpan.FromDays(1)));
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnly)).Return(scheduleDayPro1);
				Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(scheduleDay1);
				Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(scheduleDay1.IsScheduled()).Return(true);
				Expect.Call(scheduleDay2.IsScheduled()).Return(false);
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnly.AddDays(1))).Return(null);
				Expect.Call(scheduleDay1.PersonAssignment()).Return(personAssignment1);
				Expect.Call(personAssignment1.ShiftCategory).Return(shiftCat);
			}

			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_person1, _teamBlockInfo, _schedulingOptions, null);
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}
		
		[Test]
		public void ShouldAggregateRestrictionFromRoleModelIfNotNull()
		{
			var effectiveRestrictionForDayOne = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var effectiveRestrictionForDayTwo = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(7), null),
										 new EndTimeLimitation(null, TimeSpan.FromHours(17)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(null, TimeSpan.FromHours(17)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var shiftCat = new ShiftCategory("cat");
			expectedResult.ShiftCategory = shiftCat;
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var schedulePeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var schedulePeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var range1 = _mocks.DynamicMock<IScheduleRange>();
			var scheduleDay1 = _mocks.DynamicMock<IScheduleDay>();
			var scheduleDay2 = _mocks.DynamicMock<IScheduleDay>();
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

				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { _person1 }, _dateOnly,
																				 _schedulingOptions, scheduleDictionary)).IgnoreArguments()
					  .Return(effectiveRestrictionForDayOne);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> { _person1 }, _dateOnly.AddDays(1),
																				 _schedulingOptions, scheduleDictionary)).IgnoreArguments()
					  .Return(effectiveRestrictionForDayTwo);
				Expect.Call(_scheduleMatrixPro1.ActiveScheduleRange).Return(range1).Repeat.AtLeastOnce();
				Expect.Call(range1.ScheduledDay(_dateOnly)).Return(scheduleDay1);
				Expect.Call(range1.ScheduledDay(_dateOnly.AddDays(1))).Return(scheduleDay2);
				Expect.Call(scheduleDay1.IsScheduled()).Return(false);
				Expect.Call(scheduleDay2.IsScheduled()).Return(false);
				Expect.Call(_nightlyRestRule.LongestDateTimePeriodForAssignment(range1, _dateOnly)).Return(dateTimePeriod);
				Expect.Call(_nightlyRestRule.LongestDateTimePeriodForAssignment(range1, _dateOnly.AddDays(1))).Return(dateTimePeriod.MovePeriod(TimeSpan.FromDays(1)));
			}

			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_person1, _teamBlockInfo, _schedulingOptions, _shift);
				Assert.That(result.StartTimeLimitation, Is.EqualTo(expectedResult.StartTimeLimitation));
			}
		}



	}
}
