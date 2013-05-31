using System;
using System.Collections.Generic;
using System.Drawing;
using NHibernate.Criterion;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Tests for AssignmentRepository
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), TestFixture]
    [Category("LongRunning")]
    public class PersonAssignmentRepositoryTest : RepositoryTest<IPersonAssignment>
    {
        private IPersonAssignmentRepository _rep;
        private IActivity _dummyActivity;
        private IPerson _dummyAgent;
        private IPerson _dummyAgent2;
        private IScenario _dummyScenario;
        private IShiftCategory _dummyCat;
        private IShiftCategory _dummyCategory;
        private IGroupingActivity _groupAct;
        private IMultiplicatorDefinitionSet _definitionSet;


        private void cleanUp(IAggregateRoot loaded)
        {
            UnitOfWork.Clear();
            using(var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var repRemove = new Repository(uow);
                _groupAct = new GroupingActivityRepository(uow).Load(_groupAct.Id.Value);
                repRemove.Remove(_dummyActivity);
                repRemove.Remove(_groupAct);
                repRemove.Remove(_dummyCategory);
                repRemove.Remove(_dummyCat);
                repRemove.Remove(loaded);
                repRemove.Remove(_dummyAgent);
                repRemove.Remove(_dummyAgent2);
                repRemove.Remove(_dummyScenario);
                repRemove.Remove(_dummyAgent.PersonPeriodCollection[0].PersonContract.PartTimePercentage);
                repRemove.Remove(_dummyAgent.PersonPeriodCollection[0].PersonContract.ContractSchedule);
                repRemove.Remove(_dummyAgent.PersonPeriodCollection[0].PersonContract.Contract);
                repRemove.Remove(_dummyAgent.PersonPeriodCollection[0].Team);
                repRemove.Remove(_dummyAgent.PersonPeriodCollection[0].Team.Site);
                repRemove.Remove(_definitionSet);
                uow.PersistAll();                
            }
        }

        protected override void ConcreteSetup()
        {
            _dummyCat = ShiftCategoryFactory.CreateShiftCategory("dummyCat");
            _rep = RepositoryFactory.CreatePersonAssignmentRepository(UnitOfWork);
            _groupAct = new GroupingActivity("f");
            PersistAndRemoveFromUnitOfWork(_groupAct);
            _dummyActivity = ActivityFactory.CreateActivity("dummy", Color.DodgerBlue);
            _dummyActivity.GroupingActivity = _groupAct;
            PersistAndRemoveFromUnitOfWork(_dummyActivity);
            _dummyAgent = PersonFactory.CreatePerson("m");
            _dummyAgent2 = PersonFactory.CreatePerson("n");
            _dummyScenario = ScenarioFactory.CreateScenarioAggregate("Default", false);
            _dummyCategory = ShiftCategoryFactory.CreateShiftCategory("Morning");
            _definitionSet = new MultiplicatorDefinitionSet("sdf", MultiplicatorType.Overtime);

            PersistAndRemoveFromUnitOfWork(_dummyCategory);
            PersistAndRemoveFromUnitOfWork(_dummyAgent2);
            PersistAndRemoveFromUnitOfWork(_dummyScenario);
            PersistAndRemoveFromUnitOfWork(_dummyCat);
            PersistAndRemoveFromUnitOfWork(_definitionSet);

            PersonFactory.AddDefinitionSetToPerson(_dummyAgent, _definitionSet);
            IPersonPeriod per = _dummyAgent.Period(new DateOnly(2000, 1, 1));
            ISite site = SiteFactory.CreateSimpleSite("df");
            PersistAndRemoveFromUnitOfWork(site);
            per.Team.Site = site;
            PersistAndRemoveFromUnitOfWork(per.PersonContract.PartTimePercentage);
            PersistAndRemoveFromUnitOfWork(per.PersonContract.ContractSchedule);
            PersistAndRemoveFromUnitOfWork(per.PersonContract.Contract);
            PersistAndRemoveFromUnitOfWork(per.Team);
            PersistAndRemoveFromUnitOfWork(_dummyAgent);
        }

        protected override IPersonAssignment CreateAggregateWithCorrectBusinessUnit()
        {
            MainShift mainShift = MainShiftFactory.CreateMainShift(_dummyCat);
					mainShift.LayerCollection.Add(new MainShiftActivityLayer(_dummyActivity, new DateTimePeriod(2000,1,1,2000,1,2)));
            IList<IPersonalShift> persShifts = new List<IPersonalShift> {new PersonalShift(), new PersonalShift()};

        	IPersonAssignment ass = PersonAssignmentFactory.CreatePersonAssignmentAggregate(_dummyAgent,
                                                                                        mainShift,
                                                                                        persShifts,
                                                                                        _dummyScenario,
																																												new DateOnly(2000,1,1));
            IOvertimeShift ot = new OvertimeShift();
            ass.AddOvertimeShift(ot);
            ot.LayerCollection.Add(new OvertimeShiftActivityLayer(_dummyActivity, new DateTimePeriod(2000, 1, 1, 2000, 1, 2), _definitionSet));
            return ass;
        }

        protected override void VerifyAggregateGraphProperties(IPersonAssignment loadedAggregateFromDatabase)
        {
            IPersonAssignment org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Person.Name, loadedAggregateFromDatabase.Person.Name);
            Assert.AreEqual(org.PersonalShiftCollection.Count, loadedAggregateFromDatabase.PersonalShiftCollection.Count);
            Assert.AreEqual(org.OvertimeShiftCollection.Count, loadedAggregateFromDatabase.OvertimeShiftCollection.Count);
            Assert.AreEqual(org.OvertimeShiftCollection[0].LayerCollection.Count, loadedAggregateFromDatabase.OvertimeShiftCollection[0].LayerCollection.Count);
        }

		 [Test]
		 public void VerifyDatabasePeriod()
		 {
		 	PersonAssignment ass = (PersonAssignment) CreateAggregateWithCorrectBusinessUnit();
			 PersistAndRemoveFromUnitOfWork(ass);
			 Session.Refresh(ass);
		 	ass.DatabasePeriod.Should().Be.EqualTo(ass.Period);
		 }

			[Test]
        public void VerifyLoadGraphById()
        {
            IPersonAssignment ass = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(ass);

            IPersonAssignment loaded = new PersonAssignmentRepository(UnitOfWork).LoadAggregate(ass.Id.Value);
            Assert.AreEqual(ass.Id, loaded.Id);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loaded.MainShiftActivityLayers));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loaded.PersonalShiftCollection));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loaded.PersonalShiftCollection[0].LayerCollection));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loaded.ShiftCategory));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loaded.ShiftCategory.DayOfWeekJusticeValues));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loaded.OvertimeShiftCollection));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loaded.OvertimeShiftCollection[0].LayerCollection));
        }

        [Test]
        public void VerifyLoadGraphByIdReturnsNullIfNotExists()
        {
            IPersonAssignment loaded = new PersonAssignmentRepository(UnitOfWork).LoadAggregate(Guid.NewGuid());
            Assert.IsNull(loaded);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotUseNullAsScenario()
        {
            _rep.Find(new DateTimePeriod(2000, 1, 1, 2001, 1, 1), null);
        }


        [Test]
        public void CanFindAssignmentsByDatesAndScenario()
        {
            var notToFindScenario = new Scenario("NotToFind");
            var searchPeriod =
                new DateTimePeriod(new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 2, 0, 0, 0, DateTimeKind.Utc));

            ////////////setup////////////////////////////////////////////////////////////////
            IPersonAssignment agAssValid = PersonAssignmentFactory.CreateAssignmentWithMainShift(
                _dummyActivity,
                _dummyAgent,
                new DateTimePeriod(new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 2, 0, 0, 0, DateTimeKind.Utc)),
                _dummyCategory,
                _dummyScenario);
            agAssValid.AddPersonalShift(PersonalShiftFactory.CreatePersonalShift(_dummyActivity,
                                                                                 new DateTimePeriod(2007, 1, 1, 2007, 1, 2)));
            agAssValid.AddPersonalShift(PersonalShiftFactory.CreatePersonalShift(_dummyActivity,
                                                                                 new DateTimePeriod(2007, 1, 1, 2007, 1, 2)));
            agAssValid.PersonalShiftCollection[0].LayerCollection.Add(
                new PersonalShiftActivityLayer(_dummyActivity, new DateTimePeriod(2007, 1, 1, 2007, 1, 2)));
            agAssValid.AddOvertimeShift(new OvertimeShift());
            agAssValid.OvertimeShiftCollection[0].LayerCollection.Add(new OvertimeShiftActivityLayer(_dummyActivity, new DateTimePeriod(2007, 1, 1, 2007, 1, 2), _definitionSet));

            IPersonAssignment agAssInvalid = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(
                _dummyActivity,
                _dummyAgent,
                new DateTimePeriod(2006, 12, 31, 2007, 1, 1),
                notToFindScenario);
            IPersonAssignment agAssInvalid2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(
                _dummyActivity,
                _dummyAgent,
                new DateTimePeriod(new DateTime(2007, 1, 2, 5, 0, 0, DateTimeKind.Utc), new DateTime(2007, 1, 3, 0, 0, 0, DateTimeKind.Utc)),
                _dummyCategory,
                notToFindScenario);
            IPersonAssignment agAssInvalid3 = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(
                _dummyActivity,
                _dummyAgent,
                new DateTimePeriod(2007, 1, 2, 2007, 1, 3),
                notToFindScenario);

            PersistAndRemoveFromUnitOfWork(notToFindScenario);
            PersistAndRemoveFromUnitOfWork(agAssValid);
            PersistAndRemoveFromUnitOfWork(agAssInvalid);
            PersistAndRemoveFromUnitOfWork(agAssInvalid3);
            PersistAndRemoveFromUnitOfWork(agAssInvalid2);
            /////////////////////////////////////////////////////////////////////////////////

            IList<IPersonAssignment> retList = new List<IPersonAssignment>(_rep.Find(searchPeriod, _dummyScenario));

            Assert.IsTrue(retList.Contains(agAssValid));
            Assert.AreEqual(1, retList.Count);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(_dummyActivity));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(_dummyCategory));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(retList[0].OvertimeShiftCollection[0].LayerCollection));
        }

        [Test]
        public void VerifyAssignmentsCannotBeReadForDeletedPerson()
        {
            IPersonAssignment agAssValid = PersonAssignmentFactory.CreateAssignmentWithMainShift(
                    _dummyActivity,
                    _dummyAgent,
                    new DateTimePeriod(new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2007, 1, 2, 0, 0, 0, DateTimeKind.Utc)),
                    _dummyCategory,
                    _dummyScenario);
            PersistAndRemoveFromUnitOfWork(agAssValid);
            new PersonRepository(UnitOfWork).Remove(_dummyAgent);
            PersistAndRemoveFromUnitOfWork(_dummyAgent);

            Assert.AreEqual(0, _rep.Find(new DateTimePeriod(2000, 1, 1, 2010, 1, 1), _dummyScenario).Count);
        }

        /// <summary>
        /// Determines whether this instance [can find agent assignments by dates and scenario].
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-03-07
        /// </remarks>
        [Test]
        public void CanFindAgentAssignmentsByDatesAndScenario()
        {
            var notToFindScenario = new Scenario("NotToFind");
            var searchPeriod =
                new DateTimePeriod(new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 2, 4, 0, 0, DateTimeKind.Utc));

            #region setup test data

            // Assignments of _dummyAgent Data
            IPersonAssignment agAssValid = PersonAssignmentFactory.CreateAssignmentWithMainShift(
                _dummyActivity,
                _dummyAgent,
                new DateTimePeriod(new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 2, 0, 0, 0, DateTimeKind.Utc)),
                _dummyCategory,
                _dummyScenario);
            agAssValid.AddPersonalShift(PersonalShiftFactory.CreatePersonalShift(_dummyActivity,
                                                                                 new DateTimePeriod(2007, 1, 1, 2007, 1, 2)));
            agAssValid.AddPersonalShift(PersonalShiftFactory.CreatePersonalShift(_dummyActivity,
                                                                                 new DateTimePeriod(2007, 1, 1, 2007, 1, 2)));
            agAssValid.PersonalShiftCollection[0].LayerCollection.Add(
                new PersonalShiftActivityLayer(_dummyActivity, new DateTimePeriod(2007, 1, 1, 2007, 1, 2)));

            IPersonAssignment agAssInvalid = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(
                _dummyActivity,
                _dummyAgent,
                new DateTimePeriod(2006, 12, 31, 2007, 1, 1),
                notToFindScenario);
            IPersonAssignment agAssInvalid2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(
                _dummyActivity,
                _dummyAgent,
                new DateTimePeriod(new DateTime(2007, 1, 2, 5, 0, 0, DateTimeKind.Utc), new DateTime(2007, 1, 3, 0, 0, 0, DateTimeKind.Utc)),
                _dummyCategory,
                notToFindScenario);
            IPersonAssignment agAssInvalid3 = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(
                _dummyActivity,
                _dummyAgent,
                new DateTimePeriod(2007, 1, 2, 2007, 1, 3),
                notToFindScenario);

            //Assignments of _dummyAgent2 
            IPersonAssignment agAssInvalid4 = PersonAssignmentFactory.CreateAssignmentWithMainShift(
                _dummyActivity,
                _dummyAgent2,
                new DateTimePeriod(new DateTime(2007, 1, 1, 4, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 2, 4, 0, 0, DateTimeKind.Utc)),
                _dummyCategory,
                _dummyScenario);

            PersistAndRemoveFromUnitOfWork(notToFindScenario);
            PersistAndRemoveFromUnitOfWork(agAssValid);
            PersistAndRemoveFromUnitOfWork(agAssInvalid);
            PersistAndRemoveFromUnitOfWork(agAssInvalid3);
            PersistAndRemoveFromUnitOfWork(agAssInvalid2);
            PersistAndRemoveFromUnitOfWork(agAssInvalid4);

            #endregion

            IList<IPerson> persons = new List<IPerson> {_dummyAgent};
        	ICollection<IPersonAssignment> retList = _rep.Find(persons, searchPeriod, _dummyScenario);
            Assert.IsTrue(retList.Contains(agAssValid));
            Assert.AreEqual(1, retList.Count);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(_dummyActivity));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(_dummyCategory));
        }

        /// <summary>
        /// Verifies that version works with no reloading.
        /// </summary>
        [Test]
        public void VerifyVersionWorksWithNoReloading()
        {
            IPersonAssignment loaded = null;
            try
            {
                SkipRollback();

                //setup
                IPersonAssignment org = CreateAggregateWithCorrectBusinessUnit();
                IVersioned casted = org;
                _rep.Add(org);
                UnitOfWork.PersistAll();
                Assert.AreEqual(1, casted.Version);

                //Do change
                loaded = _rep.Load(org.Id.Value);
                loaded.AddPersonalShift(new PersonalShift());
                UnitOfWork.PersistAll();
                loaded = _rep.Load(org.Id.Value);
                Assert.AreEqual(2, casted.Version);

                //Check history
                Assert.IsNotNull(loaded.UpdatedOn);
                Assert.AreSame(LoggedOnPerson, loaded.UpdatedBy);

                UnitOfWork.PersistAll();

            }
            finally
            {
                cleanUp(loaded);
            }
        }

        /// <summary>
        /// Verifies that version works when aggregate reloaded.
        /// </summary>
        [Test]
        public void VerifyVersionWorksWhenAggregateReloaded()
        {
            IPersonAssignment loaded = null;
            try
            {
                SkipRollback();

                //setup
                IPersonAssignment org = CreateAggregateWithCorrectBusinessUnit();
                _rep.Add(org);
                UnitOfWork.PersistAll();
                UnitOfWork.Remove(org);
                Assert.AreEqual(1, (org).Version);

                //Do change
                loaded = _rep.LoadAggregate(org.Id.Value);
                loaded.AddPersonalShift(new PersonalShift());
                UnitOfWork.PersistAll();
                UnitOfWork.Remove(loaded);
                loaded = _rep.LoadAggregate(org.Id.Value);
                Assert.AreEqual(2, (loaded).Version);

                Assert.IsNotNull(loaded.UpdatedOn);
                Assert.AreEqual(LoggedOnPerson, loaded.UpdatedBy);
            }
            finally
            {
                cleanUp(loaded);
            }
        }


        /// <summary>
        /// Verifies the cascade delete works so removed shift reference deletes layers from database.
        /// </summary>
        [Test]
        public void VerifyCascadeDeleteWorksSoRemovedShiftReferenceDeletesLayersFromDatabase()
        {
            //Setup
            IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(
                _dummyActivity,
                _dummyAgent,
                new DateTimePeriod(2001, 1, 1, 2001, 1, 2),
                _dummyCategory,
                _dummyScenario);
            PersistAndRemoveFromUnitOfWork(ass);

            //Load
            IPersonAssignment loaded = _rep.Load(ass.Id.Value);
            bool mainShiftExists = (loaded.ShiftCategory != null);
            Assert.IsTrue(mainShiftExists); //ensures factory method creates mainshift
            int noOfPersonalShift = loaded.PersonalShiftCollection.Count;
            Assert.GreaterOrEqual(1, noOfPersonalShift); //ensures factory method creates persShifts

            //Remove shifts
            loaded.ClearMainShiftLayers();
            PersistAndRemoveFromUnitOfWork(loaded);

            foreach (int items in Session.CreateCriteria(typeof(MainShift))
                                        .SetProjection(Projections.RowCount())
                                        .List<int>())
            {
                //no mainshifts 
                Assert.AreEqual(0, items);
            }
            foreach (int items in Session.CreateCriteria(typeof(ActivityLayer))
                            .SetProjection(Projections.RowCount())
                            .List<int>())
            {
                //no layers
                Assert.AreEqual(0, items);
            }
        }

        [Test]
        public void ShouldCreateRepositoryWithUnitOfWorkFactory()
        {
            IPersonAssignmentRepository personAssignmentRepository = new PersonAssignmentRepository(UnitOfWorkFactory.Current);
            Assert.IsNotNull(personAssignmentRepository);
        }

        protected override Repository<IPersonAssignment> TestRepository(IUnitOfWork unitOfWork)
        {
            return new PersonAssignmentRepository(unitOfWork);
        }
    }
}
