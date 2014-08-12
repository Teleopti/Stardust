using NUnit.Framework;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Rta;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Rta
{
    [TestFixture]
	public class BusinessUnitInfoFinderTest
    {
		[Test]
		public void ShouldHaveWorkingConstructor()
		{
			var result = new BusinessUnitStarter(null);
			Assert.IsNotNull(result);
		}
    }
}
