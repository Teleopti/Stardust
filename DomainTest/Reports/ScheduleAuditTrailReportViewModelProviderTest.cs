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


namespace Teleopti.Ccc.DomainTest.Reports
{
	[DomainTest]
	public class ScheduleAuditTrailReportViewModelProviderTest
	{
		public ScheduleAuditTrailReportViewModelProvider Target;
		public FakeScheduleAuditTrailReport FakeScheduleAuditTrailReport;
		public FakePersonRepository PersonRepository;
		public FakeUserTimeZone TimeZone;
		public FakePersonFinderReadOnlyRepository PersonFinderReadOnlyRepository;

		private IPerson modifiedByPerson;
		private IPerson scheduledAgent;

		public void Setup(IIsolate isolate, IocConfiguration configuration)
		{
			isolate.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
		}

		[SetUp]
		public void Setup()
		{
			modifiedByPerson = PersonFactory.CreatePersonWithGuid("Joe", "Doe");
			scheduledAgent = PersonFactory.CreatePersonWithGuid("Ashley", "Andeen");
		}

		[Test]
		public void ShouldReturnViewModel()
		{
			PrepareFakeDb();
			var scheduledAgentNotInReport = PersonFactory.CreatePersonWithGuid("John", "Smith");
			PersonRepository.Has(scheduledAgentNotInReport);
			var changeDate= new DateTime(2016, 8, 1);
			var scheduleDate = new DateTime(2016, 8, 23);
			var auditTrailData = createAuditingData(changeDate, scheduleDate, scheduledAgent);
			createAuditingData(changeDate, scheduleDate, scheduledAgentNotInReport);

			var searchParam = AuditTrailSearchParams(modifiedByPerson, changeDate, scheduleDate, 100);
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

			PrepareFakeDb();
			var changedAtUtc = new DateTime(2016, 8, 1, 4, 0, 0);
			var scheduleStartUtc = new DateTime(2016, 8, 23, 2, 0, 0);
			var scheduleEndUtc = new DateTime(2016, 8, 23, 10, 0, 0);
			var auditTrailData = new ScheduleAuditingReportDataForTest()
			{
				ModifiedBy = modifiedByPerson.Id.Value.ToString(),
				ModifiedAt = changedAtUtc,
				AuditType = "New",
				Detail = "OverTime",
				ScheduledAgent = scheduledAgent.Name.ToString(),
				scheduleAgentId = scheduledAgent.Id.Value,
				ScheduleStart = scheduleStartUtc,
				ScheduleEnd = scheduleEndUtc,
				ShiftType = "Shift"
			};
			FakeScheduleAuditTrailReport.Has(auditTrailData);

			var searchParam = AuditTrailSearchParams(modifiedByPerson, new DateTime(2016, 7, 31), new DateTime(2016, 8, 22), 100);
			var vm = Target.Provide(searchParam);

			vm.Count.Should().Be.EqualTo(1);
			vm.First().ModifiedAt.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(changedAtUtc, TimeZone.TimeZone()));
			vm.First().ScheduleStart.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(scheduleStartUtc, TimeZone.TimeZone()));
			vm.First().ScheduleEnd.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(scheduleEndUtc, TimeZone.TimeZone()));
		}

		

		[Test]
		public void ShouldReturnViewModelForAllPersonsWhoChangeSchedule()
		{
			PrepareFakeDb();
			var changeDate = new DateTime(2016, 8, 1);
			var scheduleDate = new DateTime(2016, 8, 23);
			createAuditingData(changeDate, scheduleDate, scheduledAgent);

			var searchParam = AuditTrailSearchParams(null, changeDate, scheduleDate, 100);
			var vm = Target.Provide(searchParam);

			vm.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnGivenNumberOfResults()
		{
			PrepareFakeDb();
			var changeDate = new DateTime(2016, 8, 1);
			var scheduleDate = new DateTime(2016, 8, 23);
			createAuditingData(changeDate, scheduleDate, scheduledAgent);
			createAuditingData(changeDate, scheduleDate, scheduledAgent);

			var searchParam = AuditTrailSearchParams(modifiedByPerson, changeDate, scheduleDate, 1);
			var vm = Target.Provide(searchParam);

			vm.Count.Should().Be.EqualTo(1);
		}

		private ScheduleAuditingReportData createAuditingData(DateTime changeDate, DateTime scheduleDate, IPerson scheduledAgent)
		{
			var auditTrailData = new ScheduleAuditingReportDataForTest()
			{
				ModifiedBy = modifiedByPerson.Id.Value.ToString(),
				ModifiedAt = changeDate.AddHours(10),
				AuditType = "New",
				Detail = "OverTime",
				ScheduledAgent = scheduledAgent.Name.ToString(),
				scheduleAgentId = scheduledAgent.Id.Value,
				ScheduleStart = scheduleDate.AddHours(6),
				ScheduleEnd = scheduleDate.AddHours(11),
				ShiftType = "Shift"
			};
			FakeScheduleAuditTrailReport.Has(auditTrailData);
			return auditTrailData;
		}

		private void PrepareFakeDb()
		{
			PersonRepository.Has(modifiedByPerson);
			PersonRepository.Has(scheduledAgent);
			PersonFinderReadOnlyRepository.Has(scheduledAgent);
		}

		private AuditTrailSearchParams AuditTrailSearchParams(IPerson modifiedBy, DateTime changeDate, DateTime affectedDate, int maximumResults)
		{
			var searchParam = new AuditTrailSearchParams()
			{
				ChangedByPersonId = modifiedBy?.Id.Value ?? Guid.Empty,
				ChangesOccurredStartDate = changeDate,
				ChangesOccurredEndDate = changeDate,
				AffectedPeriodStartDate = affectedDate,
				AffectedPeriodEndDate = affectedDate,
				MaximumResults = maximumResults
			};
			return searchParam;
		}

	}
}
