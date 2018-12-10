using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
    /// <summary>
    /// Tests for PersonAvailabilityModelChild
    /// </summary>
    [TestFixture]
    public class PersonAvailabilityModelChildTest
    {
        private Name name1;
        private Name name2;

        private IPerson person1;
        private IPerson person2;

        private Description description1;

        private IAvailabilityRotation availability1;

        private IPersonAvailability personAvailability1;

        private DateOnly date;
        private PersonAvailabilityModelChild _target;

        [SetUp]
        public void Setup()
        {
            name1 = new Name("Kung-fu", "Panda");
            name2 = new Name("Dark Knight", "Bat Man");

            description1 = new Description("My 1 week rotation");

            person1 = PersonFactory.CreatePerson();
            person1.WithName(name1);

            person2 = PersonFactory.CreatePerson();
            person2.WithName(name2);

            availability1 = new AvailabilityRotation(description1.Name, 7);
           
            date = new DateOnly(2000, 01, 01);

            personAvailability1 = new PersonAvailability(person1, availability1, date);

            _target = new PersonAvailabilityModelChild(person1, personAvailability1, null);
        }

        [Test]
        public void VerifyCanAssignPerson()
        {
            Assert.AreEqual(person1, _target.Person);
        }

        [Test]
        public void VerifyCanAssignRotation()
        {
            Assert.IsNull(_target.CurrentRotation);
            _target.CurrentRotation = availability1;

            Assert.AreEqual(availability1, _target.CurrentRotation);
        }

        [Test]
        public void VerifyCanAssignStartDate()
        {
            _target.FromDate = date;           
                       
            Assert.AreEqual(date, _target.FromDate);
        }

        [Test]
        public void VerifyIsStartDateIsNullWhenPersonAvailabilityIsNull()
        {
            _target.PersonRotation = null;

            Assert.AreEqual(null, _target.FromDate);
        }

        [Test]
        public void VerifyFullName()
        {
            Assert.AreEqual(null, _target.PersonFullName);
            _target.PersonFullName = name1.ToString();
            Assert.AreEqual(name1.ToString(), _target.PersonFullName);
        }

        [Test]
        public void VerifyCanGetCurrentAvailability()
        {
            _target.CurrentRotation = availability1;
            Assert.AreEqual(availability1, _target.CurrentRotation);
        }
        
        [Test]
        public void VerifyCanGray()
        {
            Assert.IsFalse(_target.CanGray);
        }

        [Test]
        public void VerifySetAndGetCanBold()
        {
            Assert.IsFalse(_target.CanBold);

            _target.CanBold = true;

            Assert.IsTrue(_target.CanBold);
        }

        [Test]
        public void VerifyCanSetStartWeek()
        {
            int newStartWeek = 2;

            _target.PersonRotation = personAvailability1;
            _target.CurrentRotation = availability1;
            _target.StartWeek = newStartWeek;

            Assert.AreEqual(newStartWeek, _target.StartWeek);
        }
    }
}
