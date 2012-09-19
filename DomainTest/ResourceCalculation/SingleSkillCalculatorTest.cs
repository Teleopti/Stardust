using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class SingleSkillCalculatorTest
	{
		private ISingleSkillCalculator _target;

		[SetUp]
		public void Setup()
		{
			_target = new SingleSkillCalculator();
		}

		[Test]
		public void ShouldCalculateIfPersonSkill()
		{
			_target.Calculate();
		}
	}
}