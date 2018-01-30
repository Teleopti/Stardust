using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.Gamification
{
	[TestFixture, GamificationTest]
	public class TeamGamificationSettingProviderAndPersisterTest
	{
		public ITeamGamificationSettingProviderAndPersister Target;
		public FakeTeamRepository teamRepository;

		[Test]
		public void ShouldSortBySiteAndTeamName()
		{
			var s1 = SiteFactory.CreateSimpleSite("shenzhen").WithId();
			var s2 = SiteFactory.CreateSimpleSite("beijing").WithId();
			var t1 = TeamFactory.CreateSimpleTeam("red").WithId();
			t1.Site = s1;
			var t2 = TeamFactory.CreateSimpleTeam("blue").WithId();
			t2.Site = s1;
			var t3 = TeamFactory.CreateSimpleTeam("green").WithId();
			t3.Site = s2;
			teamRepository.Has(t1);
			teamRepository.Has(t2);
			teamRepository.Has(t3);

			var result = Target.GetTeamGamificationSettingViewModels(new List<Guid> { s1.Id.Value, s2.Id.Value });

			result[0].Team.text.Should().Be.EqualTo("beijing/green");
			result[1].Team.text.Should().Be.EqualTo("shenzhen/blue");
		}
	}
}
