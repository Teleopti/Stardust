using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Permissions.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Permissions
{
	[PermissionsTest]
	public class OrganizationSelectionControllerTest : ISetup
	{
		public OrganizationSelectionController Target;
		public FakeCurrentBusinessUnit CurrentBusinessUnit;
		public ISiteRepository SiteRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeCurrentBusinessUnit>().For<ICurrentBusinessUnit>();
		}

		[Test]
		public void ShouldReturnDynamicOptions()
		{
			CurrentBusinessUnit.FakeBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			dynamic result = Target.GetOrganizationSelection();
			((ICollection<object>) result.DynamicOptions).Count.Should()
				.Be.EqualTo(Enum.GetNames(typeof (AvailableDataRangeOption)).Length);
		}

		[Test]
		public void ShouldReturnOrganizationStructureForCurrentBusinessUnit()
		{
			CurrentBusinessUnit.FakeBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			SiteRepository.Add(SiteFactory.CreateSiteWithOneTeam("Team 1"));
			dynamic result = Target.GetOrganizationSelection();
			((Guid) result.BusinessUnit.Id).Should().Be.EqualTo(BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault());
			((ICollection<object>)result.BusinessUnit.ChildNodes).Count.Should().Be.EqualTo(1);
			((ICollection<object>)(((ICollection<dynamic>)result.BusinessUnit.ChildNodes).First()).ChildNodes).Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldIgnoreDeletedTeam()
		{
			CurrentBusinessUnit.FakeBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			var siteWithOneTeam = SiteFactory.CreateSiteWithOneTeam("Team 1");
			
			((IDeleteTag)siteWithOneTeam.TeamCollection[0]).SetDeleted();
			SiteRepository.Add(siteWithOneTeam);
			dynamic result = Target.GetOrganizationSelection();
			((ICollection<object>)(((ICollection<dynamic>)result.BusinessUnit.ChildNodes).First()).ChildNodes).Count.Should().Be.EqualTo(0);
		}

	}
}