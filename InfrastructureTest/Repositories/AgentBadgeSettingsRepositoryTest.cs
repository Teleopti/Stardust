using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Foundation;
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
	public class AgentBadgeSettingsRepository : RepositoryTest<AgentBadgeThresholdSettings>
    {
	    protected override AgentBadgeThresholdSettings CreateAggregateWithCorrectBusinessUnit()
	    {
		    return new AgentBadgeThresholdSettings
		    {
				AdherenceThreshold = new Percent(0.5),
			    AnsweredCallsThreshold = 50,
			    AHTThreshold = new TimeSpan(0, 5, 30),
			    SilverBadgeDaysThreshold = 5,
			    GoldBadgeDaysThreshold = 10
		    };
	    }

	    protected override void VerifyAggregateGraphProperties(AgentBadgeThresholdSettings loadedAggregateFromDatabase)
	    {
		    var settings = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(settings.BusinessUnit, loadedAggregateFromDatabase.BusinessUnit);
			Assert.AreEqual(settings.AdherenceThreshold, loadedAggregateFromDatabase.AdherenceThreshold);
			Assert.AreEqual(settings.AnsweredCallsThreshold, loadedAggregateFromDatabase.AnsweredCallsThreshold);
			Assert.AreEqual(settings.AHTThreshold, loadedAggregateFromDatabase.AHTThreshold);
			Assert.AreEqual(settings.SilverBadgeDaysThreshold, loadedAggregateFromDatabase.SilverBadgeDaysThreshold);
			Assert.AreEqual(settings.GoldBadgeDaysThreshold, loadedAggregateFromDatabase.GoldBadgeDaysThreshold);
	    }

	    protected override Repository<AgentBadgeThresholdSettings> TestRepository(IUnitOfWork unitOfWork)
	    {
		    return new Infrastructure.Repositories.AgentBadgeSettingsRepository(unitOfWork);
	    }
    }
}
