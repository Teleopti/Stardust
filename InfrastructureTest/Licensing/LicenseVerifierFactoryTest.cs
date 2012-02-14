using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Licensing;

namespace Teleopti.Ccc.InfrastructureTest.Licensing
{
	[TestFixture]
	public class LicenseVerifierFactoryTest
	{
		private ILicenseVerifierFactory target;

		[SetUp]
		public void Setup()
		{
			target = new LicenseVerifierFactory();
		}

		[Test]
		public void DummyTest()
		{
			//just a dummy test. Impl just needed to have an "entry point"
			Assert.IsNotNull(target.Create(null,null));
		}
	}
}