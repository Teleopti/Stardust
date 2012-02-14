using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using System.Collections.Generic;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    /// <summary>
    /// Tests for work shift
    /// </summary>
    [TestFixture]
    public class WorkShiftTest
    {
        private IWorkShift target;
        private ShiftCategory category;
        private MockRepository mocks;

        /// <summary>
        /// Run once for every test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            category = new ShiftCategory("just for test");
            target = new WorkShift(category);
            mocks = new MockRepository();
        }

        /// <summary>
        /// Verifies the properties defaults ok.
        /// </summary>
        [Test]
        public void VerifyPropertiesDefaultsOk()
        {
            Assert.AreEqual(0, target.LayerCollection.Count);
            Assert.AreSame(category, target.ShiftCategory);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyCategoryIsNotNull()
        {
            target = new WorkShift(null);
        }

        /// <summary>
        /// Verifies the clone method.
        /// </summary>
        [Test]
        public void VerifyClone()
        {
            DateTimePeriod period1 =
                new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            DateTimePeriod period2 =
                new DateTimePeriod(new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2003, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            target.SetId(Guid.NewGuid());
            target.LayerCollection.Add(new WorkShiftActivityLayer(ActivityFactory.CreateActivity("hej"), period1));
            target.LayerCollection.Add(new WorkShiftActivityLayer(ActivityFactory.CreateActivity("hej"), period2));
            WorkShift clone = (WorkShift) target.Clone();
            Assert.AreNotSame(target, clone);
            Assert.AreNotSame(target.LayerCollection, clone.LayerCollection);
            Assert.AreSame(target.LayerCollection[0].Payload, clone.LayerCollection[0].Payload);
            Assert.AreSame(target.LayerCollection[1].Payload, clone.LayerCollection[1].Payload);
            Assert.AreEqual(target.LayerCollection[0].Period, clone.LayerCollection[0].Period);
            Assert.AreEqual(target.LayerCollection[1].Period, clone.LayerCollection[1].Period);
            //Assert.IsNull(clone.Id);
            Assert.AreEqual(target.Id, clone.Id);
        }

        /// <summary>
        /// Verifies the base date.
        /// </summary>
        [Test]
        public void VerifyBaseDate()
        {
            Assert.AreEqual(DateTime.SpecifyKind(new DateTime(1800,1,1), DateTimeKind.Utc), WorkShift.BaseDate);
        }


        /// <summary>
        /// Protected constructor works.
        /// </summary>
        [Test]
        public void ProtectedConstructorWorks()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType()));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotAddNothingButWorkShiftActivityLayer()
        {
            target.LayerCollection.Add(new MainShiftActivityLayer(new Activity("fd"), new DateTimePeriod(2002, 1, 1, 2003, 1, 1)));
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyCanGetProjectionFromWorkShift()
        {
            DateTime startTime = WorkShift.BaseDate.Add(TimeSpan.FromHours(9));
            DateTime endTimeEarly = WorkShift.BaseDate.Add(TimeSpan.FromHours(16));
            DateTime endTimeLate = WorkShift.BaseDate.Add(TimeSpan.FromHours(17));
            Activity activity = new Activity("for test");
            activity.InContractTime = true;
            WorkShift workShift = new WorkShift(new ShiftCategory("for test"));
            WorkShiftActivityLayer layer1 = new WorkShiftActivityLayer(activity, new DateTimePeriod(startTime, endTimeEarly));
            WorkShiftActivityLayer layer2 = new WorkShiftActivityLayer(activity, new DateTimePeriod(startTime, endTimeLate));
            
            workShift.LayerCollection.Add(layer1);
            workShift.LayerCollection.Add(layer2);
            Assert.AreEqual(workShift.Projection.ContractTime(), endTimeLate.Subtract(startTime),"Verify ContractTime in projection");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyProtectedConstructorWorks()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(typeof(WorkShiftActivityLayer)));
        }

        [Test]
        public void VerifyProjectionRunsOnlyOnce()
        {
            IProjectionService projSvc = mocks.StrictMock<IProjectionService>();
            workShiftInTest workShift = new workShiftInTest(projSvc);

            using (mocks.Record())
            {
                Expect.Call(projSvc.CreateProjection())
                    .Return(new VisualLayerCollection(null, new List<IVisualLayer>(), new ProjectionPayloadMerger()))
                    .Repeat.Once();
            }

            using (mocks.Playback())
            {
                IVisualLayerCollection justDummy = workShift.Projection;
                justDummy = workShift.Projection;
            }
        }

        [Test]
        public void VerifyMakesNewProjectionAfterAddingNewWorkShiftLayer()
        {
            IProjectionService projSvc = mocks.StrictMock<IProjectionService>();
            workShiftInTest workShift = new workShiftInTest(projSvc);
            VisualLayerCollection collection = new VisualLayerCollection(null, new List<IVisualLayer>(), new ProjectionPayloadMerger());

            using (mocks.Record())
            {
                Expect.Call(projSvc.CreateProjection())
                    .Return(collection)
                    .Repeat.Twice();
            }
            using (mocks.Playback())
            {
                IVisualLayerCollection justDummy = workShift.Projection;
                workShift.LayerCollection.Add(new WorkShiftActivityLayer(new Activity("test"), new DateTimePeriod(WorkShift.BaseDate,WorkShift.BaseDate.Add(TimeSpan.FromHours(1)))));
                justDummy = workShift.Projection;
            }
        }

        [Test]
        public void VerifyToMainShiftWorks()
        {
            DateTimePeriod tp1 = new DateTimePeriod(WorkShift.BaseDate.AddHours(10),
                                                    WorkShift.BaseDate.AddHours(14));
            DateTimePeriod tp2 = new DateTimePeriod(WorkShift.BaseDate.AddHours(11),
                                                    WorkShift.BaseDate.AddHours(12));
            IActivity act1 = new Activity("hej");
            act1.SetId(Guid.NewGuid());
            IActivity act2 = new Activity("da");
            act2.SetId(Guid.NewGuid());

            ILayer<IActivity> layer1 =
                new WorkShiftActivityLayer(act1, tp1);
            ILayer<IActivity> layer2 =
                new WorkShiftActivityLayer(act2, tp2);

            target.LayerCollection.Add(layer1);
            target.LayerCollection.Add(layer2);
            ICccTimeZoneInfo timeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"));
            DateTime baseDateLocal = TimeZoneHelper.ConvertFromUtc(WorkShift.BaseDate, timeZoneInfo).Date.AddDays(77);
            IMainShift mainShift = target.ToMainShift(baseDateLocal, timeZoneInfo);
            Assert.AreEqual(2, mainShift.LayerCollection.Count);
            Assert.AreEqual(category, mainShift.ShiftCategory);
            Assert.AreEqual(layer1.Payload, mainShift.LayerCollection[0].Payload);
            Assert.AreEqual(WorkShift.BaseDate.AddHours(8).TimeOfDay, TimeZoneHelper.ConvertFromUtc(mainShift.LayerCollection[1].Period.StartDateTime).TimeOfDay);
            Assert.AreEqual(category, mainShift.ShiftCategory);
            Assert.IsNotNull(mainShift.LayerCollection[0].Parent);
        }

        [Test]
        public void VerifyToMainShiftWorksOnDaylightSavingTimeDate()
        {
            DateTimePeriod tp1 = new DateTimePeriod(WorkShift.BaseDate.AddHours(0).AddMinutes(15),
                                                    WorkShift.BaseDate.AddHours(2).AddMinutes(15));
            DateTimePeriod tp2 = new DateTimePeriod(WorkShift.BaseDate.AddHours(2).AddMinutes(15),
                                                    WorkShift.BaseDate.AddHours(8));

            IActivity act1 = new Activity("hej");
            act1.SetId(Guid.NewGuid());
            IActivity act2 = new Activity("da");
            act2.SetId(Guid.NewGuid());
            ILayer<IActivity> layer1 =
                new WorkShiftActivityLayer(act1, tp1);
            ILayer<IActivity> layer2 =
                new WorkShiftActivityLayer(act2, tp2);
            target.LayerCollection.Add(layer1);
            target.LayerCollection.Add(layer2);
            ICccTimeZoneInfo timeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            DateTime baseDateLocal = new DateTime(2009,3,29);
            IMainShift mainShift = target.ToMainShift(baseDateLocal, timeZoneInfo);
            IVisualLayerCollection layerCollectionWorkShift = target.ProjectionService().CreateProjection();
            IVisualLayerCollection layerCollection = mainShift.ProjectionService().CreateProjection();
            Assert.AreEqual(WorkShift.BaseDate.AddHours(1).AddMinutes(15).TimeOfDay, mainShift.LayerCollection[1].Period.StartDateTime.TimeOfDay);
            Assert.AreEqual(WorkShift.BaseDate.AddHours(7).TimeOfDay, mainShift.LayerCollection[1].Period.EndDateTime.TimeOfDay);
            Assert.AreEqual(layerCollectionWorkShift.ContractTime(), layerCollection.ContractTime());
        }

        [Test]
        public void VerifyToMainShiftWorksOnDaylightSavingTimeDateForJordanStandardTime()
        {
            DateTimePeriod tp1 = new DateTimePeriod(WorkShift.BaseDate.AddHours(1).AddMinutes(15),
                                                    WorkShift.BaseDate.AddHours(2).AddMinutes(15));
            DateTimePeriod tp2 = new DateTimePeriod(WorkShift.BaseDate.AddHours(2).AddMinutes(15),
                                                    WorkShift.BaseDate.AddHours(8));

            IActivity act1 = new Activity("hej");
            act1.SetId(Guid.NewGuid());
            IActivity act2 = new Activity("da");
            act2.SetId(Guid.NewGuid());

            ILayer<IActivity> layer1 = new WorkShiftActivityLayer(act1, tp1);
            ILayer<IActivity> layer2 = new WorkShiftActivityLayer(act2, tp2);
            target.LayerCollection.Add(layer1);
            target.LayerCollection.Add(layer2);
            
            ICccTimeZoneInfo timeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time"));
            
            DateTime baseDateLocal = new DateTime(2011, 4, 1);
            IMainShift mainShift = target.ToMainShift(baseDateLocal, timeZoneInfo);
            IVisualLayerCollection layerCollectionWorkShift = target.ProjectionService().CreateProjection();
            IVisualLayerCollection layerCollection = mainShift.ProjectionService().CreateProjection();
            Assert.AreEqual(tp1.StartDateTime.Subtract(WorkShift.BaseDate), mainShift.LayerCollection[0].Period.StartDateTimeLocal(timeZoneInfo).TimeOfDay);
            Assert.AreEqual(tp2.EndDateTime.Subtract(WorkShift.BaseDate), mainShift.LayerCollection[1].Period.EndDateTimeLocal(timeZoneInfo).TimeOfDay);
            Assert.AreEqual(layerCollectionWorkShift.ContractTime(), layerCollection.ContractTime());
        }

        [Test]
        public void VerifyToMainShiftWorksBeforeDaylightSavingTimeDateForJordanStandardTime()
        {
            DateTimePeriod tp1 = new DateTimePeriod(WorkShift.BaseDate.AddHours(1).AddMinutes(15),
                                                    WorkShift.BaseDate.AddHours(2).AddMinutes(15));
            DateTimePeriod tp2 = new DateTimePeriod(WorkShift.BaseDate.AddHours(2).AddMinutes(15),
                                                    WorkShift.BaseDate.AddHours(8));

            IActivity act1 = new Activity("hej");
            act1.SetId(Guid.NewGuid());
            IActivity act2 = new Activity("da");
            act2.SetId(Guid.NewGuid());

            ILayer<IActivity> layer1 = new WorkShiftActivityLayer(act1, tp1);
            ILayer<IActivity> layer2 = new WorkShiftActivityLayer(act2, tp2);
            target.LayerCollection.Add(layer1);
            target.LayerCollection.Add(layer2);

            ICccTimeZoneInfo timeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time"));

            DateTime baseDateLocal = new DateTime(2011, 3, 31);
            IMainShift mainShift = target.ToMainShift(baseDateLocal, timeZoneInfo);
            IVisualLayerCollection layerCollectionWorkShift = target.ProjectionService().CreateProjection();
            IVisualLayerCollection layerCollection = mainShift.ProjectionService().CreateProjection();
            Assert.AreEqual(tp1.StartDateTime.Subtract(WorkShift.BaseDate), mainShift.LayerCollection[0].Period.StartDateTimeLocal(timeZoneInfo).TimeOfDay);
            Assert.AreEqual(tp2.EndDateTime.Subtract(WorkShift.BaseDate), mainShift.LayerCollection[1].Period.EndDateTimeLocal(timeZoneInfo).TimeOfDay);
            Assert.AreEqual(layerCollectionWorkShift.ContractTime(), layerCollection.ContractTime());
        }

        [Test]
        public void VerifyToMainShiftWorksOnDaylightSavingTimeDateEnd()
        {
            DateTimePeriod tp1 = new DateTimePeriod(WorkShift.BaseDate.AddHours(2).AddMinutes(15),
                                                    WorkShift.BaseDate.AddHours(5));
            IActivity act1 = new Activity("hej");
            act1.InContractTime = true;
            act1.SetId(Guid.NewGuid());
            ILayer<IActivity> layer1 =
                new WorkShiftActivityLayer(act1, tp1);

            target.LayerCollection.Add(layer1);
            //target.LayerCollection.Add(layer2);
            ICccTimeZoneInfo timeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            DateTime baseDateLocal = new DateTime(2009, 10, 25);
            IMainShift mainShift = target.ToMainShift(baseDateLocal, timeZoneInfo);
            IVisualLayerCollection layerCollectionWorkShift = target.ProjectionService().CreateProjection();
            IVisualLayerCollection layerCollection = mainShift.ProjectionService().CreateProjection();
            Assert.AreEqual(WorkShift.BaseDate.AddHours(1).AddMinutes(15).TimeOfDay, mainShift.LayerCollection[0].Period.StartDateTime.TimeOfDay);
            Assert.AreEqual(layerCollectionWorkShift.ContractTime(), layerCollection.ContractTime());
        }

        [Test]
        public void VerifyToMainShiftOverMidnightWorks()
        {
            DateTimePeriod tp1 = new DateTimePeriod(WorkShift.BaseDate.AddHours(22),
                                        WorkShift.BaseDate.AddHours(28));
            DateTimePeriod tp2 = new DateTimePeriod(WorkShift.BaseDate.AddHours(25),
                                                    WorkShift.BaseDate.AddHours(26));

            IActivity act1 = new Activity("hej");
            IActivity act2 = new Activity("da");

            ILayer<IActivity> layer1 = new WorkShiftActivityLayer(act1, tp1);
            ILayer<IActivity> layer2 = new WorkShiftActivityLayer(act2, tp2);
            target.LayerCollection.Add(layer1);
            target.LayerCollection.Add(layer2);
            ICccTimeZoneInfo timeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"));
            DateTime shiftDate = new DateTime(2000,1,1).ToUniversalTime();
            DateTime baseDateLocal = TimeZoneHelper.ConvertFromUtc(shiftDate, timeZoneInfo);
            IMainShift mainShift = target.ToMainShift(baseDateLocal, timeZoneInfo);
            Assert.AreEqual(2, mainShift.LayerCollection.Count);
            Assert.AreEqual(new DateTimePeriod(shiftDate.AddHours(22), shiftDate.AddHours(28)), mainShift.LayerCollection[0].Period);
            Assert.AreEqual(new DateTimePeriod(shiftDate.AddHours(25), shiftDate.AddHours(26)), mainShift.LayerCollection[1].Period);
        }

        [Test]
        public void VerifyToTimePeriod()
        {
            target = WorkShiftFactory.CreateWorkShift(TimeSpan.FromHours(4),
                TimeSpan.FromDays(1).Add(TimeSpan.FromHours(6)), new Activity("Test"));
            TimePeriod? result = target.ToTimePeriod();
            Assert.AreEqual(TimeSpan.FromHours(4), result.Value.StartTime);
            Assert.AreEqual(TimeSpan.FromDays(1).Add(TimeSpan.FromHours(6)), result.Value.EndTime);
        }


        internal class workShiftInTest : WorkShift
        {
            IProjectionService _projSvc;
            public workShiftInTest(IProjectionService projSvc) 
            {
                _projSvc = projSvc;
            }

            public override IProjectionService ProjectionService()
            {
                return _projSvc;
            }
        }
    }
}