using System;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class PersonScheduleViewModelMapperTest
	{
		[Test]
		public void ShouldMapPersonName()
		{
			var target = new PersonScheduleViewModelMapper();
			var person = new Person {Name = new Name("Pierra", "B")};

			var result = target.Map(new PersonScheduleData {Person = person});

			result.Name.Should().Be("Pierra B");
		}

		[Test]
		public void ShouldMapTeam()
		{
			var target = new PersonScheduleViewModelMapper();
			var team = new Team { Description = new Description("A-Team") };
			var person = new Person();
			person.AddPersonPeriod(new PersonPeriod(DateOnly.Today.AddDays(-10), PersonContractFactory.CreatePersonContract(), new Team()));
			person.AddPersonPeriod(new PersonPeriod(DateOnly.Today, PersonContractFactory.CreatePersonContract(), team));
			person.AddPersonPeriod(new PersonPeriod(DateOnly.Today.AddDays(10), PersonContractFactory.CreatePersonContract(), new Team()));

			var result = target.Map(new PersonScheduleData { Date = DateTime.Today, Person = person });

			result.Team.Should().Be("A-Team");
		}

		[Test]
		public void ShouldMapSite()
		{
			var target = new PersonScheduleViewModelMapper();
			var team = new Team
				{
					Site = new Site("Moon"),
					Description = new Description("A-Team")
				};
			var person = new Person();
			person.AddPersonPeriod(new PersonPeriod(DateOnly.Today, PersonContractFactory.CreatePersonContract(), team));

			var result = target.Map(new PersonScheduleData { Date = DateTime.Today, Person = person });

			result.Site.Should().Be("Moon");
		}

		[Test]
		public void ShouldMapLayerColor()
		{
			var target = new PersonScheduleViewModelMapper();

			dynamic layer1 = new ExpandoObject();
			layer1.Color = "Green";
			dynamic layer2 = new ExpandoObject();
			layer2.Color = "Yellow";
			dynamic shift = new ExpandoObject();
			shift.Test = "Value";
			shift.Projection = new[] {layer1, layer2};

			var result = target.Map(new PersonScheduleData {Shift = shift});

			result.Layers.First().Color.Should().Be("Green");
			result.Layers.ElementAt(1).Color.Should().Be("Yellow");
		}
	}
}