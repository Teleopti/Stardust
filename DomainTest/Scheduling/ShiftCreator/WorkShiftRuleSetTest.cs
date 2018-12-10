using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
    [TestFixture]
    public class WorkShiftRuleSetTest
    {
        private WorkShiftRuleSet _target;
        private MockRepository _mocks;
        private IWorkShiftTemplateGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _generator = new WorkShiftTemplateGenerator(ActivityFactory.CreateActivity("sample"),
                                               new TimePeriodWithSegment(10, 0, 12, 0, 60),
                                               new TimePeriodWithSegment(11, 0, 13, 0, 60),
                                               ShiftCategoryFactory.CreateShiftCategory("sample"));

            _target = new WorkShiftRuleSet(_generator);
        }


        [Test]
        public void VerifyProtectedConstructorWorks()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType()));
        }

        [Test]
        public void VerifyCanReadProperties()
        {
            Assert.AreSame(_generator, _target.TemplateGenerator);
            Assert.IsNotNull(_target.ExtenderCollection);
            Assert.IsNotNull(_target.LimiterCollection);
            Assert.AreEqual(new Description(), _target.Description);
            Assert.AreEqual(0, _target.RuleSetBagCollection.Count);
			Assert.AreEqual(false, _target.OnlyForRestrictions);
        }

        [Test]
        public void VerifyCannotSetNullGeneratorInConstructor()
        {
			Assert.Throws<ArgumentNullException>(() => _target = new WorkShiftRuleSet(null));
        }

        [Test]
        public void CanSetProperties()
        {
            var desc = new Description("asldökf");
            _target.Description = desc;
            Assert.AreEqual(desc, _target.Description);

        	_target.OnlyForRestrictions = true;
			Assert.AreEqual(true, _target.OnlyForRestrictions);
        }

        [Test]
        public void CanReadRuleSetBag()
        {
            IRuleSetBag rsBag = new RuleSetBag();
            rsBag.AddRuleSet(_target);
            Assert.IsTrue(_target.RuleSetBagCollection.Contains(rsBag));
            rsBag.RemoveRuleSet(_target);
            Assert.IsFalse(_target.RuleSetBagCollection.Contains(rsBag));
        }

        [Test]
        public void VerifyCanDeleteExtender()
        {
            var ext = _mocks.StrictMock<IWorkShiftExtender>();
            using (_mocks.Record())
            {
                Expect.Call(ext.Parent).Return(null);
                ext.SetParent(_target);
            }
            using (_mocks.Playback())
            {
                _target.AddExtender(ext);
                Assert.AreEqual(1, _target.ExtenderCollection.Count);
                Assert.AreSame(ext, _target.ExtenderCollection[0]);
                _target.DeleteExtender(ext);
                Assert.AreEqual(0, _target.ExtenderCollection.Count);
            }
        }

        [Test]
        public void VerifyParent()
        {
            IWorkShiftExtender ext =
                new AutoPositionedActivityExtender(ActivityFactory.CreateActivity("sdf"), new TimePeriodWithSegment(), new TimeSpan(2));
            IWorkShiftLimiter limit = new ContractTimeLimiter(new TimePeriod(), TimeSpan.FromMinutes(15));
            _target.AddExtender(ext);
            _target.AddLimiter(limit);
            Assert.AreSame(_target, ext.Parent);
            Assert.AreSame(_target, limit.Parent);
        }

        [Test]
        public void VerifyCannotAddNullAsExtender()
        {
			Assert.Throws<ArgumentNullException>(() => _target.AddExtender(null));
        }

        [Test]
        public void VerifyCanDeleteLimiter()
        {
            var limiter = _mocks.StrictMock<IWorkShiftLimiter>();
            using (_mocks.Record())
            {
                Expect.Call(limiter.Parent).Return(null);
                limiter.SetParent(_target);
            }
            using (_mocks.Playback())
            {
                _target.AddLimiter(limiter);
                Assert.AreEqual(1, _target.LimiterCollection.Count);
                Assert.AreSame(limiter, _target.LimiterCollection[0]);
                _target.DeleteLimiter(limiter);
                Assert.AreEqual(0, _target.LimiterCollection.Count);
            }

        }

        [Test]
        public void VerifyCannotAddNullAsLimiter()
        {
			Assert.Throws<ArgumentNullException>(() => _target.AddLimiter(null));
        }


       

        [Test]
        public void VerifyICloneableEntity()
        {
            ((IEntity)_target).SetId(Guid.NewGuid());
            _target.Description = new Description("Original Description");

            // Entity clone testing.

            IWorkShiftRuleSet targetCloned = _target.EntityClone();
            Assert.IsNotNull(targetCloned);
            Assert.AreEqual(_target.Id, targetCloned.Id);
            Assert.AreEqual(_target.Description, targetCloned.Description);

            targetCloned.Description = new Description("Clone of OriginalDescription");
            Assert.AreNotEqual(_target.Description, targetCloned.Description);

            // None entity clone testing.

            targetCloned = _target.NoneEntityClone();
            Assert.IsNotNull(targetCloned);
            Assert.IsNull(targetCloned.Id);
        }

        [Test]
        public void VerifyCollectionsCloned()
        {
            ((IEntity)_target).SetId(Guid.NewGuid());

            IWorkShiftExtender ext = new ActivityRelativeEndExtender(ActivityFactory.CreateActivity("sample"), new TimePeriodWithSegment(2, 0, 5, 0, 10), new TimePeriodWithSegment(9, 0, 10, 0, 10));
            IWorkShiftLimiter limiter = new ContractTimeLimiter(new TimePeriod(11, 12, 13, 15), TimeSpan.FromMinutes(15));
            var rsBag = new RuleSetBag();

            _target.AddExtender(ext);
            _target.AddLimiter(limiter);
            rsBag.AddRuleSet(_target);

            // Entity clone testing.
            IWorkShiftRuleSet targetCloned = _target.EntityClone();
            doAssertsForCollectionsCloned(_target, targetCloned, true);

            // None entity clone testing.

            targetCloned = _target.NoneEntityClone();
            doAssertsForCollectionsCloned(_target, targetCloned, false);
        }

        [Test]
        public void VerifyDefaultAccessibility()
        {
            Assert.AreEqual(DefaultAccessibility.Included, _target.DefaultAccessibility);

            _target.DefaultAccessibility = DefaultAccessibility.Excluded;
            Assert.AreEqual(DefaultAccessibility.Excluded, _target.DefaultAccessibility);

            _target.DefaultAccessibility = DefaultAccessibility.Included;
            Assert.AreEqual(DefaultAccessibility.Included, _target.DefaultAccessibility);
        }

        [Test]
        public void VerifyCanAddDaysOfWeek()
        {
            _target.AddAccessibilityDayOfWeek(DayOfWeek.Saturday);
            _target.AddAccessibilityDayOfWeek(DayOfWeek.Sunday);

            Assert.AreEqual(2, _target.AccessibilityDaysOfWeek.Count());
            Assert.IsTrue(_target.AccessibilityDaysOfWeek.Contains(DayOfWeek.Saturday));
            Assert.IsTrue(_target.AccessibilityDaysOfWeek.Contains(DayOfWeek.Sunday));
        }

        [Test]
        public void VerifyCannotAddSameDayOfWeekTwice()
        {
            _target.AddAccessibilityDayOfWeek(DayOfWeek.Sunday);
            _target.AddAccessibilityDayOfWeek(DayOfWeek.Sunday);

            Assert.AreEqual(1, _target.AccessibilityDaysOfWeek.Count());
        }

        [Test]
        public void VerifyCanRemoveDaysOfWeek()
        {
            _target.AddAccessibilityDayOfWeek(DayOfWeek.Saturday);
            _target.AddAccessibilityDayOfWeek(DayOfWeek.Sunday);

            Assert.AreEqual(2, _target.AccessibilityDaysOfWeek.Count());

            _target.RemoveAccessibilityDayOfWeek(DayOfWeek.Sunday);

            Assert.IsFalse(_target.AccessibilityDaysOfWeek.Contains(DayOfWeek.Sunday));
        }

        [Test]
        public void VerifyCanRemoveNonExistingDayOfWeek()
        {
            _target.RemoveAccessibilityDayOfWeek(DayOfWeek.Sunday);

            Assert.AreEqual(0, _target.AccessibilityDaysOfWeek.Count());
        }

        [Test]
        public void VerifyCanAddDatesToDefaultAvailability()
        {
            var date1 = new DateOnly(2008, 7, 1);
            var date2 = new DateOnly(2008, 8, 1);

            _target.AddAccessibilityDate(date1);
            _target.AddAccessibilityDate(date2);

            Assert.AreEqual(2, _target.AccessibilityDates.Count());
            Assert.IsTrue(_target.AccessibilityDates.Contains(date1));
            Assert.IsTrue(_target.AccessibilityDates.Contains(date2));
        }

        [Test]
        public void VerifyCannotAddSameDateTwice()
        {
            var date1 = new DateOnly(2008, 7, 1);
            _target.AddAccessibilityDate(date1);
            _target.AddAccessibilityDate(date1);

            Assert.AreEqual(1, _target.AccessibilityDates.Count());
        }

        [Test]
        public void VerifyCanRemoveDate()
        {
            var date1 = new DateOnly(2008, 7, 1);
            var date2 = new DateOnly(2008, 8, 1);

            _target.AddAccessibilityDate(date1);
            _target.AddAccessibilityDate(date2);

            Assert.AreEqual(2, _target.AccessibilityDates.Count());
            Assert.IsTrue(_target.AccessibilityDates.Contains(date1));
            Assert.IsTrue(_target.AccessibilityDates.Contains(date2));

            _target.RemoveAccessibilityDate(date1);

            Assert.IsFalse(_target.AccessibilityDates.Contains(date1));
        }

        [Test]
        public void VerifyCanRemoveNonExistingDate()
        {
            var date1 = new DateOnly(2008, 7, 1);
            _target.RemoveAccessibilityDate(date1);

            Assert.AreEqual(0, _target.AccessibilityDates.Count());
        }

        [Test]
        public void VerifyDateIsValidForWorkShiftRuleSet()
        {

            var date1 = new DateOnly(2008, 7, 1);
            var date11 = new DateOnly(2008, 7, 1);
            var date2 = new DateOnly(2008, 8, 1);

            _target.AddAccessibilityDate(date11);
            _target.DefaultAccessibility = DefaultAccessibility.Included;

            Assert.IsFalse(_target.IsValidDate(date1));
            Assert.IsTrue(_target.IsValidDate(date2));

            _target.DefaultAccessibility = DefaultAccessibility.Excluded;

            Assert.IsTrue(_target.IsValidDate(date1));
            Assert.IsFalse(_target.IsValidDate(date2));
        }


        [Test]
        public void VerifyDayIsValidForWorkShiftRuleSet()
        {
            var date1 = new DateOnly(2008, 7, 1);
            var date2 = new DateOnly(2008, 8, 1);

            _target.AddAccessibilityDayOfWeek(DayOfWeek.Tuesday);
            _target.DefaultAccessibility = DefaultAccessibility.Included;

            Assert.IsFalse(_target.IsValidDate(date1));
            Assert.IsTrue(_target.IsValidDate(date2));

            _target.DefaultAccessibility = DefaultAccessibility.Excluded;

            Assert.IsTrue(_target.IsValidDate(date1));
            Assert.IsFalse(_target.IsValidDate(date2));
        }

        [Test]
        public void VerifyDayAndDateIsValidForWorkShiftRuleSet()
        {
            var date1 = new DateOnly(2008, 7, 1);
            var date2 = new DateOnly(2008, 8, 1);
            var date3 = new DateOnly(2008, 8, 2);
            var date31 = new DateOnly(2008, 8, 2);
            _target.AddAccessibilityDayOfWeek(DayOfWeek.Tuesday);
            _target.AddAccessibilityDate(date31);
            _target.DefaultAccessibility = DefaultAccessibility.Included;

            Assert.IsFalse(_target.IsValidDate(date1));
            Assert.IsFalse(_target.IsValidDate(date3));
            Assert.IsTrue(_target.IsValidDate(date2));

            _target.DefaultAccessibility = DefaultAccessibility.Excluded;

            Assert.IsTrue(_target.IsValidDate(date1));
            Assert.IsTrue(_target.IsValidDate(date3));
            Assert.IsFalse(_target.IsValidDate(date2));
        }

	    [Test]
	    public void InsertExtender_ShouldInsertToExtenderCollection()
	    {
		    var activity = new Activity("name");
		    var periodSegment = new TimePeriodWithSegment();
			IWorkShiftExtender otherExtender = new ActivityRelativeEndExtender(activity, periodSegment, periodSegment);
			IWorkShiftExtender extender = new ActivityAbsoluteStartExtender(activity, periodSegment, periodSegment);
			
			_target.AddExtender(otherExtender);
			_target.InsertExtender(0, extender);
		    _target.ExtenderCollection[0].Should().Be.EqualTo(extender);
	    }

		[Test]
		public void InsertExtender_InsertNull_ThrowArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _target.InsertExtender(0, null));
		}

	    [Test]
	    public void InsertExtender_ParentNotNull_ThrowArgumentException()
	    {
		    IWorkShiftExtender extender = new ActivityAbsoluteStartExtender(ActivityFactory.CreateActivity("Hepp"),
		                                                                    new TimePeriodWithSegment(),
		                                                                    new TimePeriodWithSegment());
			extender.SetParent(WorkShiftRuleSetFactory.Create());
			Assert.Throws<ArgumentException>(() => _target.InsertExtender(0, extender));
	    }

	    private static void doAssertsForCollectionsCloned(IWorkShiftRuleSet target, IWorkShiftRuleSet cloned, bool checkForRuleSetBags)
        {
            Assert.AreEqual(1, cloned.ExtenderCollection.Count);
            Assert.AreNotSame(target.ExtenderCollection[0], cloned.ExtenderCollection[0]);
            Assert.AreEqual(cloned, cloned.ExtenderCollection[0].Parent);

            Assert.AreEqual(1, cloned.LimiterCollection.Count);
            Assert.AreNotSame(target.LimiterCollection[0], cloned.LimiterCollection[0]);
            Assert.AreEqual(cloned, cloned.LimiterCollection[0].Parent);

            if (checkForRuleSetBags)
                Assert.IsEmpty(cloned.RuleSetBagCollection);

            cloned.DeleteLimiter(cloned.LimiterCollection[0]);
            cloned.DeleteExtender(cloned.ExtenderCollection[0]);

            Assert.AreEqual(1, target.ExtenderCollection.Count);
            Assert.AreEqual(0, cloned.ExtenderCollection.Count);
            Assert.AreEqual(1, target.LimiterCollection.Count);
            Assert.AreEqual(0, cloned.LimiterCollection.Count);
        }
    }
}
