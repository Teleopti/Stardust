using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.AgentInfo
{
    [TestFixture]
    public class TeamTest
    {	
        [Test]
        public void ShouldCreateWithDefaultProperties()
        {
			var target = new Team();

	        target.Id.Should().Be(null);
			target.IsChoosable.Should().Be.True();
        }
		
        [Test]
        public void ShouldSetAndGetStuff()
		{
			var site = SiteFactory.CreateSimpleSite("Site1");
			var scorecard = new Scorecard();
			var target = new Team
			{
				Scorecard = scorecard,
				Site = site
			};
			target.SetDescription(new Description("Happy Agents"));

			target.Scorecard.Should().Be(scorecard);
			target.Site.Should().Be(site);
			target.Description.Name.Should().Be("Happy Agents");
			target.SiteAndTeam.Should().Be("Site1/Happy Agents");
		}
		
		[Test]
		public void ShouldPublishTeamNameChangedEvent()
		{
			var target = new Team().WithId();

			target.SetDescription(new Description("Set Name"));

			var @event = target.PopAllEvents(null).OfType<TeamNameChangedEvent>().Single();
			@event.TeamId.Should().Be(target.Id);
			@event.Name.Should().Be("Set Name");
		}
	}
}