using System;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Staffing
{
	[TestFixture]
	[DomainTest]
	[AllTogglesOn]
	public class PersonAccessContextReaderServiceTest : IIsolateSystem
	{
		public IPersonAccessAuditRepository PersonAccessAuditRepository;
		public PersonAccessContextReaderService Target;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeUserCulture UserCulture;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeUserCulture>().For<IUserCulture>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}

		[Test]
		public void ShouldLoadPersonAccessAuditContext()
		{
			dynamic role = new {RoleId = Guid.NewGuid(), Name = "Name"};
			PersonAccessAuditRepository.Add(
				new PersonAccess(PersonFactory.CreatePersonWithId(), 
					PersonFactory.CreatePersonWithId(), 
					PersonAuditActionType.GrantRole.ToString(), 
					PersonAuditActionResult.Change.ToString(),
					JsonConvert.SerializeObject(role)));
			var model = Target.LoadAll().Single();

			//model.Data.Should().Contain("Role = 1");
			//model.Data.Should().Contain("PersonAuditActionResult = X");
			//model.Data.Should().Contain("PersonAuditActionResult = Y");
		}
	}
}