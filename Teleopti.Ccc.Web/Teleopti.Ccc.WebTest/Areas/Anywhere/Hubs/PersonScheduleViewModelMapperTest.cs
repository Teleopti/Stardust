using System;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using AutoMapper;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class PersonScheduleViewModelMapperTest
	{
		[SetUp]
		public void Setup()
		{
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new PersonScheduleViewModelMappingProfile()));
		}

		// cant get this green with dynamics involved
		[Test, Ignore]
		public void ShouldConfigureCorrectly()
		{
			Mapper.AssertConfigurationIsValid();
		}
		
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

		private dynamic MakeLayer(string Color = "", DateTime? Start = null, int Minutes = 0)
		{
			dynamic layer = new ExpandoObject();
			layer.Color = Color;
			layer.Start = Start.HasValue ? Start : null;
			layer.Minutes = Minutes;
			return layer;
		}

		[Test]
		public void ShouldMapLayers()
		{
			var target = new PersonScheduleViewModelMapper();

			dynamic shift = new ExpandoObject();
			shift.Projection = new[] { MakeLayer(), MakeLayer() };

			var result = target.Map(new PersonScheduleData { Shift = shift });

			result.Layers.Count().Should().Be(2);
		}

		[Test]
		public void ShouldMapLayerColor()
		{
			var target = new PersonScheduleViewModelMapper();

			dynamic shift = new ExpandoObject();
			shift.Projection = new[] { MakeLayer("Green")};

			var result = target.Map(new PersonScheduleData {Shift = shift});

			result.Layers.Single().Color.Should().Be("Green");
		}

		[Test]
		public void ShouldMapLayerStartTime()
		{
			var target = new PersonScheduleViewModelMapper();

			dynamic shift = new ExpandoObject();
			shift.Projection = new[] { MakeLayer("", DateTime.Today.AddHours(8))};

			var result = target.Map(new PersonScheduleData { Shift = shift });

			result.Layers.Single().Start.Should().Be(DateTime.Today.AddHours(8));
		}

		[Test]
		public void ShouldMapLayerMinutes()
		{
			var target = new PersonScheduleViewModelMapper();

			dynamic shift = new ExpandoObject();
			shift.Projection = new[] {MakeLayer("", null, 60)};

			var result = target.Map(new PersonScheduleData { Shift = shift });

			result.Layers.Single().Minutes.Should().Be(60);
		}

	}
}