using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	[ForecastProviderTest]
	public class MissingForecastProviderTest
	{
		public IMissingForecastProvider Target;
		public IExistingForecastRepository FakeExistingForecastRepository;

		[Test]
		public void ShouldReturnMissingForecastForSkill()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			((FakeExistingForecastRepository) FakeExistingForecastRepository).CustomResult = new List
				<Tuple<string, IEnumerable<DateOnlyPeriod>>>
			{
				new Tuple<string, IEnumerable<DateOnlyPeriod>>("Direct Sales", new DateOnlyPeriod[] {})
			};
			var missingForecast = Target.GetMissingForecast(range);

			missingForecast.First().MissingRanges[0].StartDate.Should().Be.EqualTo(range.StartDate.Date);
			missingForecast.First().MissingRanges[0].EndDate.Should().Be.EqualTo(range.EndDate.Date);
		}

		[Test]
		public void ShouldReturnMissingForecastForStartOfMonth()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			((FakeExistingForecastRepository)FakeExistingForecastRepository).CustomResult = new List
				<Tuple<string, IEnumerable<DateOnlyPeriod>>>
			{
				new Tuple<string, IEnumerable<DateOnlyPeriod>>("Direct Sales", new[] {new DateOnlyPeriod(2015,5,11,2015,5,31)})
			};
			var missingForecast = Target.GetMissingForecast(range);

			missingForecast.First().MissingRanges.Length.Should().Be.EqualTo(1);
			missingForecast.First().MissingRanges[0].StartDate.Should().Be.EqualTo(range.StartDate.Date);
			missingForecast.First().MissingRanges[0].EndDate.Should().Be.EqualTo(new DateTime(2015,5,10));
		}

		[Test]
		public void ShouldReturnMissingForecastForEndOfMonth()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			((FakeExistingForecastRepository)FakeExistingForecastRepository).CustomResult = new List
				<Tuple<string, IEnumerable<DateOnlyPeriod>>>
			{
				new Tuple<string, IEnumerable<DateOnlyPeriod>>("Direct Sales", new[] {new DateOnlyPeriod(2015,5,1,2015,5,21)})
			};

			var missingForecast = Target.GetMissingForecast(range);

			missingForecast.First().MissingRanges.Length.Should().Be.EqualTo(1);
			missingForecast.First().MissingRanges[0].StartDate.Should().Be.EqualTo(new DateTime(2015, 5, 22));
			missingForecast.First().MissingRanges[0].EndDate.Should().Be.EqualTo(range.EndDate.Date);
		}

		[Test]
		public void ShouldReturnMissingForecastForStartAndEndOfMonth()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			((FakeExistingForecastRepository)FakeExistingForecastRepository).CustomResult = new List
				<Tuple<string, IEnumerable<DateOnlyPeriod>>>
			{
				new Tuple<string, IEnumerable<DateOnlyPeriod>>("Direct Sales",
						new[] {new DateOnlyPeriod(2015, 5, 11, 2015, 5, 19)})
			};

			var missingForecast = Target.GetMissingForecast(range);

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
			((FakeExistingForecastRepository)FakeExistingForecastRepository).CustomResult = new List
				<Tuple<string, IEnumerable<DateOnlyPeriod>>>
			{
				new Tuple<string, IEnumerable<DateOnlyPeriod>>("Direct Sales",
					new[] {new DateOnlyPeriod(2015, 5, 31, 2015, 5, 31)})
			};
			var missingForecast = Target.GetMissingForecast(range);

			missingForecast.First().MissingRanges.Length.Should().Be.EqualTo(1);

			missingForecast.First().MissingRanges[0].StartDate.Should().Be.EqualTo(new DateTime(2015, 5, 1));
			missingForecast.First().MissingRanges[0].EndDate.Should().Be.EqualTo(new DateTime(2015,5,30));
		}

		[Test]
		public void ShouldNotReturnSkillWithForecast()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			((FakeExistingForecastRepository)FakeExistingForecastRepository).CustomResult = new List
				<Tuple<string, IEnumerable<DateOnlyPeriod>>>
			{
				new Tuple<string, IEnumerable<DateOnlyPeriod>>("Direct Sales",
						new[] {new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31)})
			};
			var missingForecast = Target.GetMissingForecast(range);

			missingForecast.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnSortedSkills()
		{
			var range = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			((FakeExistingForecastRepository)FakeExistingForecastRepository).CustomResult = new List
				<Tuple<string, IEnumerable<DateOnlyPeriod>>>
			{
				new Tuple<string, IEnumerable<DateOnlyPeriod>>("Direct Sales", new DateOnlyPeriod[] {}),
					new Tuple<string, IEnumerable<DateOnlyPeriod>>("A skill", new DateOnlyPeriod[] {})
			};
			var missingForecast = Target.GetMissingForecast(range);

			missingForecast.First().SkillName.Should().Be("A skill");
			missingForecast.Last().SkillName.Should().Be("Direct Sales");
		}
	}

	public class ForecastProviderTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<FakeExistingForecastRepository>().As<IExistingForecastRepository>().SingleInstance();
			builder.RegisterType<MissingForecastProvider>().As<IMissingForecastProvider>().SingleInstance();
			builder.RegisterInstance<IScenarioRepository>(new FakeScenarioRepository(ScenarioFactory.CreateScenario("Default", true, true))).SingleInstance();
		}
		
	}
}