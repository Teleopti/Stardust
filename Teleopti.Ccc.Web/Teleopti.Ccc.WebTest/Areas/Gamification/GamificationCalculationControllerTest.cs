using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Gamification.Controller;

namespace Teleopti.Ccc.WebTest.Areas.Gamification
{
	[GamificationTest]
	[TestFixture]
	public class GamificationCalculationControllerTest
	{
		public GamificationCalculationController Target;
		public FakeAgentBadgeWithRankTransactionRepository AgentBadgeWithRankTransactionRepository;
		public FakeAgentBadgeTransactionRepository AgentBadgeTransactionRepository;

		[Test]
		public void ShouldResetAgentBadges()
		{
			AgentBadgeTransactionRepository.Add(new AgentBadgeTransaction());
			AgentBadgeWithRankTransactionRepository.Add(new AgentBadgeWithRankTransaction());

			Target.ResetBadge();

			AgentBadgeTransactionRepository.LoadAll().Any().Should().Be.False();
			AgentBadgeWithRankTransactionRepository.LoadAll().Any().Should().Be.False();
		}
	}
}
