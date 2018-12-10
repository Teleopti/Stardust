using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Restriction
{
    [TestFixture]
    public class PreferenceRestrictionTest
    {
        private IPreferenceDay _target;
        private IPreferenceRestriction _preferenceRestrictionNew;
        private IPerson _person;
        private DateOnly _dateOnly;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson();
            _dateOnly = new DateOnly(2009,2,2);
            _preferenceRestrictionNew = new PreferenceRestriction();

            _target = new PreferenceDay(_person,_dateOnly,_preferenceRestrictionNew);
        }

        [Test]
        public void CanSetAndReadProperties()
        {
            Assert.AreEqual(_person,_target.Person);
            Assert.AreEqual(_dateOnly,_target.RestrictionDate);
            Assert.IsNull(_target.TemplateName);

            IShiftCategory shiftCategory = new ShiftCategory("dl");
            _target.Restriction.ShiftCategory = shiftCategory;
            Assert.AreEqual(shiftCategory, _target.Restriction.ShiftCategory);
            var dayOffTemplate = new DayOffTemplate(new Description("c"));
            _target.Restriction.DayOffTemplate = dayOffTemplate;
            Assert.AreEqual(dayOffTemplate,_target.Restriction.DayOffTemplate);

            _target.TemplateName = "My template";

            IAbsence absence = new Absence();
            _target.Restriction.Absence = absence;
            Assert.AreEqual(absence, _target.Restriction.Absence);
        }

        [Test]
        public void CanAddActivityRestriction()
        {
            Assert.AreEqual(0, _target.Restriction.ActivityRestrictionCollection.Count);
            IActivity activity = new Activity("asf");
            ActivityRestriction activityRestriction = new ActivityRestriction(activity);
            _target.Restriction.AddActivityRestriction(activityRestriction);

            Assert.AreEqual(1, _target.Restriction.ActivityRestrictionCollection.Count);
            
        }

        [Test]
        public void CanChangeActivityRestriction()
        {
            CanAddActivityRestriction();
            Assert.AreEqual(1, _target.Restriction.ActivityRestrictionCollection.Count);
            IActivity activity = new Activity("LVL");
           // ActivityRestriction activityRestriction = new ActivityRestriction(activity);
            _target.Restriction.ActivityRestrictionCollection[0].Activity = activity;
            Assert.AreEqual(activity,_target.Restriction.ActivityRestrictionCollection[0].Activity);
            //_target.Restriction.ActivityRestrictionCollection[0] = activityRestriction;
            //Assert.AreEqual(_target.Restriction.ActivityRestrictionCollection[0], activityRestriction);
        }

        [Test]
        public void CanRemoveActivityRestriction()
        {
            CanAddActivityRestriction();
            IActivityRestriction restriction = _target.Restriction.ActivityRestrictionCollection[0];
            _target.Restriction.RemoveActivityRestriction(restriction);
            Assert.AreEqual(0, _target.Restriction.ActivityRestrictionCollection.Count);
        }

        [Test]
        public void VerifyCreateTransient()
        {
            _target.SetId(Guid.NewGuid());
            _target.Restriction.SetId(Guid.NewGuid());
            var transient = (IPreferenceDay)_target.CreateTransient();

            Assert.IsNull(transient.Id);
            Assert.IsNull(transient.Restriction.Id);
        }
        [Test]
        public void VerifyMustHave()
        {
            Assert.IsFalse(_target.Restriction.MustHave);
            _target.Restriction.MustHave = true;
            Assert.IsTrue(_target.Restriction.MustHave);
        }

        [Test]
        public void VerifyNoneEntityClone()
        {
            IAbsence absence = new Absence();
            _preferenceRestrictionNew.SetId(Guid.NewGuid());
            _preferenceRestrictionNew.Absence = absence; 
            IActivityRestriction activityRestriction = new ActivityRestriction(ActivityFactory.CreateActivity("hej"));
            activityRestriction.SetId(Guid.NewGuid());
            _preferenceRestrictionNew.AddActivityRestriction(activityRestriction);
            IPreferenceRestriction nonEntityClone = _preferenceRestrictionNew.NoneEntityClone();
            Assert.IsNull(nonEntityClone.Id);
            Assert.IsNull(nonEntityClone.ActivityRestrictionCollection[0].Id);
            Assert.AreEqual(_preferenceRestrictionNew.ShiftCategory, nonEntityClone.ShiftCategory);
            Assert.AreEqual(_preferenceRestrictionNew.DayOffTemplate, nonEntityClone.DayOffTemplate);
            Assert.AreEqual(_preferenceRestrictionNew.Absence, nonEntityClone.Absence);


        }

        [Test]
        public void VerifyEntityClone()
        {
            IAbsence absence = new Absence();
            _preferenceRestrictionNew.SetId(Guid.NewGuid());
            _preferenceRestrictionNew.Absence = absence;
            IPreferenceRestriction entityClone = _preferenceRestrictionNew.EntityClone();
            Assert.IsNotNull(entityClone.Id);
            Assert.AreEqual(_preferenceRestrictionNew.Id, entityClone.Id);
            Assert.AreEqual(_preferenceRestrictionNew.ShiftCategory, entityClone.ShiftCategory);
            Assert.AreEqual(_preferenceRestrictionNew.DayOffTemplate, entityClone.DayOffTemplate);
            Assert.AreEqual(_preferenceRestrictionNew.Absence, entityClone.Absence);
        }

        [Test]
        public void VerifyStrangeDates()
        {
            PreferenceRestriction restriction = new PreferenceRestriction();
            restriction.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(20, 0, 0), null);
            restriction.EndTimeLimitation = new EndTimeLimitation(new TimeSpan(6, 0, 0), new TimeSpan(7, 0, 0));
            IPreferenceDay day = new PreferenceDay(_person, _dateOnly, restriction);
            Assert.AreEqual(
                TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_dateOnly.Date.AddHours(20),
                                                                     _dateOnly.Date.AddHours(36),
                                                                     _person.PermissionInformation.DefaultTimeZone()),
                day.Period);
        }

        [Test]
        public void VerifyIsRestriction()
        {
            _preferenceRestrictionNew = new PreferenceRestriction();
            Assert.IsFalse(_preferenceRestrictionNew.IsRestriction());

            _preferenceRestrictionNew = new PreferenceRestriction();
            _preferenceRestrictionNew.Absence = new Absence();
            Assert.IsTrue(_preferenceRestrictionNew.IsRestriction());

            _preferenceRestrictionNew = new PreferenceRestriction();
            _preferenceRestrictionNew.ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej");
            Assert.IsTrue(_preferenceRestrictionNew.IsRestriction());

            _preferenceRestrictionNew = new PreferenceRestriction();
            _preferenceRestrictionNew.DayOffTemplate = new DayOffTemplate(new Description("hej"));
            Assert.IsTrue(_preferenceRestrictionNew.IsRestriction());

            _preferenceRestrictionNew = new PreferenceRestriction();
            _preferenceRestrictionNew.AddActivityRestriction(new ActivityRestriction(ActivityFactory.CreateActivity("hej")));
            Assert.IsTrue(_preferenceRestrictionNew.IsRestriction());

        }
    }
}
