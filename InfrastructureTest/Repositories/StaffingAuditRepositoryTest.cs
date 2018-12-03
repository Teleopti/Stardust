using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{

	[TestFixture]
	public class StaffingAuditRepositoryTest : RepositoryTest<IStaffingAudit>
	{
		private IStaffingAudit _staffingAudit;


		protected override IStaffingAudit CreateAggregateWithCorrectBusinessUnit()
		{
			_staffingAudit =
				new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "BPO", "filename", "");
			return _staffingAudit;
		}

		protected override void VerifyAggregateGraphProperties(IStaffingAudit loadedAggregateFromDatabase)
		{
			loadedAggregateFromDatabase.ActionPerformedById.Should().Be(_staffingAudit.ActionPerformedById);
			loadedAggregateFromDatabase.ActionPerformedBy.Should().Be(_staffingAudit.ActionPerformedBy);
			loadedAggregateFromDatabase.Action.Should().Be(_staffingAudit.Action);
			loadedAggregateFromDatabase.BpoName.Should().Be(_staffingAudit.BpoName);
			loadedAggregateFromDatabase.Area.Should().Be(_staffingAudit.Area);
			loadedAggregateFromDatabase.ImportFileName.Should().Be(_staffingAudit.ImportFileName);
		}

		protected override Repository<IStaffingAudit> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new StaffingAuditRepository(currentUnitOfWork);
		}

		[Test]
		public void ShouldLoadAuditsWithinPeriod()
		{
			var rep = new StaffingAuditRepository(CurrUnitOfWork);
			var staffingAudit = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "BPO", "filename", "");
			var staffingAudit2 = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "BPO", "filename", "");
			staffingAudit2.TimeStamp = DateTime.UtcNow.AddDays(-100);
			
			PersistAndRemoveFromUnitOfWork(staffingAudit);
			PersistAndRemoveFromUnitOfWork(staffingAudit2);
			
			var audits = rep.LoadAudits(LoggedOnPerson, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow);
			audits.Count().Should().Be(1);
			
			audits.First().Should().Be.EqualTo(staffingAudit);
			audits.First().Area.Should().Be("BPO");
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

		[Test]//, Repeat(1000)]
		public void ShouldLoadAuditsMatchingBpoName()
		{
			var staffingAudit = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "", "filename1", "BPO1");
			var staffingAudit2 = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "", "filename1", "BPO2");
			var staffingAudit3 = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "", "filename1", "xxBPO2xx");

			PersistAndRemoveFromUnitOfWork(staffingAudit);
			PersistAndRemoveFromUnitOfWork(staffingAudit2);
			PersistAndRemoveFromUnitOfWork(staffingAudit3);

			var rep = new StaffingAuditRepository(CurrUnitOfWork);

			var audits = rep.LoadAudits(LoggedOnPerson, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow, "x");
			audits.Count().Should().Be(1);
			audits.First().BpoName.Should().Be.EqualTo("xxBPO2xx");
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
		
	[Test]
		public void ShouldReturnSortedAuditsBasedOnTimestamp()
		{
			var now = new DateTime(2018, 11, 29, 3, 0, 0, DateTimeKind.Utc);
			var rep = new StaffingAuditRepository(CurrUnitOfWork);
			var staffingAudit1 = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "BPO", "filename", "");
			staffingAudit1.TimeStamp = new DateTime(2018,11,27,3,0,0,DateTimeKind.Utc);
			var staffingAudit2 =
				new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "BPO", "filename", "")
				{
					TimeStamp =  new DateTime(2018,11,26,3,0,0,DateTimeKind.Utc)
				};
			var staffingAudit3 =
				new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "BPO", "filename", "")
				{
					TimeStamp = new DateTime(2018, 11, 29, 3, 0, 0, DateTimeKind.Utc)
				};

			PersistAndRemoveFromUnitOfWork(staffingAudit1);
			PersistAndRemoveFromUnitOfWork(staffingAudit2);
			PersistAndRemoveFromUnitOfWork(staffingAudit3);
			var audits = rep.LoadAudits(LoggedOnPerson, now.AddDays(-5), now.AddDays(5)).ToList();
			audits.Count.Should().Be(3);

			audits.First().Should().Be(staffingAudit3);
			audits.Second().Should().Be(staffingAudit1);
			audits.Third().Should().Be(staffingAudit2);
		}
		
		[Test]
		public void ShouldOnlyReturnTop100()
		{
			var now = new DateTime(2018, 11, 29, 3, 0, 0, DateTimeKind.Utc);
			var rep = new StaffingAuditRepository(CurrUnitOfWork);
			foreach (var i in Enumerable.Range(0, 200))
			{
				var staffingAudit = CreateAggregateWithCorrectBusinessUnit();
				staffingAudit.TimeStamp = now;
				PersistAndRemoveFromUnitOfWork(staffingAudit);
			}

			var audits = rep.LoadAudits(LoggedOnPerson, now.AddDays(-200), now.AddDays(100));
			audits.Count().Should().Be(100);
		}

		[Test]
		public void ShouldLoadAuditFilterOnPerson()
		{
			var rep = new StaffingAuditRepository(CurrUnitOfWork);
			var person2 = PersonFactory.CreatePersonWithGuid("Kalle", "Anka");
			var staffingAudit = new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportStaffing, "BPO", "filename", "");
			var staffingAudit2 = new StaffingAudit(person2, StaffingAuditActionConstants.ImportStaffing, "BPO", "filename", "");

			PersistAndRemoveFromUnitOfWork(staffingAudit);
			PersistAndRemoveFromUnitOfWork(staffingAudit2);

			var audits = rep.LoadAudits(LoggedOnPerson, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow);
			audits.Count().Should().Be(1);
			audits.First().ActionPerformedById.Should().Be.EqualTo(LoggedOnPerson.Id.GetValueOrDefault());
		}
	}
}
