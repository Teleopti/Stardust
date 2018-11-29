using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Infrastructure.Util;

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
			//Retry.Handle<Exception>()
			//	.WaitAndRetry()
			//	.Do(() => { audits.Count().Should().Be(1); });

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

		[Test]
		public void ShouldLoadAuditMatchingFileName()
		{
			var rep = new StaffingAuditRepository(CurrUnitOfWork);
			var staffingAudit = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "BPO", "filename1", "");
			var staffingAudit2 = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "BPO", "filename2", "");
			var staffingAudit3 = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "BPO", "xxxfilename2xxx", "");

			PersistAndRemoveFromUnitOfWork(staffingAudit);
			PersistAndRemoveFromUnitOfWork(staffingAudit2);
			PersistAndRemoveFromUnitOfWork(staffingAudit3);
			Thread.Sleep(10);

			var audits = rep.LoadAudits(LoggedOnPerson, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow, "filename2");
			audits.Count().Should().Be(2);
			audits.Should().Contain(staffingAudit2);
			audits.Should().Contain(staffingAudit3);
		}

		[Test]
		public void ShouldLoadAuditsWithoutSearchWord()
		{
			var rep = new StaffingAuditRepository(CurrUnitOfWork);
			var staffingAudit = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "BPO", "filename1", "");
			var staffingAudit2 = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "BPO", "filename2", "");

			PersistAndRemoveFromUnitOfWork(staffingAudit);
			PersistAndRemoveFromUnitOfWork(staffingAudit2);
			Thread.Sleep(10);
			var audits = rep.LoadAudits(LoggedOnPerson, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow);
			audits.Count().Should().Be(2);
		}

		[Test, Repeat(1000)]
		public void ShouldLoadAuditsMatchingBpoName()
		{
			
			var staffingAudit = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "", "filename1", "BPO1");
			var staffingAudit2 = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "", "filename1", "BPO2");
			var staffingAudit3 = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "", "filename1", "xxBPO2xx");

			PersistAndRemoveFromUnitOfWork(staffingAudit);
			PersistAndRemoveFromUnitOfWork(staffingAudit2);
			PersistAndRemoveFromUnitOfWork(staffingAudit3);

			var rep = new StaffingAuditRepository(CurrUnitOfWork);
			var audits = rep.LoadAudits(LoggedOnPerson, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow, "BPO2");
			audits.Count().Should().Be(2);
			audits.Should().Contain(staffingAudit2);
			audits.Should().Contain(staffingAudit3);
		}

		[Test]
		public void ShouldLoadAuditsMatchingBpoNameAndFilename()
		{
			var rep = new StaffingAuditRepository(CurrUnitOfWork);
			var staffingAudit = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "", "fileNameX", "BPO1");
			var staffingAudit2 = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "", "filename1", "BPOX");
			var staffingAudit3 = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "", "filename2", "BPO2");

			PersistAndRemoveFromUnitOfWork(staffingAudit);
			PersistAndRemoveFromUnitOfWork(staffingAudit2);
			PersistAndRemoveFromUnitOfWork(staffingAudit3);
			Thread.Sleep(10);
			var audits = rep.LoadAudits(LoggedOnPerson, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow, "X");
			audits.Count().Should().Be(2);
			audits.Should().Contain(staffingAudit);
			audits.Should().Contain(staffingAudit2);
		}

		[Test]
		public void ShouldLoadNoAuditsWithWrongSearchWord()
		{
			var rep = new StaffingAuditRepository(CurrUnitOfWork);
			var staffingAudit = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "", "fileName", "BPO");
			var staffingAudit2 = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "", "filename2", "BPO2");
			var staffingAudit3 = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "", "filename3", "BPO3");

			PersistAndRemoveFromUnitOfWork(staffingAudit);
			PersistAndRemoveFromUnitOfWork(staffingAudit2);
			PersistAndRemoveFromUnitOfWork(staffingAudit3);

			var audits = rep.LoadAudits(LoggedOnPerson, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow, "X");
			audits.Count().Should().Be(0);
		}
	}
}
