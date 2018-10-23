using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Staffing
{
	[TestFixture]
	[DomainTest]
	[AllTogglesOn]
	public class PersonAccessContextReaderServiceTest //: IIsolateSystem
	{
		public IPersonAccessAuditRepository PersonAccessAuditRepository;
		public PersonAccessContextReaderService Target;
		public FakeApplicationRoleRepository ApplicationRoleRepository;
		

		//public void Isolate(IIsolate isolate)
		//{
		//	isolate.UseTestDouble<FakeUserCulture>().For<IUserCulture>();
		//	isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		//}

		[Test]
		public void ShouldLoadPersonAccessAuditContext()
		{
			var appRole = ApplicationRoleFactory.CreateRole("Superman", "The man").WithId();
			ApplicationRoleRepository.Add(appRole);
			dynamic role = new {RoleId = appRole.Id.GetValueOrDefault(), Name = appRole.DescriptionText};

			PersonAccessAuditRepository.Add(
				new PersonAccess(PersonFactory.CreatePersonWithId(), 
					PersonFactory.CreatePersonWithId(), 
					PersonAuditActionType.GrantRole.ToString(), 
					PersonAuditActionResult.Change.ToString(),
					JsonConvert.SerializeObject(role)));
			var model = Target.LoadAll().Single();

			model.Data.Should().Be.EqualTo("Role: Superman Action: GrantRole");
		}
	}
}