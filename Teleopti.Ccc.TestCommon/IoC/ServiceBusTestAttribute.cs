using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class ServiceBusTestAttribute : IoCTestAttribute
	{
		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);

			isolate.UseTestDouble<FakeStorageSimple>().For<IFakeStorage>();
			isolate.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			isolate.UseTestDouble<FakeTeamGamificationSettingRepository>().For<ITeamGamificationSettingRepository>();
			isolate.UseTestDouble<FakePushMessageRepository>().For<IPushMessageRepository>();
			isolate.UseTestDouble<AgentBadgeCalculator>().For<IAgentBadgeCalculator>();
			isolate.UseTestDouble<AgentBadgeWithRankCalculator>().For<IAgentBadgeWithRankCalculator>();
			isolate.UseTestDouble<FakeAgentBadgeRepository>().For<IAgentBadgeRepository>();
			isolate.UseTestDouble<FakeAgentBadgeWithRankRepository>().For<IAgentBadgeWithRankRepository>();
			isolate.UseTestDouble<FakeGlobalSettingDataRepository>().For<IGlobalSettingDataRepository>();
			isolate.UseTestDouble<CalculateBadges>().For<CalculateBadges>();
		}
	}
}
