using NUnit.Framework;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Aop
{
	[DomainTest]
	[NoDefaultData]
	public class ThrowIfRepositoriesAreUsedOnTestMethodTest
	{
		public IPersonRepository SomeRepository;

		[Test]
		[ThrowIfRepositoriesAreUsed]
		public void ShouldThrowRepositoriesMustNotBeUsedException()
		{
			Assert.Throws<MustNotBeUsedException>(() =>
			{
				SomeRepository.LoadAll();
			});
		}
	}
}