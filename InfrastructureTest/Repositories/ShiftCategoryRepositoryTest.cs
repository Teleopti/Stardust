using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Tests for PersonAssignmentRepository
    /// </summary>
    [TestFixture]
    [Category("BucketB")]
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
	    public void ShouldPersistRank()
	    {
			IShiftCategory cat = CreateAggregateWithCorrectBusinessUnit();
			cat.Rank = 42;
			PersistAndRemoveFromUnitOfWork(cat);

			var loadedCat = ShiftCategoryRepository.DONT_USE_CTOR(UnitOfWork).FindAll();
			Assert.AreEqual(42, loadedCat[0].Rank);
	    }

        /// <summary>
        /// Determines whether this instance can be created.
        /// </summary>
        [Test]
        public void CanCreate()
        {
            ShiftCategoryRepository.DONT_USE_CTOR(UnitOfWork);
        }

        protected override Repository<IShiftCategory> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return ShiftCategoryRepository.DONT_USE_CTOR(currentUnitOfWork);
        }
    }
}