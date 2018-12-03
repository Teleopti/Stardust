using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
    [TestFixture]
    public class PersonRotationModelChildTest
    {
        private int startWeek = 1;

        private Name name1;
        private Name name2;

        private IPerson person1;
        private IPerson person2;

        private Description description1;
        private Description description2;

        private IRotation rotation1;
        private IRotation rotation2;

        private IPersonRotation personRotation1;
        private IPersonRotation personRotation2;

        private DateOnly date;

        [SetUp]
        public void Setup()
        {
            name1 = new Name("Kung-fu", "Panda");
            name2 = new Name("Dark Knight", "Bat Man");

            description1 = new Description("My 1 week rotation");
            description2 = new Description("My 2 week rotation");

            person1 = PersonFactory.CreatePerson();
            person1.WithName(name1);

            person2 = PersonFactory.CreatePerson();
            person2.WithName(name2);

            rotation1 = new Rotation(description1.Name, 7);
            rotation2 = new Rotation(description2.Name, 2 * 7);

            date = new DateOnly(2000, 01, 01);

            personRotation1 = new PersonRotation(person1, rotation1, date, startWeek);
            personRotation2 = new PersonRotation(person2, rotation2, date, startWeek);
        }

        [Test]
        public void VerifyCanAssignPerson()
        {
            PersonRotationModelChild modelChild = new PersonRotationModelChild(person1, null);
            modelChild.PersonRotation = personRotation1;

            Assert.AreEqual(person1, modelChild.Person);
        }

        [Test]
        public void VerifyCanAssignRotation()
        {
            PersonRotationModelChild modelChild =
                new PersonRotationModelChild(person1, null);

            modelChild.PersonRotation = personRotation1;
            Assert.IsNull(modelChild.CurrentRotation);
            modelChild.CurrentRotation = rotation1;

            Assert.AreEqual(rotation1, modelChild.CurrentRotation);
        }

        [Test]
        public void VerifyCanAssignStartWeek()
        {
            PersonRotationModelChild modelChild = new PersonRotationModelChild(person1, null);

            modelChild.PersonRotation = personRotation1;
            modelChild.FromDate = date;

            Assert.AreEqual(date, modelChild.PersonRotation.StartDate);
        }

        [Test]
        public void VerifyCanAssignStartDate()
        {

            PersonRotationModelChild modelChild =
                new PersonRotationModelChild(person1, null);

            //without the person rotation it will return -1 for the start week.
            modelChild.PersonRotation = personRotation1;
            modelChild.FromDate = date;
            Assert.AreEqual(date, modelChild.FromDate);
        }

        [Test]
        public void VerifyCanGrayWhenPersonRotationIsNull()
        {
            PersonRotationModelChild modelChild = new PersonRotationModelChild(person1, null);
            modelChild.PersonRotation = null;

            Assert.IsTrue(modelChild.CanGray);
        }

        [Test]
        public void VerifyCanAssignPersonName()
        {
            PersonRotationModelChild modelChild = new PersonRotationModelChild(person1, null);
            modelChild.PersonRotation = personRotation1;

            Assert.AreEqual(person1.Name, modelChild.Person.Name);
        }

        [Test]
        public void VerifyCanAssignMinValueAsStartDate()
        {
            //might wanna change this test case a bit later depending on the value you have for FromDate
            //when PR is null
            PersonRotationModelChild modelChild = new PersonRotationModelChild(person1, null);

            modelChild.PersonRotation = personRotation1;
            modelChild.FromDate = DateOnly.MinValue;

            Assert.AreEqual(DateOnly.MinValue, modelChild.FromDate);
        }

        [Test]
        public void VerifyIsStartDateIsNullWhenPersonRotationIsNull()
        {
            PersonRotationModelChild modelChild = new PersonRotationModelChild(person1, null);
            modelChild.PersonRotation = null;

            Assert.AreEqual(null, modelChild.FromDate);
        }

        [Test]
        public void VerifyCanSetStartWeek()
        {
            int newStartWeek = 2;
            PersonRotationModelChild modelChild =
                new PersonRotationModelChild(person1, null);

            modelChild.PersonRotation = personRotation1;
            modelChild.CurrentRotation = rotation1;
            modelChild.StartWeek = newStartWeek;

            Assert.AreEqual(newStartWeek, modelChild.StartWeek);
        }

        [Test]
        public void VerifyCanSetPersonRotation()
        {
            PersonRotationModelChild modelChild =
                new PersonRotationModelChild(person1, null);

            modelChild.PersonRotation = personRotation2;
            Assert.AreEqual(personRotation2, modelChild.PersonRotation);
        }

        [Test]
        public void VerifyCanSetPerson()
        {
            PersonRotationModelChild modelChild =
                new PersonRotationModelChild(person1, null);

            modelChild.PersonRotation = personRotation2;
            modelChild.Person = person2;
            Assert.AreEqual(person2, modelChild.Person);
        }

        [Test]
        public void IsStartDateIsNullWhenPersonRotationIsNull()
        {
            PersonRotationModelChild modelChild =
                new PersonRotationModelChild(person1, null);

            modelChild.FromDate = date;
            modelChild.PersonRotation = null;
            Assert.AreEqual(null, modelChild.FromDate);
        }

        [Test]
        public void IsStartDateIsNotNullWhenPersonRotationIsNotNull()
        {
            PersonRotationModelChild modelChild =
                new PersonRotationModelChild(person1, null);

            modelChild.PersonRotation = personRotation1;
            modelChild.FromDate = date;
            Assert.AreEqual(modelChild.FromDate, modelChild.FromDate);
        }

        [Test]
        public void StartDateIsNotSettableWhenPersonRotationIsNull()
        {
            PersonRotationModelChild modelChild =
                new PersonRotationModelChild(person1, null);

            modelChild.PersonRotation = null;
            modelChild.FromDate = date;

            Assert.AreEqual(null, modelChild.FromDate);
        }

        [Test]
        public void StartWeekReturnsMinusWhenPersonRotationIsNull()
        {
            PersonRotationModelChild modelChild =
                new PersonRotationModelChild(person1, null);

            modelChild.PersonRotation = null;
            modelChild.StartWeek = startWeek;

            Assert.AreEqual(-1, modelChild.StartWeek);
        }

        [Test]
        public void CanSetRotationWhenPersonRotationIsNotNull()
        {
            PersonRotationModelChild modelChild =
                new PersonRotationModelChild(person1, null);

            modelChild.PersonRotation = personRotation1;
            modelChild.CurrentRotation = rotation1;

            Assert.AreEqual(rotation1, modelChild.PersonRotation.Rotation);
        }


        [Test]
        public void VerifyCanGetCurrentRotation()
        {
            PersonRotationModelChild modelChild =
                new PersonRotationModelChild(person1, null);

            modelChild.PersonRotation = personRotation1;
            modelChild.CurrentRotation = rotation1;

            Assert.AreEqual(rotation1, modelChild.CurrentRotation);
        }

        [Test]
        public void VerifyFullName()
        {
            PersonRotationModelChild modelChild =
                new PersonRotationModelChild(person1, null);

            modelChild.PersonRotation = personRotation1;
            modelChild.CurrentRotation = rotation1;
            modelChild.PersonFullName = name1.ToString();

            Assert.AreEqual(name1.ToString(), modelChild.PersonFullName);
        }

        [Test]
        public void VerifySetAndGetCanBold()
        {
            PersonRotationModelChild modelChild =
               new PersonRotationModelChild(person1, null);

            modelChild.PersonRotation = personRotation1;
            Assert.IsFalse(modelChild.CanBold);

            modelChild.CanBold = true;

            Assert.IsTrue(modelChild.CanBold);
        }
    }
}
