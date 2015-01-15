using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[TestFixture]
	public class AnalyticsFactScheduleTimeHandlerTest
	{
		private AnalyticsFactScheduleTimeHandler _target;
		private IAnalyticsScheduleRepository _rep;
		private IList<IAnalyticsActivity> _activities;
		private readonly Guid _guidActInPaid = Guid.NewGuid();
		private readonly Guid _guidActInReady = Guid.NewGuid();
		private readonly Guid _guidActInBoth = Guid.NewGuid();
		private readonly Guid _guidActInNone = Guid.NewGuid();

		private readonly Guid _guidAbsInPaid = Guid.NewGuid();
		private readonly Guid _guidAbsNotPaid = Guid.NewGuid();
		private List<IAnalyticsAbsence> _absences;
		private List<IAnalyticsGeneric> _overtimes;
		private List<IAnalyticsShiftLength> _shiftLengths;

		[SetUp]
		public void Setup()
		{
			_rep = MockRepository.GenerateMock<IAnalyticsScheduleRepository>();
			_target = new AnalyticsFactScheduleTimeHandler(_rep);

			_activities = new List<IAnalyticsActivity>
			{
				new AnalyticsActivity {ActivityCode = _guidActInPaid, ActivityId = 1, InPaidTime = true, InReadyTime = false},
				new AnalyticsActivity {ActivityCode = _guidActInReady, ActivityId = 2, InPaidTime = false, InReadyTime = true},
				new AnalyticsActivity {ActivityCode = _guidActInBoth, ActivityId = 3, InPaidTime = true, InReadyTime = true},
				new AnalyticsActivity {ActivityCode = _guidActInNone, ActivityId = 4, InPaidTime = false, InReadyTime = false}
			};

			_absences = new List<IAnalyticsAbsence>
			{
				new AnalyticsAbsence {AbsenceCode = _guidAbsInPaid, AbsenceId = 1, InPaidTime = true},
				new AnalyticsAbsence {AbsenceCode = _guidAbsNotPaid, AbsenceId = 2, InPaidTime = false}
			};
			_overtimes = new List<IAnalyticsGeneric>
			{
				new AnalyticsGeneric { Id = 3, Code = Guid.NewGuid() }
			};
			_shiftLengths = new List<IAnalyticsShiftLength>
			{
				new AnalyticsShiftLength{ Id = 6, ShiftLength = 120 }
			};
		}

		[Test]
		public void ShouldSetActivityTimeIfNotAbsence()
		{
			var start = new DateTime(2014, 12, 4, 8, 0, 0);
			var layer = new ProjectionChangedEventLayer
			{
				IsAbsence = false,
				ContractTime = TimeSpan.FromMinutes(10),
				PayloadId = _guidActInPaid,
				WorkTime = TimeSpan.FromMinutes(10),
				StartDateTime = start,
				EndDateTime = start.AddMinutes(10),
			};
			_rep.Stub(x => x.Overtimes()).Return(_overtimes);
			_rep.Stub(x => x.Activities()).Return(_activities);
			_rep.Stub(x => x.ShiftLengths()).Return(_shiftLengths);
			var result = _target.Handle(layer, 12, 22, _shiftLengths.First().ShiftLength);

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
			result.ReadyTimeMinues.Should().Be.EqualTo(0);
			result.OverTimeId.Should().Be.EqualTo(-1);
			result.ScenarioId.Should().Be.EqualTo(22);
			result.ShiftCategoryId.Should().Be.EqualTo(12);
			result.ShiftLengthId.Should().Be.EqualTo(_shiftLengths.First().Id);
		}

		[Test]
		public void ShouldHandlePaidTimeAndReadyTime()
		{
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

			_rep.Stub(x => x.Overtimes()).Return(_overtimes);
			_rep.Stub(x => x.Activities()).Return(_activities);
			_rep.Stub(x => x.ShiftLengths()).Return(_shiftLengths);
			var result = _target.Handle(layer, 12, 22, _shiftLengths.First().ShiftLength);

			result.AbsenceId.Should().Be.EqualTo(-1);
			result.ActivityId.Should().Be.EqualTo(3);
			result.PaidTimeMinutes.Should().Be.EqualTo(12);
			result.PaidTimeActivityMinutes.Should().Be.EqualTo(12);
			result.PaidTimeAbsenceMinutes.Should().Be.EqualTo(0);

			result.ReadyTimeMinues.Should().Be.EqualTo(12);
			result.OverTimeId.Should().Be.EqualTo(-1);
		}


		[Test]
		public void ShouldSetAbsenceTimeIfAbsence()
		{
			var start = new DateTime(2014, 12, 4, 8, 0, 0);
			var layer = new ProjectionChangedEventLayer
			{
				IsAbsence = true,
				ContractTime = TimeSpan.FromMinutes(10),
				PayloadId = _guidAbsInPaid,
				WorkTime = TimeSpan.FromMinutes(10),
				StartDateTime = start,
				EndDateTime = start.AddMinutes(10)
			};
			_rep.Stub(x => x.Overtimes()).Return(_overtimes);
			_rep.Stub(x => x.Absences()).Return(_absences);
			_rep.Stub(x => x.ShiftLengths()).Return(_shiftLengths);
			var result = _target.Handle(layer, 12, 22, _shiftLengths.First().ShiftLength);

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
			result.ReadyTimeMinues.Should().Be.EqualTo(0);
			result.OverTimeId.Should().Be.EqualTo(-1);
			result.OverTimeMinutes.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldSetOvertime()
		{
			
			var start = new DateTime(2014, 12, 4, 8, 0, 0);
			var layer = new ProjectionChangedEventLayer
			{
				Overtime = TimeSpan.FromMinutes(10),
				MultiplicatorDefinitionSetId = _overtimes[0].Code,
				StartDateTime = start,
				EndDateTime = start.AddMinutes(10)
			};
			
			_rep.Stub(x => x.Overtimes()).Return(_overtimes);
			_rep.Stub(x => x.Activities()).Return(_activities);
			_rep.Stub(x => x.ShiftLengths()).Return(_shiftLengths);

			var result = _target.Handle(layer, 12, 22, _shiftLengths.First().ShiftLength);

			result.OverTimeMinutes.Should().Be.EqualTo(10);
			result.OverTimeId.Should().Be.EqualTo(_overtimes[0].Id);
		}

		[Test]
		public void ShouldMapAbsenceId()
		{
			_rep.Stub(x => x.Absences()).Return(_absences);
			var absence =_target.MapAbsenceId(_absences[0].AbsenceCode);
			absence.AbsenceId.Should().Be.EqualTo(_absences[0].AbsenceId);
		}

		[Test]
		public void ShouldFailToMapAbsenceId()
		{
			_rep.Stub(x => x.Absences()).Return(_absences);
			var absence = _target.MapAbsenceId(Guid.NewGuid());
			absence.Should().Be.Null();
		}

		[Test]
		public void ShouldMapOvertimeId()
		{
			_rep.Stub(x => x.Overtimes()).Return(_overtimes);
			var overtimeId = _target.MapOvertimeId(_overtimes[0].Code);
			overtimeId.Should().Be.EqualTo(_overtimes[0].Id);
		}

		[Test]
		public void ShouldFailToMapOvertimeId()
		{
			_rep.Stub(x => x.Overtimes()).Return(new List<IAnalyticsGeneric>());
			var overtimeId = _target.MapOvertimeId(Guid.NewGuid());
			overtimeId.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldMapNewShiftLengthId()
		{
			_rep.Stub(x => x.ShiftLengths()).Return(new List<IAnalyticsShiftLength>());
			_rep.Stub(x => x.ShiftLengthId(30)).Return(77);

			var shiftLengthId = _target.MapShiftLengthId(30);

			shiftLengthId.Should().Be.EqualTo(77);
		}
	}
}