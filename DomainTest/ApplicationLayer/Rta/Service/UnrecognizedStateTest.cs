using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class UnrecognizedStateTest
	{
		public FakeRtaDatabase Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

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

		[Test]
		public void ShouldNotAddStateCodeIfNotStateReceived()
		{
			var businesUnitId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Database
				.WithBusinessUnit(businesUnitId)
				.WithDefaultStateGroup()
				.WithUser("usercode", personId);

			Target.CheckForActivityChange(personId, businesUnitId);

			Database.AddedStateCode.Should().Be.Null();
		}

		[Test]
		public void ShouldAddStateCodeAsNameWhenCheckingForActivityChange()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId)
				.WithDefaultStateGroup()
				.WithUser("usercode", personId, businessUnitId)
				.WithExistingState(personId, "statecode");

			Target.CheckForActivityChange(personId, businessUnitId);

			Database.AddedStateCode.Name.Should().Be("statecode");
		}
	}
}