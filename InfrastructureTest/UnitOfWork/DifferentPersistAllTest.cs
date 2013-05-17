﻿using System.Collections.Generic;
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
            IGroupingActivity groupingActivity = GroupingActivityFactory.CreateSimpleGroupingActivity();
	        var sc = ass.ShiftCategory;
            IPayload pLoad = ass.MainShift.LayerCollection[0].Payload;
            try
            {
                SkipRollback();

                PersistAndRemoveFromUnitOfWork(groupingActivity);
                PersistAndRemoveFromUnitOfWork(ass.Scenario);
                PersistAndRemoveFromUnitOfWork(ass.ShiftCategory);
                ass.MainShiftActivityLayers.First().Payload.GroupingActivity = groupingActivity;
                PersistAndRemoveFromUnitOfWork(ass.MainShiftActivityLayers.First().Payload);
                PersistAndRemoveFromUnitOfWork(groupingActivity);
                new PersonAssignmentRepository(UnitOfWork).Add(ass);
                UnitOfWork.PersistAll();
                ass.ClearMainShiftLayers();
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
                    rep.Remove(groupingActivity);
                    rep.Remove(pLoad);
                    rep.Remove(ass.Scenario);

                    uowTemp.PersistAll();
                }
            }
        }
    }
}
