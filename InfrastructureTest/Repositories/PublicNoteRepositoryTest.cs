using System.Collections.Generic;
using NUnit.Framework;
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
    public class PublicNoteRepositoryTest: RepositoryTest<IPublicNote>
    {
	    private IPerson personToCreate;
	    private IScenario scenario;

	    protected override void ConcreteSetup()
		{
			personToCreate = PersonFactory.CreatePerson("Kalle");
			scenario = new Scenario("Active");
			PersistAndRemoveFromUnitOfWork(personToCreate);
			PersistAndRemoveFromUnitOfWork(scenario);
		}

	    protected override IPublicNote CreateAggregateWithCorrectBusinessUnit()
        {
			var dateOnly = new DateOnly(2010,4,1);
			const string text = "Agent was hit by shrinkage";

            IPublicNote note = new PublicNote(personToCreate, dateOnly, scenario, text);

            return note;
        }

        protected override void VerifyAggregateGraphProperties(IPublicNote loadedAggregateFromDatabase)
        {
            IPublicNote org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Scenario.Description, loadedAggregateFromDatabase.Scenario.Description);
            Assert.AreEqual(org.Person.Name, loadedAggregateFromDatabase.Person.Name);
            Assert.AreEqual(org.GetScheduleNote(new NoFormatting()), loadedAggregateFromDatabase.GetScheduleNote(new NoFormatting()));
            Assert.AreEqual(org.NoteDate, loadedAggregateFromDatabase.NoteDate);
        }

        protected override Repository<IPublicNote> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new PublicNoteRepository(currentUnitOfWork); 
        }

        [Test]
        public void VerifyFindByPeriodAndScenario()
        {
            IPublicNote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            var repository = new PublicNoteRepository(UnitOfWork);
            Assert.AreEqual(1, repository.Find(new DateTimePeriod(2010, 4, 1, 2010, 4, 2), note.Scenario).Count);
        }

        [Test]
        public void VerifyDoNotFindByPeriodAndScenario()
        {
            IPublicNote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            var repository = new PublicNoteRepository(UnitOfWork);
            Assert.AreEqual(0, repository.Find(new DateTimePeriod(2010, 4, 14, 2010, 4, 21), note.Scenario).Count);
        }

        [Test]
        public void VerifyFindByPeriodPersonsAndScenario()
        {
            IPublicNote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            var repository = new PublicNoteRepository(UnitOfWork);
            ICollection<IPerson> persons = new List<IPerson> {note.Person};
            Assert.AreEqual(1, repository.Find(new DateOnlyPeriod(2010, 4, 1, 2010, 4, 2), persons, note.Scenario).Count);
        }

        [Test]
        public void VerifyDoNotFindByPeriodPersonsAndScenario()
        {
            IPerson personWithoutNotes = PersonFactory.CreatePerson("Sven");
            IPublicNote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            PersistAndRemoveFromUnitOfWork(personWithoutNotes);
            var repository = new PublicNoteRepository(UnitOfWork);
            ICollection<IPerson> persons = new List<IPerson> { personWithoutNotes };
            Assert.AreEqual(0, repository.Find(new DateOnlyPeriod(2010, 4, 1, 2010, 4, 2), persons, note.Scenario).Count);
        }

        [Test]
        public void VerifyFindByDatePersonAndScenario()
        {
            IPublicNote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            var repository = new PublicNoteRepository(UnitOfWork);
            Assert.IsNotNull(repository.Find(note.NoteDate, note.Person, note.Scenario));
        }

        [Test]
        public void VerifyDoNotFindByDatePersonAndScenario()
        {
            IPublicNote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            var repository = new PublicNoteRepository(UnitOfWork);
            Assert.IsNull(repository.Find(new DateOnly(2011, 1, 1), note.Person, note.Scenario));
        }

        [Test]
        public void VerifyLoadAggregate()
        {
            IPublicNote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            var repository = new PublicNoteRepository(UnitOfWork);
            Assert.IsNotNull(note.Id);
            Assert.AreEqual(note, repository.LoadAggregate(note.Id.Value));
        }
    }
}
