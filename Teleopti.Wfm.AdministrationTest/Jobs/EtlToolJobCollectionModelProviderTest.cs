using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Wfm.Administration.Core.EtlTool;

namespace Teleopti.Wfm.AdministrationTest.Jobs
{
	[AdministrationTest]
	public class EtlToolJobCollectionModelProviderTest
	{
		public EtlToolJobCollectionModelProvider Target;

		[Test, Ignore("under construction")]
		public void ShouldReturnJobCollection()
		{
			var result = Target.Create();
			result.Count.Should().Be.GreaterThanOrEqualTo(13);
		}


	}
}
