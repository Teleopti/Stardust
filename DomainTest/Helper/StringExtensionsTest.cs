using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.DomainTest.Helper
{
	[TestFixture]
	public class StringExtensionsTest
	{
		[Test]
		public void ShouldContainsExact()
		{
			"apa".ContainsIgnoreCase("apa")
				.Should().Be.True();
		}

		[Test]
		public void ShouldContainsPartOfIgnoreCase()
		{
			"en liten aPa".ContainsIgnoreCase("apa")
				.Should().Be.True();
		}

		[Test]
		public void ShouldNotContain()
		{
			"en liten gris".ContainsIgnoreCase("apa")
				.Should().Be.False();
		}

		[Test]
		public void CompareToNullShouldReturnFalse()
		{
			"en liten gris".ContainsIgnoreCase(null)
				.Should().Be.False();
		}

		[Test]
		public void CompareFromNullShouldReturnFalse()
		{
			((string)null).ContainsIgnoreCase("apa")
				.Should().Be.False();
		}

		[Test]
		public void CompareFromAndToNullShouldReturnTrue()
		{
			((string)null).ContainsIgnoreCase(null)
					.Should().Be.True();
		}
	}
}