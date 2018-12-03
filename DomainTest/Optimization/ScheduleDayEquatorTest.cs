using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization
{
    /// <summary>
    /// Test for ScheduleDayEquator
    /// </summary>
    /// <remarks>
    /// See also ScheduleDayEquatorTest.doc document about testcases
    /// </remarks>
    [TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class ScheduleDayEquatorTest
    {
        private ScheduleDayEquator _target;

        [SetUp]
        public void Setup()
        {
            _target = new ScheduleDayEquator(new EditableShiftMapper());
        }

	    [TearDown]
	    public void Teardown()
	    {
		    Thread.CurrentPrincipal = null;
	    }
	
		#region Day Off and MainShift testcases when days has different significant parts

		[Test]
        public void ShouldBothDayOffAndMainShiftEqualIfOriginalHaveDayOffCurrentHasMainShift()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();
            SetIdOnShiftCategories(original, current, Guid.NewGuid());
			setSameIdOnActivites(original, current);
            Assert.IsTrue(_target.MainShiftEquals(original, current));

            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOff"));
            original.CreateAndAddDayOff(dayOffTemplate);

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsTrue(_target.MainShiftEquals(original, current));

            original.DeleteMainShift();
						Assert.AreEqual(0, original.PersonAssignment().MainActivities().Count());

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsTrue(_target.MainShiftEquals(original, current));
        }

        [Test]
        public void ShouldDayOffBeDifferentMainShiftEqualIfOriginalHaveMainShiftCurrentHasDayOff()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();
            SetIdOnShiftCategories(original, current, Guid.NewGuid());
			setSameIdOnActivites(original, current);
            Assert.IsTrue(_target.MainShiftEquals(original, current));

            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOff"));
            current.CreateAndAddDayOff(dayOffTemplate);

            Assert.IsFalse(_target.DayOffEquals(original, current));
            Assert.IsTrue(_target.MainShiftEquals(original, current));

            current.DeleteMainShift();
						Assert.AreEqual(0, current.PersonAssignment().MainActivities().Count());

            Assert.IsFalse(_target.DayOffEquals(original, current));
            Assert.IsTrue(_target.MainShiftEquals(original, current));
        }

        #endregion

        #region MainShift testcases when both days has MainShift as significant part

        [Test]
        public void ShouldMainShiftBeDifferentIfOriginalReplacesMainShift()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();

            SetIdOnShiftCategories(original, current, Guid.NewGuid());
	        var newMainShift = original.GetEditorShift();
            original.AddMainShift(newMainShift);

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsFalse(_target.MainShiftEquals(original, current));
        }

        [Test]
        public void ShouldMainShiftBeDifferentIfCurrentReplacesMainShift()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();

            SetIdOnShiftCategories(original, current, Guid.NewGuid());
	        var newMainShift = current.GetEditorShift();
            current.AddMainShift(newMainShift);

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsFalse(_target.MainShiftEquals(original, current));
        }

        [Test]
        public void ShouldMainShiftBeEqualIfHasCopyOfTheSameMainShift()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();
			setSameIdOnActivites(original, current);
            SetIdOnShiftCategories(original, current, Guid.NewGuid());

            Assert.IsTrue(_target.MainShiftEquals(original, current));
        }

        [Test]
        public void ShouldMainShiftBeDifferentIfDifferentNumberOfActivities()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShiftWithDifferentActivities();
            SetIdOnShiftCategories(original, current, Guid.NewGuid());

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsFalse(_target.MainShiftEquals(original, current));
        }

        [Test]
        public void ShouldMainShiftBeDifferentIfSameNumberOfLayerDifferentOrder()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();
            IPersonAssignment personAssingment = current.PersonAssignment();
	        var category = personAssingment.ShiftCategory;
            Assert.AreEqual(2, personAssingment.MainActivities().Count());

            // change order
            var activity1 = current.GetEditorShift().LayerCollection[0];
			var activity2 = current.GetEditorShift().LayerCollection[1];

	        var mainShift = new EditableShift(category);
			mainShift.LayerCollection.Add(activity2);
			mainShift.LayerCollection.Add(activity1);
			new EditableShiftMapper().SetMainShiftLayers(personAssingment, mainShift);

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsFalse(_target.MainShiftEquals(original, current));
        }

        [Test]
        public void ShouldMainShiftBeDifferentIfOneActivityHasChangedPeriodLength()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay scheduleDay1 = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay scheduleDay2 = schedulePartFactory.CreatePartWithMainShift();
            SetIdOnShiftCategories(scheduleDay1, scheduleDay2, Guid.NewGuid());

            IActivity activity = ActivityFactory.CreateActivity("Hej");
            DateTimePeriod layerPeriod =
                scheduleDay2.GetEditorShift().LayerCollection[1].Period.ChangeEndTime(TimeSpan.FromHours(1));
            IShiftCategory category = scheduleDay1.PersonAssignment().ShiftCategory;
			new EditableShiftMapper().SetMainShiftLayers(scheduleDay2.PersonAssignment(), EditableShiftFactory.CreateEditorShift(activity, layerPeriod, category));

            Assert.IsTrue(_target.DayOffEquals(scheduleDay1, scheduleDay2));
            Assert.IsFalse(_target.MainShiftEquals(scheduleDay1, scheduleDay2));
        }

        [Test]
        public void ShouldMainShiftBeDifferentIfSameMainShiftButDifferentShiftCategory()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();
            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            original.PersonAssignment().ShiftCategory.SetId(Guid.NewGuid());
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();
            current.PersonAssignment().ShiftCategory.SetId(Guid.NewGuid());

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsFalse(_target.MainShiftEquals(original, current));

        }

        [Test]
        public void ShouldBothDayOffAndMainShiftBeEqualIfOriginalHaveDayOffCurrentHasDayOff()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current2 = schedulePartFactory.CreatePartWithMainShift();
            SetIdOnShiftCategories(original, current, Guid.NewGuid());
			setSameIdOnActivites(original, current);
            Assert.IsTrue(_target.MainShiftEquals(original, current));

            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOff"));
            original.CreateAndAddDayOff(dayOffTemplate);
            current.CreateAndAddDayOff(dayOffTemplate);
            current2.CreateAndAddDayOff(dayOffTemplate);

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsTrue(_target.MainShiftEquals(original, current));
            Assert.IsTrue(_target.DayOffEquals(original, current2));
            Assert.IsTrue(_target.MainShiftEquals(original, current2));

            original.DeleteMainShift();
						Assert.AreEqual(0, original.PersonAssignment().MainActivities().Count());

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsTrue(_target.MainShiftEquals(original, current));

            current.DeleteMainShift();
						Assert.AreEqual(0, current.PersonAssignment().MainActivities().Count());

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsTrue(_target.MainShiftEquals(original, current));
        }

        [Test]
        public void ShouldBothDayOffAndMainShiftBeEqualIfOriginalHasDayOffCurrentHasDayOffWithMainShiftDeleted()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();
            SetIdOnShiftCategories(original, current, Guid.NewGuid());
			setSameIdOnActivites(original, current);
            Assert.IsTrue(_target.MainShiftEquals(original, current));

            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOff"));
            original.CreateAndAddDayOff(dayOffTemplate);
            current.CreateAndAddDayOff(dayOffTemplate);

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsTrue(_target.MainShiftEquals(original, current));

            original.DeleteMainShift();
            current.DeleteMainShift();
            Assert.AreEqual(0, original.PersonAssignment().MainActivities().Count());
						Assert.AreEqual(0, current.PersonAssignment().MainActivities().Count());

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsTrue(_target.MainShiftEquals(original, current));
        }

        [Test]
        public void ShouldDayOffBeEqualMainShiftDifferentIfOriginalHasNoneCurrentHasMainShift()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();
            SetIdOnShiftCategories(original, current, Guid.NewGuid());
			setSameIdOnActivites(original, current);

            Assert.IsTrue(_target.MainShiftEquals(original, current));
            original.DeleteMainShift();

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsFalse(_target.MainShiftEquals(original, current));
        }

        #endregion

		[Test]
		public void ShouldCheckLayerActivity()
		{
			var period = new DateTimePeriod(2013, 02, 19, 2013, 02, 19);
			var shiftCategory = new ShiftCategory("C");
			var currentShift = EditableShiftFactory.CreateEditorShift(new Activity("A"), period, shiftCategory);
			var otherShift = EditableShiftFactory.CreateEditorShift(new Activity("B"), period, shiftCategory);

			bool result = _target.MainShiftEquals(otherShift, currentShift);
			Assert.IsFalse(result);
		}
		
		[Test]
		public void ShouldMainShiftBeEqualIfHasDifferentDate()
		{
			var period1 = new DateTimePeriod(2013, 02, 19, 2013, 02, 19);
			var period2 = new DateTimePeriod(2013, 02, 20, 2013, 02, 20);
			var shiftCategory = new ShiftCategory("C");
			var activity = new Activity("A");
			activity.SetId(Guid.NewGuid());
			var currentShift = EditableShiftFactory.CreateEditorShift(activity, period1, shiftCategory);
			var otherShift = EditableShiftFactory.CreateEditorShift(activity, period2, shiftCategory);
			
			Assert.IsFalse(_target.MainShiftEquals(otherShift, currentShift));
			Assert.IsTrue(_target.MainShiftBasicEquals(otherShift, currentShift));
		}

		[Test]
		public void ShouldHandleDaylightSavingTime()
		{
			var period1 = new DateTimePeriod(new DateTime(2013, 3, 30, 8, 0, 0, DateTimeKind.Utc),
			                                 new DateTime(2013, 3, 30, 17, 0, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2013, 3, 31, 7, 0, 0, DateTimeKind.Utc),
			                                 new DateTime(2013, 3, 31, 16, 0, 0, DateTimeKind.Utc));
			var shiftCategory = new ShiftCategory("C");
			var activity = new Activity("A");
			activity.SetId(Guid.NewGuid());
			var currentShift = EditableShiftFactory.CreateEditorShift(activity, period1, shiftCategory);
			var otherShift = EditableShiftFactory.CreateEditorShift(activity, period2, shiftCategory);
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, person);

			Assert.IsTrue(_target.MainShiftBasicEquals(otherShift, currentShift));
		}

		#region Bugfix 19056

		/// <summary>
		/// Bugfix for 19056> Error "ScheduleDayEquator.MainShiftEquals" during optimization on days with only a personal shift.
		/// </summary>
		[Test]
		public void ShouldReturnFalseIfCurrentPersonAssignmentHasNoMainShift()
		{
			SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

			IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
			IScheduleDay current = schedulePartFactory.CreatePartWithoutMainShift();
			
			schedulePartFactory.AddPersonalLayer(current);

			Assert.IsFalse(_target.MainShiftEquals(original, current));
		}

		[Test]
		public void ShouldReturnFalseIfOriginalPersonAssignmentHasNoMainShift()
		{
			SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

			IScheduleDay original = schedulePartFactory.CreatePartWithoutMainShift();
			IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();

			schedulePartFactory.AddPersonalLayer(original);

			Assert.IsFalse(_target.MainShiftEquals(original, current));
		}

		[Test]
		public void ShouldReturnFalseIfNorOriginalNorCurrentPersonAssignmentHasNoMainShift()
		{
			SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

			IScheduleDay original = schedulePartFactory.CreatePartWithoutMainShift();
			IScheduleDay current = schedulePartFactory.CreatePartWithoutMainShift();

			schedulePartFactory.AddPersonalLayer(original);
			schedulePartFactory.AddPersonalLayer(current);

			Assert.IsFalse(_target.MainShiftEquals(original, current));
		}

		#endregion

		#region "Bug 20756"
		[Test]
		public void ShouldReturnFalseIfCurrentIsNoneAndOriginalIsMainShift()
		{
			var schedulePartFactory = CreateSchedulePartFactory();

			var original = schedulePartFactory.CreatePartWithMainShift();
			var current = schedulePartFactory.CreatePartWithoutMainShift();
			
			Assert.IsFalse(_target.MainShiftEquals(original, current));
		}
		
		#endregion
		
		private static void SetIdOnShiftCategories(IScheduleDay scheduleDay1, IScheduleDay scheduleDay2, Guid shiftCategoryId)
        {
            scheduleDay1.PersonAssignment().ShiftCategory.SetId(shiftCategoryId);
            scheduleDay2.PersonAssignment().ShiftCategory.SetId(shiftCategoryId);
        }

        private static SchedulePartFactoryForDomain CreateSchedulePartFactory()
        {
            IPerson person = PersonFactory.CreatePerson();
            IScenario scenario = new Scenario("TestScenario");
            DateTime startDate = new DateTime(2010, 01, 10, 0, 0, 0, DateTimeKind.Utc);
            DateTime endDate = startDate.AddDays(10);
            DateTimePeriod dateTimePeriod = new DateTimePeriod(startDate, endDate);
            ISkill skill = SkillFactory.CreateSkill("TestSkill");

            return new SchedulePartFactoryForDomain(person, scenario,
                                                    dateTimePeriod, skill);
        }

		private static void setSameIdOnActivites(IScheduleDay scheduleDay1, IScheduleDay scheduleDay2)
		{
			var id1 = Guid.NewGuid();
			var id2 = Guid.NewGuid();
			scheduleDay1.PersonAssignment().MainActivities().First().Payload.SetId(id1);
			scheduleDay2.PersonAssignment().MainActivities().First().Payload.SetId(id1);
			scheduleDay1.PersonAssignment().MainActivities().Last().Payload.SetId(id2);
			scheduleDay2.PersonAssignment().MainActivities().Last().Payload.SetId(id2);
		}
    }
}
