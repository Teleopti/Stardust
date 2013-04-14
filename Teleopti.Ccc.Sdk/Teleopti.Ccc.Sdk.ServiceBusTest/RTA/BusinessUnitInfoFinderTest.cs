using NUnit.Framework;
using Teleopti.Ccc.Sdk.ServiceBus.Rta;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Rta
{
    [TestFixture]
	public class BusinessUnitInfoFinderTest
    {
		[Test]
		public void ShouldHaveWorkingConstructor()
		{
			var result = new BusinessUnitInfoFinder(null);
			Assert.IsNotNull(result);
		}
    }
}
