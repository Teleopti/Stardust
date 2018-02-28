using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.Rta.ViewModels.HistoricalAdherenceViewModelBuilder
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class BuildTest
	{
		public Domain.Rta.ViewModels.HistoricalAdherenceViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldGetPerson()
		{
			Now.Is("2016-10-10 15:00");
			var person = Guid.NewGuid();
			var name = RandomName.Make();
			Database.WithAgent(person, name);

			var data = Target.Build(person);

			data.PersonId.Should().Be(person);
			data.AgentName.Should().Be(name);
		}

		[Test]
		public void ShouldGetForCorrectDate()
		{
			Now.Is("2016-10-10 15:00");
			var person = Guid.NewGuid();

			Database
				.WithAgent(person, "name");

			Database
				.WithAdherenceOut(person, "2016-10-08 00:00")
				.WithAdherenceOut(person, "2016-10-10 00:00");

			var data = Target.Build(person);

			data.PersonId.Should().Be(person);
			data.Now.Should().Be("2016-10-10T15:00:00");
			data.OutOfAdherences.Should().Have.Count.EqualTo(1);
		}

	}
}