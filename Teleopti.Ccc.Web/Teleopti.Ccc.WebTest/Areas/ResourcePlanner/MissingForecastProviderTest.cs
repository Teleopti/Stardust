using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class MissingForecastProviderTest
	{
		[Test]
		public void ShouldReturnMissingForecastForSkill()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			var scenario = ScenarioFactory.CreateScenario("Default", true, false);
			var existingForecastsRepository = MockRepository.GenerateMock<IExistingForecastRepository>();
			existingForecastsRepository.Stub(x => x.ExistingForecastForAllSkills(range, scenario))
				.Return(new List<Tuple<string, IEnumerable<DateOnlyPeriod>>>
				{
					new Tuple<string, IEnumerable<DateOnlyPeriod>>("Direct Sales", new DateOnlyPeriod[] {})
				}
				);
			
			var target = new MissingForecastProvider(new FakeScenarioRepository(scenario), existingForecastsRepository);
			var missingForecast = target.GetMissingForecast(range);

			missingForecast.First().MissingRanges[0].StartDate.Should().Be.EqualTo(range.StartDate.Date);
			missingForecast.First().MissingRanges[0].EndDate.Should().Be.EqualTo(range.EndDate.Date);
		}

		[Test]
		public void ShouldReturnMissingForecastForStartOfMonth()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			var scenario = ScenarioFactory.CreateScenario("Default", true, false);
			var existingForecastsRepository = MockRepository.GenerateMock<IExistingForecastRepository>();
			existingForecastsRepository.Stub(x => x.ExistingForecastForAllSkills(range, scenario))
				.Return(new List<Tuple<string, IEnumerable<DateOnlyPeriod>>>
				{
					new Tuple<string, IEnumerable<DateOnlyPeriod>>("Direct Sales", new[] {new DateOnlyPeriod(2015,5,11,2015,5,31)})
				}
				);

			var target = new MissingForecastProvider(new FakeScenarioRepository(scenario), existingForecastsRepository);
			var missingForecast = target.GetMissingForecast(range);

			missingForecast.First().MissingRanges.Length.Should().Be.EqualTo(1);
			missingForecast.First().MissingRanges[0].StartDate.Should().Be.EqualTo(range.StartDate.Date);
			missingForecast.First().MissingRanges[0].EndDate.Should().Be.EqualTo(new DateTime(2015,5,10));
		}

		[Test]
		public void ShouldReturnMissingForecastForEndOfMonth()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			var scenario = ScenarioFactory.CreateScenario("Default", true, false);
			var existingForecastsRepository = MockRepository.GenerateMock<IExistingForecastRepository>();
			existingForecastsRepository.Stub(x => x.ExistingForecastForAllSkills(range, scenario))
				.Return(new List<Tuple<string, IEnumerable<DateOnlyPeriod>>>
				{
					new Tuple<string, IEnumerable<DateOnlyPeriod>>("Direct Sales", new[] {new DateOnlyPeriod(2015,5,1,2015,5,21)})
				}
				);

			var target = new MissingForecastProvider(new FakeScenarioRepository(scenario), existingForecastsRepository);
			var missingForecast = target.GetMissingForecast(range);

			missingForecast.First().MissingRanges.Length.Should().Be.EqualTo(1);
			missingForecast.First().MissingRanges[0].StartDate.Should().Be.EqualTo(new DateTime(2015, 5, 22));
			missingForecast.First().MissingRanges[0].EndDate.Should().Be.EqualTo(range.EndDate.Date);
		}

		[Test]
		public void ShouldReturnMissingForecastForStartAndEndOfMonth()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			var scenario = ScenarioFactory.CreateScenario("Default", true, false);
			var existingForecastsRepository = MockRepository.GenerateMock<IExistingForecastRepository>();
			existingForecastsRepository.Stub(x => x.ExistingForecastForAllSkills(range, scenario))
				.Return(new List<Tuple<string, IEnumerable<DateOnlyPeriod>>>
				{
					new Tuple<string, IEnumerable<DateOnlyPeriod>>("Direct Sales",
						new[] {new DateOnlyPeriod(2015, 5, 11, 2015, 5, 19)})
				}
				);

			var target = new MissingForecastProvider(new FakeScenarioRepository(scenario), existingForecastsRepository);
			var missingForecast = target.GetMissingForecast(range);

			missingForecast.First().MissingRanges.Length.Should().Be.EqualTo(2);

			missingForecast.First().MissingRanges[0].StartDate.Should().Be.EqualTo(range.StartDate.Date);
			missingForecast.First().MissingRanges[0].EndDate.Should().Be.EqualTo(new DateTime(2015, 5, 10));

			missingForecast.First().MissingRanges[1].StartDate.Should().Be.EqualTo(new DateTime(2015, 5, 20));
			missingForecast.First().MissingRanges[1].EndDate.Should().Be.EqualTo(range.EndDate.Date);
		}

		[Test]
		public void ShouldReturnMissingForecastForAllMonthMinusOneDay()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			var scenario = ScenarioFactory.CreateScenario("Default", true, false);
			var existingForecastsRepository = MockRepository.GenerateMock<IExistingForecastRepository>();
			existingForecastsRepository.Stub(x => x.ExistingForecastForAllSkills(range, scenario))
				.Return(new List<Tuple<string, IEnumerable<DateOnlyPeriod>>>
				{
					new Tuple<string, IEnumerable<DateOnlyPeriod>>("Direct Sales",
						new[] {new DateOnlyPeriod(2015, 5, 31, 2015, 5, 31)})
				}
				);

			var target = new MissingForecastProvider(new FakeScenarioRepository(scenario), existingForecastsRepository);
			var missingForecast = target.GetMissingForecast(range);

			missingForecast.First().MissingRanges.Length.Should().Be.EqualTo(1);

			missingForecast.First().MissingRanges[0].StartDate.Should().Be.EqualTo(new DateTime(2015, 5, 1));
			missingForecast.First().MissingRanges[0].EndDate.Should().Be.EqualTo(new DateTime(2015,5,30));
		}

		[Test]
		public void ShouldNotReturnSkillWithForecast()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			var scenario = ScenarioFactory.CreateScenario("Default", true, false);
			var existingForecastsRepository = MockRepository.GenerateMock<IExistingForecastRepository>();
			existingForecastsRepository.Stub(x => x.ExistingForecastForAllSkills(range, scenario))
				.Return(new List<Tuple<string, IEnumerable<DateOnlyPeriod>>>
				{
					new Tuple<string, IEnumerable<DateOnlyPeriod>>("Direct Sales",
						new[] {new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31)})
				}
				);

			var target = new MissingForecastProvider(new FakeScenarioRepository(scenario), existingForecastsRepository);
			var missingForecast = target.GetMissingForecast(range);

			missingForecast.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnSortedSkills()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			var scenario = ScenarioFactory.CreateScenario("Default", true, false);
			var existingForecastsRepository = MockRepository.GenerateMock<IExistingForecastRepository>();
			existingForecastsRepository.Stub(x => x.ExistingForecastForAllSkills(range, scenario))
				.Return(new List<Tuple<string, IEnumerable<DateOnlyPeriod>>>
				{
					new Tuple<string, IEnumerable<DateOnlyPeriod>>("Direct Sales", new DateOnlyPeriod[] {}),
					new Tuple<string, IEnumerable<DateOnlyPeriod>>("A skill", new DateOnlyPeriod[] {})
				}
				);

			var target = new MissingForecastProvider(new FakeScenarioRepository(scenario), existingForecastsRepository);
			var missingForecast = target.GetMissingForecast(range);

			missingForecast.First().SkillName.Should().Be("A skill");
			missingForecast.Last().SkillName.Should().Be("Direct Sales");
		}
	}
}