using NUnit.Framework;
using SharpTestsEx;
using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests AvailabilityRepository
    ///</summary>
    [TestFixture]
    [Category("LongRunning")]
	public class AgentBadgeTransactionRepositoryTest : RepositoryTest<IAgentBadgeTransaction>
    {
	    private IPerson person;

	    protected override IAgentBadgeTransaction CreateAggregateWithCorrectBusinessUnit()
	    {
			person = PersonFactory.CreatePerson();
			PersistAndRemoveFromUnitOfWork(person);
		    return new AgentBadgeTransaction
		    {
			    Amount = 1,
			    BadgeType = BadgeType.AverageHandlingTime,
			    CalculatedDate = new DateOnly(2014, 9, 9),
			    Description = "test",
			    InsertedOn = new DateTime(2014, 9, 10),
			    Person = person
		    };
	    }

	    protected override void VerifyAggregateGraphProperties(IAgentBadgeTransaction loadedAggregateFromDatabase)
	    {
		    var newBadgeTran = CreateAggregateWithCorrectBusinessUnit();

			Assert.AreEqual(newBadgeTran.Person.Name, loadedAggregateFromDatabase.Person.Name);
			Assert.AreEqual(newBadgeTran.BadgeType, loadedAggregateFromDatabase.BadgeType);
			Assert.AreEqual(newBadgeTran.Amount, loadedAggregateFromDatabase.Amount);
			Assert.AreEqual(newBadgeTran.CalculatedDate, loadedAggregateFromDatabase.CalculatedDate);
			Assert.AreEqual(newBadgeTran.Description, loadedAggregateFromDatabase.Description);
			Assert.AreEqual(newBadgeTran.InsertedOn, loadedAggregateFromDatabase.InsertedOn);
	    }

	    protected override Repository<IAgentBadgeTransaction> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
	    {
			return new AgentBadgeTransactionRepository(currentUnitOfWork);
	    }

	    [Test]
	    public void ShouldResetBadges()
	    {
		    UnitOfWork.PersistAll();
		    CleanUpAfterTest();

		    var target = new AgentBadgeTransactionRepository(UnitOfWork);
		    target.ResetAgentBadges();

		    var result = target.Find(person);
		    result.Should().Be.Empty();
	    }
    }
}
