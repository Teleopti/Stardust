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
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class AgentBageStepDefinition
	{
		[Given(@"I have badges with")]
		public void GivenIHaveBadges(Table table)
		{
			var agentBadges = table.CreateSet<BadgeConfigurable>();
			agentBadges.ForEach(a => DataMaker.Data().Apply(a));
		}

		[Then(@"I should see I have (\d*) bronze badge, (\d*) silver badge and (\d*) gold badge")]
		public void ThenIShouldSeeIHaveBadge(int bronzeBadgeCount, int silverBadgeCount, int goldBadgeCount)
		{
			Browser.Interactions.AssertExists("#BadgePanel");
			Browser.Interactions.AssertAnyContains("#BadgePanel .GoldBadge", goldBadgeCount.ToString(CultureInfo.InvariantCulture));
			Browser.Interactions.AssertAnyContains("#BadgePanel .SilverBadge", silverBadgeCount.ToString(CultureInfo.InvariantCulture));
			Browser.Interactions.AssertAnyContains("#BadgePanel .BronzeBadge", bronzeBadgeCount.ToString(CultureInfo.InvariantCulture));
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
			var badgeType = Interfaces.Domain.BadgeType.AnsweredCalls;
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
				GoldBadge = Gold,
			};

			var rep = new PersonRepository(uow);
			var people = rep.LoadAll();
			var person = people.First(p => p.Name == user.Name);
			person.AddBadge(AgentBadge);
		}
	}
}
