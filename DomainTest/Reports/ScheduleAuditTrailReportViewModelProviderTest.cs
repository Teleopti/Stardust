using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Reports.Controllers;

namespace Teleopti.Ccc.DomainTest.Reports
{
	[DomainTest]
	public class ScheduleAuditTrailReportViewModelProviderTest
	{
		public ScheduleAuditTrailReportViewModelProvider Target;
		public FakeScheduleAuditTrailReport FakeScheduleAuditTrailReport;

		[Test, Ignore("Under progress")]
		public void ShouldReturnViewModel()
		{
			ScheduleAuditingReportData auditTrailData = new ScheduleAuditingReportData()
			{
				ModifiedBy = "Ashley",
				ModifiedAt = new DateTime(2016, 8, 1),
				AuditType = "New",
				Detail = "OverTime",
				ScheduledAgent = "John",
				ScheduleStart = new DateTime(2016, 8, 23, 8, 0, 0),
				ScheduleEnd = new DateTime(2016, 8, 23, 17, 0, 0),
				ShiftType = "Shift"
			};
			FakeScheduleAuditTrailReport.Has(auditTrailData);
			var searchParam = new AuditTrailSearchParams()
			{
				ChangedByPersonId = Guid.NewGuid(),
				AffectedPeriodStartDate = new DateTime(2016,8,23),
				AffectedPeriodEndDate = new DateTime(2016,8,23),
				ChangesOccurredStartDate = new DateTime(2016,8,1),
				ChangesOccurredEndDate = new DateTime(2016,8,1)
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
	}
}
