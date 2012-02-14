using System.Collections.Generic;
using NUnit.Framework;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
    [TestFixture]
    public class PersonAvailabilityModelParentTest
    {
        private Name name1;
        private Name name2;

        private IPerson person1;
        private IPerson person2;

        private Description description1;

        private IAvailabilityRotation availability1;

        private IPersonAvailability personAvailability1;

        private CommonNameDescriptionSetting commonNameDecroption1;

        private DateOnly date;
        private PersonAvailabilityModelParent _target;
        private PersonAvailabilityModelParent _target2;

        [SetUp]
        public void Setup()
        {
            name1 = new Name("Kung-fu", "Panda");
            name2 = new Name("Dark Knight", "Bat Man");

            description1 = new Description("My 1 week rotation");

            person1 = PersonFactory.CreatePerson();
            person1.Name = name1;

            person2 = PersonFactory.CreatePerson();
            person2.Name = name2;

            availability1 = new AvailabilityRotation(description1.Name, 7);

            date = new DateOnly(2000, 01, 01);

            personAvailability1 = new PersonAvailability(person1, availability1, date);

            commonNameDecroption1 = new CommonNameDescriptionSetting();

            _target = new PersonAvailabilityModelParent(person1, personAvailability1, null);
            _target2 = new PersonAvailabilityModelParent(person1, personAvailability1, commonNameDecroption1);
        }

        [Test]
        public void VerifyCanAssignPerson()
        {
            Assert.AreEqual(person1, _target.Person);
        }

        [Test]
        public void VerifyCanAssignAvailability()
        {
            _target.PersonRotation = personAvailability1;
            Assert.AreEqual(personAvailability1, _target.PersonRotation);


        }

        [Test]
        public void VerifyCanGetPersonFullName()
        {
            Assert.AreEqual(person1.Name.ToString(), _target.PersonFullName);
        }

        [Test]
        public void VerifyCanGetPersonCommonName()
        {
            Assert.AreEqual(person1.Name.ToString(), _target2.PersonFullName);
        }

        [Test]
        public void VerifyCanAssignStartDate()
        {
            _target.FromDate = date;
            Assert.AreEqual(date, _target.FromDate);
        }

        [Test]
        public void VerifyCanGrayWhenPersonRotationIsNull()
        {
            _target.PersonRotation = null;
            Assert.IsTrue(_target.CanGray);
        }


        [Test]
        public void VerifyCanSetExpandState()
        {
            _target.ExpandState = true;
            Assert.IsTrue(_target.ExpandState);
        }

        [Test]
        public void VerifyCanSetRotationCount()
        {
            int rotationCount = 233;

            _target.RotationCount = rotationCount;
            Assert.AreEqual(rotationCount, _target.RotationCount);
        }

        [Test]
        public void VerifyCanSetGridControl()
        {
            using (GridControl grid = new GridControl())
            {
                _target.GridControl = grid;

                Assert.AreEqual(grid, _target.GridControl);
            }

        }

        [Test]
        public void VerifySetAndGetCanBold()
        {

            Assert.IsFalse(_target.CanBold);

            _target.CanBold = true;

            Assert.IsTrue(_target.CanBold);
        }

        [Test]
        public void VerifyMaxWeek()
        {
            PersonAvailabilityModelParent adapterParent =
                new PersonAvailabilityModelParent(person1, null, commonNameDecroption1);
            adapterParent.StartWeek = 5;
            Assert.AreEqual(adapterParent.StartWeek, -1);
            adapterParent.PersonRotation = personAvailability1;
            adapterParent.StartWeek = 5;
            Assert.AreEqual(adapterParent.StartWeek, 5);
            adapterParent.StartWeek = int.MaxValue;
            Assert.AreNotEqual(adapterParent.StartWeek, int.MaxValue);

        }

        [Test]
        public void VerifyResetCanBoldPropertyOfChildAdapters()
        {
            using (GridControl grid = new GridControl())
            {

                PersonAvailabilityModelChild adapter1 = new PersonAvailabilityModelChild(person1, personAvailability1,
                                                                                         null);

                PersonAvailabilityModelChild adapter2 = new PersonAvailabilityModelChild(person2, personAvailability1,
                                                                                         null);


                adapter1.CanBold = true;
                adapter2.CanBold = true;

                IList<PersonAvailabilityModelChild> adapterCollection = new
                    List<PersonAvailabilityModelChild>();
                adapterCollection.Add(adapter1);
                adapterCollection.Add(adapter2);

                grid.Tag = adapterCollection;

                _target.GridControl = grid;

                _target.ResetCanBoldPropertyOfChildAdapters();

                IList<PersonAvailabilityModelChild> childAdapters = _target.GridControl.Tag as
                                                                    IList<PersonAvailabilityModelChild>;


                Assert.IsNotNull(childAdapters);
                Assert.AreEqual(2, childAdapters.Count);
                Assert.IsFalse(childAdapters[0].CanBold);
                Assert.IsFalse(childAdapters[1].CanBold);
            }
        }
    }
}

