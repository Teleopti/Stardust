using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Permissions.Controllers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Permissions
{
	public class OrganizationSelectionControllerTest
	{
		[Test]
		public void ShouldReturnDynamicOptions()
		{
			var currentBusinessUnit = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();

			currentBusinessUnit.Stub(x => x.Current()).Return(BusinessUnitFactory.BusinessUnitUsedInTest);
			siteRepository.Stub(x => x.LoadAll()).Return(new List<ISite>());

			var target = new OrganizationSelectionController(currentBusinessUnit, siteRepository);
			dynamic result = target.GetOrganizationSelection();

			((ICollection<object>) result.DynamicOptions).Count.Should()
				.Be.EqualTo(Enum.GetNames(typeof (AvailableDataRangeOption)).Length);
		}

		[Test]
		public void ShouldReturnOrganizationStructureForCurrentBusinessUnit()
		{
			var currentBusinessUnit = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();

			currentBusinessUnit.Stub(x => x.Current()).Return(BusinessUnitFactory.BusinessUnitUsedInTest);
			siteRepository.Stub(x => x.LoadAll()).Return(new List<ISite> {SiteFactory.CreateSiteWithOneTeam("Team 1")});

			var target = new OrganizationSelectionController(currentBusinessUnit,siteRepository);
			dynamic result = target.GetOrganizationSelection();

			((Guid) result.BusinessUnit.Id).Should()
				.Be.EqualTo(BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault());

			((ICollection<object>)result.BusinessUnit.ChildNodes).Count.Should().Be.EqualTo(1);
			((ICollection<object>)(((ICollection<dynamic>)result.BusinessUnit.ChildNodes).First()).ChildNodes).Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldIgnoreDeletedTeam()
		{
			var currentBusinessUnit = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();

			currentBusinessUnit.Stub(x => x.Current()).Return(BusinessUnitFactory.BusinessUnitUsedInTest);
			var siteWithOneTeam = SiteFactory.CreateSiteWithOneTeam("Team 1");
			((IDeleteTag)siteWithOneTeam.TeamCollection[0]).SetDeleted();
			siteRepository.Stub(x => x.LoadAll()).Return(new List<ISite> { siteWithOneTeam });


			var target = new OrganizationSelectionController(currentBusinessUnit, siteRepository);
			dynamic result = target.GetOrganizationSelection();

			((ICollection<object>)(((ICollection<dynamic>)result.BusinessUnit.ChildNodes).First()).ChildNodes).Count.Should().Be.EqualTo(0);
		}
	}
}