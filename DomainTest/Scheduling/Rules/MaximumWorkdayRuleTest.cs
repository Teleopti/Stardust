using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	public class MaximumWorkdayRuleTest
	{
		private MaximumWorkdayRule _target;

		[SetUp]
		public void Setup()
		{
			_target = new MaximumWorkdayRule();
		}

		[Test]
		public void ShouldCreateRuleAndAccessSimpleProperties()
		{
			Assert.IsNotNull(_target);
			Assert.IsTrue(_target.Configurable);
			Assert.IsFalse(_target.IsMandatory);
			Assert.IsTrue(_target.HaltModify);
			_target.HaltModify = false;
			Assert.IsFalse(_target.HaltModify);
			Assert.IsTrue(_target.Description == Resources.DescriptionOfMaximumWorkdayRule);
		}
	}
}
