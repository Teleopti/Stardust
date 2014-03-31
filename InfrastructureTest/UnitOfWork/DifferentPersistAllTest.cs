using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
    [TestFixture]
    public class DifferentPersistAllTest : DatabaseTest
    {
        [Test]
        public void VerifyRootChangeInfoWhenNonRootIsDeleted()
        {
            IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(ScenarioFactory.CreateScenarioAggregate(),
                                                                                          LoggedOnPerson,
                                                                                          new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
	        var sc = ass.ShiftCategory;
					IPayload pLoad = ass.MainActivities().First().Payload;
            try
            {
                SkipRollback();

                PersistAndRemoveFromUnitOfWork(ass.Scenario);
                PersistAndRemoveFromUnitOfWork(ass.ShiftCategory);
                PersistAndRemoveFromUnitOfWork(ass.MainActivities().First().Payload);
                new PersonAssignmentRepository(UnitOfWork).Add(ass);
                UnitOfWork.PersistAll();
                ass.ClearMainActivities();
                IEnumerable<IRootChangeInfo> changes = UnitOfWork.PersistAll();

                UnitOfWork.Dispose();
                Assert.AreEqual(1, changes.Count());
                Assert.AreEqual(DomainUpdateType.Update, changes.First().Status);
                Assert.AreSame(ass, changes.First().Root);

            }
            finally
            {
                using (IUnitOfWork uowTemp = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
                {
                    IRepository rep = new Repository(uowTemp);
                    rep.Remove(ass);
                    rep.Remove(sc);
                    rep.Remove(pLoad);
                    rep.Remove(ass.Scenario);

                    uowTemp.PersistAll();
                }
            }
        }
    }
}
