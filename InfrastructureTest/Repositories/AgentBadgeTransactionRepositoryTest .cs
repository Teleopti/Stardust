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

	    protected override void TeardownForRepositoryTest()
	    {
		    if (person != null && person.Id != null)
		    {
			    using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			    {
					var personRep = new PersonRepository(uow);
				    personRep.Remove(person);
					uow.PersistAll();
			    }
		    }

		    base.TeardownForRepositoryTest();
	    }

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

	    protected override Repository<IAgentBadgeTransaction> TestRepository(IUnitOfWork unitOfWork)
	    {
			return new Infrastructure.Repositories.AgentBadgeTransactionRepository(unitOfWork);
	    }

	    [Test]
	    public void ShouldResetBadges()
	    {
		    UnitOfWork.PersistAll();
		    SkipRollback();

		    var target = (Infrastructure.Repositories.AgentBadgeTransactionRepository) TestRepository(UnitOfWork);
		    target.ResetAgentBadges();

		    var result = target.Find(person);
		    result.Should().Be.Empty();
	    }
    }
}
