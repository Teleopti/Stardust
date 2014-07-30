using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

using Table = TechTalk.SpecFlow.Table;

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

		[Given(@"I have badges")]
		public void GivenIHaveBadges(Table table)
		{
			var agentBadges = table.CreateSet<BadgeConfigurable>();
			agentBadges.ForEach(a => DataMaker.Data().Apply(a));
		}

		[Then(@"I should see I have badge")]
		public void ThenIShouldSeeIHaveBadge()
		{
			ScenarioContext.Current.Pending();
		}


	}

	public class BadgeConfigurable : IUserSetup
	{
		public string BadgeType { get; set; }
		public int Bronze { get; set; }
		public int Silver { get; set; }
		public int Gold { get; set; }

		public IAgentBadge AgentBadge;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			BadgeType badgeType = Interfaces.Domain.BadgeType.AnsweredCalls;
			if (BadgeType == "AnsweredCalls")
			{
				badgeType = Interfaces.Domain.BadgeType.AnsweredCalls;
			}
			if (BadgeType == "AHT")
			{
				badgeType = Interfaces.Domain.BadgeType.AverageHandlingTime;
			}
			if (BadgeType == "Adherence")
			{
				badgeType = Interfaces.Domain.BadgeType.Adherence;
			}
			AgentBadge = new AgentBadge()
			{
				BadgeType = badgeType,
				BronzeBadge = Bronze,
				SilverBadge = Silver,
				GoldenBadge = Gold,
			};
			
			var rep = new PersonRepository(uow);
			var people = rep.LoadAll();
			var person = people.First(p => p.Name == user.Name);
			person.AddBadge(AgentBadge);
		}
	}
}
