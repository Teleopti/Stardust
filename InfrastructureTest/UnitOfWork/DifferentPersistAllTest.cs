﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
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
            IGroupingActivity groupingActivity = GroupingActivityFactory.CreateSimpleGroupingActivity();
            IPayload pLoad = ass.MainShift.LayerCollection[0].Payload;
            try
            {
                SkipRollback();

                PersistAndRemoveFromUnitOfWork(groupingActivity);
                PersistAndRemoveFromUnitOfWork(ass.Scenario);
                PersistAndRemoveFromUnitOfWork(ass.MainShift.ShiftCategory);
                ass.MainShift.LayerCollection[0].Payload.GroupingActivity = groupingActivity;
                PersistAndRemoveFromUnitOfWork(ass.MainShift.LayerCollection[0].Payload);
                PersistAndRemoveFromUnitOfWork(groupingActivity);
                new PersonAssignmentRepository(UnitOfWork).Add(ass);
                UnitOfWork.PersistAll();
                ass.MainShift.LayerCollection.Remove(ass.MainShift.LayerCollection[0]);
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
                    rep.Remove(ass.MainShift.ShiftCategory);
                    rep.Remove(groupingActivity);
                    rep.Remove(pLoad);
                    rep.Remove(ass.Scenario);

                    uowTemp.PersistAll();
                }
            }
        }

        [Test]
        public void VerifyLicenseCheckerIsCalled()
        {
            var p = PersonFactory.CreatePerson();
            try
            {
                SkipRollback();
            	var lic = Mocks.StrictMock<ICheckLicenseAtPersist>();
                
                new Repository(UnitOfWork).Add(p);
                UnitOfWork.PersistAll(null,lic);
            	Expect.Call(() => lic.Verify(UnitOfWork, null));
                
            }
            finally
            {
                UnitOfWork.Dispose();
                using (var uowTemp = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
                {
                    IRepository rep = new Repository(uowTemp);
                    rep.Remove(p);
                    uowTemp.PersistAll();
                }
            }
        }
    }
}
