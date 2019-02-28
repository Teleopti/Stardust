using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Tests for Multiplicator Repository.
    /// </summary>
    [TestFixture, Category("BucketB")]
    public class MultiplicatorRepositoryTest: RepositoryTest<IMultiplicator>
    {
        protected override void ConcreteSetup()
        {
           
        }

        protected override IMultiplicator CreateAggregateWithCorrectBusinessUnit()
        {
            IMultiplicator multiplicator = new Multiplicator(MultiplicatorType.OBTime);
            multiplicator.Description = new Description("MK","Multi Weekend");
            multiplicator.DisplayColor = Color.Blue;
            multiplicator.ExportCode = "M1";
            multiplicator.MultiplicatorValue = 10;

            return multiplicator;
        }

        protected override void VerifyAggregateGraphProperties(IMultiplicator loadedAggregateFromDatabase)
        {
            IMultiplicator multiplicator = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(multiplicator.Description.Name, loadedAggregateFromDatabase.Description.Name);
            Assert.AreEqual(multiplicator.Description.ShortName, loadedAggregateFromDatabase.Description.ShortName);
            Assert.AreEqual(multiplicator.DisplayColor.ToArgb(), loadedAggregateFromDatabase.DisplayColor.ToArgb());
            Assert.AreEqual(multiplicator.ExportCode, loadedAggregateFromDatabase.ExportCode);
            Assert.AreEqual(multiplicator.MultiplicatorType, loadedAggregateFromDatabase.MultiplicatorType);
            Assert.AreEqual(multiplicator.MultiplicatorValue, loadedAggregateFromDatabase.MultiplicatorValue);
        }

        protected override Repository<IMultiplicator> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return MultiplicatorRepository.DONT_USE_CTOR(currentUnitOfWork);
        }

        /// <summary>
        /// Verifies the name of the load sorted by.
        /// </summary>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2009-01-10
        /// </remarks>
        [Test]
        public void VerifyLoadSortedByName()
        {
            IMultiplicator multiplicator1 = MultiplicatorFactory.CreateMultiplicator("OverTime", "MO", Color.DodgerBlue,MultiplicatorType.OBTime,5);
            IMultiplicator multiplicator2 = MultiplicatorFactory.CreateMultiplicator("OBTime", "OB", Color.Yellow, MultiplicatorType.OBTime,10 );

            PersistAndRemoveFromUnitOfWork(multiplicator1);
            PersistAndRemoveFromUnitOfWork(multiplicator2);

            IMultiplicatorRepository rep = MultiplicatorRepository.DONT_USE_CTOR(UnitOfWork);
            IList<IMultiplicator> lst = rep.LoadAllSortByName();

            Assert.AreEqual(2, lst.Count);
            Assert.AreEqual("OBTime", lst[0].Description.Name);
            Assert.AreEqual("OverTime", lst[1].Description.Name);
        }

        [Test]
        public void VerifyLoadAllByTypeAndSortByName()
        {
            IMultiplicator multiplicator1 = MultiplicatorFactory.CreateMultiplicator("OverTime", "MO", Color.DodgerBlue, MultiplicatorType.OBTime, 5);
            IMultiplicator multiplicator2 = MultiplicatorFactory.CreateMultiplicator("OBTime", "OB", Color.Yellow, MultiplicatorType.OBTime, 10);

            PersistAndRemoveFromUnitOfWork(multiplicator1);
            PersistAndRemoveFromUnitOfWork(multiplicator2);

            IMultiplicatorRepository rep = MultiplicatorRepository.DONT_USE_CTOR(UnitOfWork);
            IList<IMultiplicator> lst = rep.LoadAllByTypeAndSortByName(MultiplicatorType.OBTime);

            Assert.AreEqual(2, lst.Count);
            Assert.AreEqual(MultiplicatorType.OBTime, lst[0].MultiplicatorType);
            Assert.AreEqual(MultiplicatorType.OBTime, lst[1].MultiplicatorType);
        }
    }
}
