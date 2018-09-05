﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Rta.Test.ApplicationLayer.ViewModels
{
	[TestFixture]
	[DomainTest]
	public class PhoneStateViewModelBuilderTest
	{
		public FakeDatabase Database;
		public PhoneStateViewModelBuilder Target;

		[Test]
		public void ShouldBuild()
		{
			var phoneId = Guid.NewGuid();
			Database
				.WithStateGroup(phoneId, "phone");

			var result = Target.Build().Single();

			result.Id.Should().Be(phoneId);
			result.Name.Should().Be("phone");
		}

		[Test]
		public void ShouldReturnAll()
		{
			var phoneId = Guid.NewGuid();
			Database
				.WithStateGroup(phoneId, "phone");

			var result = Target.Build();

			result.Single().Id.Should().Be(phoneId);
			result.Single().Name.Should().Be("phone");
		}
	}
}