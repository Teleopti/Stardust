using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting.Import;

namespace Teleopti.Ccc.DomainTest.Forecasting.Import
{
	[TestFixture]
	public class IsColumnCountInRowValidTest
	{
		private IsColumnCountInRowValid _target;

		[SetUp]
		public void Setup()
		{
			_target = new IsColumnCountInRowValid();
		}

		[Test]
		public void ShouldCheckIfColumnCountIsValidOrNot()
		{
			Assert.That(_target.IsSatisfiedBy(new[] {"a", "a", "a", "a", "a"}), Is.False);
			Assert.That(_target.IsSatisfiedBy(new[] {"a", "a", "a", "a", "a", "a"}), Is.True);
			Assert.That(_target.IsSatisfiedBy(new[] {"a", "a", "a", "a", "a", "a", "a"}), Is.True);
			Assert.That(_target.IsSatisfiedBy(new[] { "a", "a", "a", "a", "a", "a", "a", "a" }), Is.False);
		}

	}
}
