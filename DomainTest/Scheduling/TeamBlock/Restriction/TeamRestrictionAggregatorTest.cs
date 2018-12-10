using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Restriction
{
	[TestFixture]
	public class TeamRestrictionAggregatorTest
	{
		private TeamRestrictionAggregator _target;
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private MockRepository _mocks;
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
		private ShiftProjectionCache _shift;
		private WorkShift _workShift;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_teamBlockSchedulingOptions = new TeamBlockSchedulingOptions();
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_target = new TeamRestrictionAggregator(_effectiveRestrictionCreator, _teamBlockSchedulingOptions);
			_dateOnly = new DateOnly(2013, 11, 12);
			_person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("bill"), _dateOnly);
			_person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("ball"), _dateOnly);
			_scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPro2 = _mocks.StrictMock<IScheduleMatrixPro>();
			_group = new Group(new List<IPerson> { _person1, _person2 }, "Hej");
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro1, _scheduleMatrixPro2 };
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			ITeamInfo teamInfo = new TeamInfo(_group, groupMatrixes);
			_blockPeriod = _dateOnly.ToDateOnlyPeriod();
			_teamBlockInfo = new TeamBlockInfo(teamInfo, new BlockInfo(_blockPeriod));
			_schedulingOptions = new SchedulingOptions();
			_schedulingOptions.TeamSameShiftCategory = false;
			_schedulingOptions.BlockSameShiftCategory = false;
			_workShift = new WorkShift(new ShiftCategory("Test"));
			_shift = new ShiftProjectionCache(_workShift, new DateOnlyAsDateTimePeriod(_dateOnly,TimeZoneInfo.Utc));
		}


		[Test]
		public void ShouldAggregateExistedEffectiveRestriction()
		{
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(null, TimeSpan.FromHours(17)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(null, TimeSpan.FromHours(17)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();

			using (_mocks.Record())
			{
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_group.GroupMembers, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestriction);
			}

			using (_mocks.Playback())
			{
				var result = _target.Aggregate(scheduleDictionary, _dateOnly, _teamBlockInfo, _schedulingOptions, null);
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void ShouldAggregateWithSameStartTimeRestriction()
		{
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			
			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(9), TimeSpan.FromHours(9)),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var scheduleDay1 = _mocks.DynamicMock<IScheduleDay>();
			var scheduleDayPro1 = _mocks.DynamicMock<IScheduleDayPro>();
			var projectionService1 = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
			var period1 = new DateTimePeriod(new DateTime(2013, 11, 12, 9, 0, 0, DateTimeKind.Utc),
										   new DateTime(2013, 11, 12, 17, 30, 0, DateTimeKind.Utc));
			_schedulingOptions.UseTeam = true;
			_schedulingOptions.TeamSameStartTime = true;

			using (_mocks.Record())
			{
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_group.GroupMembers, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestriction);
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
				var result = _target.Aggregate(scheduleDictionary, _dateOnly, _teamBlockInfo, _schedulingOptions, null);
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void ShouldAggregateWithSameEndTimeRestriction()
		{
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(17)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var scheduleDay1 = _mocks.DynamicMock<IScheduleDay>();
			var scheduleDayPro1 = _mocks.DynamicMock<IScheduleDayPro>();
			var projectionService1 = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
			var period1 = new DateTimePeriod(new DateTime(2013, 11, 12, 9, 0, 0, DateTimeKind.Utc),
										   new DateTime(2013, 11, 12, 17, 0, 0, DateTimeKind.Utc));

			_schedulingOptions.UseTeam = true;
			_schedulingOptions.TeamSameEndTime = true;

			using (_mocks.Record())
			{
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_group.GroupMembers, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestriction);
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
				var result = _target.Aggregate(scheduleDictionary, _dateOnly, _teamBlockInfo, _schedulingOptions, null);
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}
		
		[Test]
		public void ShouldAggregateWithSameShiftCategoryRestriction()
		{
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var shiftCat = ShiftCategoryFactory.CreateShiftCategory("cat");
			expectedResult.ShiftCategory = shiftCat;
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var scheduleDay1 = _mocks.DynamicMock<IScheduleDay>();
			var scheduleDayPro1 = _mocks.DynamicMock<IScheduleDayPro>();
			var personAssignment1 = _mocks.StrictMock<IPersonAssignment>();
			_schedulingOptions.UseTeam = true;
			_schedulingOptions.TeamSameShiftCategory = true;

			using (_mocks.Record())
			{
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_group.GroupMembers, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestriction);
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnly)).Return(scheduleDayPro1);
				Expect.Call(_scheduleMatrixPro2.GetScheduleDayByKey(_dateOnly)).Return(null);
				Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(scheduleDay1);
				Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(scheduleDay1.PersonAssignment()).Return(personAssignment1);
				Expect.Call(personAssignment1.ShiftCategory).Return(shiftCat);
			}

			using (_mocks.Playback())
			{
				var result = _target.Aggregate(scheduleDictionary, _dateOnly, _teamBlockInfo, _schedulingOptions, null);
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void ShouldAggregateFromRoleModel()
		{
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), null),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(10)),
										 new EndTimeLimitation(TimeSpan.FromHours(16), TimeSpan.FromHours(16)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			expectedResult.ShiftCategory = _workShift.ShiftCategory;
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_schedulingOptions.UseTeam = true;
			_schedulingOptions.TeamSameStartTime = true;
			_schedulingOptions.TeamSameEndTime = true;
			_schedulingOptions.TeamSameShiftCategory = true;

			_workShift.LayerCollection.Add(new WorkShiftActivityLayer(new Activity("Phone"),
				_dateOnly.ToDateTimePeriod(TimeZoneInfo.Utc)
					.ChangeStartTime(TimeSpan.FromHours(10))
					.ChangeEndTime(TimeSpan.FromHours(-8))));
			
			using (_mocks.Record())
			{
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_group.GroupMembers, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestriction);
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnly)).Return(null).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.GetScheduleDayByKey(_dateOnly)).Return(null).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				var result = _target.Aggregate(scheduleDictionary, _dateOnly, _teamBlockInfo, _schedulingOptions, _shift);
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

	}
}
