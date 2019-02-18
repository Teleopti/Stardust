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
	public class OrganizationSelectionProviderTest : IIsolateSystem
	{
		public OrganizationSelectionProvider Target;
		public FakeCurrentBusinessUnit CurrentBusinessUnit;
		public ISiteRepository SiteRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeCurrentBusinessUnit>().For<ICurrentBusinessUnit>();
		}

		[Test]
		public void ShouldReturnDynamicOptions()
		{
			CurrentBusinessUnit.FakeBusinessUnit(BusinessUnitUsedInTests.BusinessUnit);
			var result = Target.Provide();
			result.DynamicOptions.Length.Should()
				.Be.EqualTo(Enum.GetNames(typeof(AvailableDataRangeOption)).Length);
		}
		
		[Test]
		public void ShouldReturnOrganizationStructureForCurrentBusinessUnit()
		{
			CurrentBusinessUnit.FakeBusinessUnit(BusinessUnitUsedInTests.BusinessUnit);
			SiteRepository.Add(SiteFactory.CreateSiteWithOneTeam("Team 1"));
			var result = Target.Provide();
			((Guid)result.BusinessUnit.Id).Should().Be.EqualTo(BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault());
			result.BusinessUnit.ChildNodes.Length.Should().Be.EqualTo(1);
			result.BusinessUnit.ChildNodes.First().ChildNodes.Length.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldIgnoreDeletedTeam()
		{
			CurrentBusinessUnit.FakeBusinessUnit(BusinessUnitUsedInTests.BusinessUnit);
			var siteWithOneTeam = SiteFactory.CreateSiteWithOneTeam("Team 1");

			((IDeleteTag)siteWithOneTeam.TeamCollection[0]).SetDeleted();
			SiteRepository.Add(siteWithOneTeam);
			var result = Target.Provide();
			result.BusinessUnit.ChildNodes.First().ChildNodes.Length.Should().Be.EqualTo(0);
		}
	}
}
