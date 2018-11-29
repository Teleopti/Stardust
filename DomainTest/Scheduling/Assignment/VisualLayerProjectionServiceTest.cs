using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;

using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	public class VisualLayerProjectionServiceTest
	{
		private IAbsence absence;
		private IPerson person;

		[SetUp]
		public void Setup()
		{
			person = PersonFactory.CreatePerson();
			absence = AbsenceFactory.CreateAbsence("Tandl�karen");
		}

		[Test]
		public void VerifyCombineReturnTwoWhenNotIntersecting()
		{
			IActivity act = ActivityFactory.CreateActivity("test");
			var layer1 = new MainShiftLayer(act, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var layer2 = new MainShiftLayer(act, new DateTimePeriod(1910, 1, 5, 1911, 1, 6));


			var retList = new[] {layer1, layer2}.CreateProjection();

			Assert.AreEqual(2, retList.Count());
			Assert.AreEqual(layer2.Payload, retList.First().Payload);
			Assert.AreEqual(layer2.Period, retList.First().Period);
			Assert.AreEqual(layer1.Payload, retList.Last().Payload);
			Assert.AreEqual(layer1.Period, retList.Last().Period);
		}

		[Test]
		public void VerifyNoCombiningWhenTheSameActivityButOneIsOvertime()
		{
			IMultiplicatorDefinitionSet defSet = new MultiplicatorDefinitionSet("d", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(person, defSet);
			IPersonAssignment ass = PersonAssignmentFactory.CreatePersonAssignment(person, ScenarioFactory.CreateScenarioAggregate());
			IActivity act = ActivityFactory.CreateActivity("the one");
			ass.AddActivity(act, createPeriod(10,19));
			ass.AddOvertimeActivity(act, createPeriod(16, 17), defSet);
			
			IVisualLayerCollection res = ass.ProjectionService().CreateProjection();
			Assert.AreEqual(3, res.Count());
			Assert.AreEqual(TimeSpan.FromHours(8), res.ContractTime());
		}


		[Test]
		public void VerifyCombineReturnTwoWhenIntersectingAndOriginalLayerStarts()
		{
			IActivity act = ActivityFactory.CreateActivity("test");
			IActivity newActivity = ActivityFactory.CreateActivity("sdfsdf");
			var layer2 = new MainShiftLayer(newActivity, new DateTimePeriod(2000, 1, 5, 2010, 1, 1));
			var layer1 = new MainShiftLayer(act, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));

			var retList = new[] { layer1, layer2 }.CreateProjection();

			Assert.AreEqual(2, retList.Count());
			Assert.AreEqual(layer1.Payload, retList.First().Payload);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2000, 1, 5), retList.First().Period);
			Assert.AreEqual(layer2.Payload, retList.Last().Payload);
			Assert.AreEqual(layer2.Period, retList.Last().Period);
		}

		[Test]
		public void VerifyCombineReturnTwoWhenIntersectingAndNotOriginalLayerStarts()
		{
			IActivity act = ActivityFactory.CreateActivity("test");
			IActivity newActivity = ActivityFactory.CreateActivity("sdfsdf");
			var layer2 = new MainShiftLayer(newActivity, new DateTimePeriod(1900, 1, 5, 2000, 1, 5));
			var layer1 = new MainShiftLayer(act, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));

			var retList = new[] { layer1, layer2 }.CreateProjection();

			Assert.AreEqual(2, retList.Count());
			Assert.AreEqual(layer2.Payload, retList.First().Payload);
			Assert.AreEqual(layer2.Period, retList.First().Period);
			Assert.AreEqual(layer1.Payload, retList.Last().Payload);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 5, 2001, 1, 1), retList.Last().Period);
		}

		[Test]
		public void VerifyCombineReturnOneWhenIntersectingOneActivity()
		{
			IActivity act = ActivityFactory.CreateActivity("test");
			var layer2 = new MainShiftLayer(act, new DateTimePeriod(2000, 1, 5, 2010, 1, 1));
			var layer1 = new MainShiftLayer(act, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));

			var proj = new[] { layer1, layer2 }.CreateProjection();

			Assert.AreSame(act, proj.Single().Payload);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2010, 1, 1), proj.Single().Period);
		}

		[Test]
		public void VerifyCombineReturnThreeWhenInside()
		{
			IActivity act = ActivityFactory.CreateActivity("test");
			IActivity newActivity = ActivityFactory.CreateActivity("sdfsdf");
			var layer2 = new MainShiftLayer(newActivity, new DateTimePeriod(2000, 1, 5, 2000, 1, 6));
			var layer1 = new MainShiftLayer(act, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));

			var retList = new[] { layer1, layer2 }.CreateProjection().ToList();

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
			var layer2 = new MainShiftLayer(act, new DateTimePeriod(2000, 1, 5, 2000, 1, 6));
			var layer1 = new MainShiftLayer(act, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));

			var retList = new[] { layer1, layer2 }.CreateProjection();

			Assert.AreEqual(layer1.Payload, retList.Single().Payload);
			Assert.AreEqual(layer1.Period, retList.Single().Period);
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
			var layer1 = new MainShiftLayer(act1, period1);
			var layer2 = new MainShiftLayer(act2, period2);
			var layer3 = new MainShiftLayer(act3, period3);
			var layer4 = new MainShiftLayer(act5, period6);

			//Expected layers
			var layer5 = new MainShiftLayer(act1, period1);
			var layer6 = new MainShiftLayer(act2, period2);
			var layer7 = new MainShiftLayer(act3, period8);
			var layer8 = new MainShiftLayer(act5, period6);
			var layer9 = new MainShiftLayer(act3, period7);

			orderedListLayerList.Add(layer5);
			orderedListLayerList.Add(layer6);
			orderedListLayerList.Add(layer7);
			orderedListLayerList.Add(layer8);
			orderedListLayerList.Add(layer9);

			var result = new[] { layer1, layer2, layer3, layer4 }.CreateProjection().ToList();

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
			var layer1 = new MainShiftLayer(act1, period1);
			var layer2 = new MainShiftLayer(act2, period2);
			var layer3 = new MainShiftLayer(act3, period3);
			var layer4 = new MainShiftLayer(act5, period6);

			//Expected layers
			var layer5 = new MainShiftLayer(act2, period4);
			var layer6 = new MainShiftLayer(act3, period5);
			var layer7 = new MainShiftLayer(act5, period6);
			var layer8 = new MainShiftLayer(act3, period7);


			orderedListLayerList.Add(layer5);
			orderedListLayerList.Add(layer6);
			orderedListLayerList.Add(layer7);
			orderedListLayerList.Add(layer8);

			var result = new[] { layer1, layer2, layer3, layer4 }.CreateProjection().ToList();

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
			var layer1 = new MainShiftLayer(act1, period1);
			var layer2 = new MainShiftLayer(act2, period2);
			var layer3 = new MainShiftLayer(act3, period3);
			var layer4 = new MainShiftLayer(act5, period6);

			//Expected layers
			var layer5 = new MainShiftLayer(act2, period4); //10-12 Rast
			var layer6 = new MainShiftLayer(act3, period5); //12-15:30 M�te
			var layer7 = new MainShiftLayer(act5, period6); //15:30-18 M�lsamtal


			orderedListLayerList.Add(layer5);
			orderedListLayerList.Add(layer6);
			orderedListLayerList.Add(layer7);

			var resWrapper = new[] { layer1, layer2, layer3, layer4 }.CreateProjection().ToList();

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

			var layer1 = new MainShiftLayer(act1, period1);
			var layer2 = new MainShiftLayer(act2, period2);
			var layer3 = new MainShiftLayer(act3, period3);

			DateTimePeriod period4 =
				new DateTimePeriod(new DateTime(2001, 1, 1, 16, 45, 0, DateTimeKind.Utc),
								   new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));
			var proj = new[] { layer1, layer2, layer3 }.CreateProjection();

			IList<IVisualLayer> payloads = new List<IVisualLayer>(proj.FilterLayers(period4));


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

			
			IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
			IPersonAssignment personAss = PersonAssignmentFactory.CreatePersonAssignment(person,scenario);
			personAss.AddPersonalActivity(act3, period3);
			personAss.AddActivity(act1, period1);
			personAss.AddActivity(act2, period2);
			IPersonAbsence personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, period4, absence);

			var currentAuthorization = CurrentAuthorization.Make();
			var day =
				ExtractedSchedule.CreateScheduleDay(
					new ScheduleDictionary(scenario,
										   new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2002, 1, 1)), new PersistableScheduleDataPermissionChecker(currentAuthorization), currentAuthorization),
					person, new DateOnly(2001, 1, 1), currentAuthorization);
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
			var layerFactory = new VisualLayerFactory();
			var refAct = ActivityFactory.CreateActivity("f");
			var period = new DateTimePeriod(2000,1,1,2001,1,2);
			var actLayer = layerFactory.CreateShiftSetupLayer(refAct, period);
			var absenceLayer = layerFactory.CreateAbsenceSetupLayer(AbsenceFactory.CreateAbsence("vacation"), actLayer, period);
			var target = new VisualLayerProjectionService();
			target.Add(absenceLayer);
			foreach (var vlayer in target.CreateProjection())
			{
				var layer = vlayer as VisualLayer;
				Assert.IsNotNull(layer);
				Assert.AreSame(refAct, layer.HighestPriorityActivity);
			}
		}

		[Test]
		public void VerifyMainShiftIsOverwrittenByOvertime()
		{
			IMultiplicatorDefinitionSet defSet =dummyDefinitionSet();
			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
																							ScenarioFactory.CreateScenarioAggregate(), createPeriod(12, 16));
			PersonFactory.AddDefinitionSetToPerson(ass.Person, defSet);
			IActivity overTimeActivity = ActivityFactory.CreateActivity("d");
			overTimeActivity.InWorkTime = true;
			ass.AddOvertimeActivity(overTimeActivity, createPeriod(14, 15), defSet);

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
			var activity = ActivityFactory.CreateActivity("d");
			activity.InWorkTime = true;
			ass.AddOvertimeActivity(activity, createPeriod(14, 15), defSet);

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
			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(person,
																				ScenarioFactory.CreateScenarioAggregate(), ActivityFactory.CreateActivity("d"), createPeriod(12,16));
			PersonFactory.AddDefinitionSetToPerson(ass.Person, defSet);
			ass.AddOvertimeActivity(ActivityFactory.CreateActivity("d"), createPeriod(14, 15), defSet);


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
	}
}
