using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class VisualLayerProjectionServiceTest
	{
		private IShift shift;
		private IAbsence absence;
		private IPerson person;

		[SetUp]
		public void Setup()
		{
			shift = new fakeShift();
			person = PersonFactory.CreatePerson();
			absence = AbsenceFactory.CreateAbsence("Tandl�karen");
		}

		[Test]
		public void VerifyCombineReturnTwoWhenNotIntersecting()
		{
			IActivity act = ActivityFactory.CreateActivity("test");
			var layer1 = new MainShiftActivityLayer(act, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var layer2 = new MainShiftActivityLayer(act, new DateTimePeriod(1910, 1, 5, 1911, 1, 6));
			shift.LayerCollection.Add(layer1);
			shift.LayerCollection.Add(layer2);

			IProjectionService svc = shift.ProjectionService();
			IList<IVisualLayer> retList = new List<IVisualLayer>(svc.CreateProjection());

			Assert.AreEqual(2, retList.Count);
			Assert.AreEqual(layer2.Payload, retList[0].Payload);
			Assert.AreEqual(layer2.Period, retList[0].Period);
			Assert.AreEqual(layer1.Payload, retList[1].Payload);
			Assert.AreEqual(layer1.Period, retList[1].Period);
		}

		[Test]
		public void VerifyNoCombiningWhenTheSameActivityButOneIsOvertime()
		{
			IMultiplicatorDefinitionSet defSet = new MultiplicatorDefinitionSet("d", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(person, defSet);
			IPersonAssignment ass = PersonAssignmentFactory.CreatePersonAssignment(person, ScenarioFactory.CreateScenarioAggregate());
			IActivity act = ActivityFactory.CreateActivity("the one");
			ass.SetMainShiftLayers(new[]{new MainShiftActivityLayerNew(act, createPeriod(10,19))}, new ShiftCategory("cat"));
			OvertimeShift ot = new OvertimeShift();
			ass.AddOvertimeShift(ot);
			ot.LayerCollection.Add(new OvertimeShiftActivityLayer(act, createPeriod(16, 17), defSet));
			
			IVisualLayerCollection res = ass.ProjectionService().CreateProjection();
			Assert.AreEqual(3, res.Count());
			Assert.AreEqual(TimeSpan.FromHours(8), res.ContractTime());
		}


		[Test]
		public void VerifyCombineReturnTwoWhenIntersectingAndOriginalLayerStarts()
		{
			IActivity act = ActivityFactory.CreateActivity("test");
			IActivity newActivity = ActivityFactory.CreateActivity("sdfsdf");
			ActivityLayer layer2 = new MainShiftActivityLayer(newActivity, new DateTimePeriod(2000, 1, 5, 2010, 1, 1));
			ActivityLayer layer1 = new MainShiftActivityLayer(act, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			shift.LayerCollection.Add(layer1);
			shift.LayerCollection.Add(layer2);

			IProjectionService svc = shift.ProjectionService();

			IList<IVisualLayer> retList = new List<IVisualLayer>(svc.CreateProjection());

			Assert.AreEqual(2, retList.Count);
			Assert.AreEqual(layer1.Payload, retList[0].Payload);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2000, 1, 5), retList[0].Period);
			Assert.AreEqual(layer2.Payload, retList[1].Payload);
			Assert.AreEqual(layer2.Period, retList[1].Period);
		}

		[Test]
		public void VerifyCombineReturnTwoWhenIntersectingAndNotOriginalLayerStarts()
		{
			IActivity act = ActivityFactory.CreateActivity("test");
			IActivity newActivity = ActivityFactory.CreateActivity("sdfsdf");
			ActivityLayer layer2 = new MainShiftActivityLayer(newActivity, new DateTimePeriod(1900, 1, 5, 2000, 1, 5));
			ActivityLayer layer1 = new MainShiftActivityLayer(act, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			shift.LayerCollection.Add(layer1);
			shift.LayerCollection.Add(layer2);

			IProjectionService svc = shift.ProjectionService();

			IList<IVisualLayer> retList = new List<IVisualLayer>(svc.CreateProjection());
			Assert.AreEqual(2, retList.Count);
			Assert.AreEqual(layer2.Payload, retList[0].Payload);
			Assert.AreEqual(layer2.Period, retList[0].Period);
			Assert.AreEqual(layer1.Payload, retList[1].Payload);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 5, 2001, 1, 1), retList[1].Period);
		}

		[Test]
		public void VerifyCombineReturnOneWhenIntersectingOneActivity()
		{
			IActivity act = ActivityFactory.CreateActivity("test");
			ActivityLayer layer2 = new MainShiftActivityLayer(act, new DateTimePeriod(2000, 1, 5, 2010, 1, 1));
			ActivityLayer layer1 = new MainShiftActivityLayer(act, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			shift.LayerCollection.Add(layer1);
			shift.LayerCollection.Add(layer2);

			IProjectionService svc = shift.ProjectionService();
			IList<IVisualLayer> resWrapper = new List<IVisualLayer>(svc.CreateProjection());

			Assert.AreEqual(1, resWrapper.Count);
			Assert.AreSame(act, resWrapper[0].Payload);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2010, 1, 1), resWrapper[0].Period);
		}

		[Test]
		public void VerifyCombineReturnThreeWhenInside()
		{
			IActivity act = ActivityFactory.CreateActivity("test");
			IActivity newActivity = ActivityFactory.CreateActivity("sdfsdf");
			ActivityLayer layer2 = new MainShiftActivityLayer(newActivity, new DateTimePeriod(2000, 1, 5, 2000, 1, 6));
			ActivityLayer layer1 = new MainShiftActivityLayer(act, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			shift.LayerCollection.Add(layer1);
			shift.LayerCollection.Add(layer2);

			IProjectionService svc = shift.ProjectionService();

			IList<IVisualLayer> retList = new List<IVisualLayer>(svc.CreateProjection());
			Assert.AreEqual(3, retList.Count);
			Assert.AreEqual(act, retList[0].Payload);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2000, 1, 5), retList[0].Period);
			Assert.AreEqual(newActivity, retList[1].Payload);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 5, 2000, 1, 6), retList[1].Period);
			Assert.AreEqual(act, retList[2].Payload);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 6, 2001, 1, 1), retList[2].Period);
		}


		[Test]
		public void VerifyCombineReturnOneWhenInsideAndOneActivity()
		{
			IActivity act = ActivityFactory.CreateActivity("test");
			ActivityLayer layer2 = new MainShiftActivityLayer(act, new DateTimePeriod(2000, 1, 5, 2000, 1, 6));
			ActivityLayer layer1 = new MainShiftActivityLayer(act, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			shift.LayerCollection.Add(layer1);
			shift.LayerCollection.Add(layer2);

			IProjectionService svc = shift.ProjectionService();
			IList<IVisualLayer> resWrapper = new List<IVisualLayer>(svc.CreateProjection());
			Assert.AreEqual(1, resWrapper.Count);
			Assert.AreEqual(layer1.Payload, resWrapper[0].Payload);
			Assert.AreEqual(layer1.Period, resWrapper[0].Period);
		}

		/// <summary>
		/// Verifies that a correct list of non ovelapping activityLayers is returned when a new layer that is overlapping exisiting layers is added.
		/// </summary>
		/// 
		[Test]
		public void CanReturnCorrectLayerListWhenNewLayerOverlapsExistingLayers()
		{
			LayerCollection<IActivity> orderedListLayerList = new LayerCollection<IActivity>();
			IActivity act1 = ActivityFactory.CreateActivity("Telefon");
			IActivity act2 = ActivityFactory.CreateActivity("Rast");
			IActivity act3 = ActivityFactory.CreateActivity("M�te");
			IActivity act5 = ActivityFactory.CreateActivity("M�lsamtal");

			DateTimePeriod period1 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 10, 00, 0, DateTimeKind.Utc), new DateTime(2001, 1, 1, 11, 00, 0, DateTimeKind.Utc));
			DateTimePeriod period2 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 11, 00, 0, DateTimeKind.Utc), new DateTime(2001, 1, 1, 12, 00, 0, DateTimeKind.Utc));
			DateTimePeriod period3 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 12, 00, 0, DateTimeKind.Utc), new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));

			DateTimePeriod period6 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 13, 30, 0, DateTimeKind.Utc), new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc));
			DateTimePeriod period7 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc), new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));
			DateTimePeriod period8 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 12, 00, 0, DateTimeKind.Utc), new DateTime(2001, 1, 1, 13, 30, 0, DateTimeKind.Utc));

			//Original layers in list
			ActivityLayer layer1 = new MainShiftActivityLayer(act1, period1);
			ActivityLayer layer2 = new MainShiftActivityLayer(act2, period2);
			ActivityLayer layer3 = new MainShiftActivityLayer(act3, period3);
			ActivityLayer layer4 = new MainShiftActivityLayer(act5, period6);

			//Expected layers
			ActivityLayer layer5 = new MainShiftActivityLayer(act1, period1);
			ActivityLayer layer6 = new MainShiftActivityLayer(act2, period2);
			ActivityLayer layer7 = new MainShiftActivityLayer(act3, period8);
			ActivityLayer layer8 = new MainShiftActivityLayer(act5, period6);
			ActivityLayer layer9 = new MainShiftActivityLayer(act3, period7);

			shift.LayerCollection.Add(layer1);
			shift.LayerCollection.Add(layer2);
			shift.LayerCollection.Add(layer3);
			shift.LayerCollection.Add(layer4);

			orderedListLayerList.Add(layer5);
			orderedListLayerList.Add(layer6);
			orderedListLayerList.Add(layer7);
			orderedListLayerList.Add(layer8);
			orderedListLayerList.Add(layer9);

			IProjectionService svc = shift.ProjectionService();
			IList<IVisualLayer> result = new List<IVisualLayer>(svc.CreateProjection());

			Assert.AreEqual(orderedListLayerList[0].Period, result[0].Period);
			Assert.AreEqual(orderedListLayerList[1].Period, result[1].Period);
			Assert.AreEqual(orderedListLayerList[2].Period, result[2].Period);
			Assert.AreEqual(orderedListLayerList[3].Period, result[3].Period);
			Assert.AreEqual(orderedListLayerList[4].Period, result[4].Period);

			Assert.AreEqual(orderedListLayerList[0].Payload, result[0].Payload);
			Assert.AreEqual(orderedListLayerList[1].Payload, result[1].Payload);
			Assert.AreEqual(orderedListLayerList[2].Payload, result[2].Payload);
			Assert.AreEqual(orderedListLayerList[3].Payload, result[3].Payload);
			Assert.AreEqual(orderedListLayerList[4].Payload, result[4].Payload);

			Assert.AreEqual(orderedListLayerList.Count, result.Count);
		}

		/// <summary>
		/// Verifies that a correct list of layers can be returned when order index and start times are not same
		/// </summary>
		[Test]
		public void CanReturnCorrectListOfLayersWhenOrderIndexAndStartTimesAreNotSame()
		{
			LayerCollection<IActivity> orderedListLayerList = new LayerCollection<IActivity>();
			IActivity act1 = ActivityFactory.CreateActivity("Telefon");
			IActivity act2 = ActivityFactory.CreateActivity("Rast");
			IActivity act3 = ActivityFactory.CreateActivity("M�te");
			IActivity act5 = ActivityFactory.CreateActivity("M�lsamtal");

			DateTimePeriod period1 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));
			DateTimePeriod period2 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 10, 00, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 12, 00, 0, DateTimeKind.Utc));
			DateTimePeriod period3 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 12, 00, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));

			DateTimePeriod period4 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 10, 00, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 12, 00, 0, DateTimeKind.Utc));
			DateTimePeriod period5 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 12, 00, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 13, 30, 0, DateTimeKind.Utc));
			DateTimePeriod period6 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 13, 30, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc));
			DateTimePeriod period7 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));

			//Original layers in list
			ActivityLayer layer1 = new MainShiftActivityLayer(act1, period1);
			ActivityLayer layer2 = new MainShiftActivityLayer(act2, period2);
			ActivityLayer layer3 = new MainShiftActivityLayer(act3, period3);
			ActivityLayer layer4 = new MainShiftActivityLayer(act5, period6);

			//Expected layers
			ActivityLayer layer5 = new MainShiftActivityLayer(act2, period4);
			ActivityLayer layer6 = new MainShiftActivityLayer(act3, period5);
			ActivityLayer layer7 = new MainShiftActivityLayer(act5, period6);
			ActivityLayer layer8 = new MainShiftActivityLayer(act3, period7);

			shift.LayerCollection.Add(layer1);
			shift.LayerCollection.Add(layer2);
			shift.LayerCollection.Add(layer3);
			shift.LayerCollection.Add(layer4);

			orderedListLayerList.Add(layer5);
			orderedListLayerList.Add(layer6);
			orderedListLayerList.Add(layer7);
			orderedListLayerList.Add(layer8);

			IProjectionService svc = shift.ProjectionService();
			IList<IVisualLayer> result = new List<IVisualLayer>(svc.CreateProjection());

			Assert.AreEqual(orderedListLayerList.Count, result.Count);

			Assert.AreEqual(orderedListLayerList[0].Period, result[0].Period);
			Assert.AreEqual(orderedListLayerList[1].Period, result[1].Period);
			Assert.AreEqual(orderedListLayerList[2].Period, result[2].Period);
			Assert.AreEqual(orderedListLayerList[3].Period, result[3].Period);

			Assert.AreEqual(orderedListLayerList[0].Payload, result[0].Payload);
			Assert.AreEqual(orderedListLayerList[1].Payload, result[1].Payload);
			Assert.AreEqual(orderedListLayerList[2].Payload, result[2].Payload);
			Assert.AreEqual(orderedListLayerList[3].Payload, result[3].Payload);
		}

		/// <summary>
		/// Verifies that a correct list of layers can be returned when order index and start times are not same
		/// </summary>
		[Test]
		public void TestMethod()
		{
			LayerCollection<IActivity> orderedListLayerList = new LayerCollection<IActivity>();
			IActivity act1 = ActivityFactory.CreateActivity("Telefon");
			IActivity act2 = ActivityFactory.CreateActivity("Rast");
			IActivity act3 = ActivityFactory.CreateActivity("M�te");
			IActivity act5 = ActivityFactory.CreateActivity("M�lsamtal");

			DateTimePeriod period1 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));
			DateTimePeriod period2 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 10, 00, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 12, 00, 0, DateTimeKind.Utc));
			DateTimePeriod period3 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 12, 00, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc));

			DateTimePeriod period4 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 10, 00, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 12, 00, 0, DateTimeKind.Utc));
			DateTimePeriod period5 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 12, 00, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 15, 30, 0, DateTimeKind.Utc));

			DateTimePeriod period6 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 15, 30, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 18, 00, 0, DateTimeKind.Utc));

			//Original layers in list
			ActivityLayer layer1 = new MainShiftActivityLayer(act1, period1);
			ActivityLayer layer2 = new MainShiftActivityLayer(act2, period2);
			ActivityLayer layer3 = new MainShiftActivityLayer(act3, period3);
			ActivityLayer layer4 = new MainShiftActivityLayer(act5, period6);

			//Expected layers
			ActivityLayer layer5 = new MainShiftActivityLayer(act2, period4); //10-12 Rast
			ActivityLayer layer6 = new MainShiftActivityLayer(act3, period5); //12-15:30 M�te
			ActivityLayer layer7 = new MainShiftActivityLayer(act5, period6); //15:30-18 M�lsamtal

			shift.LayerCollection.Add(layer1);
			shift.LayerCollection.Add(layer2);
			shift.LayerCollection.Add(layer3);
			shift.LayerCollection.Add(layer4);

			orderedListLayerList.Add(layer5);
			orderedListLayerList.Add(layer6);
			orderedListLayerList.Add(layer7);

			IProjectionService svc = shift.ProjectionService();
			IList<IVisualLayer> resWrapper = new List<IVisualLayer>(svc.CreateProjection());

			Assert.AreEqual(orderedListLayerList[0].Period, resWrapper[0].Period);
			Assert.AreEqual(orderedListLayerList[1].Period, resWrapper[1].Period);
			Assert.AreEqual(orderedListLayerList[2].Period, resWrapper[2].Period);


			Assert.AreEqual(orderedListLayerList[0].Payload, resWrapper[0].Payload);
			Assert.AreEqual(orderedListLayerList[1].Payload, resWrapper[1].Payload);
			Assert.AreEqual(orderedListLayerList[2].Payload, resWrapper[2].Payload);

			Assert.AreEqual(orderedListLayerList.Count, resWrapper.Count);
		}

		[Test]
		public void VerifyGetPeriodPayloads()
		{
			IActivity act1 = ActivityFactory.CreateActivity("test");
			IActivity act2 = ActivityFactory.CreateActivity("sdfsdf");
			IActivity act3 = ActivityFactory.CreateActivity("prr");
			DateTimePeriod period1 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 16, 50, 0, DateTimeKind.Utc));
			DateTimePeriod period2 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 16, 50, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 18, 00, 0, DateTimeKind.Utc));
			DateTimePeriod period3 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 16, 48, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 16, 55, 0, DateTimeKind.Utc));

			ActivityLayer layer1 = new MainShiftActivityLayer(act1, period1);
			ActivityLayer layer2 = new MainShiftActivityLayer(act2, period2);
			ActivityLayer layer3 = new MainShiftActivityLayer(act3, period3);

			shift.LayerCollection.Add(layer1);
			shift.LayerCollection.Add(layer2);
			shift.LayerCollection.Add(layer3);

			DateTimePeriod period4 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 16, 45, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));
			IProjectionService projectionService = shift.ProjectionService();
			projectionService.CreateProjection();
			IList<IVisualLayer> payloads = new List<IVisualLayer>(projectionService.CreateProjection().FilterLayers(period4));


			Assert.AreEqual(3, payloads.Count);
			Assert.AreEqual(payloads[0].Payload, act1);
			Assert.AreEqual(3, payloads[0].Period.ElapsedTime().TotalMinutes);
			Assert.AreEqual(payloads[1].Payload, act3);
			Assert.AreEqual(7, payloads[1].Period.ElapsedTime().TotalMinutes);
			Assert.AreEqual(payloads[2].Payload, act2);
			Assert.AreEqual(5, payloads[2].Period.ElapsedTime().TotalMinutes);
		}


		[Test]
		public void VerifyProjectionWithActivitiesAndAbsences()
		{
			IActivity act1 = ActivityFactory.CreateActivity("Telefon");
			IActivity act2 = ActivityFactory.CreateActivity("Rast");
			IActivity act3 = ActivityFactory.CreateActivity("M�te"); 
			absence = AbsenceFactory.CreateAbsence("Tandl�karen");

			DateTimePeriod period1 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 8, 00, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));
			DateTimePeriod period2 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 11, 00, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 12, 00, 0, DateTimeKind.Utc));
			DateTimePeriod period3 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 12, 00, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 18, 00, 0, DateTimeKind.Utc));

			DateTimePeriod period4 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 2, 00, 00, 0, DateTimeKind.Utc));

			//Original layers in list
			PersonalShiftActivityLayer layer3 = new PersonalShiftActivityLayer(act3, period3);


			PersonalShift personShift = new PersonalShift();
			personShift.LayerCollection.Add(layer3);

			IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
			IPersonAssignment personAss = PersonAssignmentFactory.CreatePersonAssignment(person,scenario);
			personAss.AddPersonalShift(personShift);
			personAss.SetMainShiftLayers(new[]
				{
					new MainShiftActivityLayerNew(act1, period1), 
					new MainShiftActivityLayerNew(act2, period2)
				}, new ShiftCategory("test"));
			IPersonAbsence personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, period4, absence);

			var day =
				ExtractedSchedule.CreateScheduleDay(
					new ScheduleDictionary(scenario,
										   new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2002, 1, 1))),
					person, new DateOnly(2001, 1, 1));
			day.Add(personAss);
			day.Add(personAbsence);

			var proj = day.ProjectionService().CreateProjection();
			IList<IVisualLayer> resWrapper = new List<IVisualLayer>(proj);
			Assert.AreEqual(4, resWrapper.Count);
			Assert.AreEqual(new DateTimePeriod(new DateTime(2001, 1, 1, 8, 00, 0, DateTimeKind.Utc),
														  new DateTime(2001, 1, 1, 18, 00, 0, DateTimeKind.Utc)), proj.Period());
		}

		[Test]
		public void VerifyReferencedActivityLayer()
		{
			IVisualLayerFactory layerFactory = new VisualLayerFactory();
			IActivity refAct = ActivityFactory.CreateActivity("f");
			DateTimePeriod period = new DateTimePeriod(2000,1,1,2001,1,2);
			IVisualLayer actLayer = layerFactory.CreateShiftSetupLayer(refAct, period, person);
			IVisualLayer layer = layerFactory.CreateAbsenceSetupLayer(AbsenceFactory.CreateAbsence("vacation"), actLayer, period);
			VisualLayerProjectionService target = new VisualLayerProjectionService(person);
			target.Add(layer);
			foreach (VisualLayer vlayer in target.CreateProjection())
			{
				Assert.IsNotNull(vlayer);
				Assert.AreSame(refAct, vlayer.HighestPriorityActivity);
			}
		}

		[Test]
		public void VerifyMainShiftIsOverwrittenByOvertime()
		{
			IMultiplicatorDefinitionSet defSet =dummyDefinitionSet();
			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(ScenarioFactory.CreateScenarioAggregate(),
																							person,
																							createPeriod(12, 16));
			PersonFactory.AddDefinitionSetToPerson(ass.Person, defSet);
			IActivity overTimeActivity = ActivityFactory.CreateActivity("d");
			IOvertimeShift ot = new OvertimeShift();
			ass.AddOvertimeShift(ot);
			ot.LayerCollection.Add(new OvertimeShiftActivityLayer(overTimeActivity, createPeriod(14, 15), defSet));

			IVisualLayerCollection org = ass.ProjectionService().CreateProjection();
			IList<IVisualLayer> proj = new List<IVisualLayer>(org);

			Assert.AreEqual(3, proj.Count);
			Assert.AreEqual(createPeriod(12,14), proj[0].Period);
			Assert.AreEqual(createPeriod(14,15), proj[1].Period);
			Assert.AreEqual(createPeriod(15,16), proj[2].Period);
			Assert.AreEqual(TimeSpan.FromHours(1), org.Overtime());
			Assert.IsNull(proj[0].DefinitionSet);
			Assert.AreSame(defSet, proj[1].DefinitionSet);
			Assert.IsNull(proj[2].DefinitionSet);
		}

		[Test]
		public void VerifyOvertimeExistsInProjectionWithoutMainShift()
		{
			IMultiplicatorDefinitionSet defSet = dummyDefinitionSet();
			IPersonAssignment ass = PersonAssignmentFactory.CreatePersonAssignment(person, ScenarioFactory.CreateScenarioAggregate());
			PersonFactory.AddDefinitionSetToPerson(ass.Person, defSet);
			IOvertimeShift ot = new OvertimeShift();
			ass.AddOvertimeShift(ot);
			ot.LayerCollection.Add(new OvertimeShiftActivityLayer(ActivityFactory.CreateActivity("d"), createPeriod(14, 15), defSet));

			IVisualLayerCollection org = ass.ProjectionService().CreateProjection();
			IList<IVisualLayer> proj = new List<IVisualLayer>(org);

			Assert.AreEqual(1, proj.Count);
			Assert.AreEqual(createPeriod(14, 15), proj[0].Period);
			Assert.AreEqual(TimeSpan.FromHours(1), org.Overtime());
		}

		[Test]
		public void PersonalShiftNotVisibleIfOnlyOvertimeAndNoMainShift()
		{
			IMultiplicatorDefinitionSet defSet = dummyDefinitionSet();
			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(ActivityFactory.CreateActivity("d"), 
																				person,
																				createPeriod(12,16),
																				ScenarioFactory.CreateScenarioAggregate());
			PersonFactory.AddDefinitionSetToPerson(ass.Person, defSet);
			IOvertimeShift ot = new OvertimeShift();
			ass.AddOvertimeShift(ot);
			ot.LayerCollection.Add(new OvertimeShiftActivityLayer(ActivityFactory.CreateActivity("d"), createPeriod(14, 15), defSet));


			IList<IVisualLayer> proj = new List<IVisualLayer>(ass.ProjectionService().CreateProjection());
			//hur m�nga lager? 3, 0 eller 1?
			Assert.AreEqual(1, proj.Count);

		}

		private static IMultiplicatorDefinitionSet dummyDefinitionSet()
		{
			return new MultiplicatorDefinitionSet("d", MultiplicatorType.Overtime);
		}

		private static DateTimePeriod createPeriod(int startHour, int endHour)
		{
			DateTime date = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			DateTime start = date.AddHours(startHour);
			DateTime end = date.AddHours(endHour);
			return new DateTimePeriod(start, end);
		}

		private class fakeShift : MainShift
		{
		}
	}
}
