using System.Collections.Generic;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Testclass for GroupingAbsenceRepository
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
    public class GroupingAbsenceRepositoryTest : RepositoryTest<GroupingAbsence>
    {
        /// <summary>
        /// Runs every test implemented by repositorie's concrete implementation
        /// </summary>
        protected override void ConcreteSetup()
        {
        }

        /// <summary>
        /// Determines whether this instance can be created.
        /// </summary>
        [Test]
        public void CanCreate()
        {
            new GroupingAbsenceRepository(UnitOfWork);
        }


        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override GroupingAbsence CreateAggregateWithCorrectBusinessUnit()
        {
            IList<Absence> absences = new List<Absence>();
            Absence absence1 = new Absence();
            absence1.Description = new Description("1");
            Absence absence2 = new Absence();
            absence2.Description = new Description("2");
            
            absences.Add(absence1);
            absences.Add(absence2);

            PersistAndRemoveFromUnitOfWork(absence1);
            PersistAndRemoveFromUnitOfWork(absence2);

            return GroupingAbsenceFactory.CreateGroupingAbsenceAggregate("myName", absences);
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase"></param>
        protected override void VerifyAggregateGraphProperties(GroupingAbsence loadedAggregateFromDatabase)
        {
            Assert.IsNotEmpty(loadedAggregateFromDatabase.Description.Name);
            Assert.AreEqual(2, loadedAggregateFromDatabase.AbsenceCollection.Count);
        }

        protected override Repository<GroupingAbsence> TestRepository(IUnitOfWork unitOfWork)
        {
            return new GroupingAbsenceRepository(unitOfWork);
        }
    }
}