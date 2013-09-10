using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class SkillResolutionProviderTest
	{
		private ISkillResolutionProvider _target;

		[SetUp]
		public void Setup()
		{
			_target = new SkillResolutionProvider();
		}

		[Test]
		public void ShouldProvideMinimumResolution()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			skill1.DefaultResolution = 30;
			var skill2 = SkillFactory.CreateSkill("skill2");
			skill2.DefaultResolution = 15;

			var result = _target.MinimumResolution(new[] {skill1, skill2});

			Assert.That(result, Is.EqualTo(15));
		}
	}
}
