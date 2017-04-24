using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;

namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
    [TestFixture]
    public class CreateWorkShiftsFromTemplateTest
    {
        private ICreateWorkShiftsFromTemplate target;
        private IWorkShift template;
        private IList<IWorkShiftExtender> extenders;
        private IList<IWorkShiftLimiter> limiters;
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            template = mocks.StrictMock<IWorkShift>();
            extenders = new List<IWorkShiftExtender>();
            limiters = new List<IWorkShiftLimiter>();
            target = new CreateWorkShiftsFromTemplate();
        }

        #region Extender tests
        [Test]
        public void VerifySimpleExtend()
        {
            IWorkShiftExtender extender = mocks.StrictMock<IWorkShiftExtender>();
            IList<IWorkShift> retValue = new List<IWorkShift>();
            IWorkShift retShift = mocks.StrictMock<IWorkShift>();
            retValue.Add(retShift);
            extenders.Add(extender);

            using (mocks.Record())
            {
                Expect.Call(extender.ReplaceWithNewShifts(template))
                    .Return(retValue);
            }
            using (mocks.Playback())
            {
                IList<IWorkShift> ret = target.Generate(template, extenders, limiters, null);
                Assert.AreEqual(1, ret.Count);
                Assert.AreSame(retShift, ret[0]);
            }
        }

        [Test]
        public void VerifyEmptyReturnOfDefinitionStopsChainedCalls()
        {
            IWorkShiftExtender extender1 = mocks.StrictMock<IWorkShiftExtender>();
            IWorkShiftExtender extender2 = mocks.StrictMock<IWorkShiftExtender>();
            IList<IWorkShift> emptyReturn = new List<IWorkShift>();

            extenders.Add(extender1);
            extenders.Add(extender2);

            using (mocks.Record())
            {
                Expect.Call(extender1.ReplaceWithNewShifts(template))
                    .Return(emptyReturn);
            }
            using (mocks.Playback())
            {
				IList<IWorkShift> ret = target.Generate(template, extenders, limiters, null);
                Assert.AreEqual(0, ret.Count);
            }
        }

        [Test]
        public void VerifyMultipleExtenders()
        {
            IWorkShiftExtender extender1 = mocks.StrictMock<IWorkShiftExtender>();
            IWorkShiftExtender extender2 = mocks.StrictMock<IWorkShiftExtender>();
            IList<IWorkShift> retValue1 = new List<IWorkShift>();
            IList<IWorkShift> retValue2 = new List<IWorkShift>();
            IWorkShift retShift1 = mocks.StrictMock<IWorkShift>();
            IWorkShift retShift2 = mocks.StrictMock<IWorkShift>();
            retValue1.Add(retShift1);
            retValue2.Add(retShift2);
            extenders.Add(extender1);
            extenders.Add(extender2);

            using (mocks.Record())
            {
                Expect.Call(extender1.ReplaceWithNewShifts(template)).Return(retValue1);
                Expect.Call(extender2.ReplaceWithNewShifts(retShift1)).Return(retValue2);
            }
            using (mocks.Playback())
            {
				IList<IWorkShift> ret = target.Generate(template, extenders, limiters, null);
                Assert.AreEqual(1, ret.Count);
                Assert.AreSame(retShift2, ret[0]);
            }
        }
    
        #endregion

        #region Limiter tests
        [Test]
        public void VerifySimpleLimiterEnd()
        {
            IWorkShiftLimiter limiter = mocks.StrictMock<IWorkShiftLimiter>();
            IList<IWorkShift> retValue = new List<IWorkShift>();
            IWorkShift retShift = mocks.StrictMock<IWorkShift>();
            IVisualLayerCollection projTemplate = new VisualLayerCollection(null, new List<IVisualLayer>(), new ProjectionPayloadMerger());
            retValue.Add(retShift);
            limiters.Add(limiter);

            using (mocks.Record())
            {
                expectProjectionForShift(template, projTemplate);
                Expect.Call(limiter.IsValidAtStart(template, extenders)).Return(true);
                Expect.Call(limiter.IsValidAtEnd(projTemplate)).Return(false);
            }
            using (mocks.Playback())
            {
				IList<IWorkShift> ret = target.Generate(template, extenders, limiters, null);
                Assert.AreEqual(0, ret.Count);
            }
        }

        [Test]
        public void VerifySimpleLimiterStart()
        {
            IWorkShiftLimiter limiter = mocks.StrictMock<IWorkShiftLimiter>();
            limiters.Add(limiter);

            using (mocks.Record())
            {
                Expect.Call(limiter.IsValidAtStart(template, extenders)).Return(false);
            }
            using (mocks.Playback())
            {
				IList<IWorkShift> ret = target.Generate(template, extenders, limiters, null);
                Assert.AreEqual(0, ret.Count);
            }
        }

        [Test]
        public void VerifyMix()
        {
            IWorkShiftLimiter limiter1 = mocks.StrictMock<IWorkShiftLimiter>();
            IWorkShiftLimiter limiter2 = mocks.StrictMock<IWorkShiftLimiter>();
            IWorkShiftExtender extender1 = mocks.StrictMock<IWorkShiftExtender>();
            IList<IWorkShift> retValue1 = new List<IWorkShift>();
            IWorkShift retShift1 = mocks.StrictMock<IWorkShift>();
            IWorkShift retShift2 = mocks.StrictMock<IWorkShift>();
            IVisualLayerCollection projShift1 = new VisualLayerCollection(null, new List<IVisualLayer>(), new ProjectionPayloadMerger());
            IVisualLayerCollection projShift2 = new VisualLayerCollection(null, new List<IVisualLayer>(), new ProjectionPayloadMerger());
            retValue1.Add(retShift1);
            retValue1.Add(retShift2);
            limiters.Add(limiter1);
            limiters.Add(limiter2);
            extenders.Add(extender1);

            using (mocks.Record())
            {
                expectProjectionForShift(retShift1, projShift1);
                expectProjectionForShift(retShift2, projShift2);
                Expect.Call(limiter1.IsValidAtStart(template, extenders)).Return(true);
                Expect.Call(limiter2.IsValidAtStart(template, extenders)).Return(true);
                Expect.Call(extender1.ReplaceWithNewShifts(template)).Return(retValue1);
                Expect.Call(limiter1.IsValidAtEnd(projShift1)).Return(true);
                Expect.Call(limiter2.IsValidAtEnd(projShift1)).Return(true);
                Expect.Call(limiter1.IsValidAtEnd(projShift2)).Return(false);
                //Expect.Call(limiter2.IsValidAtEnd(projShift2)).Return(false); <--shouldn't run because prior returns false
            }
            using (mocks.Playback())
            {
				IList<IWorkShift> ret = target.Generate(template, extenders, limiters, null);
                Assert.AreEqual(1, ret.Count);
                Assert.AreSame(retShift1, ret[0]);
            }
        }

        #endregion

        private void expectProjectionForShift(IWorkShift shift, IVisualLayerCollection projection)
        {
            IProjectionService projectionService = mocks.StrictMock<IProjectionService>();
            Expect.Call(shift.ProjectionService())
                .Return(projectionService);
            Expect.Call(projectionService.CreateProjection())
                .Return(projection);
        }
    }
}