using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{

	[TestFixture]
	public class StaffingAuditRepositoryTest : RepositoryTest<IStaffingAudit>
	{
		protected override IStaffingAudit CreateAggregateWithCorrectBusinessUnit()
		{
			return new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing,"BPO","filename", "");
		}

		protected override void VerifyAggregateGraphProperties(IStaffingAudit loadedAggregateFromDatabase)
		{
			var org = CreateAggregateWithCorrectBusinessUnit();
			Assert.That(org.Action.Equals(loadedAggregateFromDatabase.Action));
		}

		protected override Repository<IStaffingAudit> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new StaffingAuditRepository(currentUnitOfWork);
		}

		[Test]
		public void ShouldLoadAudit()
		{
			var rep = new StaffingAuditRepository(CurrUnitOfWork);
			var staffingAudit = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "BPO", "filename", "");
			var staffingAudit2 = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "BPO", "filename", "");
			staffingAudit2.TimeStamp = DateTime.UtcNow.AddDays(-100);
			
			PersistAndRemoveFromUnitOfWork(staffingAudit);
			PersistAndRemoveFromUnitOfWork(staffingAudit2);

			var audits = rep.LoadAudits(LoggedOnPerson, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow);
			audits.Count().Should().Be(1);
		}

		[Test]
		public void ShouldPurgeOldAudits()
		{
			var rep = new StaffingAuditRepository(CurrUnitOfWork);
			var staffingAudit = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "BPO", "filename", "");
			var staffingAudit2 =
				new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "BPO", "filename", "")
				{
					TimeStamp = DateTime.UtcNow.AddDays(-50)
				};
			var staffingAudit3 =
				new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "BPO", "filename", "")
				{
					TimeStamp = DateTime.UtcNow.AddDays(-100)
				};

			PersistAndRemoveFromUnitOfWork(staffingAudit);
			PersistAndRemoveFromUnitOfWork(staffingAudit2);
			PersistAndRemoveFromUnitOfWork(staffingAudit3);
			var audits = rep.LoadAudits(LoggedOnPerson, DateTime.UtcNow.AddDays(-200), DateTime.UtcNow);
			audits.Count().Should().Be(3);

			rep.PurgeOldAudits(DateTime.UtcNow.AddDays(-60));

			audits = rep.LoadAudits(LoggedOnPerson, DateTime.UtcNow.AddDays(-200), DateTime.UtcNow);
			audits.Count().Should().Be(2);
		}
	}
}
