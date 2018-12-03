using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
    [TestFixture]
    public class RestrictionExtractorTest
    {
        private RestrictionExtractor _target;
        private MockRepository _mocks;
        private IPerson _person;
        private IScheduleDay _part;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new RestrictionExtractor(new RestrictionCombiner(), new RestrictionRetrievalOperation());
            _person = PersonFactory.CreatePerson();
			_part = _mocks.StrictMock<IScheduleDay>();
        }

        [Test]
        public void VerifyCanGetRestrictionLists()
        {
            var result = extract(new List<IRestrictionBase>(), new IScheduleData[0]);
            Assert.AreEqual(0, result.AvailabilityList.Count());
            Assert.AreEqual(0, result.StudentAvailabilityList.Count());
            Assert.AreEqual(0, result.PreferenceList.Count());
            Assert.AreEqual(0, result.RotationList.Count());
        }

        [Test]
        public void VerifyCombinedRestriction()
        {
            var result = extract(new List<IRestrictionBase>(), new IScheduleData[0]);
            var combined = result.CombinedRestriction(new SchedulingOptions { UseRotations = true, UsePreferences = true, UseAvailability = true, UseStudentAvailability = true, UsePreferencesMustHaveOnly = false });
            Assert.IsNotNull(combined);
        }

        [Test]
        public void VerifyCombinedRestrictionPreference()
        {
            var sr = new PreferenceRestriction {ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("Hej")};
	        var pd = new PreferenceDay(_person, new DateOnly(2009, 1, 1), sr);
            var result = extract(new List<IRestrictionBase>{sr}, new []{pd});

	        var combined =
		        result.CombinedRestriction(new SchedulingOptions
		        {
			        UseRotations = true,
			        UsePreferences = true,
			        UseAvailability = true,
			        UseStudentAvailability = true,
			        UsePreferencesMustHaveOnly = false
		        });
            Assert.IsNotNull(combined);
            Assert.AreEqual(1, result.PreferenceList.Count());
        }

        [Test]
        public void VerifyCombinedRestrictionWithNotMustHavePreference()
        {
            var sr = new PreferenceRestriction {ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("Hej")};
	        var pd = new PreferenceDay(_person, new DateOnly(2009, 1, 1), sr);
            var result = extract(new List<IRestrictionBase> { sr }, new []{pd});

	        var combined =
		        result.CombinedRestriction(new SchedulingOptions
		        {
			        UseRotations = true,
			        UsePreferences = true,
			        UseAvailability = true,
			        UseStudentAvailability = true,
			        UsePreferencesMustHaveOnly = true
		        });
            Assert.IsNotNull(combined);
            Assert.IsFalse(combined.IsPreferenceDay);
        }

        [Test]
        public void VerifyCombinedRestrictionWithMustHavePreference()
        {
            var sr = new PreferenceRestriction
            {
	            MustHave = true,
	            ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("Hej")
            };
	        var pd = new PreferenceDay(_person, new DateOnly(2009, 1, 1), sr);
            var result = extract(new List<IRestrictionBase> { sr }, new []{pd});

	        var combined =
		        result.CombinedRestriction(new SchedulingOptions
		        {
			        UseRotations = true,
			        UsePreferences = true,
			        UseAvailability = true,
			        UseStudentAvailability = true,
			        UsePreferencesMustHaveOnly = true
		        });
            Assert.IsNotNull(combined);
            Assert.IsTrue(combined.IsPreferenceDay);
        }

        [Test]
        public void VerifyCombinedRestrictionStudentAvailability()
        {
            var sr = new StudentAvailabilityRestriction();
            var pr = new StudentAvailabilityDay(_person, new DateOnly(2009, 1, 1), new List<IStudentAvailabilityRestriction>{sr})
            {
	            NotAvailable = false
            };
            var result = extract(new List<IRestrictionBase>(), new [] { pr });
            var combined = result.CombinedRestriction(new SchedulingOptions { UseRotations = true, UsePreferences = true, UseAvailability = true, UseStudentAvailability = true, UsePreferencesMustHaveOnly = false });
            Assert.IsNotNull(combined);
            Assert.IsFalse(combined.NotAvailable);
			Assert.IsTrue(combined.IsStudentAvailabilityDay);
        }

		[Test]
		public void VerifyCombinedRestrictionStudentAvailabilityNotDefined()
		{
			var result = extract(new List<IRestrictionBase>(), new IScheduleData[0]);
			var combined = result.CombinedRestriction(new SchedulingOptions { UseRotations = true, UsePreferences = true, UseAvailability = true, UseStudentAvailability = true, UsePreferencesMustHaveOnly = false });
			Assert.IsNotNull(combined);
			Assert.IsTrue(combined.NotAvailable);
		}

        [Test]
        public void VerifyCombinedRestrictionAvailability()
        {
            IRestrictionBase sr = new AvailabilityRestriction();
            sr.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(8,0,0),null );
            var result = extract(new List<IRestrictionBase> { sr }, new IScheduleData[0]);
            var combined = result.CombinedRestriction(new SchedulingOptions { UseRotations = true, UsePreferences = true, UseAvailability = true, UseStudentAvailability = true, UsePreferencesMustHaveOnly = false });
            Assert.IsNotNull(combined);
        }
        [Test]
        public void VerifyCombinedRestrictionStudentAvailabilityNotAvailable()
        {
            IAvailabilityRestriction availabilityRestriction = new AvailabilityRestriction();
            availabilityRestriction.NotAvailable = true;
            var result = extract(new List<IRestrictionBase> { availabilityRestriction }, new IScheduleData[0]);
            var combined = result.CombinedRestriction(new SchedulingOptions { UseRotations = true, UsePreferences = true, UseAvailability = false, UseStudentAvailability = true, UsePreferencesMustHaveOnly = false });
            Assert.IsNotNull(combined);
            Assert.IsTrue(combined.NotAvailable);
        }

        [Test]
        public void VerifyCombinedRestrictionAvailabilityNotAvailable()
        {
            IAvailabilityRestriction availabilityRestriction = new AvailabilityRestriction();
            availabilityRestriction.NotAvailable = true;
            var result = extract(new List<IRestrictionBase> { availabilityRestriction }, new IScheduleData[0]);
            var combined = result.CombinedRestriction(new SchedulingOptions { UseRotations = true, UsePreferences = true, UseAvailability = true, UseStudentAvailability = false, UsePreferencesMustHaveOnly = false });
            Assert.IsNotNull(combined);
            Assert.IsTrue(combined.NotAvailable);
        }

        [Test]
        public void VerifyCombinedRestrictionRotation()
        {
            IRestrictionBase sr = new RotationRestriction();
            sr.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(8, 0, 0), null);
            var result = extract(new List<IRestrictionBase> { sr }, new IScheduleData[0]);
            var combined = result.CombinedRestriction(new SchedulingOptions{UseRotations = true, UsePreferences = true, UseAvailability = true,UseStudentAvailability = true, UsePreferencesMustHaveOnly = false});
            Assert.IsNotNull(combined);
			Assert.IsTrue(combined.IsRotationDay);
        }

		  [Test]
		  public void VerifyEqualsReturnFalseIfOneElementIsNull()
		  {
			  var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			  var dateTime = new DateTime(2006, 12, 23, 7, 30, 0);
			  dateTime = TimeZoneHelper.ConvertToUtc(dateTime, timeZone);
			  var dateTimePeriod = new DateTimePeriod(dateTime, TimeZoneHelper.ConvertToUtc(dateTime.AddHours(12), timeZone));
			  TimeZoneInfo timeZoneInfo = _person.PermissionInformation.DefaultTimeZone();
			  IStudentAvailabilityRestriction restriction = new StudentAvailabilityRestriction();
			  DateTime start = dateTimePeriod.StartDateTimeLocal(timeZoneInfo);
			  DateTime end = dateTimePeriod.EndDateTimeLocal(timeZoneInfo);
			  restriction.StartTimeLimitation = new StartTimeLimitation(start.TimeOfDay, null);
			  restriction.EndTimeLimitation = new EndTimeLimitation(null, end.AddMinutes(-1).TimeOfDay);
			  var pr = new StudentAvailabilityDay(_person, new DateOnly(2009, 1, 1), new List<IStudentAvailabilityRestriction> { restriction })
			  {
				  NotAvailable = false
			  };
			  Assert.IsFalse(pr.Equals(null));
		  }

        private IExtractedRestrictionResult extract(IEnumerable<IRestrictionBase> restrictions, IScheduleData[] studentRestriction)
        {
            using(_mocks.Record())
            {
                Expect.Call(_part.PersonRestrictionCollection()).Return(studentRestriction).Repeat.Any();
                Expect.Call(_part.RestrictionCollection()).Return(restrictions).Repeat.Any();
            }

            using (_mocks.Playback())
            {
				return _target.Extract(_part);
            }
        }
    }
}