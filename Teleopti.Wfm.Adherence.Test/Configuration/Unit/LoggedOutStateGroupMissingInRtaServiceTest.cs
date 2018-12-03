using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Wfm.Adherence.Domain.Configuration;
using Teleopti.Wfm.Adherence.Test.States.Unit.Service;

namespace Teleopti.Wfm.Adherence.Test.Configuration.Unit
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

			var expected = "LoggedOutStateGroupMissingInRtaService";
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

			var expected = "LoggedOutStateGroupMissingInRtaService";
			result.Select(x => x.Resource)
				.Should().Not.Contain(expected);
		}
	}
}