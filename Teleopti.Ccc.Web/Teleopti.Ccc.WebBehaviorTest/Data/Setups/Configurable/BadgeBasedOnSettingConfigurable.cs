using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class BadgeBasedOnSettingConfigurable: IUserSetup
	{
		public string BadgeType { get; set; }
		public int Bronze { get; set; }
		public int Silver { get; set; }
		public int Gold { get; set; }
		public DateTime LastCalculatedDate { get; set; }

		public AgentBadgeTransaction AgentBadge;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var badgeType = Domain.InterfaceLegacy.Domain.BadgeType.GetBadgeType(BadgeType);

			var setting = new GamificationSettingRepository(uow).FindAllGamificationSettingsSortedByDescription().First();
			var goldToSilverBadgeRate = setting.GoldToSilverBadgeRate;
			var silverToBronzeBadgeRate = setting.SilverToBronzeBadgeRate;
			var totalBadgeAmount = (Gold * goldToSilverBadgeRate + Silver) * silverToBronzeBadgeRate + Bronze;

			var rep = new PersonRepository(new ThisUnitOfWork(uow));
			var people = rep.LoadAll();
			var person = people.First(p => p.Name == user.Name);

			AgentBadge = new AgentBadgeTransaction
			{
				Person = person,
				BadgeType = badgeType,
				Amount = totalBadgeAmount,
				CalculatedDate = new DateOnly(LastCalculatedDate),
				Description = "test",
				InsertedOn = new DateTime(2014, 8, 20, 0, 0, 0, DateTimeKind.Utc)
			};

			var badgeRep = new AgentBadgeTransactionRepository(uow);
			badgeRep.Add(AgentBadge);
		}
	}
}
