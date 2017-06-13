using NUnit.Framework;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Aop
{
	[DomainTest]
	[ThrowIfRepositoriesAreUsed]
	public class ThrowIfRepositoriesAreUsedOnTestFixtureTest
	{
		public IPersonRepository SomeRepository;

		[Test]
		public void ShouldThrowRepositoriesMustNotBeUsedException()
		{
			Assert.Throws<RepositoriesMustNotBeUsedException>(() =>
			{
				SomeRepository.LoadAll();
			});
		}
	}
}