using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
    [TestFixture]
    public class ContractTimeLimiterTest
    {
        private ContractTimeLimiter target;
        private TimePeriod contractTimeLimit;
        private MockRepository mocks;
        private Activity actInContract;
        private Activity actNotInContract;
        private TimeSpan _segment;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            _segment = TimeSpan.FromMinutes(15);
            contractTimeLimit = new TimePeriod(8, 0, 9, 0);
            target = new ContractTimeLimiter(contractTimeLimit, _segment);
            actNotInContract = new Activity("no");
            actNotInContract.InContractTime = false;
            actInContract = new Activity("yes");
        }


        [Test]
        public void VerifyProtectedConstructor()
        {
            Assert.IsTrue(
                ReflectionHelper.HasDefaultConstructor(target.GetType()));
        }

        [Test]
        public void VerifyIsValidEndTrue()
        {
            IVisualLayerCollection projection = mocks.StrictMock<IVisualLayerCollection>();
            IVisualLayerCollection projection2 = mocks.StrictMock<IVisualLayerCollection>();
            IVisualLayerCollection projection3 = mocks.StrictMock<IVisualLayerCollection>();
            using(mocks.Record())
            {
                Expect.Call(projection.ContractTime()).Return(new TimeSpan(8, 0, 0));
                Expect.Call(projection2.ContractTime()).Return(new TimeSpan(8, 30, 0));
                Expect.Call(projection3.ContractTime()).Return(new TimeSpan(9, 0, 0));
            }
            using(mocks.Playback())
            {
                Assert.IsTrue(target.IsValidAtEnd(projection));
                Assert.IsTrue(target.IsValidAtEnd(projection2));
                Assert.IsTrue(target.IsValidAtEnd(projection3));                
            }
        }

        [Test]
        public void SegmentIsBasedOnStart()
        {
            contractTimeLimit = new TimePeriod(8, 5, 9, 0);
            var projection = mocks.StrictMock<IVisualLayerCollection>();
            var projection2 = mocks.StrictMock<IVisualLayerCollection>();
            target = new ContractTimeLimiter(contractTimeLimit, _segment);
            using (mocks.Record())
            {
                Expect.Call(projection.ContractTime()).Return(new TimeSpan(8, 20, 0));
                Expect.Call(projection2.ContractTime()).Return(new TimeSpan(8, 15, 0));
            }
            using (mocks.Playback())
            {
                Assert.IsTrue(target.IsValidAtEnd(projection));
                Assert.IsFalse(target.IsValidAtEnd(projection2));
            }
        }

        [Test]
        public void VerifyIsValidEndFalse()
        {
            IVisualLayerCollection projection = mocks.StrictMock<IVisualLayerCollection>();
            IVisualLayerCollection projection2 = mocks.StrictMock<IVisualLayerCollection>();
            IVisualLayerCollection projection3 = mocks.StrictMock<IVisualLayerCollection>();

            using (mocks.Record())
            {
                Expect.Call(projection.ContractTime()).Return(new TimeSpan(7, 59, 0));
                Expect.Call(projection2.ContractTime()).Return(new TimeSpan(9, 1, 0));
                Expect.Call(projection3.ContractTime()).Return(new TimeSpan(8, 14, 0));
            }
            using(mocks.Playback())
            {
                Assert.IsFalse(target.IsValidAtEnd(projection));
                Assert.IsFalse(target.IsValidAtEnd(projection2));
                Assert.IsFalse(target.IsValidAtEnd(projection3));
            }
        }

        [Test]
        public void CanSetProperties()
        {
            TimePeriod newTime = new TimePeriod(11,12,13,15);
            target.TimeLimit = newTime;
            Assert.AreEqual(newTime, target.TimeLimit);

            TimeSpan newSegment = TimeSpan.FromMinutes(15);
            target.LengthSegment = newSegment;
            Assert.AreEqual(newSegment, target.LengthSegment);
        }

        [Test]
        public void VerifyIsValidStartTrue()
        {
            //kan potentiellt ge 4*0.15 timmar=1h
            IWorkShiftExtender ext1 =
                new AutoPositionedActivityExtender(actNotInContract, new TimePeriodWithSegment(0, 10, 0, 15, 4),
                                                   TimeSpan.FromMinutes(1), 4);
            //nope
            IWorkShiftExtender ext2 =
                new ActivityAbsoluteStartExtender(actInContract, new TimePeriodWithSegment(2, 0, 2, 0, 10),
                                                  new TimePeriodWithSegment(11, 0, 11, 0, 10));
            //nope
            IWorkShiftExtender ext3 =
                new ActivityAbsoluteStartExtender(actInContract, new TimePeriodWithSegment(2, 0, 2, 0, 10),
                                                  new TimePeriodWithSegment(13, 0, 13, 0, 10));
            //kan potentiellt ge 2h
            IWorkShiftExtender ext4 =
                new ActivityAbsoluteStartExtender(actNotInContract, new TimePeriodWithSegment(1, 0, 2, 0, 10),
                                                  new TimePeriodWithSegment(14, 0, 14, 0, 10));
            IList<IWorkShiftExtender> extenders = new List<IWorkShiftExtender> {ext1, ext2, ext3, ext4};
            
            //tot-max 3h -> ej mer än 12h eftersom limitern satt till 8-9h
            //ok värden= 8-12h

            IWorkShift shiftok1 = WorkShiftFactory.Create(new TimeSpan(2,0,0), new TimeSpan(11,0,0));
            IWorkShift shiftok2 = WorkShiftFactory.Create(new TimeSpan(1,0,0), new TimeSpan(13,0,0));
            IWorkShift shiftfalse1 = WorkShiftFactory.Create(new TimeSpan(5,0,0), new TimeSpan(17,1,0));
            IWorkShift shiftfalse2 = WorkShiftFactory.Create(new TimeSpan(10, 0, 0), new TimeSpan(17, 59, 0));
            

            Assert.IsTrue(target.IsValidAtStart(shiftok1, extenders));
            Assert.IsTrue(target.IsValidAtStart(shiftok2, extenders));
            Assert.IsFalse(target.IsValidAtStart(shiftfalse1, extenders));
            Assert.IsFalse(target.IsValidAtStart(shiftfalse2, extenders));
        }

        [Test]
        public void VerifyICloneableEntity()
        {
            ContractTimeLimiter targetCloned = (ContractTimeLimiter)target.EntityClone();
            Assert.IsNotNull(targetCloned);
            Assert.AreEqual(target.Id, targetCloned.Id);

            target.TimeLimit = new TimePeriod(11, 12, 13, 15);
            Assert.AreNotEqual(target.TimeLimit, targetCloned.TimeLimit);

            targetCloned = (ContractTimeLimiter)target.NoneEntityClone();
            Assert.IsNotNull(targetCloned);
            Assert.IsNull(targetCloned.Id);
        }
    }
}