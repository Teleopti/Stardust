using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.DomainTest.FakeData;
using Teleopti.Ccc.Infrastructure.Foundation.Cache;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
    public class RuleSetProjectionServiceCacheTest
    {
        private IRuleSetProjectionService target;
        private MockRepository mocks;
        private IShiftCreatorService shiftCreatorService;

        [SetUp]
        public void Setup()
        {
            mocks=new MockRepository();
            shiftCreatorService = mocks.CreateMock<IShiftCreatorService>();
            target = new RuleSetProjectionServiceCache(new RuleSetProjectionService(shiftCreatorService), new DictionaryCache<IEnumerable<IVisualLayerCollection>>());
        }

        [Test]
        public void VerifyCacheIsCalledProperlyOnTransientWorkShift()
        {
            var cache = new DictionaryCache<IEnumerable<IVisualLayerCollection>>();
            IWorkShift ws = WorkShiftFactory.CreateWorkShift(TimeSpan.FromHours(1), TimeSpan.FromHours(8), new Activity("sd"));
            WorkShiftRuleSet workShiftRuleSet = new WorkShiftRuleSet(mocks.CreateMock<IWorkShiftTemplateGenerator>());

            using (mocks.Record())
            {
                Expect.Call(shiftCreatorService.Generate(workShiftRuleSet))
                    .Return(new List<IWorkShift> { ws });
            }
            using (mocks.Playback())
            {
                Assert.AreEqual(1, target.ProjectionCollection(workShiftRuleSet).Count());
                //don't call generate again
                Assert.AreEqual(1, target.ProjectionCollection(workShiftRuleSet).Count());
            }
            cache.Clear();
        }

        [Test]
        public void VerifyCacheIsUsedProperlyOnPersistedWorkShift()
        {
            var cache = new DictionaryCache<IEnumerable<IVisualLayerCollection>>();
            IWorkShift ws = WorkShiftFactory.CreateWorkShift(TimeSpan.FromHours(1), TimeSpan.FromHours(8), new Activity("sd"));
            WorkShiftRuleSet workShiftRuleSet = new WorkShiftRuleSet(mocks.CreateMock<IWorkShiftTemplateGenerator>());
            ((IEntity)workShiftRuleSet).SetId(Guid.NewGuid());

            using (mocks.Record())
            {
                Expect.Call(shiftCreatorService.Generate(workShiftRuleSet))
                    .Return(new List<IWorkShift> { ws });
            }
            using (mocks.Playback())
            {
                Assert.AreEqual(1, target.ProjectionCollection(workShiftRuleSet).Count());
                //don't call generate again
                Assert.AreEqual(1, target.ProjectionCollection(workShiftRuleSet).Count());
            }
            cache.Clear();
        }
    }
}