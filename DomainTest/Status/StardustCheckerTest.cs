using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Status;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Status
{
	[DomainTest]
	public class StardustCheckerTest
	{
		public StardustChecker Target;
		public FakeGetAllWorkerNodes GetAllWorkerNodes;

		[Test]
		public void ShouldReturnExpectedString()
		{
			GetAllWorkerNodes.Has(true);
			GetAllWorkerNodes.Has(false);
			GetAllWorkerNodes.Has(true);
			GetAllWorkerNodes.Has(true);

			Target.Execute().Output
				.Should().Be.EqualTo(string.Format(Target.Output, 4, 3));
		}

		[Test]
		public void ShouldSucceedIfAnyNodeIsAlive()
		{
			GetAllWorkerNodes.Has(false);
			GetAllWorkerNodes.Has(false);
			GetAllWorkerNodes.Has(true);
			GetAllWorkerNodes.Has(false);
			
			Target.Execute().Success
				.Should().Be.True();
		}

		[Test]
		public void ShouldFailIfNoNodeIsAlive()
		{
			GetAllWorkerNodes.Has(false);
			GetAllWorkerNodes.Has(false);

			Target.Execute().Success
				.Should().Be.False();
		}
	}
}