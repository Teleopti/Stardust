using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsFactScheduleTimeMapperTest
	{
		public AnalyticsFactScheduleTimeMapper Target;
		public FakeAnalyticsScheduleRepository AnalyticsSchedules;
		public FakeAnalyticsActivityRepository AnalyticsActivities;
		public FakeAnalyticsOvertimeRepository AnalyticsOvertimes;
		public FakeAnalyticsAbsenceRepository AnalyticsAbsences;

		private IList<AnalyticsActivity> _activities;
		private IList<AnalyticsAbsence> _absences;
		private IList<AnalyticsOvertime> _overtimes;
		private IList<IAnalyticsShiftLength> _shiftLengths;

		private readonly Guid _guidActInPaid = Guid.NewGuid();
		private readonly Guid _guidActInReady = Guid.NewGuid();
		private readonly Guid _guidActInBoth = Guid.NewGuid();
		private readonly Guid _guidActInNone = Guid.NewGuid();
		private readonly Guid _guidAbsInPaid = Guid.NewGuid();
		private readonly Guid _guidAbsNotPaid = Guid.NewGuid();

		[SetUp]
		public void Setup()
		{
			_absences = new List<AnalyticsAbsence>
			{
				new AnalyticsAbsence {AbsenceCode = _guidAbsInPaid, AbsenceId = 1, InPaidTime = true},
				new AnalyticsAbsence {AbsenceCode = _guidAbsNotPaid, AbsenceId = 2, InPaidTime = false}
			};

			_activities = new List<AnalyticsActivity>
			{
				new AnalyticsActivity {ActivityCode = _guidActInPaid, ActivityId = 1, InPaidTime = true, InReadyTime = false},
				new AnalyticsActivity {ActivityCode = _guidActInReady, ActivityId = 2, InPaidTime = false, InReadyTime = true},
				new AnalyticsActivity {ActivityCode = _guidActInBoth, ActivityId = 3, InPaidTime = true, InReadyTime = true},
				new AnalyticsActivity {ActivityCode = _guidActInNone, ActivityId = 4, InPaidTime = false, InReadyTime = false}
			};
			
			_overtimes = new List<AnalyticsOvertime>
			{
				new AnalyticsOvertime { OvertimeId = 3, OvertimeCode = Guid.NewGuid() }
			};

			_shiftLengths = new List<IAnalyticsShiftLength>
			{
				new AnalyticsShiftLength{ Id = 6, ShiftLength = 120 }
			};
		}

		[Test]
		public void ShouldSetActivityTimeIfNotAbsence()
		{
			_activities.ForEach(a => AnalyticsActivities.AddActivity(a));
			_absences.ForEach(a => AnalyticsAbsences.AddAbsence(a));
			_overtimes.ForEach(o => AnalyticsOvertimes.AddOrUpdate(o));
			_shiftLengths.ForEach(sl => AnalyticsSchedules.Has(sl));

			var start = new DateTime(2014, 12, 4, 8, 0, 0);
			var layer = new ProjectionChangedEventLayer
			{
				IsAbsence = false,
				ContractTime = TimeSpan.FromMinutes(10),
				PayloadId = _guidActInPaid,
				WorkTime = TimeSpan.FromMinutes(10),
				StartDateTime = start,
				EndDateTime = start.AddMinutes(10)
			};
			var result = Target.Handle(layer, AnalyticsActivities.Activities().ToDictionary(k => k.ActivityCode), 12, 22, _shiftLengths.First().Id, TimeSpan.Zero);

			result.AbsenceId.Should().Be.EqualTo(-1);
			result.ActivityId.Should().Be.EqualTo(1);
			result.PaidTimeMinutes.Should().Be.EqualTo(10);
			result.PaidTimeActivityMinutes.Should().Be.EqualTo(10);
			result.PaidTimeAbsenceMinutes.Should().Be.EqualTo(0);
			result.WorkTimeActivityMinutes.Should().Be.EqualTo(10);
			result.WorkTimeAbsenceMinutes.Should().Be.EqualTo(0);
			result.WorkTimeMinutes.Should().Be.EqualTo(10);
			result.ContractTimeMinutes.Should().Be.EqualTo(10);
			result.ContractTimeActivityMinutes.Should().Be.EqualTo(10);
			result.ContractTimeAbsenceMinutes.Should().Be.EqualTo(0);
			result.ReadyTimeMinutes.Should().Be.EqualTo(0);
			result.OverTimeId.Should().Be.EqualTo(-1);
			result.ScenarioId.Should().Be.EqualTo(22);
			result.ShiftCategoryId.Should().Be.EqualTo(12);
			result.ShiftLengthId.Should().Be.EqualTo(_shiftLengths.First().Id);
		}

		[Test]
		public void ShouldHandlePaidTimeAndReadyTime()
		{
			_activities.ForEach(a => AnalyticsActivities.AddActivity(a));
			_absences.ForEach(a => AnalyticsAbsences.AddAbsence(a));
			_overtimes.ForEach(o => AnalyticsOvertimes.AddOrUpdate(o));
			_shiftLengths.ForEach(sl => AnalyticsSchedules.Has(sl));

			var start = new DateTime(2014, 12, 4, 8, 0, 0);
			var layer = new ProjectionChangedEventLayer
			{
				IsAbsence = false,
				ContractTime = TimeSpan.FromMinutes(12),
				PayloadId = _guidActInBoth,
				WorkTime = TimeSpan.FromMinutes(12),
				StartDateTime = start,
				EndDateTime = start.AddMinutes(12)
			};
			var result = Target.Handle(layer, AnalyticsActivities.Activities().ToDictionary(k => k.ActivityCode), 12, 22, _shiftLengths.First().Id, TimeSpan.Zero);

			result.AbsenceId.Should().Be.EqualTo(-1);
			result.ActivityId.Should().Be.EqualTo(3);
			result.PaidTimeMinutes.Should().Be.EqualTo(12);
			result.PaidTimeActivityMinutes.Should().Be.EqualTo(12);
			result.PaidTimeAbsenceMinutes.Should().Be.EqualTo(0);

			result.ReadyTimeMinutes.Should().Be.EqualTo(12);
			result.OverTimeId.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldSetAbsenceTimeIfAbsence()
		{
			_activities.ForEach(a => AnalyticsActivities.AddActivity(a));
			_absences.ForEach(a => AnalyticsAbsences.AddAbsence(a));
			_overtimes.ForEach(o => AnalyticsOvertimes.AddOrUpdate(o));
			_shiftLengths.ForEach(sl => AnalyticsSchedules.Has(sl));

			var start = new DateTime(2014, 12, 4, 8, 0, 0);
			var layer = new ProjectionChangedEventLayer
			{
				IsAbsence = true,
				ContractTime = TimeSpan.FromMinutes(10),
				PayloadId = _guidAbsInPaid,
				WorkTime = TimeSpan.FromMinutes(10),
				PaidTime = TimeSpan.FromMinutes(10),
				StartDateTime = start,
				EndDateTime = start.AddMinutes(10)
			};
			var result = Target.Handle(layer, AnalyticsActivities.Activities().ToDictionary(k => k.ActivityCode), 12, 22, _shiftLengths.First().Id, TimeSpan.Zero);

			result.ShiftCategoryId.Should().Be.EqualTo(-1);
			result.AbsenceId.Should().Be.EqualTo(1);
			result.ActivityId.Should().Be.EqualTo(-1);
			result.PaidTimeMinutes.Should().Be.EqualTo(10);
			result.PaidTimeActivityMinutes.Should().Be.EqualTo(0);
			result.PaidTimeAbsenceMinutes.Should().Be.EqualTo(10);
			result.WorkTimeActivityMinutes.Should().Be.EqualTo(0);
			result.WorkTimeAbsenceMinutes.Should().Be.EqualTo(10);
			result.WorkTimeMinutes.Should().Be.EqualTo(10);
			result.ContractTimeMinutes.Should().Be.EqualTo(10);
			result.ContractTimeActivityMinutes.Should().Be.EqualTo(0);
			result.ContractTimeAbsenceMinutes.Should().Be.EqualTo(10);
			result.ReadyTimeMinutes.Should().Be.EqualTo(0);
			result.OverTimeId.Should().Be.EqualTo(-1);
			result.OverTimeMinutes.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldSetOvertime()
		{
			_activities.ForEach(a => AnalyticsActivities.AddActivity(a));
			_absences.ForEach(a => AnalyticsAbsences.AddAbsence(a));
			_overtimes.ForEach(o => AnalyticsOvertimes.AddOrUpdate(o));
			_shiftLengths.ForEach(sl => AnalyticsSchedules.Has(sl));

			var start = new DateTime(2014, 12, 4, 8, 0, 0);
			var layer = new ProjectionChangedEventLayer
			{
				Overtime = TimeSpan.FromMinutes(10),
				MultiplicatorDefinitionSetId = _overtimes[0].OvertimeCode,
				StartDateTime = start,
				EndDateTime = start.AddMinutes(10)
			};

			var result = Target.Handle(layer, AnalyticsActivities.Activities().ToDictionary(k => k.ActivityCode), 12, 22, _shiftLengths.First().Id, TimeSpan.Zero);

			result.OverTimeMinutes.Should().Be.EqualTo(10);
			result.OverTimeId.Should().Be.EqualTo(_overtimes[0].OvertimeId);
		}

		[Test]
		public void ShouldMapAbsenceId()
		{
			_activities.ForEach(a => AnalyticsActivities.AddActivity(a));
			_absences.ForEach(a => AnalyticsAbsences.AddAbsence(a));
			_overtimes.ForEach(o => AnalyticsOvertimes.AddOrUpdate(o));
			_shiftLengths.ForEach(sl => AnalyticsSchedules.Has(sl));

			var absence = Target.MapAbsenceId(_absences[0].AbsenceCode);
			absence.AbsenceId.Should().Be.EqualTo(_absences[0].AbsenceId);
		}

		[Test]
		public void ShouldFailToMapAbsenceId()
		{
			_activities.ForEach(a => AnalyticsActivities.AddActivity(a));
			_absences.ForEach(a => AnalyticsAbsences.AddAbsence(a));
			_overtimes.ForEach(o => AnalyticsOvertimes.AddOrUpdate(o));
			_shiftLengths.ForEach(sl => AnalyticsSchedules.Has(sl));

			var absence = Target.MapAbsenceId(Guid.NewGuid());
			absence.Should().Be.Null();
		}

		[Test]
		public void ShouldMapOvertimeId()
		{
			_activities.ForEach(a => AnalyticsActivities.AddActivity(a));
			_absences.ForEach(a => AnalyticsAbsences.AddAbsence(a));
			_overtimes.ForEach(o => AnalyticsOvertimes.AddOrUpdate(o));
			_shiftLengths.ForEach(sl => AnalyticsSchedules.Has(sl));

			var overtimeId = Target.MapOvertimeId(_overtimes[0].OvertimeCode);

			overtimeId.Should().Be.EqualTo(_overtimes[0].OvertimeId);
		}

		[Test]
		public void ShouldFailToMapOvertimeId()
		{
			_activities.ForEach(a => AnalyticsActivities.AddActivity(a));
			_absences.ForEach(a => AnalyticsAbsences.AddAbsence(a));
			_shiftLengths.ForEach(sl => AnalyticsSchedules.Has(sl));

			var overtimeId = Target.MapOvertimeId(Guid.NewGuid());

			overtimeId.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldMapNewShiftLengthId()
		{
			_activities.ForEach(a => AnalyticsActivities.AddActivity(a));
			_absences.ForEach(a => AnalyticsAbsences.AddAbsence(a));
			_overtimes.ForEach(o => AnalyticsOvertimes.AddOrUpdate(o));
			AnalyticsSchedules.Has(new AnalyticsShiftLength {Id = 77, ShiftLength = 30});

			var shiftLengthId = Target.MapShiftLengthId(30);

			shiftLengthId.Should().Be.EqualTo(77);
		}
	}
}