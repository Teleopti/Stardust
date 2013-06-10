using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	/// <summary>
	/// Tests PersonAssignment
	/// </summary>
	[TestFixture]
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
		public void AtLeastOneMainShiftLayerMustBeSet()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => 
				target.SetMainShiftLayers(new IMainShiftActivityLayerNew[0], new ShiftCategory("foo")));
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
			Assert.AreEqual(PersonAssignment.UndefinedPeriod, target.Period);
		}

		/// <summary>
		/// Check that AgentAssignment is created correct.
		/// </summary>
		[Test]
		public void CanCreateAssignmentAndPropertiesAreSet()
		{
			Assert.AreEqual(0, target.PersonalShiftCollection.Count);
			Assert.AreEqual(null, target.Id);
			Assert.AreSame(testPerson, target.Person);
			Assert.AreSame(testScenario, target.Scenario);
			Assert.AreEqual(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment, target.FunctionPath);
			target.MainShiftActivityLayers.Should().Be.Empty();
			target.ShiftCategory.Should().Be.Null();
			Assert.IsNull(target.CreatedBy);
			Assert.IsNull(target.UpdatedBy);
			Assert.IsNull(target.CreatedOn);
			Assert.IsNull(target.UpdatedOn);
			Assert.IsNull(target.Version);
			Assert.AreEqual(BusinessUnitFactory.BusinessUnitUsedInTest, target.BusinessUnit);
			DateTime date = DateTime.Now;
			target.ZOrder = date;
			Assert.AreEqual(date, target.ZOrder);
		}


		/// <summary>
		/// Verifies the clear personal shift works.
		/// </summary>
		[Test]
		public void VerifyClearPersonalShiftWorks()
		{
			target.AddPersonalShift(new PersonalShift());
			target.AddPersonalShift(new PersonalShift());
			Assert.AreEqual(2, target.PersonalShiftCollection.Count);
			target.ClearPersonalShift();
			Assert.AreEqual(0, target.PersonalShiftCollection.Count);
		}

		/// <summary>
		/// PersonalShiftCol property should be locked.
		/// </summary>
		[Test]
		[ExpectedException(typeof (NotSupportedException))]
		public void PersonalShiftCollectionPropertyShouldBeLocked()
		{
			ICollection<IPersonalShift> temp = target.PersonalShiftCollection;
			temp.Add(new PersonalShift());
		}

		[Test]
		public void VerifyCreateWithReplacedParameters()
		{
			var newPer = new Person();
			var newScen = new Scenario("new scen");

			target = PersonAssignmentFactory.CreateAssignmentWithMainShift(testScenario, testPerson, new DateTimePeriod(2000,1,1,2000,1,2));
			target.SetId(Guid.NewGuid());
			var moveToTheseParameters = new PersonAssignment(newPer, newScen, new DateOnly(2000, 1, 1));

			IPersistableScheduleData newAss = ((PersonAssignment)target).CloneAndChangeParameters(moveToTheseParameters);
			
			Assert.IsNull(newAss.Id);
			Assert.AreSame(newPer, newAss.Person);
			Assert.AreSame(newScen, newAss.Scenario);
			Assert.AreEqual(new DateTimePeriod(2000,1,1,2000,1,2), newAss.Period);
		}

		[Test]
		public void VerifyAddAndRemoveOvertimeShift()
		{
			Assert.AreEqual(0, target.OvertimeShiftCollection.Count);
			IOvertimeShift overtimeShift = new OvertimeShift();
			target.AddOvertimeShift(overtimeShift);
			Assert.AreEqual(1, target.OvertimeShiftCollection.Count);
			target.RemoveOvertimeShift(overtimeShift);
			Assert.AreEqual(0, target.OvertimeShiftCollection.Count);
		}

		[Test]
		public void VerifyReferenceBackToAssignmentWorksFromOvertimeShift()
		{
			IMultiplicatorDefinitionSet defSet = new MultiplicatorDefinitionSet("d", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(testPerson, defSet);
			OvertimeShift overtimeShift = new OvertimeShift();
			OvertimeShiftActivityLayer actLay = new OvertimeShiftActivityLayer(new Activity("d"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2), defSet);
			target.AddOvertimeShift(overtimeShift);
			overtimeShift.LayerCollection.Add(actLay);
			Assert.AreSame(target, overtimeShift.Parent);
			Assert.AreSame(target, ((IAggregateEntity)actLay).Root());
			Assert.AreSame(target, ((IAggregateEntity)overtimeShift).Root());
		}

		/// <summary>
		/// Verifies a personal shift can be added.
		/// </summary>
		[Test]
		public void VerifyPersonalShiftCanBeAdded()
		{
			PersonalShift shift = new PersonalShift();
			target.AddPersonalShift(shift);
			Assert.Contains(shift, target.PersonalShiftCollection);
		}

		/// <summary>
		/// Duplicate personal shifts should be ignored when added to list.
		/// </summary>
		[Test]
		public void DoNotDuplicatePersonalShiftInstancesWhenAddedToList()
		{
			PersonalShift shift = new PersonalShift();
			target.AddPersonalShift(shift);
			target.AddPersonalShift(shift);
			Assert.AreSame(shift, target.PersonalShiftCollection[0]);
			Assert.AreEqual(1, target.PersonalShiftCollection.Count);
		}

		/// <summary>
		/// Verifies that the reference back to assignment works from a personal shift instance.
		/// </summary>
		[Test]
		public void VerifyReferenceBackToAssignmentWorksFromAPersonalShift()
		{
			PersonalShift personalShift = new PersonalShift();
			Activity act = ActivityFactory.CreateActivity("TestActivity");
			act.GroupingActivity = new GroupingActivity("test");
			PersonalShiftActivityLayer actLay = new PersonalShiftActivityLayer(act, new DateTimePeriod());
			personalShift.LayerCollection.Add(actLay);
			target.AddPersonalShift(personalShift);
			Assert.AreSame(target, personalShift.Parent);
			Assert.AreSame(target, ((IAggregateEntity) actLay).Root());
			Assert.AreSame(target, ((IAggregateEntity) personalShift).Root());
		}

		[Test]
		public void VerifyMainReference()
		{
			Assert.AreSame(testPerson, target.MainRoot);
		}

		/// <summary>
		/// Null personal shifts are not allowed.
		/// </summary>
		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void NullPersonalShiftsAreNotAllowed()
		{
			target.AddPersonalShift(null);
		}

		/// <summary>
		/// BaseShift can be set on an assignement.
		/// </summary>
		[Test]
		public void CanSetBaseShift()
		{
			var category = new ShiftCategory("Morning");
			MainShift baseShift = MainShiftFactory.CreateMainShift(new Activity("hej"), target.Period, category);
			target.SetMainShift(baseShift);
			Assert.AreEqual(target.MainShiftActivityLayers.Count(), baseShift.LayerCollection.Count);
			Assert.AreSame(target.ShiftCategory, baseShift.ShiftCategory);
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
		/// Protected constructor works.
		/// </summary>
		[Test]
		public void ProtectedConstructorWorks()
		{
			target = new testAssignment();
			Assert.IsNotNull(target);
		}

		/// <summary>
		/// Verifies the set main shift does not accept null.
		/// </summary>
		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void VerifySetMainShiftDoesNotAcceptNull()
		{
			target = new testAssignment();
			target.SetMainShift(null);
		}

		/// <summary>
		/// Verifies the period property.
		/// </summary>
		[Test]
		public void VerifyPeriod()
		{
			IMainShift testShift = MainShiftFactory.CreateMainShiftWithThreeActivityLayers();

			target.SetMainShift(testShift);

			DateTime expectedStart = testShift.LayerCollection[0].Period.StartDateTime;
			DateTime expectedEnd = testShift.LayerCollection[2].Period.EndDateTime;
			DateTimePeriod expectedPeriod = new DateTimePeriod(expectedStart, expectedEnd);
			DateTimePeriod resultPeriod = target.Period;
			Assert.AreEqual(expectedPeriod, resultPeriod);
		}

		[Test]
		public void VerifyDatabasePeriod()
		{
			var testShift = MainShiftFactory.CreateMainShiftWithThreeActivityLayers();
			target.SetMainShift(testShift);

			((PersonAssignment)target).DatabasePeriod.Should().Be.EqualTo(new DateTimePeriod());
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

			IPersonalShift personalShift1 = PersonalShiftFactory.CreatePersonalShift(activity, personalShiftPeriod1);
			IPersonalShift personalShift2 = PersonalShiftFactory.CreatePersonalShift(activity, personalShiftPeriod2);
			IPersonalShift personalShift3 = PersonalShiftFactory.CreatePersonalShift(activity, personalShiftPeriod3);

			target.AddPersonalShift(personalShift1);
			target.AddPersonalShift(personalShift2);
			target.AddPersonalShift(personalShift3);

			Assert.AreEqual(expected, target.Period);
		}

		[Test]
		public void VerifyOnlyMainShift()
		{
			DateTime start = new DateTime(2001, 1, 1, 11, 0, 0, DateTimeKind.Utc);
			DateTime end = start.AddHours(1);
			IActivity activity = new Activity("act");
			IShiftCategory shiftCategory = new ShiftCategory("shiftCategory");

			DateTimePeriod expected = new DateTimePeriod(start, end);
			DateTimePeriod mainShiftPeriod = new DateTimePeriod(start, end);

			IMainShift mainShift = MainShiftFactory.CreateMainShift(activity, mainShiftPeriod, shiftCategory);

			target.SetMainShift(mainShift);

			Assert.AreEqual(expected, target.Period);
		}

	  
		[Test]
		public void VerifyPeriodWithLongPersonalShiftOutsideMainShift()
		{
			DateTime start = new DateTime(2001, 1, 1, 11, 0, 0, DateTimeKind.Utc);
			DateTime end = start.AddHours(1);
			IActivity activity = new Activity("act");
			IShiftCategory shiftCategory = new ShiftCategory("shiftCategory");

			DateTimePeriod mainShiftPeriod = new DateTimePeriod(start, end);
			DateTimePeriod personalShiftPeriod = mainShiftPeriod.ChangeStartTime(TimeSpan.FromHours(-1)).ChangeEndTime(TimeSpan.FromHours(1));

			IMainShift mainShift = MainShiftFactory.CreateMainShift(activity, mainShiftPeriod, shiftCategory);
			IPersonalShift personalShift = PersonalShiftFactory.CreatePersonalShift(activity, personalShiftPeriod);

			target.AddPersonalShift(personalShift);
			target.SetMainShift(mainShift);

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

			IMainShift mainShift = MainShiftFactory.CreateMainShift(activity, mainShiftPeriod, shiftCategory);
			IPersonalShift personalShift1 = PersonalShiftFactory.CreatePersonalShift(activity, personalPeriod1);
			IPersonalShift personalShift2 = PersonalShiftFactory.CreatePersonalShift(activity, personalPeriod2);
			IPersonalShift personalShift3 = PersonalShiftFactory.CreatePersonalShift(activity, personalPeriod3);

			target.AddPersonalShift(personalShift1);
			target.AddPersonalShift(personalShift2);
			target.AddPersonalShift(personalShift3);
			target.SetMainShift(mainShift);

			Assert.AreEqual(expectedPeriod, target.Period);
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
			target.MainShiftActivityLayers.Should().Be.Empty();
			Assert.AreEqual(0, targetClone.PersonalShiftCollection.Count);
		}

		[Test]
		public void VerifyProjectionIsEmptyIfNoMainShift()
		{
			target.AddPersonalShift(PersonalShiftFactory.CreatePersonalShift(ActivityFactory.CreateActivity("sdf"), new DateTimePeriod(2000,1,1,2001,1,1)));
			IProjectionService svc = target.ProjectionService();
			Assert.IsNull(svc.CreateProjection().Period());
		}

		[Test]
		public void VerifyHasProjection()
		{
			Assert.IsFalse(target.HasProjection);
			target.SetMainShift(MainShiftFactory.CreateMainShiftWithThreeActivityLayers());
			Assert.IsTrue(target.HasProjection);
		}

		[Test]
		public void VerifyProjection()
		{
			Activity mainShiftActivity = ActivityFactory.CreateActivity("mainshift");
			Activity persShiftActivity = ActivityFactory.CreateActivity("persShfit");
			MainShift mainShift =
				MainShiftFactory.CreateMainShift(mainShiftActivity, new DateTimePeriod(2000, 1, 1, 2010, 1, 1), ShiftCategoryFactory.CreateShiftCategory("sdf"));
			PersonalShift persShift =
				PersonalShiftFactory.CreatePersonalShift(persShiftActivity, new DateTimePeriod(2002, 1, 1, 2003, 1, 1));

			target.SetMainShift(mainShift);
			target.AddPersonalShift(persShift);
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

			var mainShift = MainShiftFactory.CreateMainShift(mainShiftActivity, mainShiftPeriod, ShiftCategoryFactory.CreateShiftCategory("shiftcategory"));
			var persShift = PersonalShiftFactory.CreatePersonalShift(personalShiftActivity, personalShiftPeriod);

			target.SetMainShift(mainShift);
			target.AddPersonalShift(persShift);

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
			Activity persShiftActivity = ActivityFactory.CreateActivity("persShfit");
			PersonalShift persShift =
				PersonalShiftFactory.CreatePersonalShift(persShiftActivity, new DateTimePeriod(2002, 1, 1, 2003, 1, 1));
			target.AddPersonalShift(persShift);
			PersonalShift persShift1 =
				PersonalShiftFactory.CreatePersonalShift(persShiftActivity, new DateTimePeriod(2002, 1, 1, 2003, 1, 1));
			target.AddPersonalShift(persShift1);

			Assert.AreEqual(2, target.PersonalShiftCollection.Count);
			target.RemovePersonalShift(persShift);

			Assert.AreEqual(1, target.PersonalShiftCollection.Count);
			Assert.AreEqual(persShift1, target.PersonalShiftCollection[0]);
		}

		[Test]
		public void VerifyICloneableEntity()
		{
			DateTime zOrder = new DateTime(2000,1,1);
			target.SetId(Guid.NewGuid());
			target.ZOrder = zOrder;

			IActivity persShiftActivity = ActivityFactory.CreateActivity("persShfit");
			IPersonalShift persShift =
				PersonalShiftFactory.CreatePersonalShift(persShiftActivity, new DateTimePeriod(2002, 1, 1, 2003, 1, 1));
			persShift.SetId(Guid.NewGuid());
			IOvertimeShift overtime = new OvertimeShift();
			overtime.SetId(Guid.NewGuid());

			target.AddOvertimeShift(overtime);
			target.AddPersonalShift(persShift);

			IPersonAssignment pAss = target.EntityClone();
			Assert.AreEqual(target.Id, pAss.Id);
			Assert.AreEqual(target.PersonalShiftCollection[0].Id, pAss.PersonalShiftCollection[0].Id);
			Assert.AreEqual(target.Person.Id, pAss.Person.Id);
			Assert.AreEqual(zOrder, target.ZOrder);
			Assert.AreEqual(target.OvertimeShiftCollection[0].Id, pAss.OvertimeShiftCollection[0].Id);

			pAss = target.NoneEntityClone();
			Assert.AreEqual(target.Person.Id, pAss.Person.Id);
			Assert.IsNull(pAss.Id);
			Assert.IsNull(pAss.PersonalShiftCollection[0].Id);
			Assert.IsNull(pAss.OvertimeShiftCollection[0].Id);
			Assert.AreEqual(zOrder, target.ZOrder);

			pAss = (IPersonAssignment)target.CreateTransient();
			Assert.AreEqual(target.Person.Id, pAss.Person.Id);
			Assert.IsNull(pAss.Id);
			Assert.IsNull(pAss.PersonalShiftCollection[0].Id);
			Assert.IsNull(pAss.OvertimeShiftCollection[0].Id);
			Assert.AreEqual(zOrder, target.ZOrder);
		}

		[Test]
		public void PersonalShiftMustBeInsideMainShiftOrOvertimeToBeIncludedInProjection()
		{
			var defSet = new MultiplicatorDefinitionSet("d", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(testPerson, defSet);
			var act = new Activity("sdf");
			var ot = new OvertimeShift();
			target.AddOvertimeShift(ot);
			ot.LayerCollection.Add(new OvertimeShiftActivityLayer(act,
									new DateTimePeriod(2000, 1, 1, 2000, 1, 2),
									defSet));

			var mShift = new MainShift(new ShiftCategory("d"));
			mShift.LayerCollection.Add(new MainShiftActivityLayer(act,
											new DateTimePeriod(2000, 1, 5, 2000, 1, 6)));
			target.SetMainShift(mShift);

			Assert.AreEqual(2, target.ProjectionService().CreateProjection().Count());

			var pShift = new PersonalShift();
			target.AddPersonalShift(pShift);
			pShift.LayerCollection.Add(new PersonalShiftActivityLayer(act,
											new DateTimePeriod(2000, 1, 3, 2000, 1, 4)));
			Assert.AreEqual(2, target.ProjectionService().CreateProjection().Count());
		}

		private class testAssignment : PersonAssignment
		{
		}
	}
}
