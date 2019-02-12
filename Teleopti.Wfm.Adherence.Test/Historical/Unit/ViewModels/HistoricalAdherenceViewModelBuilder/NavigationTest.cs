using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.HistoricalAdherenceViewModelBuilder
{
	[DomainTest]
	public class NavigationTest
	{
		public Adherence.Historical.HistoricalAdherenceViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldIncludeLast7Days()
		{
			Now.Is("2018-01-02 09:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person);

			var viewModel = Target.Build(person, "2018-01-02".Date());

			viewModel.Navigation.First.Should().Be("20171227");
			viewModel.Navigation.Last.Should().Be("20180102");
		}

		[Test]
		public void ShouldIncludeLast7DaysForAgentInChina()
		{
			Now.Is("2018-01-15 21:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, "name", TimeZoneInfoFactory.ChinaTimeZoneInfo());

			var viewModel = Target.Build(person, "2018-01-15".Date());

			viewModel.Navigation.First.Should().Be("20180110");
			viewModel.Navigation.Last.Should().Be("20180116");
		}
	}
}