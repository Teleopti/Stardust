using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	[TestFixture]
	public class BusinessUnitControllerTest
	{
		[Test]
		public void ShouldGetAllBusinessUnitsIfPermitted()
		{
			var buRepository = new FakeBusinessUnitRepository(null);
			var bu1 = new BusinessUnit("bu1").WithId();
			var bu2 = new BusinessUnit("bu2").WithId();
			buRepository.Add(bu1);
			buRepository.Add(bu2);
			var currentBusinessUnit = new SpecificBusinessUnit(bu1);
			
			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			person.PermissionInformation.ApplicationRoleCollection[0].AvailableData = new AvailableData {AvailableDataRange = AvailableDataRangeOption.Everyone};
			var loggedOnUser = new FakeLoggedOnUser(person);
			
			var target = new BusinessUnitController(buRepository, loggedOnUser, currentBusinessUnit);
			var result = target.Index().Result<List<BusinessUnitViewModel>>().ToList();

			result[0].Id.Should().Be(bu1.Id);
			result[0].Name.Should().Be(bu1.Name);
			result[1].Id.Should().Be(bu2.Id);
			result[1].Name.Should().Be(bu2.Name);
		}
		
		[Test]
		public void ShouldGetAllBusinessUnitsIfPermittedWithCurrentBusinessUnitAtFirstPosition()
		{
			var buRepository = new FakeBusinessUnitRepository(null);
			var bu1 = new BusinessUnit("bu1").WithId();
			var bu2 = new BusinessUnit("bu2").WithId();
			buRepository.Add(bu1);
			buRepository.Add(bu2);
			var currentBusinessUnit = new SpecificBusinessUnit(bu2);

			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			person.PermissionInformation.ApplicationRoleCollection[0].AvailableData = new AvailableData {AvailableDataRange = AvailableDataRangeOption.Everyone};
			var loggedOnUser = new FakeLoggedOnUser(person);

			var target = new BusinessUnitController(buRepository, loggedOnUser, currentBusinessUnit);
			var result = target.Index().Result<List<BusinessUnitViewModel>>().ToList();

			result[0].Id.Should().Be(bu2.Id);
			result[0].Name.Should().Be(bu2.Name);
			result[1].Id.Should().Be(bu1.Id);
			result[1].Name.Should().Be(bu1.Name);
		}
		
		[Test]
		public void ShouldGetOnlyOwnBusinessUnitIfNotPermittedToAccessAll()
		{
			var bu1 = BusinessUnitFactory.BusinessUnitUsedInTest;
			var person = PersonFactory.CreatePerson();
			var loggedOnUser = new FakeLoggedOnUser(person);
			var applicationRole = new ApplicationRole();
			applicationRole.SetBusinessUnit(bu1);
			person.PermissionInformation.AddApplicationRole(applicationRole);
			
			var target = new BusinessUnitController(null, loggedOnUser,null);
			var result = target.Index().Result<IEnumerable<BusinessUnitViewModel>>();

			result.Single().Id.Should().Be(bu1.Id);
			result.Single().Name.Should().Be(bu1.Name);
		}

		[Test]
		public void ShouldGetCurrentLoggedOnBusinessUnit()
		{
			var bu1 = BusinessUnitFactory.BusinessUnitUsedInTest;
			var buRepository = new FakeBusinessUnitRepository(null);
			buRepository.Add(bu1);
			var currentBusinessUnit = new SpecificBusinessUnit(bu1);

			var target = new BusinessUnitController(buRepository, null, currentBusinessUnit);
			var result = target.Current().Result<BusinessUnitViewModel>();

			result.Id.Should().Be(bu1.Id);
			result.Name.Should().Be(bu1.Name);
		}
	}
}
