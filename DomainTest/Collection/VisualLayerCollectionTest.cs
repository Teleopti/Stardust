﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Collection
{
    [TestFixture]
    public class VisualLayerCollectionTest
    {
        private IVisualLayerCollection target;
        private IList<IVisualLayer> internalCollection;
        private IActivity dummyPayload;
        private IPerson dummyPerson;
        private IVisualLayerFactory visualLayerFactory;
        private IVisualLayerFactory visualLayerOvertimeFactory;

        [SetUp]
        public void Setup()
        {
            visualLayerFactory = new VisualLayerFactory();
            visualLayerOvertimeFactory = new VisualLayerOvertimeFactory();
            dummyPerson = PersonFactory.CreatePerson("dummy");
            internalCollection = new List<IVisualLayer>();
            target = new VisualLayerCollection(dummyPerson, internalCollection, new ProjectionPayloadMerger())
                         {
                             PeriodOptimizer = new FilterLayerNoOptimizer()
                         };
            dummyPayload = new Activity("f");
        }

        [Test]
        public void VerifyPeriod()
        {
            Assert.IsNull(target.Period());
            
            internalCollection.Add(createLayer(new DateTimePeriod(2000, 1, 1, 2001, 1, 1), dummyPayload));
            Assert.AreEqual(new DateTimePeriod(2000,1,1,2001,1,1), target.Period());
            
            internalCollection.Add(createLayer(new DateTimePeriod(2001, 1, 1, 2002, 1, 1), dummyPayload));
            target = new VisualLayerCollection(dummyPerson, internalCollection, new ProjectionPayloadMerger());
            Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2002, 1, 1), target.Period());
            
            internalCollection.Add(createLayer(new DateTimePeriod(2002, 1, 1, 2003, 1, 1), dummyPayload));
            target = new VisualLayerCollection(dummyPerson, internalCollection, new ProjectionPayloadMerger());
            Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2003, 1, 1), target.Period());
        }

        [Test]
        public void VerifyEnumerator()
        {
            IVisualLayer vLayer1 = createLayer(new DateTimePeriod(2000, 1, 1, 2001, 1, 1), dummyPayload);
            IVisualLayer vLayer2 = createLayer(new DateTimePeriod(2001, 1, 1, 2002, 1, 1), dummyPayload);
            IVisualLayer vLayer3 = createLayer(new DateTimePeriod(2003, 1, 1, 2004, 1, 1), new Activity("should not be merged"));

            internalCollection.Add(vLayer1);
            internalCollection.Add(vLayer2);
            internalCollection.Add(vLayer3);

            IList<IVisualLayer> enumeratorRet = new List<IVisualLayer>();
            foreach (IVisualLayer layer in target)
            {
                enumeratorRet.Add(layer);
            }
            Assert.AreEqual(new DateTimePeriod(2000,1,1,2002,1,1), enumeratorRet[0].Period);
            Assert.AreSame(dummyPayload, enumeratorRet[0].Payload);
            Assert.AreEqual(vLayer3.Period, enumeratorRet[1].Period);
            Assert.AreSame(vLayer3.Payload, enumeratorRet[1].Payload);
            Assert.AreEqual(2, enumeratorRet.Count);
        }

        [Test]
        public void VerifyFilterLayers()
        {
            IActivity okActivity = new Activity("ok");
            IActivity okNoActivity = new Activity("nope");
            internalCollection.Add(visualLayerFactory.CreateShiftSetupLayer(okActivity, new DateTimePeriod(2000, 1, 1, 2001, 1, 1)));
            internalCollection.Add(visualLayerFactory.CreateShiftSetupLayer(okNoActivity, new DateTimePeriod(2001, 1, 1, 2002, 1, 1)));
            internalCollection.Add(visualLayerFactory.CreateShiftSetupLayer(okActivity, new DateTimePeriod(2002, 1, 1, 2003, 1, 1)));

            IVisualLayerCollection ret = target.FilterLayers(okActivity);
            Assert.AreEqual(2, ret.Count());
            Assert.AreEqual(0, target.FilterLayers(new Activity("sdf")).Count());
        }

        [Test]
        public void VerifyFilterLayersByLayerType()
        {
            IRtaStateGroup stateGroup = new Domain.RealTimeAdherence.RtaStateGroup("test", false, true);
            internalCollection.Add(visualLayerFactory.CreateShiftSetupLayer(dummyPayload, new DateTimePeriod(2001, 1, 1, 2001, 1, 2)));
            IVisualLayer actLayer = visualLayerFactory.CreateShiftSetupLayer(new Activity("sd"),
                                                                 new DateTimePeriod(2001, 1, 1, 2001, 1, 2).MovePeriod(TimeSpan.FromHours(1)));
            internalCollection.Add(visualLayerFactory.CreateResultLayer(stateGroup, actLayer, new DateTimePeriod(2001, 1, 1, 2001, 1, 2).MovePeriod(TimeSpan.FromHours(1))));
            
            IVisualLayerCollection ret = target.FilterLayers<IRtaStateGroup>();
            Assert.AreEqual(1,ret.Count());
            Assert.IsInstanceOf<IRtaStateGroup>(ret.ElementAt(0).Payload);
            ret = target.FilterLayers<IPayload>();
            Assert.AreEqual(2, ret.Count());
        }

        [Test]
        public void VerifyHasLayers()
        {
            internalCollection.Add(visualLayerFactory.CreateShiftSetupLayer(dummyPayload, new DateTimePeriod(2000, 1, 1, 2001, 1, 1)));
			target = new VisualLayerCollection(dummyPerson, internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};     
        }

		[Test]
		public void VerifyHasNotLayers()
		{
			Assert.IsFalse(target.HasLayers);
		}

        [Test]
        public void PersonShouldBeSetOnLayerWhenEnumerating()
        {
            internalCollection.Add(visualLayerFactory.CreateShiftSetupLayer(dummyPayload, 
                                                            new DateTimePeriod(2000, 1, 1, 2001, 1, 1)));
            Assert.AreNotSame(dummyPerson, ((VisualLayer)internalCollection.First()).Person);
            Assert.AreSame(dummyPerson, ((VisualLayer)target.First()).Person);
        }

        [Test]
        public void VerifyFilterLayersWithAdjacentLayers()
        {
            IActivity phone = new Activity("phone");
            internalCollection.Add(visualLayerFactory.CreateShiftSetupLayer(phone,
                                new DateTimePeriod(new DateTime(2006, 1, 1, 18, 0, 0, DateTimeKind.Utc), new DateTime(2006, 1, 1, 23, 0, 0, DateTimeKind.Utc))));
            internalCollection.Add(visualLayerFactory.CreateShiftSetupLayer(phone,
                                new DateTimePeriod(new DateTime(2006, 1, 1, 23, 0, 0, DateTimeKind.Utc), new DateTime(2006, 1, 2, 6, 0, 0, DateTimeKind.Utc))));

            IVisualLayerCollection ret =
                target.FilterLayers(
                    new DateTimePeriod(new DateTime(2006, 1, 1, 23, 0, 0, DateTimeKind.Utc), new DateTime(2006, 1, 1, 23, 15, 0, DateTimeKind.Utc)));

            IList<IVisualLayer> testList = new List<IVisualLayer>(ret);

            Assert.AreEqual(1, testList.Count);
            Assert.AreEqual(phone, testList[0].Payload);
        }

        [Test]
        public void VerifyFilterLayers2()
        {
            IActivity phone = new Activity("phone");
            IAbsence absence = new Absence();
            internalCollection.Add(
                visualLayerFactory.CreateShiftSetupLayer(phone, new DateTimePeriod(new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2006, 1, 1, 0, 5, 0, DateTimeKind.Utc))));
            internalCollection.Add(createAbsenceLayer(absence, new DateTimePeriod(new DateTime(2006, 1, 1, 0, 5, 0, DateTimeKind.Utc), new DateTime(2006, 1, 1, 0, 9, 0, DateTimeKind.Utc))));
            internalCollection.Add(
                visualLayerFactory.CreateShiftSetupLayer(phone,
                                new DateTimePeriod(new DateTime(2006, 1, 1, 0, 9, 0, DateTimeKind.Utc), new DateTime(2006, 1, 1, 0, 15, 0, DateTimeKind.Utc))));

            IVisualLayerCollection ret =
                target.FilterLayers(
                    new DateTimePeriod(new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2006, 1, 1, 0, 15, 0, DateTimeKind.Utc)));

            IList<IVisualLayer> testList = new List<IVisualLayer>(ret);

            Assert.AreEqual(5, testList[0].Period.ElapsedTime().TotalMinutes);
            Assert.AreEqual(4, testList[1].Period.ElapsedTime().TotalMinutes);
            Assert.AreEqual(6, testList[2].Period.ElapsedTime().TotalMinutes);
        }

        private IVisualLayer createAbsenceLayer(IAbsence absence, DateTimePeriod period)
        {
            IVisualLayer actLayer = visualLayerFactory.CreateShiftSetupLayer(new Activity("for test"), period);
            return visualLayerFactory.CreateAbsenceSetupLayer(absence, actLayer, period);
        }

        [Test]
        public void VerifyFilteredLayersWithNarrowingIndex()
        {
            IActivity phone = new Activity("phone");
            DateTimePeriod period = new DateTimePeriod(new DateTime(2006, 1, 1, 1, 0, 0, DateTimeKind.Utc),
                                                       new DateTime(2006, 1, 1, 2, 0, 0, DateTimeKind.Utc));

            for (int i = 0; i < 100; i++)
            {
                period = period.MovePeriod(TimeSpan.FromDays(1));
                internalCollection.Add(visualLayerFactory.CreateShiftSetupLayer(phone, period));
            }

            IVisualLayerCollection ret =
                target.FilterLayers(
                    new DateTimePeriod(new DateTime(2006, 1, 30, 1, 15, 0, DateTimeKind.Utc), new DateTime(2006, 1, 30, 1, 20, 0, DateTimeKind.Utc)));

            IList<IVisualLayer> testList = new List<IVisualLayer>(ret);
            Assert.AreEqual(1, testList.Count);
        }

        [Test]
        public void VerifyIsSatisfiedBy()
        {
            MockRepository mocks = new MockRepository();
            ISpecification<IVisualLayerCollection> spec = mocks.StrictMock<ISpecification<IVisualLayerCollection>>();
            using(mocks.Record())
            {
                using(mocks.Ordered())
                {
                    Expect.Call(spec.IsSatisfiedBy(target))
                        .Return(true);
                    Expect.Call(spec.IsSatisfiedBy(target))
                        .Return(false);
                }
            }
            using(mocks.Playback())
            {
                Assert.IsTrue(target.IsSatisfiedBy(spec));
                Assert.IsFalse(target.IsSatisfiedBy(spec));
            }
        }

        [Test]
        public void VerifyDefaultPeriodOptimizer()
        {
            //defined here at the test
            Assert.AreEqual(typeof(FilterLayerNoOptimizer), target.PeriodOptimizer.GetType());
            //default one
            Assert.AreEqual(typeof(NextPeriodOptimizer), new VisualLayerCollection(null, new List<IVisualLayer>(), new ProjectionPayloadMerger()).PeriodOptimizer.GetType());
        }

        #region ContractTime tests
        [Test]
        public void VerifyContractTimeReturnsZeroIfNoLayers()
        {
            Assert.AreEqual(new TimeSpan(0), target.ContractTime());
        }

        [Test]
        public void VerifyContractTimeOneActivityInContractTime()
        {
            internalCollection.Add(createLayer(new DateTimePeriod(2000, 1, 1, 2000, 1, 2), true));

            Assert.AreEqual(new TimeSpan(1,0,0,0), target.ContractTime());
        }
        [Test]
        public void VerifyContractTimeOneActivityNotInContractTime()
        {
            internalCollection.Add(createLayer(new DateTimePeriod(2000, 1, 1, 2000, 1, 2), false));

            Assert.AreEqual(TimeSpan.Zero, target.ContractTime());
        }
        [Test]
        public void VerifyContractTimeMultipleActivities()
        {
            internalCollection.Add(createLayer(new DateTimePeriod(2000, 1, 1, 2000, 1, 2), true));
            internalCollection.Add(createLayer(new DateTimePeriod(2000, 1, 2, 2000, 1, 3), false));
            internalCollection.Add(createLayer(new DateTimePeriod(2000, 1, 3, 2000, 1, 4), true));

            Assert.AreEqual(new TimeSpan(2,0,0,0), target.ContractTime());
        }

        [Test]
        public void VerifyContractWithAbsence()
        {
            //yes
            internalCollection.Add(createLayer(new DateTimePeriod(2000, 1, 1, 2000, 1, 2), true, true));
            //three no:s
            internalCollection.Add(createLayer(new DateTimePeriod(2000, 1, 2, 2000, 1, 3), true, false));
            internalCollection.Add(createLayer(new DateTimePeriod(2000, 1, 3, 2000, 1, 4), false, true));
            internalCollection.Add(createLayer(new DateTimePeriod(2000, 1, 4, 2000, 1, 5), false, false));
            
            Assert.AreEqual(new TimeSpan(1,0,0,0), target.ContractTime());
        }

		[Test]
		public void ShouldCalculateContractTimeForPartOfDay()
		{
			var absence1Period = new DateTimePeriod(new DateTime(2000, 1, 1, 5, 0, 0, DateTimeKind.Utc),
			                                       new DateTime(2000, 1, 1, 6, 0, 0, DateTimeKind.Utc));
			var absence2Period = new DateTimePeriod(new DateTime(2000, 1, 1, 6, 0, 0, DateTimeKind.Utc),
												   new DateTime(2000, 1, 1, 6, 30, 0, DateTimeKind.Utc));
			var absence3Period = new DateTimePeriod(new DateTime(2000, 1, 1, 6, 30, 0, DateTimeKind.Utc),
												   new DateTime(2000, 1, 1, 7, 0, 0, DateTimeKind.Utc));
			var afterAbsencePeriod = new DateTimePeriod(new DateTime(2000, 1, 1, 7, 0, 0, DateTimeKind.Utc),
												   new DateTime(2000, 1, 1, 9, 0, 0, DateTimeKind.Utc));
			var activity1Period = new DateTimePeriod(new DateTime(2000, 1, 1, 4, 0, 0, DateTimeKind.Utc),
												   new DateTime(2000, 1, 1, 6, 0, 0, DateTimeKind.Utc));
			var activity2Period = new DateTimePeriod(new DateTime(2000, 1, 1, 6, 30, 0, DateTimeKind.Utc),
												   new DateTime(2000, 1, 1, 9, 0, 0, DateTimeKind.Utc));
			var activityNotInContractTimePeriod = new DateTimePeriod(new DateTime(2000, 1, 1, 6, 0, 0, DateTimeKind.Utc),
												   new DateTime(2000, 1, 1, 6, 30, 0, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			absence.InContractTime = true;

			internalCollection.Add(visualLayerFactory.CreateAbsenceSetupLayer(absence, createLayer(activity1Period, true), absence1Period));
			internalCollection.Add(visualLayerFactory.CreateAbsenceSetupLayer(absence, createLayer(activityNotInContractTimePeriod, false), absence2Period));
			internalCollection.Add(visualLayerFactory.CreateAbsenceSetupLayer(absence, createLayer(activity2Period, true), absence3Period));
			internalCollection.Add(createLayer(afterAbsencePeriod, true));
			
			Assert.AreEqual(TimeSpan.FromMinutes(30), target.ContractTime(absence1Period.MovePeriod(TimeSpan.FromMinutes(30))));
		}

        #endregion

        #region ReadyTime tests
        [Test]
        public void VerifyReadyTimeReturnsZeroIfNoLayers()
        {
            Assert.AreEqual(new TimeSpan(0), target.ReadyTime());
        }

        [Test]
        public void VerifyReadyTimeOneActivityInReadyTime()
        {
            internalCollection.Add(createVisualLayerWithActivityForReadyTime(new DateTimePeriod(2000, 1, 1, 2000, 1, 2), true));

            Assert.AreEqual(new TimeSpan(1, 0, 0, 0), target.ReadyTime());
        }
        [Test]
        public void VerifyReadyTimeOneActivityNotInContractTime()
        {
            internalCollection.Add(createVisualLayerWithActivityForReadyTime(new DateTimePeriod(2000, 1, 1, 2000, 1, 2), false));

            Assert.AreEqual(TimeSpan.Zero, target.ReadyTime());
        }
        [Test]
        public void VerifyReadyTimeMultipleActivities()
        {
            internalCollection.Add(createVisualLayerWithActivityForReadyTime(new DateTimePeriod(2000, 1, 1, 2000, 1, 2), true));
            internalCollection.Add(createVisualLayerWithActivityForReadyTime(new DateTimePeriod(2000, 1, 2, 2000, 1, 3), false));
            internalCollection.Add(createVisualLayerWithActivityForReadyTime(new DateTimePeriod(2000, 1, 3, 2000, 1, 4), true));

            Assert.AreEqual(new TimeSpan(2, 0, 0, 0), target.ReadyTime());
        }

        [Test]
        public void VerifyReadyTimeWithAbsenceAlwaysZero()
        {
            internalCollection.Add(createVisualLayerWithAbsenceForReadyTime(new DateTimePeriod(2000, 1, 1, 2000, 1, 2), true));
            internalCollection.Add(createVisualLayerWithAbsenceForReadyTime(new DateTimePeriod(2000, 1, 2, 2000, 1, 3), false));

            Assert.AreEqual(TimeSpan.Zero, target.ReadyTime());
        }
        #endregion

        [Test]
        public void VerifyOvertime()
        {
            IMultiplicatorDefinitionSet set =
                MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("overtime",
                                                                                   MultiplicatorType.Overtime);
            internalCollection.Add(createLayerWithOvertime(set, new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));

            set =
                MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("ob",
                                                                                   MultiplicatorType.OBTime);
            internalCollection.Add(createLayerWithOvertime(set, new DateTimePeriod(2000, 1, 2, 2000, 1, 3)));

            set =
                MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("overtime1",
                                                                                   MultiplicatorType.Overtime);
            internalCollection.Add(createLayerWithOvertime(set, new DateTimePeriod(2000, 1, 3, 2000, 1, 4)));

            internalCollection.Add(createVisualLayerWithAbsenceForReadyTime(new DateTimePeriod(2000, 1, 4, 2000, 1, 5), false));

            Assert.AreEqual(TimeSpan.FromDays(2), target.Overtime());
            IDictionary<string, TimeSpan> ret = target.TimePerDefinitionSet();
            Assert.AreEqual(3, ret.Count);
            Assert.AreEqual(TimeSpan.FromDays(1), ret["overtime"]);
            Assert.AreEqual(TimeSpan.FromDays(1), ret["ob"]);
            Assert.AreEqual(TimeSpan.FromDays(1), ret["overtime1"]);
        }

        #region PaidTime tests
        [Test]
        public void VerifyPaidTimeReturnsZeroIfNoLayers()
        {
            Assert.AreEqual(new TimeSpan(0), target.PaidTime());
        }

        [Test]
        public void VerifyPaidTimeMultipleActivities()
        {
            internalCollection.Add(createLayerInPaidTime(true, new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
            internalCollection.Add(createLayerInPaidTime(false, new DateTimePeriod(2000, 1, 2, 2000, 1, 3)));
            internalCollection.Add(createLayerInPaidTime(true, new DateTimePeriod(2000, 1, 3, 2000, 1, 4)));

            Assert.AreEqual(new TimeSpan(2, 0, 0, 0), target.PaidTime());
        }

        [Test]
        public void VerifyPaidTimeWithAbsence()
        {
            //yes
            internalCollection.Add(createLayerInPaidTime(true, true, new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
            //three no:s
            internalCollection.Add(createLayerInPaidTime(true, false, new DateTimePeriod(2000, 1, 2, 2000, 1, 3)));
            internalCollection.Add(createLayerInPaidTime(false, true, new DateTimePeriod(2000, 1, 3, 2000, 1, 4)));
            internalCollection.Add(createLayerInPaidTime(false, false, new DateTimePeriod(2000, 1, 4, 2000, 1, 5)));

            Assert.AreEqual(new TimeSpan(1, 0, 0, 0), target.PaidTime());
        }
        #endregion

        #region WorkTime tests
        [Test]
        public void VerifyWorkTimeReturnsZeroIfNoLayers()
        {
            Assert.AreEqual(new TimeSpan(0), target.WorkTime());
        }

        [Test]
        public void VerifyWorkTimeMultipleActivities()
        {
            internalCollection.Add(createLayerInWorkTime(true, new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
            internalCollection.Add(createLayerInWorkTime(false, new DateTimePeriod(2000, 1, 2, 2000, 1, 3)));
            internalCollection.Add(createLayerInWorkTime(true, new DateTimePeriod(2000, 1, 3, 2000, 1, 4)));

            Assert.AreEqual(new TimeSpan(2, 0, 0, 0), target.WorkTime());
        }

        [Test]
        public void VerifyWorkTimeWithAbsence()
        {
            //yes
            internalCollection.Add(createLayerInWorkTime(true, true, new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
            //three no:s
            internalCollection.Add(createLayerInWorkTime(true, false, new DateTimePeriod(2000, 1, 2, 2000, 1, 3)));
            internalCollection.Add(createLayerInWorkTime(false, true, new DateTimePeriod(2000, 1, 3, 2000, 1, 4)));
            internalCollection.Add(createLayerInWorkTime(false, false, new DateTimePeriod(2000, 1, 4, 2000, 1, 5)));

            Assert.AreEqual(new TimeSpan(1, 0, 0, 0), target.WorkTime());
        }
        #endregion

        private IVisualLayer createLayer(DateTimePeriod period, IActivity activity)
        {
            return visualLayerFactory.CreateShiftSetupLayer(activity, period);
        }

        private IVisualLayer createLayer(DateTimePeriod period, 
                                                bool activityInContractTime)
        {
            IActivity activity = new Activity("for test");
            activity.InContractTime = activityInContractTime;
            return visualLayerFactory.CreateShiftSetupLayer(activity, period);
        }

        private IVisualLayer createLayerInWorkTime(bool inWorkTime, DateTimePeriod period)
        {
            IActivity underActivity = new Activity("for test");
            underActivity.InWorkTime = inWorkTime;
            IVisualLayer ret = visualLayerFactory.CreateShiftSetupLayer(underActivity, period);
            return ret;
        }

        private IVisualLayer createLayerInWorkTime(bool inWorkTime, bool absInContractTime, DateTimePeriod period)
        {
            IAbsence absence = new Absence();
            absence.InWorkTime = absInContractTime;
            IActivity underActivity = new Activity("for test");
            underActivity.InWorkTime = inWorkTime;
            IVisualLayer underLayer = visualLayerFactory.CreateShiftSetupLayer(underActivity, period);
            IVisualLayer ret = visualLayerFactory.CreateAbsenceSetupLayer(absence, underLayer, period);
            return ret;
        }

        private IVisualLayer createLayerWithOvertime(IMultiplicatorDefinitionSet multiplicatorDefinitionSet, DateTimePeriod period)
        {
            IActivity underActivity = new Activity("for test");
            IVisualLayer ret = visualLayerOvertimeFactory.CreateShiftSetupLayer(underActivity, period);
            ((VisualLayer) ret).DefinitionSet = multiplicatorDefinitionSet;
            return ret;
        }

        private IVisualLayer createLayerInPaidTime(bool inPaidTime, DateTimePeriod period)
        {
            IActivity underActivity = new Activity("for test");
            underActivity.InPaidTime = inPaidTime;
            IVisualLayer ret = visualLayerFactory.CreateShiftSetupLayer(underActivity, period);
            return ret;
        }

        private IVisualLayer createLayerInPaidTime(bool inPaidTime, bool absInPaidTime, DateTimePeriod period)
        {
            IAbsence absence = new Absence();
            absence.InPaidTime = absInPaidTime;
            IActivity underActivity = new Activity("for test");
            underActivity.InPaidTime = inPaidTime;
            IVisualLayer underLayer = visualLayerFactory.CreateShiftSetupLayer(underActivity, period);
            IVisualLayer ret = visualLayerFactory.CreateAbsenceSetupLayer(absence, underLayer, period);
            return ret;
        }

        private IVisualLayer createLayer(DateTimePeriod period,
                                        bool activityInContractTime,
                                        bool absenceInContractTime)
        {
            IAbsence absence = new Absence();
            absence.InContractTime = absenceInContractTime;
            IActivity underActivity = new Activity("for test");
            underActivity.InContractTime = activityInContractTime;
            IVisualLayer underLayer = visualLayerFactory.CreateShiftSetupLayer(underActivity, period);
            IVisualLayer ret = visualLayerFactory.CreateAbsenceSetupLayer(absence, underLayer, period);
            return ret;
        }

        private IVisualLayer createVisualLayerWithActivityForReadyTime(DateTimePeriod period, 
                                                bool activityInReadyTime)
        {
            IActivity payload = createActivity(activityInReadyTime);
            IVisualLayer ret = visualLayerFactory.CreateShiftSetupLayer(payload, period);
            return ret;
        }

        private static IActivity createActivity(bool activityInReadyTime)
        {
            IActivity payload = new Activity("for test");
            payload.InReadyTime = activityInReadyTime;
            return payload;
        }

        private IVisualLayer createVisualLayerWithAbsenceForReadyTime(DateTimePeriod period, bool activityInReadyTime)
        {
            IActivity payload = createActivity(activityInReadyTime);
            IVisualLayer actLayer = visualLayerFactory.CreateShiftSetupLayer(payload, period);
            return visualLayerFactory.CreateAbsenceSetupLayer(new Absence(), actLayer, period);
        }
    }
}
