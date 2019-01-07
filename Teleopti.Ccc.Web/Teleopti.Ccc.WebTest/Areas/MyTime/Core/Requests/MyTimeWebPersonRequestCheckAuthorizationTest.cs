using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Requests
{
	[TestFixture]
	public class MyTimeWebPersonRequestCheckAuthorizationTest
	{
		[Test]
		public void ShouldHaveCoverage()
		{
			var target = new SlimPersonRequestCheckAuthorization(new FullPermission());
			target.HasEditRequestPermission(null);
			target.HasViewRequestPermission(null);
			target.VerifyEditRequestPermission(null);
		}
	}
}
