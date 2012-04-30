using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
	[TestFixture]
	public class RuleSetProjectionEntityServiceTest
    {
        private MockRepository mocks;
        private IShiftCreatorService shiftCreatorService;

        [SetUp]
        public void Setup()
        {
            mocks=new MockRepository();
            shiftCreatorService = mocks.StrictMock<IShiftCreatorService>();
        }

        //copied from old test in workshiftrulesettest
        [Test]
        public void VerifyProjectionCollection()
        {
            IActivity testActivity = ActivityFactory.CreateActivity("test");
            IActivity breakActivity = ActivityFactory.CreateActivity("lunch");
            DateTimePeriod breakPeriod = new DateTimePeriod(new DateTime(1800, 1, 1, 4, 0, 0, DateTimeKind.Utc), new DateTime(1800, 1, 1, 5, 0, 0, DateTimeKind.Utc));

            IWorkShift ws1 = WorkShiftFactory.CreateWorkShift(TimeSpan.FromHours(1), TimeSpan.FromHours(8), testActivity);
            ws1.LayerCollection.Add(new WorkShiftActivityLayer(breakActivity, breakPeriod));
            IWorkShift ws2 = WorkShiftFactory.CreateWorkShift(TimeSpan.FromHours(1), TimeSpan.FromHours(9), testActivity);
            ws2.LayerCollection.Add(new WorkShiftActivityLayer(breakActivity, breakPeriod));
            IList<IWorkShift> listOfWorkShifts = new List<IWorkShift>();
            listOfWorkShifts.Add(ws1);
            listOfWorkShifts.Add(ws2);

            var target = new RuleSetProjectionEntityService(shiftCreatorService);
            WorkShiftRuleSet workShiftRuleSet = new WorkShiftRuleSet(mocks.StrictMock<IWorkShiftTemplateGenerator>());

            Expect.Call(shiftCreatorService.Generate(workShiftRuleSet)).Return(listOfWorkShifts);
            mocks.ReplayAll();
            var retList = target.ProjectionCollection(workShiftRuleSet);
            Assert.AreEqual(2, retList.Count());
            Assert.AreEqual(3, retList.First().VisualLayerCollection.Count());
        }
    }
}
