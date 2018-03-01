using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Gamification.Controller;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Gamification
{
	[GamificationTest]
	[TestFixture]
	public class GamificationCalculationControllerTest
	{
		public GamificationCalculationController Target;
		public FakeAgentBadgeWithRankTransactionRepository AgentBadgeWithRankTransactionRepository;
		public FakeAgentBadgeTransactionRepository AgentBadgeTransactionRepository;
		public FakeJobResultRepository JobResultRepository;

		[Test]
		public void ShouldResetAgentBadges()
		{
			AgentBadgeTransactionRepository.Add(new AgentBadgeTransaction());
			AgentBadgeWithRankTransactionRepository.Add(new AgentBadgeWithRankTransaction());

			Target.ResetBadge();

			AgentBadgeTransactionRepository.LoadAll().Any().Should().Be.False();
			AgentBadgeWithRankTransactionRepository.LoadAll().Any().Should().Be.False();
		}

		[Test]
		public void ShouldCreatNewJob()
		{
			var peroid = DateOnly.Today.ToDateOnlyPeriod();
			Target.NewRecalculateBadgeJob(peroid);

			var jobResult = JobResultRepository.LoadAll().ToList();
			jobResult.Count.Should().Be.EqualTo(1);
			jobResult[0].JobCategory.Should().Be.EqualTo(JobCategory.WebRecalculateBadge);
			jobResult[0].Period.Should().Be.EqualTo(peroid);
		}
	}
}
