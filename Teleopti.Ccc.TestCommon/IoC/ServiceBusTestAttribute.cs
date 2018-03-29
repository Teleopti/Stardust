using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class ServiceBusTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			system.UseTestDouble<FakeStorageSimple>().For<IFakeStorage>();
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			system.UseTestDouble<FakeTeamGamificationSettingRepository>().For<ITeamGamificationSettingRepository>();
			system.UseTestDouble<FakePushMessageRepository>().For<IPushMessageRepository>();
			system.UseTestDouble<AgentBadgeCalculator>().For<IAgentBadgeCalculator>();
			system.UseTestDouble<AgentBadgeWithRankCalculator>().For<IAgentBadgeWithRankCalculator>();
			system.UseTestDouble<FakeAgentBadgeRepository>().For<IAgentBadgeRepository>();
			system.UseTestDouble<FakeAgentBadgeWithRankRepository>().For<IAgentBadgeWithRankRepository>();
			system.UseTestDouble<FakeGlobalSettingDataRepository>().For<IGlobalSettingDataRepository>();
			system.UseTestDouble<CalculateBadges>().For<CalculateBadges>();
		}
	}
}
