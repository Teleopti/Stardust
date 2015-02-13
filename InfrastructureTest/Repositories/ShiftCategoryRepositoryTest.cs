using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Tests for PersonAssignmentRepository
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
    public class ShiftCategoryRepositoryTest : RepositoryTest<IShiftCategory>
    {
        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            //ShiftCategoryRepository rep = new ShiftCategoryRepository(UnitOfWork);
        }


        /// <summary>
        /// Creates an aggreagte using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IShiftCategory CreateAggregateWithCorrectBusinessUnit()
        {
	        return ShiftCategoryFactory.CreateShiftCategory("Morning");
        }


        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IShiftCategory loadedAggregateFromDatabase)
        {
            IShiftCategory org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Description.Name, loadedAggregateFromDatabase.Description.Name);
            Assert.AreEqual(org.DisplayColor.ToArgb(), loadedAggregateFromDatabase.DisplayColor.ToArgb());
            Assert.AreEqual(org.Description.ShortName, loadedAggregateFromDatabase.Description.ShortName);
        }

        [Test]
        public void VerifyDayValuesCanBeSavedAndRead()
        {
            IShiftCategory cat = CreateAggregateWithCorrectBusinessUnit();
            cat.DayOfWeekJusticeValues[DayOfWeek.Thursday] = 4;
            cat.DayOfWeekJusticeValues[DayOfWeek.Sunday] = 2;
            PersistAndRemoveFromUnitOfWork(cat);

            IShiftCategory loadedCat = TestRepository(UnitOfWork).Load(cat.Id.Value);
            Assert.AreEqual(4, loadedCat.DayOfWeekJusticeValues[DayOfWeek.Thursday]);
            Assert.AreEqual(2, loadedCat.DayOfWeekJusticeValues[DayOfWeek.Sunday]);
        }

        [Test]
        public void VerifyDayValuesCanBeSavedAndRead1()
        {
            IShiftCategory cat = CreateAggregateWithCorrectBusinessUnit();
            cat.DayOfWeekJusticeValues[DayOfWeek.Thursday] = 4;
            cat.DayOfWeekJusticeValues[DayOfWeek.Sunday] = 2;
            PersistAndRemoveFromUnitOfWork(cat);

            IList<IShiftCategory> loadedCat = ((IShiftCategoryRepository)TestRepository(UnitOfWork)).FindAll();
            Assert.AreEqual(4, loadedCat[0].DayOfWeekJusticeValues[DayOfWeek.Thursday]);
            Assert.AreEqual(2, loadedCat[0].DayOfWeekJusticeValues[DayOfWeek.Sunday]);
        }

		[Test]
	    public void ShouldPersistRank()
	    {
			IShiftCategory cat = CreateAggregateWithCorrectBusinessUnit();
			cat.Rank = 42;
			PersistAndRemoveFromUnitOfWork(cat);

			var loadedCat = ((IShiftCategoryRepository)TestRepository(UnitOfWork)).FindAll();
			Assert.AreEqual(42, loadedCat[0].Rank);
	    }

        /// <summary>
        /// Determines whether this instance can be created.
        /// </summary>
        [Test]
        public void CanCreate()
        {
            new ShiftCategoryRepository(UnitOfWork);
        }

        protected override Repository<IShiftCategory> TestRepository(IUnitOfWork unitOfWork)
        {
            return new ShiftCategoryRepository(unitOfWork);
        }
    }
}