using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourcePlanner.Validation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Validation
{
	[DomainTest]
	[SkipRegistringFakeRepositories]
	public class DesktopSchedulingValidatorTest
	{
		public DesktopSchedulingValidator Target;

		[Test]
		public void ShouldRunRealValidators()
		{
			//simply take one of the validators to see that real code is executed
			var agentMissingPersonPeriod = new Person();

			var result = Target.Validate(new[] {agentMissingPersonPeriod}, new DateOnlyPeriod(2000, 1, 1, 2000, 2, 1), Enumerable.Empty<SkillMissingForecast>());

			result.InvalidResources.SelectMany(x => x.ValidationTypes)
				.Any(x => x == typeof(PersonPeriodValidator))
				.Should().Be.True();
		}

		[Test]
		[Ignore("18358")]
		public void ShouldNotCareAboutFullSchedulePeriod()
		{
			var startDate = new DateOnly(2000,1,1);
			var agent = new Person().WithSchedulePeriodOneMonth(startDate);

			var result = Target.Validate(new[] {agent}, new DateOnlyPeriod(startDate, startDate.AddDays(1)), Enumerable.Empty<SkillMissingForecast>());

			result.InvalidResources.SelectMany(x => x.ValidationTypes)
				.Any(x => x == typeof(PersonSchedulePeriodValidator))
				.Should().Be.False();
		}
	}
}