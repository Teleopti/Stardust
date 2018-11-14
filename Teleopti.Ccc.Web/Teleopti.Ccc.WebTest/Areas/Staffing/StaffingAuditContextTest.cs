using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.WebTest.Areas.Staffing
{
	[DomainTest]
	public class StaffingAuditContextTest : IIsolateSystem
	{
		private StaffingAuditContext _target;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeStaffingAuditRepository StaffingAuditRepository;
		public MutableNow Now;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}
		

		[Test]
		public void ShouldPersistUserInfoOnClearStaffingAction()
		{
			IPerson person = PersonFactory.CreatePersonWithGuid("Ashley", "Aaron");
			LoggedOnUser.SetFakeLoggedOnUser(person);
			_target = new StaffingAuditContext(StaffingAuditRepository, LoggedOnUser, Now);
			_target.Handle(new ClearBpoActionObj(){BpoGuid = Guid.NewGuid(), EndDate = DateTime.Today, StartDate = DateTime.Today.AddDays(-1)});
			StaffingAuditRepository.StaffingAuditList.First().ActionPerformedById.Should().Be
				.EqualTo(person.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldPersistActionNameOnClearStaffingAction()
		{
			IPerson person = PersonFactory.CreatePersonWithGuid("Ashley", "Aaron");
			LoggedOnUser.SetFakeLoggedOnUser(person);
			_target = new StaffingAuditContext(StaffingAuditRepository, LoggedOnUser, Now);
			_target.Handle(new ClearBpoActionObj() { BpoGuid = Guid.NewGuid(), EndDate = DateTime.Today, StartDate = DateTime.Today.AddDays(-1) });
			StaffingAuditRepository.StaffingAuditList.First().Action.Should().Be.EqualTo("ClearBpoStaffing");
		}

		[Test]
		public void ShouldPersistAllFieldsOnClearStaffingAction()
		{
			IPerson person = PersonFactory.CreatePersonWithGuid("Ashley", "Aaron");
			LoggedOnUser.SetFakeLoggedOnUser(person);
			_target = new StaffingAuditContext(StaffingAuditRepository, LoggedOnUser, Now);
			Now.Is(new DateTime(2018,10,09,10,10,10));
			var clearBpoAction = new ClearBpoActionObj()
			{
				BpoGuid = Guid.NewGuid(),
				EndDate = DateTime.Today,
				StartDate = DateTime.Today.AddDays(-1)
			};

			_target.Handle(clearBpoAction);

			var staffingAuditLog = StaffingAuditRepository.StaffingAuditList.First();
			staffingAuditLog.BpoId.Should().Be.EqualTo(clearBpoAction.BpoGuid);
			staffingAuditLog.ClearPeriodStart.Should().Be.EqualTo(clearBpoAction.StartDate);
			staffingAuditLog.ClearPeriodEnd.Should().Be.EqualTo(clearBpoAction.EndDate);
			staffingAuditLog.TimeStamp.Should().Be.EqualTo(new DateTime(2018, 10, 09, 10, 10, 10));
		}


		[Test]
		public void ShouldPersistAllFieldsOnBpoImportAction()
		{
			IPerson person = PersonFactory.CreatePersonWithGuid("Ashley", "Aaron");
			LoggedOnUser.SetFakeLoggedOnUser(person);
			_target = new StaffingAuditContext(StaffingAuditRepository, LoggedOnUser, Now);
			Now.Is(new DateTime(2018, 10, 09, 10, 10, 20));
			var importBpoAction = new ImportBpoActionObj()
			{
				FileContent = "I am content",
				FileName = "Import_File_For_Telia.txt"
			};

			_target.Handle(importBpoAction);

			var staffingAuditLog = StaffingAuditRepository.StaffingAuditList.First();
			staffingAuditLog.ActionPerformedById.Should().Be.EqualTo(LoggedOnUser.CurrentUser().Id.GetValueOrDefault());
			staffingAuditLog.Action.Should().Be.EqualTo("ImportBpo");
			staffingAuditLog.ImportFileName.Should().Be.EqualTo(importBpoAction.FileName);
			staffingAuditLog.TimeStamp.Should().Be.EqualTo(new DateTime(2018, 10, 09, 10, 10, 20));
		}
		
	}
	
}