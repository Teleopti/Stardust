using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
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

			var result = Target.For(new[] { phoneId }).Single();

			result.Id.Should().Be(phoneId);
			result.Name.Should().Be("phone");
		}

		[Test]
		public void ShouldOnlyBuildForAskedStateGroup()
		{
			var phoneId = Guid.NewGuid();
			Database
				.WithStateGroup(phoneId, "phone")
				.WithStateGroup(Guid.NewGuid(), "someOtherStateGroup");

			var result = Target.For(new[] { phoneId }).Single();

			result.Id.Should().Be(phoneId);
			result.Name.Should().Be("phone");
		}

	}
}
