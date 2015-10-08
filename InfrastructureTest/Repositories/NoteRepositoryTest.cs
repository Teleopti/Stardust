using NUnit.Framework;
using System.Collections.Generic;
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
    public class NoteRepositoryTest: RepositoryTest<INote>
    {
        private INoteRepository _repository;
        protected override INote CreateAggregateWithCorrectBusinessUnit()
        {
            var person = PersonFactory.CreatePerson("Kalle");
            var scenario = new Scenario("Active");
            var dateOnly = new DateOnly(2010,4,1);
            const string text = "Agent was hit by shrinkage";
            PersistAndRemoveFromUnitOfWork(person);
            PersistAndRemoveFromUnitOfWork(scenario);
            
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

        protected override Repository<INote> TestRepository(IUnitOfWork unitOfWork)
        {
            return new NoteRepository(unitOfWork); 
        }

        [Test]
        public void VerifyFind()
        {
            INote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            _repository = new NoteRepository(UnitOfWork);
            Assert.AreEqual(1, _repository.Find(new DateTimePeriod(2010, 4, 1, 2010, 4, 2), note.Scenario).Count);
        }

        [Test]
        public void VerifyDoNotFind()
        {
            INote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            _repository = new NoteRepository(UnitOfWork);
            Assert.AreEqual(0, _repository.Find(new DateTimePeriod(2010, 4, 14, 2010, 4, 21), note.Scenario).Count);
        }

        [Test]
        public void VerifyLoadAggregate()
        {
            INote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            _repository = new NoteRepository(UnitOfWork);
            Assert.AreEqual(note, _repository.LoadAggregate(note.Id.Value));
        }

        [Test]
        public void VerifyFindOnPersons()
        {
            INote note = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(note);
            _repository = new NoteRepository(UnitOfWork);
            Assert.AreEqual(1, _repository.Find(new DateOnlyPeriod(2010, 4, 1, 2010, 4, 2), new List<IPerson>{note.Person}, note.Scenario).Count);
        }
    }
}
