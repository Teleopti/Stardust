using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	[TestFixture]
	public class BusinessUnitControllerTest
	{
		[Test]
		public void ShouldGetAllBusinessUnitsIfPermitted()
		{
			var bu1 = new BusinessUnit("bu1").WithId();
			var bu2 = new BusinessUnit("bu2").WithId();
			var buList = new List<IBusinessUnit> {bu1, bu2};
			var buRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();
			buRepository.Stub(x => x.LoadAllBusinessUnitSortedByName()).Return(buList);
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = MockRepository.GenerateMock<IPerson>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			person.Stub(x => x.PermissionInformation.HasAccessToAllBusinessUnits()).Return(true);

			var target = new BusinessUnitController(buRepository, loggedOnUser);

			var result = (target.Index().Data as IEnumerable<BusinessUnitViewModel>).ToList();

			result[0].Id.Should().Be(bu1.Id);
			result[0].Name.Should().Be(bu1.Name);
			result[1].Id.Should().Be(bu2.Id);
			result[1].Name.Should().Be(bu2.Name);
		}
		
		[Test]
		public void ShouldGetOnlyOwnBusinessUnitIfNotPermittedToAccessAll()
		{
			var bu1 = BusinessUnitFactory.BusinessUnitUsedInTest;
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = MockRepository.GenerateMock<IPerson>();
			var permissionInfo = new PermissionInformation(person);
			var applicationRole = new ApplicationRole();
			applicationRole.SetBusinessUnit(bu1);
			permissionInfo.AddApplicationRole(applicationRole);
			person.Stub(x => x.PermissionInformation).Return(permissionInfo);
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			var target = new BusinessUnitController(null, loggedOnUser);

			var result = target.Index().Data as IEnumerable<BusinessUnitViewModel>;

			result.Single().Id.Should().Be(bu1.Id);
			result.Single().Name.Should().Be(bu1.Name);
		}
	}
}
