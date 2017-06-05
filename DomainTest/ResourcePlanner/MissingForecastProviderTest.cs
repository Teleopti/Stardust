using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourcePlanner.Validation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourcePlanner
{
	[DomainTest]
	public class MissingForecastProviderTest : ISetup
	{
		public MissingForecastProvider Target;

		[Test]
		public void ShouldReturnMissingForecastForSkill()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			var missingForecast = Target.GetMissingForecast(range, new List<SkillMissingForecast>
			{
				new SkillMissingForecast
				{
					SkillName = "Direct Sales",
					SkillId = Guid.NewGuid(),
					Periods = new DateOnlyPeriod[] {}
				}
			});

			var first = missingForecast.First().MissingRanges[0];
			first.StartDate.Should().Be.EqualTo(range.StartDate.Date);
			first.EndDate.Should().Be.EqualTo(range.EndDate.Date);
		}

		[Test]
		public void ShouldReturnMissingForecastForStartOfMonth()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);

			var missingForecast = Target.GetMissingForecast(range, new List<SkillMissingForecast>
			{
				new SkillMissingForecast
				{
					SkillName = "Direct Sales",
					SkillId = Guid.NewGuid(),
					Periods = new[] { new DateOnlyPeriod(2015, 5, 11, 2015, 5, 31) }
				}
			});
			var first = missingForecast.First();

			first.MissingRanges.Length.Should().Be.EqualTo(1);
			first.MissingRanges[0].StartDate.Should().Be.EqualTo(range.StartDate.Date);
			first.MissingRanges[0].EndDate.Should().Be.EqualTo(new DateTime(2015, 5, 10));
		}

		[Test]
		public void ShouldReturnMissingForecastForEndOfMonth()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);

			var missingForecast = Target.GetMissingForecast(range, new List<SkillMissingForecast>
			{
				new SkillMissingForecast
				{
					SkillName = "Direct Sales",
					SkillId = Guid.NewGuid(),
					Periods = new[] { new DateOnlyPeriod(2015, 5, 1, 2015, 5, 21) }
				}
			});
			var first = missingForecast.First();

			first.MissingRanges.Length.Should().Be.EqualTo(1);
			first.MissingRanges[0].StartDate.Should().Be.EqualTo(new DateTime(2015, 5, 22));
			first.MissingRanges[0].EndDate.Should().Be.EqualTo(range.EndDate.Date);
		}

		[Test]
		public void ShouldReturnMissingForecastForStartAndEndOfMonth()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			var missingForecast = Target.GetMissingForecast(range, new List<SkillMissingForecast>
			{
				new SkillMissingForecast
				{
					SkillName = "Direct Sales",
					SkillId = Guid.NewGuid(),
					Periods = new[] { new DateOnlyPeriod(2015, 5, 11, 2015, 5, 19) }
				}
			});
			var first = missingForecast.First();

			first.MissingRanges.Length.Should().Be.EqualTo(2);
			first.MissingRanges[0].StartDate.Should().Be.EqualTo(range.StartDate.Date);
			first.MissingRanges[0].EndDate.Should().Be.EqualTo(new DateTime(2015, 5, 10));
			first.MissingRanges[1].StartDate.Should().Be.EqualTo(new DateTime(2015, 5, 20));
			first.MissingRanges[1].EndDate.Should().Be.EqualTo(range.EndDate.Date);
		}

		[Test]
		public void ShouldReturnMissingForecastForAllMonthMinusOneDay()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			var missingForecast = Target.GetMissingForecast(range, new List<SkillMissingForecast>
			{
				new SkillMissingForecast
				{
					SkillName = "Direct Sales",
					SkillId = Guid.NewGuid(),
					Periods = new[] { new DateOnlyPeriod(2015, 5, 31, 2015, 5, 31) }
				}
			});
			var first = missingForecast.First();

			first.MissingRanges.Length.Should().Be.EqualTo(1);
			first.MissingRanges[0].StartDate.Should().Be.EqualTo(new DateTime(2015, 5, 1));
			first.MissingRanges[0].EndDate.Should().Be.EqualTo(new DateTime(2015, 5, 30));
		}

		[Test]
		public void ShouldNotReturnSkillWithForecast()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			var missingForecast = Target.GetMissingForecast(range, new List<SkillMissingForecast>
			{
				new SkillMissingForecast
				{
					SkillName = "Direct Sales",
					SkillId = Guid.NewGuid(),
					Periods = new[] { new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31) }
				}
			});

			missingForecast.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnSortedSkills()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			var missingForecast = Target.GetMissingForecast(range, new List<SkillMissingForecast>
			{
				new SkillMissingForecast
				{
					SkillName = "Direct Sales",
					SkillId = Guid.NewGuid(),
					Periods = new DateOnlyPeriod[] { }
				},
				new SkillMissingForecast
				{
					SkillName = "A skill",
					SkillId = Guid.NewGuid(),
					Periods = new DateOnlyPeriod[] {}
				}
			});

			missingForecast.First().SkillName.Should().Be("A skill");
			missingForecast.Last().SkillName.Should().Be("Direct Sales");
		}

		[Test]
		public void ShouldOnlyReturnErrorsForSkillsUsedByPeopleSelection()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			var person = PersonFactory.CreatePerson("_");
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2015, 5, 1));
			person.AddPersonPeriod(personPeriod);
			var skill = SkillFactory.CreateSkill("A skill").WithId();
			person.AddSkill(skill, new DateOnly(2015, 5, 1));

			var result = new ValidationResult();
			Target.FillMissingForecast(result, new []{person}, range, new List<SkillMissingForecast>
			{
				new SkillMissingForecast
				{
					SkillName = "Direct Sales",
					SkillId = Guid.NewGuid(),
					Periods = new DateOnlyPeriod[] { }
				},
				new SkillMissingForecast
				{
					SkillName = skill.Name,
					SkillId = skill.Id.Value,
					Periods = new DateOnlyPeriod[] {}
				}
			});

			result.InvalidResources.Single().ResourceName.Should().Be(skill.Name);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("Default", true, true))).For<IScenarioRepository>();
		}
	}
}