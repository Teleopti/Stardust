using NUnit.Framework;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("BucketB")]
    public class NoteRepositoryTest: RepositoryTest<INote>
    {
	    private IScenario scenario;
	    private IPerson person;

	    protected override void ConcreteSetup()
	    {
			person = PersonFactory.CreatePerson("Kalle");
			scenario = new Scenario("Active");
			PersistAndRemoveFromUnitOfWork(person);
			PersistAndRemoveFromUnitOfWork(scenario);
		}

		protected override INote CreateAggregateWithCorrectBusinessUnit()
        {
            var dateOnly = new DateOnly(2010,4,1);
            const string text = "Agent was hit by shrinkage";
            
            INote note = new Note(person, dateOnly, scenario, text);

            return note;
        }

        protected override void VerifyAggregateGraphProperties(INote loadedAggregateFromDatabase)
        {
            INote org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Scenario.Description, loadedAggregateFromDatabase.Scenario.Description);
            Assert.AreEqual(org.Person.Name, loadedAggregateFromDatabase.Person.Name);
            Assert.AreEqual(org.GetScheduleNote(new NoFormatting()), loadedAggregateFromDatabase.GetScheduleNote(new NoFormatting()));
            Assert.AreEqual(org.NoteDate, loadedAggregateFromDatabase.NoteDate);
        }

        protected override Repository<INote> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new NoteRepository(currentUnitOfWork); 
        }

        [Test]
        public void VerifyLoadAggregate()
        {
            INote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            var repository = new NoteRepository(CurrUnitOfWork);
            Assert.AreEqual(note, repository.LoadAggregate(note.Id.Value));
        }

        [Test]
        public void VerifyFindOnPersons()
        {
            INote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            var repository = new NoteRepository(CurrUnitOfWork);
            Assert.AreEqual(1, repository.Find(new DateOnlyPeriod(2010, 4, 1, 2010, 4, 2), new List<IPerson>{note.Person}, note.Scenario).Count);
        }
    }
}
