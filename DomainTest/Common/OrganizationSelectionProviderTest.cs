using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Common
{
	[DomainTest]
	public class OrganizationSelectionProviderTest : ISetup
	{
		public OrganizationSelectionProvider Target;
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
			var result = Target.Provide();
			result.DynamicOptions.Length.Should()
				.Be.EqualTo(Enum.GetNames(typeof(AvailableDataRangeOption)).Length);
		}
		
		[Test]
		public void ShouldReturnOrganizationStructureForCurrentBusinessUnit()
		{
			CurrentBusinessUnit.FakeBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			SiteRepository.Add(SiteFactory.CreateSiteWithOneTeam("Team 1"));
			var result = Target.Provide();
			((Guid)result.BusinessUnit.Id).Should().Be.EqualTo(BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault());
			result.BusinessUnit.ChildNodes.Length.Should().Be.EqualTo(1);
			result.BusinessUnit.ChildNodes.First().ChildNodes.Length.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldIgnoreDeletedTeam()
		{
			CurrentBusinessUnit.FakeBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			var siteWithOneTeam = SiteFactory.CreateSiteWithOneTeam("Team 1");

			((IDeleteTag)siteWithOneTeam.TeamCollection[0]).SetDeleted();
			SiteRepository.Add(siteWithOneTeam);
			var result = Target.Provide();
			result.BusinessUnit.ChildNodes.First().ChildNodes.Length.Should().Be.EqualTo(0);
		}
	}
}
