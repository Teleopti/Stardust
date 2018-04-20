﻿using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Configuration;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.Domain.Configuration
{
	[DomainTest]
	public class DefaultStateGroupMissingInConfigurationTest
	{
		public FakeDatabase Database;
		public ConfigurationValidator Target;

		[Test]
		public void ShouldReturnMessageWhenInvalid()
		{
			Database
				.WithBusinessUnit(null);

			var result = Target.Validate();

			result.Select(x => x.Resource)
				.Should().Contain(nameof(UserTexts.Resources.DefaultStateGroupMissingInConfiguration));
		}

		[Test]
		public void ShouldNotReturnMessageWhenValid()
		{
			Database
				.WithBusinessUnit(null)
				.WithStateGroup(null, "default", true, false);

			var result = Target.Validate();

			result.Select(x => x.Resource)
				.Should().Not.Contain(nameof(UserTexts.Resources.DefaultStateGroupMissingInConfiguration));
		}

		[Test]
		public void ShouldReturnMessageInvalid2()
		{
			Database
				.WithBusinessUnit(null)
				.WithStateGroup(null, "some group", false, false);

			var result = Target.Validate();

			result.Select(x => x.Resource)
				.Should().Contain(nameof(UserTexts.Resources.DefaultStateGroupMissingInConfiguration));
		}

		[Test]
		public void ShouldReturnMessageForEachBusinessUnit()
		{
			Database
				.WithBusinessUnit(null, "invalid business unit 1")
				.WithStateGroup(null, "some group", false, false)
				.WithBusinessUnit(null, "invalid business unit 2")
				.WithBusinessUnit(null, "valid business unit")
				.WithStateGroup(null, "default", true, false)
				;

			var result = Target.Validate();

			var resource = nameof(UserTexts.Resources.DefaultStateGroupMissingInConfiguration);
			result.FirstOrDefault(x => x.Resource == resource && x.Data.Single() == "invalid business unit 1")
				.Should().Not.Be.Null();
			result.FirstOrDefault(x => x.Resource == resource && x.Data.Single() == "invalid business unit 2")
				.Should().Not.Be.Null();
			result.FirstOrDefault(x => x.Resource == resource && x.Data.Single() == "valid business unit")
				.Should().Be.Null();
		}
	}
}