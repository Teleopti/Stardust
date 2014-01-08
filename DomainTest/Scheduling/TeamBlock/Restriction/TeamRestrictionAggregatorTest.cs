﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Restriction
{
	[TestFixture]
	public class TeamRestrictionAggregatorTest
	{
		private TeamRestrictionAggregator _target;
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private MockRepository _mocks;
		private ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private DateOnly _dateOnly;
		private IPerson _person1;
		private IPerson _person2;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private IScheduleMatrixPro _scheduleMatrixPro2;
		private GroupPerson _groupPerson;
		private DateOnlyPeriod _blockPeriod;
		private TeamBlockInfo _teamBlockInfo;
		private SchedulingOptions _schedulingOptions;
		private IShiftProjectionCache _shift;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_teamBlockSchedulingOptions = _mocks.StrictMock<ITeamBlockSchedulingOptions>();
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_target = new TeamRestrictionAggregator(_effectiveRestrictionCreator, _schedulingResultStateHolder,
			                                        _teamBlockSchedulingOptions);
			_dateOnly = new DateOnly(2013, 11, 12);
			_person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("bill"), _dateOnly);
			_person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("ball"), _dateOnly);
			_scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPro2 = _mocks.StrictMock<IScheduleMatrixPro>();
			_groupPerson = new GroupPerson(new List<IPerson> { _person1, _person2 }, _dateOnly, "Hej", Guid.Empty);
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro1, _scheduleMatrixPro2 };
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			ITeamInfo teamInfo = new TeamInfo(_groupPerson, groupMatrixes);
			_blockPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
			_teamBlockInfo = new TeamBlockInfo(teamInfo, new BlockInfo(_blockPeriod));
			_schedulingOptions = new SchedulingOptions();
			_shift = _mocks.StrictMock<IShiftProjectionCache>();
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
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_groupPerson.GroupMembers, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestriction);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameActivity(_schedulingOptions)).Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_dateOnly, _teamBlockInfo, _schedulingOptions, null);
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

			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_groupPerson.GroupMembers, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestriction);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameActivity(_schedulingOptions)).Return(false);
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
				var result = _target.Aggregate(_dateOnly, _teamBlockInfo, _schedulingOptions, null);
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

			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_groupPerson.GroupMembers, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestriction);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameActivity(_schedulingOptions)).Return(false);
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
				var result = _target.Aggregate(_dateOnly, _teamBlockInfo, _schedulingOptions, null);
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
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_groupPerson.GroupMembers, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestriction);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameActivity(_schedulingOptions)).Return(false);
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnly)).Return(scheduleDayPro1);
				Expect.Call(_scheduleMatrixPro2.GetScheduleDayByKey(_dateOnly)).Return(null);
				Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(scheduleDay1);
				Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(scheduleDay1.PersonAssignment()).Return(personAssignment1);
				Expect.Call(personAssignment1.ShiftCategory).Return(shiftCat);
			}

			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_dateOnly, _teamBlockInfo, _schedulingOptions, null);
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
			var shiftCat = ShiftCategoryFactory.CreateShiftCategory("cat");
			expectedResult.ShiftCategory = shiftCat;
			var workShift = _mocks.StrictMock<IWorkShift>();
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_groupPerson.GroupMembers, _dateOnly,
																				 _schedulingOptions, scheduleDictionary))
					  .Return(effectiveRestriction);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameActivity(_schedulingOptions)).Return(false);
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnly)).Return(null).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro2.GetScheduleDayByKey(_dateOnly)).Return(null).Repeat.AtLeastOnce();
				
				Expect.Call(_shift.WorkShiftStartTime).Return(TimeSpan.FromHours(10));
				Expect.Call(_shift.WorkShiftEndTime).Return(TimeSpan.FromHours(16));
				Expect.Call(_shift.TheWorkShift).Return(workShift);
				Expect.Call(workShift.ShiftCategory).Return(shiftCat);
				
			}

			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_dateOnly, _teamBlockInfo, _schedulingOptions, _shift);
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

	}
}
