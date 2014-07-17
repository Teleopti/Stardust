using System.Globalization;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class AgentBageStepDefinition
	{
		[Given(@"I have a silver badge for answered calls")]
		public void GivenIHaveASilverBadgeForAnsweredCalls()
		{
			ScenarioContext.Current.Pending();
		}
	}

	public class BadgeConfigurable : IUserSetup
	{


		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
		}
	}
}
