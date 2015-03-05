using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	[RtaTest]
	public class UnrecognizedStateTest
	{
		public FakeRtaDatabase Database;
		public IRta Target;

		[Test]
		public void ShouldAddStateCodeToDatabaseWhenNotRecognized()
		{
			var businesUnitId = Guid.NewGuid();
			Database
				.WithBusinessUnit(businesUnitId)
				.WithDefaultStateGroup()
				.WithUser("usercode");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "newStateCode"
			});

			Database.AddedStateCode.StateCode.Should().Be("newStateCode");
		}

		[Test, Ignore]
		public void ShouldUseDefaultStateGroupIfStateCodeIsNotRecognized()
		{
			Assert.Fail();
		}

		[Test]
		public void ShouldAddStateCodeWithDescription()
		{
			var businesUnitId = Guid.NewGuid();
			Database
				.WithBusinessUnit(businesUnitId)
				.WithDefaultStateGroup()
				.WithUser("usercode");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "newStateCode",
				StateDescription = "a new description"
			});

			Database.AddedStateCode.Name.Should().Be("a new description");
		}

	}
}