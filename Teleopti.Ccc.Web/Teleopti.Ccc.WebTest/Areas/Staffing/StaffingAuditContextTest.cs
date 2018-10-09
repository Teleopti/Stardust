using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Staffing;
using Teleopti.Ccc.Web.Areas.Staffing.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Staffing
{
	[DomainTest]
	public class StaffingAuditContextTest : IIsolateSystem
	{
		private StaffingAuditContext _target;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeStaffingAuditRepository StaffingAuditRepository;
		public MutableNow Now;

		[Test]
		public void ShouldPersistUserInfoOnClearStaffingAction()
		{
			IPerson person = PersonFactory.CreatePersonWithGuid("Ashley", "Aaron");
			LoggedOnUser.SetFakeLoggedOnUser(person);
			_target = new StaffingAuditContext(StaffingAuditRepository, LoggedOnUser, Now);
			_target.Handle(new ClearBpoActionObj(){BpoGuid = Guid.NewGuid(), EndDate = DateTime.Today, StartDate = DateTime.Today.AddDays(-1)});
			StaffingAuditRepository.StaffingAuditList.First().ActionPerformedBy.Id.GetValueOrDefault().Should().Be
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
		public void ShouldPersisAllFieldsOnClearStaffingAction()
		{
			IPerson person = PersonFactory.CreatePersonWithGuid("Ashley", "Aaron");
			LoggedOnUser.SetFakeLoggedOnUser(person);
			_target = new StaffingAuditContext(StaffingAuditRepository, LoggedOnUser, Now);
			Now.Is(new DateTime(2018,10,09,10,10,10));
			_target.Handle(new ClearBpoActionObj() { BpoGuid = Guid.NewGuid(), EndDate = DateTime.Today, StartDate = DateTime.Today.AddDays(-1) });

			StaffingAuditRepository.StaffingAuditList.First().ActionResult.Should().Be.EqualTo("Success");
			StaffingAuditRepository.StaffingAuditList.First().Data.Should().Be.EqualTo(null);
			StaffingAuditRepository.StaffingAuditList.First().Correlation.Should().Be.EqualTo(null);
			StaffingAuditRepository.StaffingAuditList.First().TimeStamp.Should().Be.EqualTo(new DateTime(2018, 10, 09, 10, 10, 10));
		}


		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}
	}



	
}