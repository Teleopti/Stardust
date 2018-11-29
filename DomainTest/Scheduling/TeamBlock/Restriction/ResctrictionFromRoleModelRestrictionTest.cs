using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Restriction
{
	[TestFixture]
	public class ResctrictionFromRoleModelRestrictionTest
	{
		private MockRepository _mocks;
		private ResctrictionFromRoleModelRestriction _target;
		private SchedulingOptions _schedulingOptions;
		private ShiftProjectionCache _shift;
		private ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private DateOnly _dateOnly;
		private List<DateOnly> _dateOnlyList;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private List<IScheduleMatrixPro> _matrixes;
		private IWorkShift _workShift;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_workShift = new WorkShift(new ShiftCategory("Test"));
			_shift = new ShiftProjectionCache(_workShift, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
			_teamBlockSchedulingOptions = new TeamBlockSchedulingOptions();
			_schedulingOptions = new SchedulingOptions();
			_target = new ResctrictionFromRoleModelRestriction(_shift, _teamBlockSchedulingOptions, _schedulingOptions);
			_dateOnly = new DateOnly(2013, 11, 14);
			_dateOnlyList = new List<DateOnly> {_dateOnly, _dateOnly.AddDays(1)};
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrixes = new List<IScheduleMatrixPro>{_scheduleMatrixPro};
		}

		[Test]
		public void ShouldAggregateStartTimeFromRoleModel()
		{
			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)),
				new EndTimeLimitation(),
				new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			_schedulingOptions.UseTeam = true;
			_schedulingOptions.TeamSameStartTime = true;
			_schedulingOptions.TeamSameShiftCategory = false;

			_workShift.LayerCollection.Add(new WorkShiftActivityLayer(new Activity("Phone"),
				_dateOnly.ToDateTimePeriod(TimeZoneInfo.Utc).ChangeStartTime(TimeSpan.FromHours(8))));

			var result = _target.ExtractRestriction(_dateOnlyList, _matrixes);
			Assert.That(result, Is.EqualTo(expectedResult));
		}

		[Test]
		public void ShouldAggregateEndTimeFromRoleModel()
		{
			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(),
				new EndTimeLimitation(TimeSpan.FromHours(18), TimeSpan.FromHours(18)),
				new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			_schedulingOptions.UseTeam = true;
			_schedulingOptions.TeamSameEndTime = true;
			_schedulingOptions.TeamSameShiftCategory = false;

			_workShift.LayerCollection.Add(new WorkShiftActivityLayer(new Activity("Phone"),
				_dateOnly.ToDateTimePeriod(TimeZoneInfo.Utc).ChangeEndTime(TimeSpan.FromHours(-6))));

			var result = _target.ExtractRestriction(_dateOnlyList, _matrixes);
			Assert.That(result, Is.EqualTo(expectedResult));
		}

		[Test]
		public void ShouldAggregateShiftCategoryFromRoleModel()
		{
			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(),
				new EndTimeLimitation(),
				new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			expectedResult.ShiftCategory = _workShift.ShiftCategory;
			_schedulingOptions.UseTeam = true;
			_schedulingOptions.TeamSameShiftCategory = true;
			
			var result = _target.ExtractRestriction(_dateOnlyList, _matrixes);
			Assert.That(result, Is.EqualTo(expectedResult));
		}
	}
}
