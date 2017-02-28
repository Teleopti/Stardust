using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.GroupPageCreator
{
	[TestFixture]
	public class GroupPageLightTest
	{

		private GroupPageLight _target;

		[SetUp]
		public void Setup()
		{
			_target = new GroupPageLight();
		}

		[Test]
		public void ShouldNotReturnAnyNullsForPropertiesWhenUsingDefaultConstructor()
		{
			Assert.IsNotNull(_target.Key);
			Assert.IsNotNull(_target.DisplayName);
		}
	}
}