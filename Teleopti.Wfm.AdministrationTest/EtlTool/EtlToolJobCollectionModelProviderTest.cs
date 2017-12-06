using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Wfm.Administration.Core.EtlTool;

namespace Teleopti.Wfm.AdministrationTest.EtlTool
{
	[AdministrationTest]
	public class EtlToolJobCollectionModelProviderTest
	{
		public EtlToolJobCollectionModelProvider Target;
		public FakeTenants AllTenants;
		public FakeBaseConfigurationRepository BaseConfigurationRepository;
		


		[Test]
		public void ShouldReturnJobCollection()
		{
			BaseConfigurationRepository.SaveBaseConfiguration("myConnString", new BaseConfiguration(1053, 15, "UTC", false));
			AllTenants.Has("Tenant");
			var result = Target.Create("Tenant");
			result.Count.Should().Be.GreaterThanOrEqualTo(13);
		}


	}
}
