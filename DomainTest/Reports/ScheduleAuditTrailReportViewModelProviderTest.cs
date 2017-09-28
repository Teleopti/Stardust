using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Reports
{
	[DomainTest]
	public class ScheduleAuditTrailReportViewModelProviderTest
	{
		public ScheduleAuditTrailReportViewModelProvider Target;
		public FakeScheduleAuditTrailReport FakeScheduleAuditTrailReport;
		public FakePersonRepository PersonRepository;
		public FakeUserTimeZone TimeZone;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			
		}

		[Test]
		public void ShouldReturnViewModel()
		{
			var person = PersonFactory.CreatePersonWithGuid("Joe", "Doe");
			PersonRepository.Has(person);
			var changedAt = new DateTime(2016, 8, 1, 10, 0, 0);
			var scheduleStart = new DateTime(2016, 8, 23, 8, 0, 0);
			var scheduleEnd = new DateTime(2016, 8, 23, 17, 0, 0);
			var auditTrailData = createAuditingData(person, changedAt, scheduleStart, scheduleEnd);

			var searchParam = new AuditTrailSearchParams()
			{
				ChangedByPersonId = person.Id.Value,
				ChangesOccurredStartDate = new DateTime(2016, 8, 1),
				ChangesOccurredEndDate = new DateTime(2016, 8, 1),
				AffectedPeriodStartDate = new DateTime(2016,8,23),
				AffectedPeriodEndDate = new DateTime(2016,8,23),
				MaximumResults = 1
			};
			var vm = Target.Provide(searchParam);

			vm.Count.Should().Be.EqualTo(1);
			vm.First().ModifiedBy.Should().Be.EqualTo(auditTrailData.ModifiedBy);
			vm.First().ModifiedAt.Should().Be.EqualTo(auditTrailData.ModifiedAt);
			vm.First().AuditType.Should().Be.EqualTo(auditTrailData.AuditType);
			vm.First().Detail.Should().Be.EqualTo(auditTrailData.Detail);
			vm.First().ScheduledAgent.Should().Be.EqualTo(auditTrailData.ScheduledAgent);
			vm.First().ScheduleStart.Should().Be.EqualTo(auditTrailData.ScheduleStart);
			vm.First().ScheduleEnd.Should().Be.EqualTo(auditTrailData.ScheduleEnd);
			vm.First().ShiftType.Should().Be.EqualTo(auditTrailData.ShiftType);
		}

		[Test]
		public void ShouldReturnViewModelForHawaiiTimeZone()
		{
			TimeZone.IsHawaii();

			var person = PersonFactory.CreatePersonWithGuid("Joe", "Doe");
			PersonRepository.Has(person);

			var changedAtUtc = new DateTime(2016, 8, 1, 4, 0, 0);
			var scheduleStartUtc = new DateTime(2016, 8, 23, 2, 0, 0);
			var scheduleEndUtc = new DateTime(2016, 8, 23, 10, 0, 0);
			createAuditingData(person, changedAtUtc, scheduleStartUtc, scheduleEndUtc);

			var searchParamLocalTime = new AuditTrailSearchParams()
			{
				ChangedByPersonId = person.Id.Value,
				ChangesOccurredStartDate = new DateTime(2016, 7, 31),
				ChangesOccurredEndDate = new DateTime(2016, 7, 31),
				AffectedPeriodStartDate = new DateTime(2016, 8, 22),
				AffectedPeriodEndDate = new DateTime(2016, 8, 22),
				MaximumResults = 1
			};
			var vm = Target.Provide(searchParamLocalTime);

			vm.Count.Should().Be.EqualTo(1);
			vm.First().ModifiedAt.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(changedAtUtc, TimeZone.TimeZone()));
			vm.First().ScheduleStart.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(scheduleStartUtc, TimeZone.TimeZone()));
			vm.First().ScheduleEnd.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(scheduleEndUtc, TimeZone.TimeZone()));
		}

		[Test]
		public void ShouldReturnViewModelForAllPersonsWhoChangeSchedule()
		{
			var person = PersonFactory.CreatePersonWithGuid("Joe", "Doe");
			PersonRepository.Has(person);
			var changedAt = new DateTime(2016, 8, 1, 10, 0, 0);
			var scheduleStart = new DateTime(2016, 8, 23, 8, 0, 0);
			var scheduleEnd = new DateTime(2016, 8, 23, 17, 0, 0);
			createAuditingData(person, changedAt, scheduleStart, scheduleEnd);

			var searchParam = new AuditTrailSearchParams()
			{
				ChangedByPersonId = Guid.Empty,
				ChangesOccurredStartDate = new DateTime(2016, 8, 1),
				ChangesOccurredEndDate = new DateTime(2016, 8, 1),
				AffectedPeriodStartDate = new DateTime(2016, 8, 23),
				AffectedPeriodEndDate = new DateTime(2016, 8, 23),
				MaximumResults = 1
			};
			var vm = Target.Provide(searchParam);

			vm.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnGivenNumberOfResults()
		{
			var person = PersonFactory.CreatePersonWithGuid("Joe", "Doe");
			PersonRepository.Has(person);
			var changedAt = new DateTime(2016, 8, 1, 10, 0, 0);
			var scheduleStart = new DateTime(2016, 8, 23, 8, 0, 0);
			var scheduleEnd = new DateTime(2016, 8, 23, 17, 0, 0);
			createAuditingData(person, changedAt, scheduleStart, scheduleEnd);
			createAuditingData(person, changedAt, scheduleStart, scheduleEnd);

			var searchParam = new AuditTrailSearchParams()
			{
				ChangedByPersonId = Guid.Empty,
				ChangesOccurredStartDate = new DateTime(2016, 8, 1),
				ChangesOccurredEndDate = new DateTime(2016, 8, 1),
				AffectedPeriodStartDate = new DateTime(2016, 8, 23),
				AffectedPeriodEndDate = new DateTime(2016, 8, 23),
				MaximumResults = 1
			};
			var vm = Target.Provide(searchParam);

			vm.Count.Should().Be.EqualTo(1);
		}

		private ScheduleAuditingReportData createAuditingData(IPerson person, DateTime changedAt, DateTime scheduleStart, DateTime scheduleEnd)
		{
			ScheduleAuditingReportData auditTrailData = new ScheduleAuditingReportData()
			{
				ModifiedBy = person.Id.Value.ToString(),
				ModifiedAt = changedAt,
				AuditType = "New",
				Detail = "OverTime",
				ScheduledAgent = "John",
				ScheduleStart = scheduleStart,
				ScheduleEnd = scheduleEnd,
				ShiftType = "Shift"
			};
			FakeScheduleAuditTrailReport.Has(auditTrailData);
			return auditTrailData;
		}
	}
}
