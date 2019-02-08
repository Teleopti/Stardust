using NUnit.Framework;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Aop
{
	[DomainTest]
	[ThrowIfRepositoriesAreUsed]
	[NoDefaultData]
	public class ThrowIfRepositoriesAreUsedOnTestFixtureTest
	{
		public IPersonRepository SomeRepository;

		[Test]
		public void ShouldThrowRepositoriesMustNotBeUsedException()
		{
			Assert.Throws<MustNotBeUsedException>(() =>
			{
				SomeRepository.LoadAll();
			});
		}
	}
}