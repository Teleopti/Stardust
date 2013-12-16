using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
    [TestFixture]
    public class RestrictionExtractorTest
    {
        private RestrictionExtractor _target;
        private MockRepository _mocks;
        private IPerson _person;
        private IScheduleDictionary _dic;
        private IScheduleRange _range;
        private IScheduleDay _part;
        private DateOnly _date;
        private ISchedulingResultStateHolder _stateHolder;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _dic = _mocks.StrictMock<IScheduleDictionary>();
            _target = new RestrictionExtractor(_stateHolder);
            _person = PersonFactory.CreatePerson();

			_range = _mocks.StrictMock<IScheduleRange>();
			_part = _mocks.StrictMock<IScheduleDay>();
            _date = new DateOnly(2010, 1, 1);
        }

        [Test]
        public void VerifyCanGetRestrictionLists()
        {
            ReadOnlyCollection<IScheduleData> personRestrictionCollection = new ReadOnlyCollection<IScheduleData>(new List<IScheduleData>());
            Extract(new List<IRestrictionBase>(), personRestrictionCollection);
            Assert.AreEqual(0, _target.AvailabilityList.Count());
            Assert.AreEqual(0, _target.StudentAvailabilityList.Count());
            Assert.AreEqual(0, _target.PreferenceList.Count());
            Assert.AreEqual(0, _target.RotationList.Count());
        }

        [Test]
        public void VerifyCombinedRestriction()
        {
            ReadOnlyCollection<IScheduleData> personRestrictionCollection = new ReadOnlyCollection<IScheduleData>(new List<IScheduleData>());
            Extract(new List<IRestrictionBase>(), personRestrictionCollection);
            IEffectiveRestriction combined = _target.CombinedRestriction(new SchedulingOptions { UseRotations = true, UsePreferences = true, UseAvailability = true, UseStudentAvailability = true, UsePreferencesMustHaveOnly = false });
            Assert.IsNotNull(combined);
        }

        [Test]
        public void VerifyCombinedRestrictionPreference()
        {
            PreferenceRestriction sr = new PreferenceRestriction();
            sr.ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("Hej");
            PreferenceDay pd = new PreferenceDay(_person, new DateOnly(2009, 1, 1), sr);
            ReadOnlyCollection<IScheduleData> personRestrictionCollection = new ReadOnlyCollection<IScheduleData>(new List<IScheduleData> { pd });
            Extract(new List<IRestrictionBase>{sr}, personRestrictionCollection);

            IEffectiveRestriction combined = _target.CombinedRestriction(new SchedulingOptions { UseRotations = true, UsePreferences = true, UseAvailability = true, UseStudentAvailability = true, UsePreferencesMustHaveOnly = false });
            Assert.IsNotNull(combined);
            Assert.AreEqual(1, _target.PreferenceList.Count());
        }

        [Test]
        public void VerifyCombinedRestrictionWithNotMustHavePreference()
        {
            PreferenceRestriction sr = new PreferenceRestriction();
            sr.ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("Hej");
            PreferenceDay pd = new PreferenceDay(_person, new DateOnly(2009, 1, 1), sr);
            ReadOnlyCollection<IScheduleData> personRestrictionCollection = new ReadOnlyCollection<IScheduleData>(new List<IScheduleData> { pd });
            Extract(new List<IRestrictionBase> { sr }, personRestrictionCollection);
            
            IEffectiveRestriction combined = _target.CombinedRestriction(new SchedulingOptions { UseRotations = true, UsePreferences = true, UseAvailability = true, UseStudentAvailability = true, UsePreferencesMustHaveOnly = true });
            Assert.IsNotNull(combined);
            Assert.IsFalse(combined.IsPreferenceDay);
        }

        [Test]
        public void VerifyCombinedRestrictionWithMustHavePreference()
        {
            PreferenceRestriction sr = new PreferenceRestriction();
            sr.MustHave = true;
            sr.ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("Hej");
            PreferenceDay pd = new PreferenceDay(_person, new DateOnly(2009, 1, 1), sr);
            ReadOnlyCollection<IScheduleData> personRestrictionCollection = new ReadOnlyCollection<IScheduleData>(new List<IScheduleData> { pd });
            Extract(new List<IRestrictionBase> { sr }, personRestrictionCollection);

            IEffectiveRestriction combined = _target.CombinedRestriction(new SchedulingOptions { UseRotations = true, UsePreferences = true, UseAvailability = true, UseStudentAvailability = true, UsePreferencesMustHaveOnly = true });
            Assert.IsNotNull(combined);
            Assert.IsTrue(combined.IsPreferenceDay);
        }

        [Test]
        public void VerifyCombinedRestrictionStudentAvailability()
        {
            StudentAvailabilityRestriction sr = new StudentAvailabilityRestriction();
            
            StudentAvailabilityDay pr = new StudentAvailabilityDay(_person, new DateOnly(2009, 1, 1), new List<IStudentAvailabilityRestriction>{sr});
            pr.NotAvailable = false;
            ReadOnlyCollection<IScheduleData> personRestrictionCollection = new ReadOnlyCollection<IScheduleData>(new List<IScheduleData> { pr });
            Extract(new List<IRestrictionBase>(), personRestrictionCollection);
            IEffectiveRestriction combined = _target.CombinedRestriction(new SchedulingOptions { UseRotations = true, UsePreferences = true, UseAvailability = true, UseStudentAvailability = true, UsePreferencesMustHaveOnly = false });
            Assert.IsNotNull(combined);
            Assert.IsFalse(combined.NotAvailable);
			Assert.IsTrue(combined.IsStudentAvailabilityDay);
        }

		[Test]
		public void VerifyCombinedRestrictionStudentAvailabilityNotDefined()
		{
			ReadOnlyCollection<IScheduleData> personRestrictionCollection = new ReadOnlyCollection<IScheduleData>(new List<IScheduleData> ());
			Extract(new List<IRestrictionBase>(), personRestrictionCollection);
			IEffectiveRestriction combined = _target.CombinedRestriction(new SchedulingOptions { UseRotations = true, UsePreferences = true, UseAvailability = true, UseStudentAvailability = true, UsePreferencesMustHaveOnly = false });
			Assert.IsNotNull(combined);
			Assert.IsTrue(combined.NotAvailable);
		}

        [Test]
        public void VerifyCombinedRestrictionAvailability()
        {
            IRestrictionBase sr = new AvailabilityRestriction();
            sr.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(8,0,0),null );
            Extract(new List<IRestrictionBase> { sr }, new ReadOnlyCollection<IScheduleData>(new List<IScheduleData>()));
            IEffectiveRestriction combined = _target.CombinedRestriction(new SchedulingOptions { UseRotations = true, UsePreferences = true, UseAvailability = true, UseStudentAvailability = true, UsePreferencesMustHaveOnly = false });
            Assert.IsNotNull(combined);
        }
        [Test]
        public void VerifyCombinedRestrictionStudentAvailabilityNotAvailable()
        {
            IAvailabilityRestriction availabilityRestriction = new AvailabilityRestriction();
            availabilityRestriction.NotAvailable = true;
            Extract(new List<IRestrictionBase> { availabilityRestriction }, new ReadOnlyCollection<IScheduleData>(new List<IScheduleData>()));
            IEffectiveRestriction combined = _target.CombinedRestriction(new SchedulingOptions { UseRotations = true, UsePreferences = true, UseAvailability = false, UseStudentAvailability = true, UsePreferencesMustHaveOnly = false });
            Assert.IsNotNull(combined);
            Assert.IsTrue(combined.NotAvailable);
        }

        [Test]
        public void VerifyCombinedRestrictionAvailabilityNotAvailable()
        {
            IAvailabilityRestriction availabilityRestriction = new AvailabilityRestriction();
            availabilityRestriction.NotAvailable = true;
            Extract(new List<IRestrictionBase> { availabilityRestriction }, new ReadOnlyCollection<IScheduleData>(new List<IScheduleData>()));
            IEffectiveRestriction combined = _target.CombinedRestriction(new SchedulingOptions { UseRotations = true, UsePreferences = true, UseAvailability = true, UseStudentAvailability = false, UsePreferencesMustHaveOnly = false });
            Assert.IsNotNull(combined);
            Assert.IsTrue(combined.NotAvailable);
        }

        [Test]
        public void VerifyCombinedRestrictionRotation()
        {
            IRestrictionBase sr = new RotationRestriction();
            sr.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(8, 0, 0), null);
            Extract(new List<IRestrictionBase> { sr }, new ReadOnlyCollection<IScheduleData>(new List<IScheduleData>()));
            IEffectiveRestriction combined = _target.CombinedRestriction(new SchedulingOptions{UseRotations = true, UsePreferences = true, UseAvailability = true,UseStudentAvailability = true, UsePreferencesMustHaveOnly = false});
            Assert.IsNotNull(combined);
			Assert.IsTrue(combined.IsRotationDay);
        }

        [Test]
        public void VerifyExtractCanRunSeveralTimesWithoutStackingData()
        {
            PreferenceRestriction sr = new PreferenceRestriction();
            sr.ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("Hej");
            PreferenceDay pd = new PreferenceDay(_person, new DateOnly(2009, 1, 1), sr);
            ReadOnlyCollection<IScheduleData> personRestrictionCollection = new ReadOnlyCollection<IScheduleData>(new List<IScheduleData> { pd });

            Extract2Times(new List<IRestrictionBase> { sr }, personRestrictionCollection);
            Assert.AreEqual(1, _target.PreferenceList.Count());
        }

		

        

        private void Extract(IEnumerable<IRestrictionBase> restrictions, ReadOnlyCollection<IScheduleData> studentRestriction)
        {
            using(_mocks.Record())
            {
                Expect.Call(_stateHolder.Schedules).Return(_dic);
                Expect.Call(_dic[_person]).Return(_range).Repeat.Any();
                Expect.Call(_range.ScheduledDay(_date)).Return(_part).Repeat.Any();
                Expect.Call(_part.PersonRestrictionCollection()).Return(studentRestriction).Repeat.Any();
                Expect.Call(_part.RestrictionCollection()).Return(restrictions).Repeat.Any();
            }

            using (_mocks.Playback())
            {
                _target.Extract(_person, _date);
            }
        }

        private void Extract2Times(IEnumerable<IRestrictionBase> restrictions, ReadOnlyCollection<IScheduleData> studentRestriction)
        {
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.Any();
                Expect.Call(_dic[_person]).Return(_range).Repeat.Any();
                Expect.Call(_range.ScheduledDay(_date)).Return(_part).Repeat.Any();
                Expect.Call(_part.PersonRestrictionCollection()).Return(studentRestriction).Repeat.Any();
                Expect.Call(_part.RestrictionCollection()).Return(restrictions).Repeat.Any();
            }

            using (_mocks.Playback())
            {
                _target.Extract(_person, _date);
                _target.Extract(_person, _date);
            }
        }

        
    }
}