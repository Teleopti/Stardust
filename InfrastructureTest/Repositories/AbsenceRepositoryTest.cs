using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	///<summary>
    /// Tests AbsenceRepository
    ///</summary>
    [TestFixture]
    [Category("BucketB")]
    public class AbsenceRepositoryTest : RepositoryTest<IAbsence>
    {
        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
        }


        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IAbsence CreateAggregateWithCorrectBusinessUnit()
        {
            IAbsence absence = AbsenceFactory.CreateAbsence("dummy", "DU", Color.DodgerBlue);
            absence.InPaidTime = true;
            absence.InWorkTime = true;
            absence.Requestable = true;
            absence.Confidential = true;
            absence.PayrollCode = "100";

            return absence;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IAbsence loadedAggregateFromDatabase)
        {
            IAbsence org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Description.Name, loadedAggregateFromDatabase.Description.Name);
            Assert.AreEqual(org.Description.ShortName, loadedAggregateFromDatabase.Description.ShortName);
            Assert.AreEqual(org.DisplayColor.ToArgb(), loadedAggregateFromDatabase.DisplayColor.ToArgb());
            Assert.AreEqual(org.Priority, loadedAggregateFromDatabase.Priority);
            Assert.AreEqual(org.InPaidTime,loadedAggregateFromDatabase.InPaidTime);
            Assert.AreEqual(org.InWorkTime, loadedAggregateFromDatabase.InWorkTime);
            Assert.AreEqual(org.Requestable, loadedAggregateFromDatabase.Requestable);
            Assert.AreEqual(org.PayrollCode, loadedAggregateFromDatabase.PayrollCode);
            Assert.AreEqual(org.Confidential, loadedAggregateFromDatabase.Confidential);
        }

        protected override Repository<IAbsence> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return AbsenceRepository.DONT_USE_CTOR(currentUnitOfWork);
        }

        [Test]
        public void VerifyUntouchedAbsenceTrackerIsNotDirty()
        {
            IAbsence abs = new Absence();
            abs.Description = new Description("dummy");
            PersistAndRemoveFromUnitOfWork(abs);
            Assert.IsFalse(UnitOfWork.IsDirty());
            AbsenceRepository.DONT_USE_CTOR(UnitOfWork).Get(abs.Id.Value);
            Assert.IsFalse(UnitOfWork.IsDirty());
        }



        /// <summary>
        /// Verifies that the loading abasence are shorted by it's name 
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-10-05
        /// </remarks>
        [Test]
        public void VerifyLoadSortedByName()
        {
            IAbsence absence1 = AbsenceFactory.CreateAbsence("Holiday", "HO", Color.DodgerBlue);
            IAbsence absence2 = AbsenceFactory.CreateAbsence("Traing", "TR", Color.Red);
            IAbsence absence3 = AbsenceFactory.CreateAbsence("Backup", "BK", Color.Yellow);

            PersistAndRemoveFromUnitOfWork(absence1);
            PersistAndRemoveFromUnitOfWork(absence2);
            PersistAndRemoveFromUnitOfWork(absence3);

            AbsenceRepository rep = AbsenceRepository.DONT_USE_CTOR(UnitOfWork);
            IList<IAbsence> lst = rep.LoadAllSortByName().ToList();

            Assert.AreEqual(3, lst.Count);
            Assert.AreEqual("Backup", lst[0].Description.Name);
            Assert.AreEqual("Holiday", lst[1].Description.Name);
            Assert.AreEqual("Traing", lst[2].Description.Name);

        }

        /// <summary>
        /// Verifies the name of the load requestable sort by.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-10-07
        /// </remarks>
        [Test]
        public void VerifyLoadRequestableSortByName()
        {
            IAbsence absence1 = AbsenceFactory.CreateAbsence("Illness", "Il", Color.LightBlue);
            IAbsence absence2 = AbsenceFactory.CreateRequestableAbsence("Holiday", "HO", Color.DodgerBlue);
            IAbsence absence3 = AbsenceFactory.CreateRequestableAbsence("Traing", "TR", Color.Red);
            IAbsence absence4 = AbsenceFactory.CreateRequestableAbsence("Backup", "BK", Color.Yellow);

            PersistAndRemoveFromUnitOfWork(absence1);
            PersistAndRemoveFromUnitOfWork(absence2);
            PersistAndRemoveFromUnitOfWork(absence3);
            PersistAndRemoveFromUnitOfWork(absence4);

            AbsenceRepository rep = AbsenceRepository.DONT_USE_CTOR(UnitOfWork);
            IList<IAbsence> lst = rep.LoadRequestableAbsence();

            Assert.AreEqual(3, lst.Count);
        }

        [Test]
        public void VerifyCanPersistDayTracker()
        {
            IAbsence absence = AbsenceFactory.CreateAbsence("Illness", "Il", Color.LightBlue);
            absence.Tracker = Tracker.CreateDayTracker();
            PersistAndRemoveFromUnitOfWork(absence);

            IAbsence loadedAbsence = AbsenceRepository.DONT_USE_CTOR(UnitOfWork).Load(absence.Id.Value);
            Assert.AreEqual(absence.Tracker, loadedAbsence.Tracker);
        }
        
        [Test]
        public void VerifyCanPersistTimeTracker()
        {
            IAbsence absence = AbsenceFactory.CreateAbsence("Illness", "Il", Color.LightBlue);
            absence.Tracker = Tracker.CreateTimeTracker();
            PersistAndRemoveFromUnitOfWork(absence);

            IAbsence loadedAbsence = AbsenceRepository.DONT_USE_CTOR(UnitOfWork).Load(absence.Id.Value);
            Assert.AreEqual(Tracker.CreateTimeTracker(), loadedAbsence.Tracker);
        }

        [Test]
        public void VerifyCanPersistCompTracker()
        {
            IAbsence absence = AbsenceFactory.CreateAbsence("Illness", "Il", Color.LightBlue);
            absence.Tracker = Tracker.CreateCompTracker();
            PersistAndRemoveFromUnitOfWork(absence);

            IAbsence loadedAbsence = AbsenceRepository.DONT_USE_CTOR(UnitOfWork).Load(absence.Id.Value);
            Assert.AreEqual(null /*Tracker.CreateCompTracker()*/, loadedAbsence.Tracker); //Not in use yet
        }

        [Test]
        public void ShouldFindAbsenceTrackerUsedByPesonAccount()
        {
            var rep = AbsenceRepository.DONT_USE_CTOR(UnitOfWork);
            rep.FindAbsenceTrackerUsedByPersonAccount();
        }
    }
}