using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest
{
	[SetUpFixture]
	[Parallelizable]
	public class OneTimeSetUp
	{
		[OneTimeSetUp]
		public void Setup()
		{
			CurrentAuthorization.DefaultTo(new FullPermission());
		}
	}
}
