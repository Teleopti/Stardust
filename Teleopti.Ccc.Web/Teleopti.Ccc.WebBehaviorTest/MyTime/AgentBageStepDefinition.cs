﻿using System.Globalization;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Interfaces.Domain;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class AgentBageStepDefinition
	{
		[Given(@"There is an agent badge settings with")]
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
			Browser.Interactions.AssertAnyContains("#BadgePanel .gold-badge", goldBadgeCount.ToString(CultureInfo.InvariantCulture));
			Browser.Interactions.AssertAnyContains("#BadgePanel .silver-badge", silverBadgeCount.ToString(CultureInfo.InvariantCulture));
			Browser.Interactions.AssertAnyContains("#BadgePanel .bronze-badge", bronzeBadgeCount.ToString(CultureInfo.InvariantCulture));
		}

		[When(@"I view badge details")]
		public void WhenIViewBadgeDetails()
		{
			Browser.Interactions.Click("#BadgePanel");
		}

		[Then(@"I should see I have (.*) bronze badges, (.*) silver badge and (.*) gold badge for (.*)")]
		public void ThenIShouldSeeIHaveBronzeBadgesSilverBadgeAndGoldBadgeForAnsweredCalls(int bronzeBadgeCount, int silverBadgeCount, int goldBadgeCount, BadgeType badgeType)
		{
			const string selectorTemplate = ".badge-detail:first-child:contains('{0}') .{1}:contains('{2}')";
			string badgeTypeName;
			switch (badgeType)
			{
				case BadgeType.Adherence:
					badgeTypeName = Resources.Adherence;
					break;
				case BadgeType.AnsweredCalls:
					badgeTypeName = Resources.AnsweredCalls;
					break;
				case BadgeType.AverageHandlingTime:
					badgeTypeName = Resources.AverageHandlingTime;
					break;
				default:
					badgeTypeName = string.Empty;
					break;
			}

			var goldSelector = string.Format(selectorTemplate, badgeTypeName, "gold-badge", goldBadgeCount);
			var silverSelector = string.Format(selectorTemplate, badgeTypeName, "silver-badge", silverBadgeCount);
			var bronzeSelector = string.Format(selectorTemplate, badgeTypeName, "bronze-badge", bronzeBadgeCount);

			Browser.Interactions.AssertExistsUsingJQuery(goldSelector);
			Browser.Interactions.AssertExistsUsingJQuery(silverSelector);
			Browser.Interactions.AssertExistsUsingJQuery(bronzeSelector);
		}

		[Then(@"There should display no badge information")]
		public void ThenThereShouldDisplayNoBadgeInformation()
		{
			Browser.Interactions.AssertNotExists(".user-name-link", "#BadgePanel");
		}
	}
}
