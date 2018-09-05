using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Configuration;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Wfm.Rta.Test.Domain.Configuration
{
	[RtaTest]
	public class LoggedOutStateGroupMissingInRtaServiceTest
	{
		public FakeDatabase Database;
		public ConfigurationValidator Target;

		[Test]
		public void ShouldReturnMessageWhenInvalid()
		{
			Database
				.WithStateGroup(null, "some group", false, false)
				.WithStateCode("some code")
				;

			var result = Target.Validate();

			var expected = nameof(Ccc.UserTexts.Resources.LoggedOutStateGroupMissingInRtaService);
			result.Single(x => x.Resource == expected)
				.Should().Not.Be.Null();
		}


		[Test]
		public void ShouldNotReturnMessageWhenValid()
		{
			Database
				.WithStateGroup(null, "some group", false, true)
				.WithStateCode("some code")
				;

			var result = Target.Validate();

			var expected = nameof(Ccc.UserTexts.Resources.LoggedOutStateGroupMissingInRtaService);
			result.Select(x => x.Resource)
				.Should().Not.Contain(expected);
		}
	}
}