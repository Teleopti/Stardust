using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Licensing.Agreements;

namespace Teleopti.Ccc.InfrastructureTest.Licensing
{
	[TestFixture]
	public class AgreementProviderTest
	{
		private AgreementProvider _provider;

		[SetUp]
		public void Setup()
		{
			_provider = new AgreementProvider();
		}

		[Test]
		public void DefaultAgreementShouldBeTheFirstInTheList()
		{
			var agreements = _provider.GetAllAgreements();
			Assert.That(agreements[0].ResourceName, Is.EqualTo(AgreementProvider.DefaultAgreement));
		}
	}
}