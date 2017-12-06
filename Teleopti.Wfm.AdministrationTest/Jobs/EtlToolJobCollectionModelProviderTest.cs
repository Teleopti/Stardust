using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Wfm.Administration.Core.EtlTool;

namespace Teleopti.Wfm.AdministrationTest.Jobs
{
	[AdministrationTest]
	public class EtlToolJobCollectionModelProviderTest
	{
		public EtlToolJobCollectionModelProvider Target;
		public FakeTenants AllTenants;

		[Test]
		public void ShouldReturnJobCollection()
		{
			AllTenants.Has("Tenant");
			var result = Target.Create("Tenant");
			result.Count.Should().Be.GreaterThanOrEqualTo(13);
		}


	}
}
