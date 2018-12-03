using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
    [TestFixture]
    public class DifferentPersistAllTest : DatabaseTest
    {
        [Test]
        public void VerifyRootChangeInfoWhenNonRootIsDeleted()
        {
            IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(LoggedOnPerson,
                                                                                          ScenarioFactory.CreateScenarioAggregate(), new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
	        var sc = ass.ShiftCategory;
					IPayload pLoad = ass.MainActivities().First().Payload;
            try
            {
                CleanUpAfterTest();

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
					var session = uowTemp.FetchSession();
					session.Delete(ass);
					session.Delete(sc);
					session.Delete(pLoad);
					session.Delete(ass.Scenario);

					uowTemp.PersistAll();
				}
            }
        }
    }
}
