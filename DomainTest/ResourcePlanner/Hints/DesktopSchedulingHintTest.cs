using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Hints
{
	[DomainTest]
	[ThrowIfRepositoriesAreUsed]
	[UseIocForFatClient]
	public class DesktopSchedulingHintTest
	{
		public CheckScheduleHints Target;

		[Test]
		public void ShouldRunRealValidators()
		{
			//simply take one of the validators to see that real code is executed
			var agentMissingPersonPeriod = new Person();

			var result = Target.Execute(new HintInput(null, new[] {agentMissingPersonPeriod}, new DateOnlyPeriod(2000, 1, 1, 2000, 2, 1), null, false));

			result.InvalidResources.SelectMany(x => x.ValidationTypes)
				.Any(x => x == typeof(PersonPeriodHint))
				.Should().Be.True();
		}

		[Test]
		public void ShouldNotCareAboutFullSchedulePeriod()
		{
			var startDate = new DateOnly(2000,1,1);
			var agent = new Person().WithSchedulePeriodOneMonth(startDate);

			var result = Target.Execute(new HintInput(null, new[] {agent}, new DateOnlyPeriod(startDate, startDate.AddDays(1)), null, false));

			result.InvalidResources.SelectMany(x => x.ValidationTypes)
				.Any(x => x == typeof(PersonSchedulePeriodHint))
				.Should().Be.False();
		}

		[Test]
		public void ShouldNotCareAboutMissingForecast()
		{
			var agent = new Person();

			var result = Target.Execute(new HintInput(null, new[] { agent }, new DateOnlyPeriod(2000, 1, 1, 2000, 2, 1), null, false));

			result.InvalidResources.SelectMany(x => x.ValidationTypes)
				.Any(x => x == typeof(MissingForecastHint))
				.Should().Be.False();
		}
	}
}