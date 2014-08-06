using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests AvailabilityRepository
    ///</summary>
    [TestFixture]
    [Category("LongRunning")]
	public class AgentBadgeSettingsRepository : RepositoryTest<IAgentBadgeThresholdSettings>
    {
	    protected override IAgentBadgeThresholdSettings CreateAggregateWithCorrectBusinessUnit()
	    {
		    return new AgentBadgeThresholdSettings
		    {
				EnableBadge = true,
				AdherenceThreshold = new Percent(0.5),
				AnsweredCallsThreshold = 50,
				AHTThreshold = new TimeSpan(0, 5, 30),
				SilverToBronzeBadgeRate = 5,
				GoldToSilverBadgeRate = 2
		    };
	    }

	    protected override void VerifyAggregateGraphProperties(IAgentBadgeThresholdSettings loadedAggregateFromDatabase)
	    {
		    var settings = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(settings.EnableBadge, true);
			Assert.AreEqual(settings.AdherenceThreshold, loadedAggregateFromDatabase.AdherenceThreshold);
			Assert.AreEqual(settings.AnsweredCallsThreshold, loadedAggregateFromDatabase.AnsweredCallsThreshold);
			Assert.AreEqual(settings.AHTThreshold, loadedAggregateFromDatabase.AHTThreshold);
			Assert.AreEqual(settings.SilverToBronzeBadgeRate, loadedAggregateFromDatabase.SilverToBronzeBadgeRate);
			Assert.AreEqual(settings.GoldToSilverBadgeRate, loadedAggregateFromDatabase.GoldToSilverBadgeRate);
	    }

	    protected override Repository<IAgentBadgeThresholdSettings> TestRepository(IUnitOfWork unitOfWork)
	    {
		    return new Infrastructure.Repositories.AgentBadgeSettingsRepository(unitOfWork);
	    }
    }
}
