using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
    public class MultiplicatorDefinitionSetRepositoryTest: RepositoryTest<IMultiplicatorDefinitionSet>
    {
        private IMultiplicator _multiplicator;

        protected override void ConcreteSetup()
        {
            _multiplicator = MultiplicatorFactory.CreateMultiplicator("OverTime", "MO", Color.DodgerBlue, MultiplicatorType.OBTime, 5);

            PersistAndRemoveFromUnitOfWork(_multiplicator);
        }

        protected override IMultiplicatorDefinitionSet CreateAggregateWithCorrectBusinessUnit()
        {
            IMultiplicatorDefinitionSet multiplicatorSet = new MultiplicatorDefinitionSet("MySet", MultiplicatorType.OBTime);
            multiplicatorSet.AddDefinition(new DateTimeMultiplicatorDefinition(_multiplicator,
                                           new DateOnly(2009, 01, 19),
                                           new DateOnly(2009,01,31),
                                           TimeSpan.FromHours(8),
                                           TimeSpan.FromHours(10)));
            multiplicatorSet.AddDefinition(new DayOfWeekMultiplicatorDefinition(_multiplicator,
                                           DayOfWeek.Monday,
                                           new TimePeriod(08, 00, 10, 00)));

            return multiplicatorSet;
        }

        protected override void VerifyAggregateGraphProperties(IMultiplicatorDefinitionSet loadedAggregateFromDatabase)
        {
            IMultiplicatorDefinitionSet multiplicator = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(multiplicator.Name, loadedAggregateFromDatabase.Name);
            Assert.AreEqual(multiplicator.DefinitionCollection.Count, loadedAggregateFromDatabase.DefinitionCollection.Count);
        }

        protected override Repository<IMultiplicatorDefinitionSet> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new MultiplicatorDefinitionSetRepository(currentUnitOfWork);
        }

        /// <summary>
        /// Verifies the name of the load sorted by.
        /// </summary>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-21
        /// </remarks>
        [Test]
        public void VerifyLoadSortedByName()
        {
            IMultiplicator multiplicator2 = MultiplicatorFactory.CreateMultiplicator("OBTime", "OB", Color.Yellow, MultiplicatorType.OBTime,10 );

            PersistAndRemoveFromUnitOfWork(multiplicator2);

            IMultiplicatorDefinitionSet multiplicatorSet = new MultiplicatorDefinitionSet("MySet", MultiplicatorType.OBTime);
            multiplicatorSet.AddDefinition(new DayOfWeekMultiplicatorDefinition(multiplicator2,
                                           DayOfWeek.Monday,
                                           new TimePeriod(08, 00, 10, 00)));
            multiplicatorSet.AddDefinition(new DateTimeMultiplicatorDefinition(_multiplicator,
                                           new DateOnly(2009, 01, 19),
                                           new DateOnly(2009, 01, 31),
                                           TimeSpan.FromHours(8),
                                           TimeSpan.FromHours(10)));
            

            PersistAndRemoveFromUnitOfWork(multiplicatorSet);

            IMultiplicatorDefinitionSetRepository rep = new MultiplicatorDefinitionSetRepository(UnitOfWork);
            IList<IMultiplicatorDefinitionSet> lst = rep.LoadAll().ToList();

            Assert.AreEqual(1, lst.Count);
            Assert.AreEqual(2, lst[0].DefinitionCollection.Count);
        }

        [Test]
        public void VerifyFindAllOvertimeDefinitions()
        {
            PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
            IMultiplicatorDefinitionSet multiplicatorSet = new MultiplicatorDefinitionSet("MyOvertimeSet", MultiplicatorType.Overtime);
            PersistAndRemoveFromUnitOfWork(multiplicatorSet);

            IMultiplicatorDefinitionSetRepository rep = new MultiplicatorDefinitionSetRepository(UnitOfWork);
            IList<IMultiplicatorDefinitionSet> lst = rep.FindAllOvertimeDefinitions();

            Assert.AreEqual(1, lst.Count);
            Assert.AreEqual("MyOvertimeSet", lst[0].Name);
        }

        [Test]
        public void VerifyCanRemoveDefinition()
        {
            var multiplicatorDefinitionSet = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(multiplicatorDefinitionSet);

            multiplicatorDefinitionSet.RemoveDefinition(multiplicatorDefinitionSet.DefinitionCollection[0]);
            PersistAndRemoveFromUnitOfWork(multiplicatorDefinitionSet);

            IMultiplicatorDefinitionSetRepository rep = new MultiplicatorDefinitionSetRepository(UnitOfWork);
            multiplicatorDefinitionSet = rep.Load(multiplicatorDefinitionSet.Id.Value);

            Assert.AreEqual(1,multiplicatorDefinitionSet.DefinitionCollection.Count);
        }
    }
}
