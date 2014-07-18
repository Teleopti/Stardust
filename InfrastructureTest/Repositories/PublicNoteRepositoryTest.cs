using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("LongRunning")]
    public class PublicNoteRepositoryTest: RepositoryTest<IPublicNote>
    {
        private IPublicNoteRepository _repository;

        protected override IPublicNote CreateAggregateWithCorrectBusinessUnit()
        {
            var personToCreate = PersonFactory.CreatePerson("Kalle");
            var scenario = new Scenario("Active");
            var dateOnly = new DateOnly(2010,4,1);
            const string text = "Agent was hit by shrinkage";
            PersistAndRemoveFromUnitOfWork(personToCreate);
            PersistAndRemoveFromUnitOfWork(scenario);

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

        protected override Repository<IPublicNote> TestRepository(IUnitOfWork unitOfWork)
        {
            return new PublicNoteRepository(unitOfWork); 
        }

        [Test]
        public void VerifyFindByPeriodAndScenario()
        {
            IPublicNote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            _repository = new PublicNoteRepository(UnitOfWork);
            Assert.AreEqual(1, _repository.Find(new DateTimePeriod(2010, 4, 1, 2010, 4, 2), note.Scenario).Count);
        }

        [Test]
        public void VerifyDoNotFindByPeriodAndScenario()
        {
            IPublicNote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            _repository = new PublicNoteRepository(UnitOfWork);
            Assert.AreEqual(0, _repository.Find(new DateTimePeriod(2010, 4, 14, 2010, 4, 21), note.Scenario).Count);
        }

        [Test]
        public void VerifyFindByPeriodPersonsAndScenario()
        {
            IPublicNote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            _repository = new PublicNoteRepository(UnitOfWork);
            ICollection<IPerson> persons = new List<IPerson> {note.Person};
            Assert.AreEqual(1, _repository.Find(new DateOnlyPeriod(2010, 4, 1, 2010, 4, 2), persons, note.Scenario).Count);
        }

        [Test]
        public void VerifyDoNotFindByPeriodPersonsAndScenario()
        {
            IPerson personWithoutNotes = PersonFactory.CreatePerson("Sven");
            IPublicNote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            PersistAndRemoveFromUnitOfWork(personWithoutNotes);
            _repository = new PublicNoteRepository(UnitOfWork);
            ICollection<IPerson> persons = new List<IPerson> { personWithoutNotes };
            Assert.AreEqual(0, _repository.Find(new DateOnlyPeriod(2010, 4, 1, 2010, 4, 2), persons, note.Scenario).Count);
        }

        [Test]
        public void VerifyFindByDatePersonAndScenario()
        {
            IPublicNote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            _repository = new PublicNoteRepository(UnitOfWork);
            Assert.IsNotNull(_repository.Find(note.NoteDate, note.Person, note.Scenario));
        }

        [Test]
        public void VerifyDoNotFindByDatePersonAndScenario()
        {
            IPublicNote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            _repository = new PublicNoteRepository(UnitOfWork);
            Assert.IsNull(_repository.Find(new DateOnly(2011, 1, 1), note.Person, note.Scenario));
        }

        [Test]
        public void VerifyLoadAggregate()
        {
            IPublicNote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            _repository = new PublicNoteRepository(UnitOfWork);
            Assert.IsNotNull(note.Id);
            Assert.AreEqual(note, _repository.LoadAggregate(note.Id.Value));
        }

        [Test]
        public void ShouldCreateRepositoryWithUnitOfWorkFactory()
        {
            var repository = new PublicNoteRepository(UnitOfWorkFactory.Current);
            Assert.IsNotNull(repository);
        }
    }
}
