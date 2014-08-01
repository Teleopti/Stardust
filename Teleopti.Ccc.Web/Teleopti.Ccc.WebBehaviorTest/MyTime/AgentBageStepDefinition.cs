using System.Globalization;
using System.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
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
		[Given(@"There is a agent badge settings with")]
		public void GivenThereIsAAgentBadgeSettingsWith(Table table)
		{
			var badgeSetting = table.CreateInstance<BadgeSettingsConfigurable>();
			DataMaker.Data().Apply(badgeSetting);
		}

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

		[When(@"I view badge details")]
		public void WhenIViewBadgeDetails()
		{
			Browser.Interactions.Click("#BadgePanel");
		}

		[Then(@"I should see I have (.*) bronze badges, (.*) silver badge and (.*) gold badge for (.*)")]
		public void ThenIShouldSeeIHaveBronzeBadgesSilverBadgeAndGoldBadgeForAnsweredCalls(int bronzeBadgeCount, int silverBadgeCount, int goldBadgeCount, string badgeType)
		{
			const string selectorTemplate = ".BadgeDetail:first-child:contains('{0}') .{1}:contains('{2}')";

			var goldSelector = string.Format(selectorTemplate, badgeType, "GoldBadge", goldBadgeCount);
			var silverSelector = string.Format(selectorTemplate, badgeType, "SilverBadge", silverBadgeCount);
			var bronzeSelector = string.Format(selectorTemplate, badgeType, "BronzeBadge", bronzeBadgeCount);

			Browser.Interactions.AssertExistsUsingJQuery(goldSelector);
			Browser.Interactions.AssertExistsUsingJQuery(silverSelector);
			Browser.Interactions.AssertExistsUsingJQuery(bronzeSelector);
		}
	}

	public class BadgeSettingsConfigurable : IDataSetup
	{
		public bool BadgeEnabled { get; set; }

		public IAgentBadgeThresholdSettings Settings;

		public void Apply(IUnitOfWork uow)
		{
			Settings = new AgentBadgeThresholdSettings
			{
				EnableBadge = BadgeEnabled
			};

			var rep = new AgentBadgeSettingsRepository(uow);
			rep.Add(Settings);
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
