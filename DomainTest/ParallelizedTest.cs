using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest
{
	[TestFixture]
	[Parallelizable(ParallelScope.Fixtures)]
	[DomainTest]
	public class ParallelizedTest
	{
		[Test]
		public void ShouldThrowIfServiceLocatorIsAccessedFromParallelizedTest()
		{
			Assert.Throws<ServiceLocatorNotAllowedException>(() => { ServiceLocator_DONTUSE.Now.UtcDateTime(); });
		}
		
		[Test]
		public void ShouldBeAbleToAccessPersonCulture()
		{
			new Person().PermissionInformation.Culture();
			new Person().PermissionInformation.UICulture();
		}
	}
}