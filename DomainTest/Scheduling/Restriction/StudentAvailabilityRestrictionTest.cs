using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;


namespace Teleopti.Ccc.DomainTest.Scheduling.Restriction
{
    [TestFixture]
    public class StudentAvailabilityRestrictionTest
    {
        private StudentAvailabilityDay _target;
        StudentAvailabilityRestriction _studentRestriction;
        private IList<IStudentAvailabilityRestriction> _studentRestrictions;
        private IPerson _person;
        private DateOnly _dateOnly;

        [SetUp]
        public void Setup()
        {
            _person = new Person();
            _dateOnly = new DateOnly(2009, 2, 2);
            _studentRestriction = new StudentAvailabilityRestriction();
            _studentRestrictions = new List<IStudentAvailabilityRestriction> { _studentRestriction };
	        _target = new StudentAvailabilityDay(_person, _dateOnly, _studentRestrictions) {NotAvailable = true};
        }

        [Test]
        public void CanSetAndReadProperties()
        {
            Assert.AreEqual(_person, _target.Person);
            Assert.AreEqual(_dateOnly, _target.RestrictionDate);
            Assert.AreEqual(1,_target.RestrictionCollection.Count);
            _target.NotAvailable = false;
            Assert.AreEqual(1, _target.RestrictionCollection.Count);
            Assert.AreEqual(_person,_target.MainRoot);
			Assert.That(_target.FunctionPath, Is.Not.Null.Or.Empty);
            Assert.IsNull(_target.Scenario);
            Assert.AreEqual(0, _target.IndexInCollection(_studentRestriction));
        }

        [Test]
        public void CanGetIndexFromCollection()
        {
            Assert.AreEqual(0, _studentRestriction.Index);
            var unParentalRestriction = new StudentAvailabilityRestriction();
            Assert.AreEqual(-1, unParentalRestriction.Index);
        }

        [Test]
        public void VerifyNoneEntityClone()
        {
            ((IStudentAvailabilityRestriction)_studentRestriction).SetId(new System.Guid());
            IStudentAvailabilityRestriction clone = _studentRestriction.NoneEntityClone();
            Assert.IsNull(clone.Id);
        }

        [Test]
        public void VerifyEntityClone()
        {
            ((IStudentAvailabilityRestriction)_studentRestriction).SetId(new System.Guid());
            IStudentAvailabilityRestriction clone = _studentRestriction.EntityClone();
            Assert.IsNotNull(clone.Id);
            Assert.AreEqual(_studentRestriction.Id, clone.Id);
        }

	    [Test]
	    public void ShouldChangeRestrictionDetails()
	    {
		    _target.Change(new TimePeriod(8,0,13,0));

		    _target.RestrictionCollection[0].StartTimeLimitation.StartTime.GetValueOrDefault()
			    .Should()
			    .Be.EqualTo(TimeSpan.FromHours(8));

			_target.RestrictionCollection[0].EndTimeLimitation.EndTime.GetValueOrDefault()
				.Should()
				.Be.EqualTo(TimeSpan.FromHours(13));
	    }

		[Test]
		public void ShouldClearPreviousRestrictionDetail()
		{
			var before = _target.RestrictionCollection[0];

			_target.Change(new TimePeriod(8, 0, 13, 0));

			_target.RestrictionCollection[0].Should().Not.Be.SameInstanceAs(before);
		}
    }
}
