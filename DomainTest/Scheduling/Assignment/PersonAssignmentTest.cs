using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

using Teleopti.Ccc.Domain.Scheduling.TimeLayer;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	/// <summary>
	/// Tests PersonAssignment
	/// </summary>
	[TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class PersonAssignmentTest
	{
		private IPersonAssignment target;
		private IPerson testPerson;
		private IScenario testScenario;

		[SetUp]
		public void Setup()
		{
			testScenario = new Scenario("sdf");
			testPerson = new Person();
			target = new PersonAssignment(testPerson, testScenario, new DateOnly(2000,1,1));
		}

		[Test]
		public void AddActivityWithTimePeriodShouldConsiderAgentTimeZone()
		{
			//Swedish, vintertid
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			testPerson.PermissionInformation.SetDefaultTimeZone(timeZone);

			target.AddActivity(new Activity("_"),new TimePeriod(8,0,16,0));

			var dateAsDateTimeUTC = DateTime.SpecifyKind(target.Date.Date,DateTimeKind.Utc);

			target.Period
				.Should()
				.Be.EqualTo(new DateTimePeriod(dateAsDateTimeUTC.AddHours(7),dateAsDateTimeUTC.AddHours(15)));
		}

		[Test]
		public void VerifyBelongsToScenario()
		{
			Assert.IsTrue(target.BelongsToScenario(target.Scenario));
			Assert.IsFalse(target.BelongsToScenario(new Scenario("f")));
			Assert.IsFalse(target.BelongsToScenario(null));
		}

		[Test]
		public void PersonAssignmentWithoutLayersShouldHaveSpecificPeriod()
		{
			var expected =
				new DateOnlyPeriod(target.Date, target.Date).ToDateTimePeriod(target.Person.PermissionInformation.DefaultTimeZone());
			target.Period.Should().Be.EqualTo(expected);
		}

		/// <summary>
		/// Check that AgentAssignment is created correct.
		/// </summary>
		[Test]
		public void CanCreateAssignmentAndPropertiesAreSet()
		{
			Assert.AreEqual(0, target.PersonalActivities().Count());
			Assert.AreEqual(null, target.Id);
			Assert.AreSame(testPerson, target.Person);
			Assert.AreSame(testScenario, target.Scenario);
			Assert.AreEqual(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment, target.FunctionPath);
			target.MainActivities().Should().Be.Empty();
			target.OvertimeActivities().Should().Be.Empty();
			target.PersonalActivities().Should().Be.Empty();
			target.ShiftCategory.Should().Be.Null();
			target.DayOff().Should().Be.Null();
			Assert.IsNull(target.UpdatedBy);
			Assert.IsNull(target.UpdatedOn);
			Assert.IsNull(target.Version);
			Assert.AreEqual(BusinessUnitUsedInTests.BusinessUnit, target.Scenario.GetOrFillWithBusinessUnit_DONTUSE());
		}


		/// <summary>
		/// Verifies the clear personal shift works.
		/// </summary>
		[Test]
		public void VerifyClearPersonalShiftWorks()
		{
			target.AddPersonalActivity(new Activity("d"), new DateTimePeriod(2000,1,1,2000,1,2));
			target.AddPersonalActivity(new Activity("d"), new DateTimePeriod(2000,1,1,2000,1,2));
			Assert.AreEqual(2, target.PersonalActivities().Count());
			target.ClearPersonalActivities();
			Assert.AreEqual(0, target.PersonalActivities().Count());
		}


		[Test]
		public void VerifyCreateWithReplacedParameters()
		{
			var newPer = new Person();
			var newScen = new Scenario("new scen");

			target = PersonAssignmentFactory.CreateAssignmentWithMainShift(testPerson, testScenario, new DateTimePeriod(2000,1,1,2000,1,2));
			target.SetId(Guid.NewGuid());
			var moveToTheseParameters = new PersonAssignment(newPer, newScen, new DateOnly(2000, 1, 1));

			var newAss = ((PersonAssignment)target).CloneAndChangeParameters(moveToTheseParameters);
			
			Assert.IsNull(newAss.Id);
			Assert.AreSame(newPer, newAss.Person);
			Assert.AreSame(newScen, newAss.Scenario);
			Assert.AreEqual(new DateTimePeriod(2000,1,1,2000,1,2), newAss.Period);
		}

		[Test]
		public void VerifyReferenceBackToAssignmentWorksFromOvertimeLayer()
		{
			IMultiplicatorDefinitionSet defSet = new MultiplicatorDefinitionSet("d", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(testPerson, defSet);
			target.AddOvertimeActivity(new Activity("d"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2), defSet);
			var layer = (IAggregateEntity)target.OvertimeActivities().Single();
			Assert.AreSame(target, layer.Parent);
			Assert.AreSame(target, layer.Root());
		}


		/// <summary>
		/// Verifies that the reference back to assignment works from a personal shift instance.
		/// </summary>
		[Test]
		public void VerifyReferenceBackToAssignmentWorksFromAPersonalLayer()
		{
			target.AddPersonalActivity(new Activity("d"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
			target.PersonalActivities().Single().Parent.Should().Be.SameInstanceAs(target);
		}

		[Test]
		public void VerifyMainReference()
		{
			Assert.AreSame(testPerson, target.MainRoot);
		}


		/// <summary>
		/// Verifies default constructor is not public.
		/// </summary>
		[Test]
		public void VerifyDisabledDefaultConstructor()
		{
			bool ret = ReflectionHelper.HasDefaultConstructor(target.GetType());
			Assert.IsTrue(ret);
		}


		/// <summary>
		/// Verifies the period property.
		/// </summary>
		[Test]
		public void VerifyPeriod()
		{
			var testShift = EditableShiftFactory.CreateEditorShiftWithThreeActivityLayers();

			new EditableShiftMapper().SetMainShiftLayers(target, testShift);

			DateTime expectedStart = testShift.LayerCollection[0].Period.StartDateTime;
			DateTime expectedEnd = testShift.LayerCollection[2].Period.EndDateTime;
			DateTimePeriod expectedPeriod = new DateTimePeriod(expectedStart, expectedEnd);
			DateTimePeriod resultPeriod = target.Period;
			Assert.AreEqual(expectedPeriod, resultPeriod);
		}

		[Test]
		public void VerifyOnlyPersonalShifts()
		{
			DateTime start = new DateTime(2001, 1, 1, 11, 0, 0, DateTimeKind.Utc);
			DateTime end = start.AddHours(1);
			IActivity activity = new Activity("act");

			DateTimePeriod personalShiftPeriod1 = new DateTimePeriod(start, end);
			DateTimePeriod personalShiftPeriod2 = personalShiftPeriod1.MovePeriod(TimeSpan.FromHours(1));
			DateTimePeriod personalShiftPeriod3 = personalShiftPeriod1.MovePeriod(TimeSpan.FromHours(4));
			DateTimePeriod expected = new DateTimePeriod(personalShiftPeriod1.StartDateTime, personalShiftPeriod3.EndDateTime);

			target.AddPersonalActivity(activity, personalShiftPeriod1);
			target.AddPersonalActivity(activity, personalShiftPeriod2);
			target.AddPersonalActivity(activity, personalShiftPeriod3);

			Assert.AreEqual(expected, target.Period);
		}

		[Test]
		public void VerifyOnlyMainShift()
		{
			DateTime start = new DateTime(2001, 1, 1, 11, 0, 0, DateTimeKind.Utc);
			DateTime end = start.AddHours(1);
			IActivity activity = new Activity("act");

			DateTimePeriod expected = new DateTimePeriod(start, end);
			DateTimePeriod mainShiftPeriod = new DateTimePeriod(start, end);

			target.AddActivity(activity, mainShiftPeriod);
		
			Assert.AreEqual(expected, target.Period);
		}

	  
		[Test]
		public void VerifyPeriodWithLongPersonalShiftOutsideMainShift()
		{
			DateTime start = new DateTime(2001, 1, 1, 11, 0, 0, DateTimeKind.Utc);
			DateTime end = start.AddHours(1);
			IActivity activity = new Activity("act");

			DateTimePeriod mainShiftPeriod = new DateTimePeriod(start, end);
			DateTimePeriod personalShiftPeriod = mainShiftPeriod.ChangeStartTime(TimeSpan.FromHours(-1)).ChangeEndTime(TimeSpan.FromHours(1));

			target.AddPersonalActivity(activity, personalShiftPeriod);
			target.AddActivity(activity, mainShiftPeriod);

			Assert.AreEqual(personalShiftPeriod, target.Period);
		}

		[Test]
		public void VerifyPeriodWithPersonalShiftInContactWithMainShift()
		{
			DateTime start = new DateTime(2001, 1, 1, 1, 0, 0, DateTimeKind.Utc);
			DateTime end = start.AddHours(4);
			IActivity activity = new Activity("act");
			IShiftCategory shiftCategory = new ShiftCategory("shiftCategory");

			DateTimePeriod mainShiftPeriod = new DateTimePeriod(start, end);
			DateTimePeriod personalPeriod1 = mainShiftPeriod.MovePeriod(TimeSpan.FromHours(-2));
			DateTimePeriod personalPeriod2 = mainShiftPeriod.MovePeriod(TimeSpan.FromHours(2));
			DateTimePeriod personalPeriod3 = mainShiftPeriod.MovePeriod(TimeSpan.FromHours(4));

			DateTimePeriod expectedPeriod = new DateTimePeriod(personalPeriod1.StartDateTime, personalPeriod3.EndDateTime);

			var mainShift = EditableShiftFactory.CreateEditorShift(activity, mainShiftPeriod, shiftCategory);
			new EditableShiftMapper().SetMainShiftLayers(target, mainShift);

			target.AddPersonalActivity(activity, personalPeriod1);
			target.AddPersonalActivity(activity, personalPeriod2);
			target.AddPersonalActivity(activity, personalPeriod3);

			Assert.AreEqual(expectedPeriod, target.Period);
		}


		[Test]
		public void PeriodExcludingPersonalActivityShouldReturnCorrectly()
		{
			DateTime start = new DateTime(2001,1,1,1,0,0,DateTimeKind.Utc);
			DateTime end = start.AddHours(4);
			IActivity activity = new Activity("act");
			
			DateTimePeriod mainShiftPeriod = new DateTimePeriod(start,end);
			DateTimePeriod personalPeriod = mainShiftPeriod.MovePeriod(TimeSpan.FromHours(-2));

			target.AddActivity(activity, mainShiftPeriod);
			target.AddPersonalActivity(activity,personalPeriod);
			
			target.PeriodExcludingPersonalActivity()
				.Should()
				.Be.EqualTo(mainShiftPeriod);
		}


		/// <summary>
		/// Verifies the restriction set is checked.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-11-07
		/// </remarks>
		[Test]
		public void VerifyRestrictionSetIsChecked()
		{
			target.CheckRestrictions();
		}

		/// <summary>
		/// Verifies the restriction set getter works.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-11-07
		/// </remarks>
		[Test]
		public void VerifyRestrictionSetIsCheckedWithoutArgument()
		{
			IRestrictionSet<IPersonAssignment> restrictionSet = target.RestrictionSet;
			Assert.IsNotNull(restrictionSet);
		}

		/// <summary>
		/// Verifies the restriction set is checked when agent assignment as object.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-11-07
		/// </remarks>
		[Test]
		public void VerifyRestrictionSetIsCheckedWhenAgentAssignmentAsObject()
		{
			new PersonAssignmentRestrictionSet().CheckEntity((object)target);
		}

		[Test]
		public void VerifyCloneWhenMainShiftIsNull()
		{
			IPersonAssignment targetClone = (IPersonAssignment) target.Clone();
			Assert.AreSame(target.Person, targetClone.Person);
			Assert.AreSame(target.Scenario, targetClone.Scenario);
			target.MainActivities().Should().Be.Empty();
			Assert.AreEqual(0, targetClone.PersonalActivities().Count());
		}

		[Test]
		public void VerifyProjectionIsEmptyIfNoMainShift()
		{
			target.AddPersonalActivity(ActivityFactory.CreateActivity("sdf"), new DateTimePeriod(2000,1,1,2001,1,1));
			IProjectionService svc = target.ProjectionService();
			Assert.IsNull(svc.CreateProjection().Period());
		}

		[Test]
		public void VerifyHasNoProjection()
		{
			target.ProjectionService().CreateProjection().Should().Be.Empty();
		}

		[Test]
		public void VerifyProjection()
		{
			var mainShiftActivity = ActivityFactory.CreateActivity("mainshift");
			var persShiftActivity = ActivityFactory.CreateActivity("persShfit");
			target.AddActivity(mainShiftActivity, new DateTimePeriod(2000, 1, 1, 2010, 1, 1));
			target.AddPersonalActivity(persShiftActivity, new DateTimePeriod(2002, 1, 1, 2003, 1, 1));
			IProjectionService svc = target.ProjectionService();
			svc.CreateProjection();
			IList<IVisualLayer> retList = new List<IVisualLayer>(svc.CreateProjection());

			Assert.AreEqual(3, retList.Count);
			Assert.AreSame(mainShiftActivity, retList[0].Payload);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2002, 1, 1), retList[0].Period);
			Assert.AreSame(persShiftActivity, retList[1].Payload);
			Assert.AreEqual(new DateTimePeriod(2002, 1, 1, 2003, 1, 1), retList[1].Period);
			Assert.AreSame(mainShiftActivity, retList[2].Payload);
			Assert.AreEqual(new DateTimePeriod(2003, 1, 1, 2010, 1, 1), retList[2].Period);
		}

		[Test]
		public void ShouldProjectAdjacentPersonalShifts()
		{
			var mainShiftActivity = ActivityFactory.CreateActivity("mainshift");
			var personalShiftActivity = ActivityFactory.CreateActivity("personalshiftactivity");

			var mainShiftStart = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var mainShiftEnd = new DateTime(2000, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			var mainShiftPeriod = new DateTimePeriod(mainShiftStart, mainShiftEnd);

			var personalShiftStart = new DateTime(2000, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			var personalShiftEnd = new DateTime(2000, 1, 1, 18, 0, 0, DateTimeKind.Utc);
			var personalShiftPeriod = new DateTimePeriod(personalShiftStart, personalShiftEnd);

			target.AddActivity(mainShiftActivity, mainShiftPeriod);

			target.AddPersonalActivity(personalShiftActivity, personalShiftPeriod);

			var svc = target.ProjectionService();
			svc.CreateProjection();
			IList<IVisualLayer> retList = new List<IVisualLayer>(svc.CreateProjection());

			Assert.AreEqual(2,retList.Count);
			Assert.AreSame(mainShiftActivity, retList[0].Payload);
			Assert.AreEqual(mainShiftPeriod, retList[0].Period);
			Assert.AreSame(personalShiftActivity, retList[1].Payload);
			Assert.AreEqual(personalShiftPeriod, retList[1].Period);
			
		}

		[Test]
		public void VerifyRemovePersonalShift()
		{
			var persShiftActivity = ActivityFactory.CreateActivity("persShfit");
			target.AddPersonalActivity(persShiftActivity, new DateTimePeriod(2002, 1, 1, 2003, 1, 1));
			target.AddPersonalActivity(persShiftActivity, new DateTimePeriod(2002, 1, 1, 2003, 1, 1));

			Assert.AreEqual(2, target.PersonalActivities().Count());
			target.RemoveActivity(target.PersonalActivities().First());

			Assert.AreEqual(1, target.PersonalActivities().Count());
		}

		[Test]
		public void VerifyICloneableEntity()
		{
			target.SetId(Guid.NewGuid());

			IActivity persShiftActivity = ActivityFactory.CreateActivity("persShfit");
			var dayOffTemplate = new DayOffTemplate(new Description());

			target.AddOvertimeActivity(persShiftActivity, new DateTimePeriod(2000,1,1,2000,1,2), MockRepository.GenerateMock<IMultiplicatorDefinitionSet>());
			target.AddPersonalActivity(persShiftActivity, new DateTimePeriod(2002, 1, 1, 2003, 1, 1));
			target.PersonalActivities().Single().SetId(Guid.NewGuid());
			target.OvertimeActivities().Single().SetId(Guid.NewGuid());
			target.SetDayOff(dayOffTemplate);

			IPersonAssignment pAss = target.EntityClone();
			Assert.AreEqual(target.Id, pAss.Id);
			Assert.AreEqual(target.PersonalActivities().Single().Id, pAss.PersonalActivities().Single().Id);
			Assert.AreEqual(target.OvertimeActivities().Single().Id, pAss.OvertimeActivities().Single().Id);
			Assert.AreEqual(target.Person.Id, pAss.Person.Id);
			pAss.DayOff().Should().Not.Be.Null();

			pAss = target.NoneEntityClone();
			Assert.AreEqual(target.Person.Id, pAss.Person.Id);
			Assert.IsNull(pAss.Id);
			Assert.IsNull(pAss.PersonalActivities().Single().Id);
			Assert.IsNull(pAss.OvertimeActivities().Single().Id);
			pAss.DayOff().Should().Not.Be.Null();

			pAss = (IPersonAssignment)target.CreateTransient();
			Assert.AreEqual(target.Person.Id, pAss.Person.Id);
			Assert.IsNull(pAss.Id);
			Assert.IsNull(pAss.PersonalActivities().Single().Id);
			Assert.IsNull(pAss.OvertimeActivities().Single().Id);
			pAss.DayOff().Should().Not.Be.Null();
		}

		[Test]
		public void NoneEntityClone_WhenLayersExists_ShouldClearTheIdOfEachLayer()
		{
			target.SetId(Guid.NewGuid());

			IActivity persShiftActivity = ActivityFactory.CreateActivity("persShfit");
			var dayOffTemplate = new DayOffTemplate(new Description());

			target.AddOvertimeActivity(persShiftActivity, new DateTimePeriod(2000, 1, 1, 2000, 1, 2), MockRepository.GenerateMock<IMultiplicatorDefinitionSet>());
			target.AddPersonalActivity(persShiftActivity, new DateTimePeriod(2002, 1, 1, 2003, 1, 1));
			target.PersonalActivities().Single().SetId(Guid.NewGuid());
			target.OvertimeActivities().Single().SetId(Guid.NewGuid());
			target.SetDayOff(dayOffTemplate);

			IPersonAssignment pAss = target.NoneEntityClone();
			Assert.AreEqual(target.Person.Id, pAss.Person.Id);
			Assert.IsNull(pAss.Id);
			Assert.IsNull(pAss.PersonalActivities().Single().Id);
			Assert.AreEqual(target.PersonalActivities().Single().Parent, pAss.PersonalActivities().Single().Parent);
			Assert.IsNull(pAss.OvertimeActivities().Single().Id);
			Assert.AreEqual(target.OvertimeActivities().Single().Parent, pAss.OvertimeActivities().Single().Parent);
			pAss.DayOff().Should().Not.Be.Null();

		}

		[Test]
		public void EntityClone_WhenLayersExists_ShouldKeepTheIdAndparentOfEachLayer()
		{
			target.SetId(Guid.NewGuid());

			IActivity persShiftActivity = ActivityFactory.CreateActivity("persShfit");
			var dayOffTemplate = new DayOffTemplate(new Description());

			target.AddOvertimeActivity(persShiftActivity, new DateTimePeriod(2000, 1, 1, 2000, 1, 2), MockRepository.GenerateMock<IMultiplicatorDefinitionSet>());
			target.AddPersonalActivity(persShiftActivity, new DateTimePeriod(2002, 1, 1, 2003, 1, 1));
			target.PersonalActivities().Single().SetId(Guid.NewGuid());
			target.OvertimeActivities().Single().SetId(Guid.NewGuid());
			target.SetDayOff(dayOffTemplate);

			IPersonAssignment pAss = target.EntityClone();
			Assert.AreEqual(target.Id, pAss.Id);
			Assert.AreEqual(target.PersonalActivities().Single().Id, pAss.PersonalActivities().Single().Id);
			Assert.AreEqual(target.PersonalActivities().Single().Parent, pAss.PersonalActivities().Single().Parent);
			Assert.AreEqual(target.OvertimeActivities().Single().Id, pAss.OvertimeActivities().Single().Id);
			Assert.AreEqual(target.OvertimeActivities().Single().Parent, pAss.OvertimeActivities().Single().Parent);
			Assert.AreEqual(target.Person.Id, pAss.Person.Id);
			pAss.DayOff().Should().Not.Be.Null();

		}

		[Test]
		public void PersonalShiftMustBeInsideMainShiftOrOvertimeToBeIncludedInProjection()
		{
			var defSet = new MultiplicatorDefinitionSet("d", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(testPerson, defSet);
			var act = new Activity("sdf");
			target.AddOvertimeActivity(act, new DateTimePeriod(2000, 1, 1, 2000, 1, 2), defSet);
			target.AddActivity(act, new DateTimePeriod(2000, 1, 5, 2000, 1, 6));

			Assert.AreEqual(2, target.ProjectionService().CreateProjection().Count());

			target.AddPersonalActivity(act, new DateTimePeriod(2000, 1, 3, 2000, 1, 4));
			Assert.AreEqual(2, target.ProjectionService().CreateProjection().Count());
		}

		[Test]
		public void PersonLayerShouldNotBePartOfProjectionIfBetweenMainLayers()
		{
			var start = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var act = new Activity("d");
			target.AddActivity(act, new DateTimePeriod(start.AddHours(1), start.AddHours(2)));
			target.AddActivity(act, new DateTimePeriod(start.AddHours(6), start.AddHours(7)));

			target.AddPersonalActivity(act, new DateTimePeriod(start.AddHours(3), start.AddHours(4)));
			target.ProjectionService().CreateProjection()
				.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldSetDayOff()
		{
			var desc = new Description("desc");
			var template = new DayOffTemplate(desc);
			target.SetDayOff(template);
			target.DayOff().Description.Should().Be.EqualTo(desc);
		}

		[Test]
		public void ShouldSetDayOffAsNull()
		{
			target.SetDayOff(null);
			target.DayOff().Should().Be.Null();
		}

		[Test]
		public void ShouldSetThisDayOffOnDestination()
		{
			var desc = new Description("desc");
			var template = new DayOffTemplate(desc);
			target.SetDayOff(template);
			var ass = new PersonAssignment(new Person(), new Scenario("d"), new DateOnly());
			target.SetThisAssignmentsDayOffOn(ass);

			ass.DayOff().Description.Should().Be.EqualTo(desc);
		}

		[Test]
		public void ShouldSetThisDayOffOnDestinationAsNull()
		{
			var desc = new Description("desc");
			var template = new DayOffTemplate(desc);
			var ass = new PersonAssignment(new Person(), new Scenario("d"), new DateOnly());
			ass.SetDayOff(template);

			target.SetThisAssignmentsDayOffOn(ass);

			ass.DayOff().Should().Be.Null();
		}

		[Test]
		public void AssignedWithDayoffFulfilled()
		{
			var template = new DayOffTemplate();
			target.SetDayOff(template);
			target.AssignedWithDayOff(template)
			      .Should().Be.True();
		}

		[Test]
		public void AssignedWithDayoffDifferent()
		{
			target.SetDayOff(new DayOffTemplate());
			target.AssignedWithDayOff(new DayOffTemplate())
						.Should().Be.False();
		}

		[Test]
		public void AssignedWithDayoffWhenAssignmentHasNoDayoff()
		{
			target.AssignedWithDayOff(new DayOffTemplate())
						.Should().Be.False();
		}

		[Test]
		public void AssignedWithDayoffCompareWithNull()
		{
			target.SetDayOff(new DayOffTemplate());
			target.AssignedWithDayOff(null)
						.Should().Be.False();
		}


		[Test]
		public void AssignedWithDayoffBothNull()
		{
			target.AssignedWithDayOff(null)
						.Should().Be.True();
		}

		[Test]
		public void ClearShouldRemoveEverything()
		{
			var activity = ActivityFactory.CreateActivity("hej");
			var period = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
			target.AddOvertimeActivity(activity, period, null);
			target.AddPersonalActivity(activity, period);
			target.AddActivity(activity, period);
			target.SetShiftCategory(ShiftCategoryFactory.CreateShiftCategory("cat"));
			target.Clear();
			target.OvertimeActivities().Should().Be.Empty();
			target.PersonalActivities().Should().Be.Empty();
			target.MainActivities().Should().Be.Empty();
			target.ShiftCategory.Should().Be.Null();

			target.SetDayOff(DayOffFactory.CreateDayOff());
			target.Clear();
			target.DayOff().Should().Be.Null();
		}

		[Test]
		public void ShouldClearIfFillWithDataIsEmpty()
		{
			var activity = ActivityFactory.CreateActivity("hej");
			var period = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
			target.AddOvertimeActivity(activity, period, null);
			target.AddPersonalActivity(activity, period);
			target.AddActivity(activity, period);
			target.FillWithDataFrom(new PersonAssignment(target.Person, target.Scenario, target.Date));
			target.OvertimeActivities().Should().Be.Empty();
			target.PersonalActivities().Should().Be.Empty();
			target.MainActivities().Should().Be.Empty();
		}

		[Test]
		public void ShouldFillWithData()
		{
			var newAss = new PersonAssignment(target.Person, target.Scenario, target.Date);
			var activity = ActivityFactory.CreateActivity("hej");
			var period = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
			newAss.AddOvertimeActivity(activity, period, null);
			newAss.AddPersonalActivity(activity, period);
			newAss.AddActivity(activity, period);
			target.FillWithDataFrom(newAss);
			target.OvertimeActivities().Should().Not.Be.Empty();
			target.PersonalActivities().Should().Not.Be.Empty();
			target.MainActivities().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldFillWithDayOff()
		{
			var newAss = new PersonAssignment(target.Person, target.Scenario, target.Date);
			newAss.SetDayOff(new DayOffTemplate());
			var activity = ActivityFactory.CreateActivity("hej");
			var period = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
			newAss.AddOvertimeActivity(activity, period, null);
			newAss.AddPersonalActivity(activity, period);
			target.FillWithDataFrom(newAss);
			target.OvertimeActivities().Should().Not.Be.Empty();
			target.PersonalActivities().Should().Not.Be.Empty();
			target.DayOff().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldKeepIdAndVersionCallingFillWithData()
		{
			var id = Guid.NewGuid();
			const int version = 123;
			target.SetId(id);
			target.SetVersion(version);
			target.FillWithDataFrom(new PersonAssignment(target.Person, target.Scenario, target.Date));
			target.Id.Should().Be.EqualTo(id);
			target.Version.Should().Be.EqualTo(version);
		}

		[Test]
		public void ShouldClearMainLayersWhenSettingDayoff()
		{
			target.AddActivity(new Activity("d"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
			target.SetDayOff(new DayOffTemplate());
			target.MainActivities().Should().Be.Empty();
		}

		[Test]
		public void ShouldRemoveDayoffWhenAddingMainLayer()
		{
			target.SetDayOff(new DayOffTemplate());
			target.AddActivity(new Activity("d"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
			target.DayOff().Should().Be.Null();
		}

		[Test]
		public void ShouldRemoveDayoffWhenInsertinggMainLayer()
		{
			target.SetDayOff(new DayOffTemplate());
			target.InsertActivity(new Activity("d"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2),0);
			target.DayOff().Should().Be.Null();
		}

		[Test]
		public void ShouldInstertMainLayerAtGivenPosition()
		{
			IActivity activity1 = new Activity("act1");
			IActivity activity2 = new Activity("act2");
			IActivity activity3 = new Activity("act3");

			var period = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);

			target.AddActivity(activity1, period);
			target.AddActivity(activity3, period);

			target.InsertActivity(activity2,period,1);

			target.MainActivities().First().Payload.Should().Be.EqualTo(activity1);
			target.MainActivities().Skip(1).First().Payload.Should().Be.EqualTo(activity2);
			target.MainActivities().Skip(2).First().Payload.Should().Be.EqualTo(activity3);
		}

		[Test]
		public void ShouldKeepOtherMainShiftLayersWhenInsertingAwMainShiftLayer()
		{
			target.AddActivity(new Activity("any activity"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
			var firstLayer = target.MainActivities().First();

			target.InsertActivity(new Activity("any activity"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2), 0);
			target.MainActivities().Skip(1).First().Should().Be.SameInstanceAs(firstLayer);
		}

		[Test]
		public void InsertPersonalLayer_WhenOtherLayersExistsOnTheAssignmanet_ShouldCreateAndInsertTheLayerAtTheGivenPosition()
		{
			var personassignment = PersonAssignmentFactory.CreateAssignmentWithOvertimePersonalAndMainshiftLayers();
			var period = personassignment.ShiftLayers.First().Period.MovePeriod(TimeSpan.FromMinutes(1));
			personassignment.InsertPersonalLayer(new Activity("for test"), period, 2);
			var expectedLayer = personassignment.ShiftLayers.Skip(2).First();

			Assert.That(expectedLayer.Payload.Name, Is.EqualTo("for test"));
			Assert.That(expectedLayer.Period, Is.EqualTo(period));			
			Assert.That(expectedLayer is PersonalShiftLayer);
		}

		[Test]
		public void InsertOvertimeLayer_WhenOtherLayersExistsOnTheAssignment_ShouldCreateAndInsertTheLayerAtTheGivenPosition()
		{
			var personassignment = PersonAssignmentFactory.CreateAssignmentWithOvertimePersonalAndMainshiftLayers();
			var period = personassignment.ShiftLayers.First().Period.MovePeriod(TimeSpan.FromMinutes(1));
			personassignment.InsertOvertimeLayer(new Activity("for test"), period, 1, new MultiplicatorDefinitionSet("multi", MultiplicatorType.Overtime));
			var expectedLayer = personassignment.ShiftLayers.Skip(1).First();

			Assert.That(expectedLayer.Payload.Name,Is.EqualTo("for test"));
			Assert.That(expectedLayer.Period,Is.EqualTo(period));
			Assert.That(expectedLayer is OvertimeShiftLayer);
		}

		[Test]
		public void ShouldClearPersonalAndOvertimeWhenSettingActivitiesAndShiftCategoryFromSource()
		{
			target.AddPersonalActivity(new Activity("d"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
			target.AddOvertimeActivity(new Activity("_"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2), new MultiplicatorDefinitionSet("_", MultiplicatorType.Overtime));
			var source = PersonAssignmentFactory.CreateAssignmentWithMainShift(testPerson,
				testScenario, new Activity("_"), new DateTimePeriod(2000, 1, 3, 2000, 1, 4), new ShiftCategory("_"));

			target.SetActivitiesAndShiftCategoryFromWithOffset(source, TimeSpan.Zero);

			target.PersonalActivities().Should().Be.Empty();
			target.OvertimeActivities().Should().Be.Empty();
		}

		[Test]
		public void ShouldOnlyAppendEventsToCurrentInstanceWhenCloned()
		{
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, false).WithId();

			target = new PersonAssignment(agent, scenario, new DateOnly(2000, 1, 1));
			target.AddPersonalActivity(new Activity("d"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
			target.AddOvertimeActivity(new Activity("_"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2),
				new MultiplicatorDefinitionSet("_", MultiplicatorType.Overtime));
			var source = PersonAssignmentFactory.CreateAssignmentWithMainShift(testPerson,
				scenario, new Activity("_"), new DateTimePeriod(2000, 1, 3, 2000, 1, 4), new ShiftCategory("_"));

			var clone = target.NoneEntityClone();
			clone.SetActivitiesAndShiftCategoryFromWithOffset(source, TimeSpan.Zero);

			var allEvents = target.PopAllEvents(null);
			allEvents.OfType<DayUnscheduledEvent>().Count().Should().Be(0);
		}

		[Test]
		public void ShouldRaiseMainShiftCategoryReplaceEvent()
		{
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var layerPeriod = new DateTimePeriod(2016, 8, 9, 2016, 8, 10);
			var assignment =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(agent,
					ScenarioFactory.CreateScenarioWithId("_", true), layerPeriod, ShiftCategoryFactory.CreateShiftCategory("current"));

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			assignment.SetShiftCategory(ShiftCategoryFactory.CreateShiftCategory("newOne"), false, new TrackedCommandInfo
			{
				OperatedPersonId = operatedPersonId,
				TrackId = trackId
			});

			var theEvent = assignment.PopAllEvents(null).OfType<MainShiftCategoryReplaceEvent>().Single();
			theEvent.PersonId.Should().Be(agent.Id.Value);
			theEvent.Date.Should().Be(new DateTime(2016, 8, 9));
			theEvent.ScenarioId.Should().Be(assignment.Scenario.Id.Value);
			theEvent.InitiatorId.Should().Be(operatedPersonId);
			theEvent.CommandId.Should().Be(trackId);
		}

		[Test]
		public void ShouldOnlyRaiseMainShiftCategoryReplaceEvent()
		{
			var agent = new Person().WithId();
			var layerPeriod = new DateTimePeriod(2016, 8, 9, 2016, 8, 10);
			var assignment =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(agent,
					ScenarioFactory.CreateScenarioWithId("_", true), layerPeriod, ShiftCategoryFactory.CreateShiftCategory("current"));

			assignment.PopAllEvents(null);

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			assignment.SetShiftCategory(ShiftCategoryFactory.CreateShiftCategory("newOne"), false, new TrackedCommandInfo
			{
				OperatedPersonId = operatedPersonId,
				TrackId = trackId
			});

			assignment.PopAllEvents(null).Count()
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldMoveActivityIfIsOvertimeLayer()
		{
			var personassignment = PersonAssignmentFactory.CreateAssignmentWithOvertimePersonalAndMainshiftLayers();
			var period = personassignment.ShiftLayers.First().Period.MovePeriod(TimeSpan.FromMinutes(1));

			var overtimeLayer = personassignment.ShiftLayers.Skip(1).First();
			var newStartTime = period.StartDateTime.AddMinutes(30);
			personassignment.MoveActivityAndKeepOriginalPriority(overtimeLayer, newStartTime, new TrackedCommandInfo { OperatedPersonId = testPerson.Id.GetValueOrDefault() });

			overtimeLayer = personassignment.ShiftLayers.Skip(1).First();
			overtimeLayer.Period.StartDateTime.Should().Be.EqualTo(newStartTime);
		}
		
		[Test]
		public void ShouldThrowExceptionWhenAddingZeroLengthMainShift()
		{
			var activity = new  Activity("_");
			var dateTime = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(dateTime, dateTime);
			
			Assert.Throws<ArgumentException>(() => target.AddActivity(activity,period));				
		}

		[Test]
		public void ShouldThrowExceptionWhenAddingZeroLengthOverTimeShift()
		{
			var activity = new  Activity("_");
			var dateTime = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(dateTime, dateTime);
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("multiplicatorset", MultiplicatorType.Overtime);
			
			Assert.Throws<ArgumentException>(() => target.AddOvertimeActivity(activity, period, multiplicatorDefinitionSet));	
		}

		[Test]
		public void ShouldThrowExceptionWhenAddingZeroLengtPersonalActivity()
		{
			var activity = new  Activity("_");
			var dateTime = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(dateTime, dateTime);

			Assert.Throws<ArgumentException>(() => target.AddPersonalActivity(activity, period));
		}
	}
}
