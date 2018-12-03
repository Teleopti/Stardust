using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Collection
{
	[TestFixture]
	public class VisualLayerCollectionTest
	{
		private List<IVisualLayer> internalCollection;
		private IActivity dummyPayload;
		private IVisualLayerFactory visualLayerFactory;
		private IVisualLayerFactory visualLayerOvertimeFactory;

		[SetUp]
		public void Setup()
		{
			visualLayerFactory = new VisualLayerFactory();
			visualLayerOvertimeFactory = new VisualLayerOvertimeFactory();
			internalCollection = new List<IVisualLayer>();

			dummyPayload = ActivityFactory.CreateActivity("f");
		}

		[Test]
		public void VerifyPeriod()
		{
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			Assert.IsNull(target.Period());

			internalCollection.Add(createLayer(new DateTimePeriod(2000, 1, 1, 2001, 1, 1), dummyPayload));
			target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger());
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2001, 1, 1), target.Period());

			internalCollection.Add(createLayer(new DateTimePeriod(2001, 1, 1, 2002, 1, 1), dummyPayload));
			target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger());
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2002, 1, 1), target.Period());

			internalCollection.Add(createLayer(new DateTimePeriod(2002, 1, 1, 2003, 1, 1), dummyPayload));
			target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger());
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2003, 1, 1), target.Period());
		}

		[Test]
		public void VerifyEnumerator()
		{
			IVisualLayer vLayer1 = createLayer(new DateTimePeriod(2000, 1, 1, 2001, 1, 1), dummyPayload);
			IVisualLayer vLayer2 = createLayer(new DateTimePeriod(2001, 1, 1, 2002, 1, 1), dummyPayload);
			IVisualLayer vLayer3 = createLayer(new DateTimePeriod(2003, 1, 1, 2004, 1, 1), ActivityFactory.CreateActivity("should not be merged"));

			internalCollection.Add(vLayer1);
			internalCollection.Add(vLayer2);
			internalCollection.Add(vLayer3);
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			IList<IVisualLayer> enumeratorRet = new List<IVisualLayer>();
			foreach (IVisualLayer layer in target)
			{
				enumeratorRet.Add(layer);
			}
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2002, 1, 1), enumeratorRet[0].Period);
			Assert.AreSame(dummyPayload, enumeratorRet[0].Payload);
			Assert.AreEqual(vLayer3.Period, enumeratorRet[1].Period);
			Assert.AreSame(vLayer3.Payload, enumeratorRet[1].Payload);
			Assert.AreEqual(2, enumeratorRet.Count);
		}

		[Test]
		public void VerifyFilterLayers()
		{
			IActivity okActivity = ActivityFactory.CreateActivity("ok");
			IActivity okNoActivity = ActivityFactory.CreateActivity("nope");
			internalCollection.Add(visualLayerFactory.CreateShiftSetupLayer(okActivity, new DateTimePeriod(2000, 1, 1, 2001, 1, 1)));
			internalCollection.Add(visualLayerFactory.CreateShiftSetupLayer(okNoActivity, new DateTimePeriod(2001, 1, 1, 2002, 1, 1)));
			internalCollection.Add(visualLayerFactory.CreateShiftSetupLayer(okActivity, new DateTimePeriod(2002, 1, 1, 2003, 1, 1)));
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			IVisualLayerCollection ret = target.FilterLayers(okActivity);
			Assert.AreEqual(2, ret.Count());
			Assert.AreEqual(0, target.FilterLayers(ActivityFactory.CreateActivity("sdf")).Count());
		}

		[Test]
		public void VerifyHasLayers()
		{
			internalCollection.Add(visualLayerFactory.CreateShiftSetupLayer(dummyPayload, new DateTimePeriod(2000, 1, 1, 2001, 1, 1)));
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			target.HasLayers.Should().Be.True();
		}

		[Test]
		public void VerifyHasNotLayers()
		{
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			Assert.IsFalse(target.HasLayers);
		}

		[Test]
		public void VerifyFilterLayersWithAdjacentLayers()
		{
			IActivity phone = ActivityFactory.CreateActivity("phone");
			internalCollection.Add(visualLayerFactory.CreateShiftSetupLayer(phone,
													new DateTimePeriod(new DateTime(2006, 1, 1, 18, 0, 0, DateTimeKind.Utc), new DateTime(2006, 1, 1, 23, 0, 0, DateTimeKind.Utc))));
			internalCollection.Add(visualLayerFactory.CreateShiftSetupLayer(phone,
													new DateTimePeriod(new DateTime(2006, 1, 1, 23, 0, 0, DateTimeKind.Utc), new DateTime(2006, 1, 2, 6, 0, 0, DateTimeKind.Utc))));
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
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
			IActivity phone = ActivityFactory.CreateActivity("phone");
			IAbsence absence = AbsenceFactory.CreateAbsence("vacation");
			internalCollection.Add(
					visualLayerFactory.CreateShiftSetupLayer(phone, new DateTimePeriod(new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2006, 1, 1, 0, 5, 0, DateTimeKind.Utc))));
			internalCollection.Add(createAbsenceLayer(absence, new DateTimePeriod(new DateTime(2006, 1, 1, 0, 5, 0, DateTimeKind.Utc), new DateTime(2006, 1, 1, 0, 9, 0, DateTimeKind.Utc))));
			internalCollection.Add(
					visualLayerFactory.CreateShiftSetupLayer(phone,
													new DateTimePeriod(new DateTime(2006, 1, 1, 0, 9, 0, DateTimeKind.Utc), new DateTime(2006, 1, 1, 0, 15, 0, DateTimeKind.Utc))));
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
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
			IVisualLayer actLayer = visualLayerFactory.CreateShiftSetupLayer(ActivityFactory.CreateActivity("for test"), period);
			return visualLayerFactory.CreateAbsenceSetupLayer(absence, actLayer, period);
		}

		[Test]
		public void VerifyFilteredLayersWithNarrowingIndex()
		{
			IActivity phone = ActivityFactory.CreateActivity("phone");
			DateTimePeriod period = new DateTimePeriod(2006, 1, 1, 1, 2006, 1, 1, 2);

			internalCollection.AddRange(Enumerable.Range(0,100).Select(i => visualLayerFactory.CreateShiftSetupLayer(phone, period.MovePeriod(TimeSpan.FromDays(i+1)))));
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
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
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			using (mocks.Record())
			{
				using (mocks.Ordered())
				{
					Expect.Call(spec.IsSatisfiedBy(target))
							.Return(true);
					Expect.Call(spec.IsSatisfiedBy(target))
							.Return(false);
				}
			}
			using (mocks.Playback())
			{
				Assert.IsTrue(target.IsSatisfiedBy(spec));
				Assert.IsFalse(target.IsSatisfiedBy(spec));
			}
		}

		[Test]
		public void VerifyDefaultPeriodOptimizer()
		{
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			//defined here at the test
			Assert.AreEqual(typeof(FilterLayerNoOptimizer), target.PeriodOptimizer.GetType());
			//default one
			Assert.AreEqual(typeof(NextPeriodOptimizer), new VisualLayerCollection(new List<IVisualLayer>(), new ProjectionPayloadMerger()).PeriodOptimizer.GetType());
		}

		[Test]
		public void VerifyContractTimeReturnsZeroIfNoLayers()
		{
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			Assert.AreEqual(new TimeSpan(0), target.ContractTime());
		}

		[Test]
		public void VerifyContractTimeOneActivityInContractTime()
		{
			internalCollection.Add(createLayer(new DateTimePeriod(2000, 1, 1, 2000, 1, 2), true));
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			Assert.AreEqual(new TimeSpan(1, 0, 0, 0), target.ContractTime());
		}
		[Test]
		public void VerifyContractTimeOneActivityNotInContractTime()
		{
			internalCollection.Add(createLayer(new DateTimePeriod(2000, 1, 1, 2000, 1, 2), false));
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			Assert.AreEqual(TimeSpan.Zero, target.ContractTime());
		}
		[Test]
		public void VerifyContractTimeMultipleActivities()
		{
			internalCollection.Add(createLayer(new DateTimePeriod(2000, 1, 1, 2000, 1, 2), true));
			internalCollection.Add(createLayer(new DateTimePeriod(2000, 1, 2, 2000, 1, 3), false));
			internalCollection.Add(createLayer(new DateTimePeriod(2000, 1, 3, 2000, 1, 4), true));
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			Assert.AreEqual(new TimeSpan(2, 0, 0, 0), target.ContractTime());
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
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			Assert.AreEqual(new TimeSpan(1, 0, 0, 0), target.ContractTime());
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
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			Assert.AreEqual(TimeSpan.FromMinutes(30), target.ContractTime(absence1Period.MovePeriod(TimeSpan.FromMinutes(30))));
		}

		
		[Test]
		public void VerifyOvertime()
		{
			IMultiplicatorDefinitionSet set =
					MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var activity = ActivityFactory.CreateActivity("test");
			activity.InWorkTime = true;

			internalCollection.Add(createLayerWithOvertime(set, new DateTimePeriod(2000, 1, 1, 2000, 1, 2), activity));

			set = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("ob", MultiplicatorType.OBTime);
			internalCollection.Add(createLayerWithOvertime(set, new DateTimePeriod(2000, 1, 2, 2000, 1, 3), activity));

			set = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("overtime1", MultiplicatorType.Overtime);
			internalCollection.Add(createLayerWithOvertime(set, new DateTimePeriod(2000, 1, 3, 2000, 1, 4), activity));

			internalCollection.Add(createVisualLayerWithAbsenceForReadyTime(new DateTimePeriod(2000, 1, 4, 2000, 1, 5), false));
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			Assert.AreEqual(TimeSpan.FromDays(2), target.Overtime());
		}

		[Test]
		public void ShouldCalculateOverTimeForPartOfDay()
		{
			var activityPeriod = new DateTimePeriod(new DateTime(2000, 1, 1, 5, 0, 0, DateTimeKind.Utc),
																						 new DateTime(2000, 1, 1, 6, 0, 0, DateTimeKind.Utc));
			var overTimePeriod = new DateTimePeriod(new DateTime(2000, 1, 1, 6, 0, 0, DateTimeKind.Utc),
																						 new DateTime(2000, 1, 1, 7, 0, 0, DateTimeKind.Utc));
			var set = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var activity = ActivityFactory.CreateActivity("Phone");

			internalCollection.Add(visualLayerFactory.CreateShiftSetupLayer(activity, activityPeriod));
			internalCollection.Add(createLayerWithOvertime(set, overTimePeriod));
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			Assert.AreEqual(TimeSpan.FromMinutes(30), target.Overtime(overTimePeriod.MovePeriod(TimeSpan.FromMinutes(-30))));
			Assert.AreEqual(TimeSpan.FromMinutes(15), target.Overtime(activityPeriod.MovePeriod(TimeSpan.FromMinutes(15))));
			Assert.AreEqual(TimeSpan.FromMinutes(0), target.Overtime(activityPeriod));
		}

		[Test]
		public void ShouldOnlyCalculateOvertimeForActivitiesInworktime()
		{
			var set = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			
			var overtimePhoneActivity = ActivityFactory.CreateActivity("Phone");
			overtimePhoneActivity.InWorkTime = true;
			var activityPeriod = new DateTimePeriod(new DateTime(2000, 1, 1, 5, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 6, 0, 0, DateTimeKind.Utc));
			internalCollection.Add(createLayerWithOvertime(set, activityPeriod, overtimePhoneActivity));

			var nonWorkTimeActivity = ActivityFactory.CreateActivity("Lunch");
			nonWorkTimeActivity.InWorkTime = false;
			var nonWorkTimeActivityPeriod = new DateTimePeriod(new DateTime(2000, 1, 1, 6, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 7, 0, 0, DateTimeKind.Utc));
			internalCollection.Add(createLayerWithOvertime(set, nonWorkTimeActivityPeriod, nonWorkTimeActivity));

			var activityPeriodAfter = new DateTimePeriod(new DateTime(2000, 1, 1, 7, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc));
			internalCollection.Add(createLayerWithOvertime(set, activityPeriodAfter, overtimePhoneActivity));
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			var overtime = target.Overtime();
			Assert.AreEqual(2, overtime.TotalHours);

			overtime =
				target.Overtime(new DateTimePeriod(new DateTime(2000, 1, 1, 5, 0, 0, DateTimeKind.Utc),
					new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc)));
			Assert.AreEqual(2, overtime.TotalHours);
		}

		[Test]
		public void VerifyPaidTimeReturnsZeroIfNoLayers()
		{
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			Assert.AreEqual(new TimeSpan(0), target.PaidTime());
		}

		[Test]
		public void VerifyPaidTimeMultipleActivities()
		{
			internalCollection.Add(createLayerInPaidTime(true, new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
			internalCollection.Add(createLayerInPaidTime(false, new DateTimePeriod(2000, 1, 2, 2000, 1, 3)));
			internalCollection.Add(createLayerInPaidTime(true, new DateTimePeriod(2000, 1, 3, 2000, 1, 4)));
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
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
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			Assert.AreEqual(new TimeSpan(1, 0, 0, 0), target.PaidTime());
		}

		[Test]
		public void VerifyPaidTimeWithAbsenceOnFilteredPeriod()
		{
			//yes
			internalCollection.Add(createLayerInPaidTime(true, true, new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
			//three no:s
			internalCollection.Add(createLayerInPaidTime(true, false, new DateTimePeriod(2000, 1, 2, 2000, 1, 3)));
			internalCollection.Add(createLayerInPaidTime(false, true, new DateTimePeriod(2000, 1, 3, 2000, 1, 4)));
			internalCollection.Add(createLayerInPaidTime(false, false, new DateTimePeriod(2000, 1, 4, 2000, 1, 5)));
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			Assert.AreEqual(new TimeSpan(1, 0, 0, 0), target.PaidTime(new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
		}
		
		[Test]
		public void VerifyWorkTimeReturnsZeroIfNoLayers()
		{
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			Assert.AreEqual(new TimeSpan(0), target.WorkTime());
		}

		[Test]
		public void VerifyWorkTimeMultipleActivities()
		{
			internalCollection.Add(createLayerInWorkTime(true, new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
			internalCollection.Add(createLayerInWorkTime(false, new DateTimePeriod(2000, 1, 2, 2000, 1, 3)));
			internalCollection.Add(createLayerInWorkTime(true, new DateTimePeriod(2000, 1, 3, 2000, 1, 4)));
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
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
			var target = new VisualLayerCollection(internalCollection, new ProjectionPayloadMerger())
			{
				PeriodOptimizer = new FilterLayerNoOptimizer()
			};
			Assert.AreEqual(new TimeSpan(1, 0, 0, 0), target.WorkTime());
		}
		
		private IVisualLayer createLayer(DateTimePeriod period, IActivity activity)
		{
			return visualLayerFactory.CreateShiftSetupLayer(activity, period);
		}

		private IVisualLayer createLayer(DateTimePeriod period, bool activityInContractTime)
		{
			IActivity activity = ActivityFactory.CreateActivity("for test");
			activity.InContractTime = activityInContractTime;
			return visualLayerFactory.CreateShiftSetupLayer(activity, period);
		}

		private IVisualLayer createLayerInWorkTime(bool inWorkTime, DateTimePeriod period)
		{
			IActivity underActivity = ActivityFactory.CreateActivity("for test");
			underActivity.InWorkTime = inWorkTime;
			IVisualLayer ret = visualLayerFactory.CreateShiftSetupLayer(underActivity, period);
			return ret;
		}

		private IVisualLayer createLayerInWorkTime(bool inWorkTime, bool absInContractTime, DateTimePeriod period)
		{
			IAbsence absence = AbsenceFactory.CreateAbsence("vacation");
			absence.InWorkTime = absInContractTime;
			IActivity underActivity = ActivityFactory.CreateActivity("for test");
			underActivity.InWorkTime = inWorkTime;
			IVisualLayer underLayer = visualLayerFactory.CreateShiftSetupLayer(underActivity, period);
			IVisualLayer ret = visualLayerFactory.CreateAbsenceSetupLayer(absence, underLayer, period);
			return ret;
		}

		private IVisualLayer createLayerWithOvertime(IMultiplicatorDefinitionSet multiplicatorDefinitionSet, DateTimePeriod period)
		{
			IActivity underActivity = ActivityFactory.CreateActivity("for test");
			underActivity.InWorkTime = true;
			IVisualLayer ret = visualLayerOvertimeFactory.CreateShiftSetupLayer(underActivity, period);
			((VisualLayer)ret).DefinitionSet = multiplicatorDefinitionSet;
			return ret;
		}

		private IVisualLayer createLayerWithOvertime(IMultiplicatorDefinitionSet multiplicatorDefinitionSet,
			DateTimePeriod period, IActivity activity)
		{
			IVisualLayer ret = visualLayerOvertimeFactory.CreateShiftSetupLayer(activity, period);
			((VisualLayer)ret).DefinitionSet = multiplicatorDefinitionSet;
			return ret;
		}

		private IVisualLayer createLayerInPaidTime(bool inPaidTime, DateTimePeriod period)
		{
			IActivity underActivity = ActivityFactory.CreateActivity("for test");
			underActivity.InPaidTime = inPaidTime;
			IVisualLayer ret = visualLayerFactory.CreateShiftSetupLayer(underActivity, period);
			return ret;
		}

		private IVisualLayer createLayerInPaidTime(bool inPaidTime, bool absInPaidTime, DateTimePeriod period)
		{
			IAbsence absence = AbsenceFactory.CreateAbsence("vacation");
			absence.InPaidTime = absInPaidTime;
			IActivity underActivity = ActivityFactory.CreateActivity("for test");
			underActivity.InPaidTime = inPaidTime;
			IVisualLayer underLayer = visualLayerFactory.CreateShiftSetupLayer(underActivity, period);
			IVisualLayer ret = visualLayerFactory.CreateAbsenceSetupLayer(absence, underLayer, period);
			return ret;
		}

		private IVisualLayer createLayer(DateTimePeriod period, bool activityInContractTime, bool absenceInContractTime)
		{
			IAbsence absence = AbsenceFactory.CreateAbsence("vacation");
			absence.InContractTime = absenceInContractTime;
			IActivity underActivity = ActivityFactory.CreateActivity("for test");
			underActivity.InContractTime = activityInContractTime;
			IVisualLayer underLayer = visualLayerFactory.CreateShiftSetupLayer(underActivity, period);
			IVisualLayer ret = visualLayerFactory.CreateAbsenceSetupLayer(absence, underLayer, period);
			return ret;
		}
		
		private static IActivity createActivity(bool activityInReadyTime)
		{
			IActivity payload = ActivityFactory.CreateActivity("for test");
			payload.InReadyTime = activityInReadyTime;
			return payload;
		}

		private IVisualLayer createVisualLayerWithAbsenceForReadyTime(DateTimePeriod period, bool activityInReadyTime)
		{
			var payload = createActivity(activityInReadyTime);
			var actLayer = visualLayerFactory.CreateShiftSetupLayer(payload, period);
			return visualLayerFactory.CreateAbsenceSetupLayer(AbsenceFactory.CreateAbsence("vacation"), actLayer, period);
		}
	}
}
