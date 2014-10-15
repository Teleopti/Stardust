using System;
using System.Globalization;
using System.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.UserTexts;
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

	public class BadgeSettingsConfigurable : IDataSetup
	{
		public bool BadgeEnabled { get; set; }

		public int SilverToBronzeRate { get; set; }
		public int GoldToSilverRate { get; set; }
		public bool AnsweredCallsUsed { get; set; }
		public bool AdherenceUsed { get; set; }
		public bool AHTUsed { get; set; }

		public IAgentBadgeThresholdSettings Settings;

		public void Apply(IUnitOfWork uow)
		{
			var rep = new AgentBadgeSettingsRepository(uow);
			Settings = rep.GetSettings();

			Settings.EnableBadge = BadgeEnabled;
			Settings.SilverToBronzeBadgeRate = SilverToBronzeRate;
			Settings.GoldToSilverBadgeRate = GoldToSilverRate;
			Settings.AdherenceBadgeTypeSelected = AdherenceUsed;
			Settings.AHTBadgeTypeSelected = AHTUsed;
			Settings.AnsweredCallsBadgeTypeSelected = AnsweredCallsUsed;

			rep.PersistSettingValue(Settings);
		}
	}

	public class BadgeConfigurable : IUserSetup
	{
		public string BadgeType { get; set; }
		public int Bronze { get; set; }
		public int Silver { get; set; }
		public int Gold { get; set; }
		public DateTime LastCalculatedDate { get; set; }

		public AgentBadgeTransaction AgentBadge;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			BadgeType badgeType;
			if (!Enum.TryParse(BadgeType, true, out badgeType))
			{
				throw  new ArgumentException(@"BadgeType", string.Format("\"{0}\" is not a valid badge type.", BadgeType));
			}
			var setting = new AgentBadgeSettingsRepository(uow).GetSettings();
			var goldToSilverBadgeRate = setting.GoldToSilverBadgeRate;
			var silverToBronzeBadgeRate = setting.SilverToBronzeBadgeRate;
			var totalBadgeAmount = (Gold*goldToSilverBadgeRate + Silver)*silverToBronzeBadgeRate + Bronze;
			
			var rep = new PersonRepository(uow);
			var people = rep.LoadAll();
			var person = people.First(p => p.Name == user.Name);

			AgentBadge = new AgentBadgeTransaction
			{
				Person = person,
				BadgeType = badgeType,
				Amount = totalBadgeAmount,
				CalculatedDate = new DateOnly(LastCalculatedDate),
				Description = "test",
				InsertedOn = new DateTime(2014, 8, 20)
			};

			var badgeRep = new AgentBadgeTransactionRepository(uow);
			badgeRep.Add(AgentBadge);
		}
	}
}
