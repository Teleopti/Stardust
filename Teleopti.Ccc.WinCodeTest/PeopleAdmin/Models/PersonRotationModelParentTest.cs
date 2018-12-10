using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
    [TestFixture]
    public class PersonRotationModelParentTest
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

        private CommonNameDescriptionSetting _commonNameDescription1;

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

            _commonNameDescription1 = new CommonNameDescriptionSetting();
        }

        [Test]
        public void VerifyRotationWeekCountUsedByGridToBindByReflection()
        {
            PersonRotationModelParent modelParent =
                new PersonRotationModelParent(person1, null);
            modelParent.PersonRotation = personRotation1;

            Assert.AreEqual(1, modelParent.RotationWeekCount.Count);
        }

        [Test]
        public void VerifyCanAssignPerson()
        {
            PersonRotationModelParent modelParent =
                new PersonRotationModelParent(person1, null);
            modelParent.PersonRotation = personRotation1;

            Assert.AreEqual(person1, modelParent.Person);
        }

        [Test]
        public void VerifyCamAssignRotation()
        {
            PersonRotationModelParent modelParent =
                new PersonRotationModelParent(person1, null);
            modelParent.PersonRotation = personRotation1;
            Assert.AreEqual(personRotation1, modelParent.PersonRotation);
        }

        [Test]
        public void VerifyMaxWeek()
        {
            PersonRotationModelParent modelParent =
                new PersonRotationModelParent(person1, null);
            modelParent.StartWeek = 5;
            Assert.AreEqual(-1, modelParent.StartWeek);
            Assert.IsNull(modelParent.FromDate);
            modelParent.PersonRotation = personRotation1;
            modelParent.StartWeek = 5;
            Assert.AreEqual(personRotation1.StartDay, (modelParent.StartWeek-1)*7);
            Assert.AreEqual(5, modelParent.StartWeek);
            modelParent.StartWeek = int.MaxValue;
            Assert.AreNotEqual(int.MaxValue, modelParent.StartWeek);
            
        }

        [Test]
        public void VerifyCanGetPersonFullName()
        {
            PersonRotationModelParent modelParent =
                new PersonRotationModelParent(person1, null);
            Assert.AreEqual(person1.Name.ToString(), modelParent.PersonFullName);
        }

        [Test]
        public void VerifyCanGetPersonCommonName()
        {
            PersonRotationModelParent modelParent =
                new PersonRotationModelParent(person1, _commonNameDescription1);
            Assert.AreEqual(person1.Name.ToString(), modelParent.PersonFullName);
        }

        [Test]
        public void VerifyCanAssignStartWeek()
        {
            PersonRotationModelChild modelParent =
                new PersonRotationModelChild(person2, null);
            modelParent.PersonRotation = personRotation2;

            Assert.AreEqual(date, modelParent.PersonRotation.StartDate);
        }

        [Test]
        public void VerifyCanAssignStartDate()
        {

            PersonRotationModelParent modelParent =
                new PersonRotationModelParent(person1, null);

            modelParent.PersonRotation = personRotation1;
            modelParent.FromDate = date;


            Assert.AreEqual(date, modelParent.FromDate);
        }

        [Test]
        public void VerifyCanGrayWhenPersonRotationIsNull()
        {
            PersonRotationModelParent modelParent =
                new PersonRotationModelParent(person1, null);
            modelParent.PersonRotation = null;
            Assert.IsTrue(modelParent.CanGray);
        }

        [Test]
        public void VerifyCanSetExpandState()
        {
            PersonRotationModelParent modelParent =
                new PersonRotationModelParent(person1, null);
            modelParent.PersonRotation = personRotation1;
            modelParent.ExpandState = true;
            Assert.IsTrue(modelParent.ExpandState);
        }

        [Test]
        public void VerifyCanSetRotationCount()
        {
            int rotationCount = 233;

            PersonRotationModelParent modelParent =
                new PersonRotationModelParent(person1, null);
            modelParent.PersonRotation = personRotation1;
            modelParent.RotationCount = rotationCount;

            Assert.AreEqual(rotationCount, modelParent.RotationCount);
        }

        [Test]
        public void VerifyCanSetGridControl()
        {
            using (GridControl grid = new GridControl())
            {
                PersonRotationModelParent modelParent = new PersonRotationModelParent(person1, null);

                modelParent.PersonRotation = personRotation1;
                modelParent.GridControl = grid;

                Assert.AreEqual(grid, modelParent.GridControl);
            }
        }


        [Test]
        public void VerifySetAndGetCanBold()
        {
            PersonRotationModelParent modelParent = new PersonRotationModelParent(person1, null);
            Assert.IsFalse(modelParent.CanBold);

            modelParent.CanBold = true;

            Assert.IsTrue(modelParent.CanBold);
        }

        [Test]
        public void VerifyResetCanBoldPropertyOfChildAdapters()
        {

            PersonRotationModelParent model = new PersonRotationModelParent(person1, null);

            using (GridControl grid = new GridControl())
            {

                PersonRotationModelChild adapter1 = new PersonRotationModelChild(person1, null);

                PersonRotationModelChild adapter2 = new PersonRotationModelChild(person2, null);


                adapter1.CanBold = true;
                adapter2.CanBold = true;

                IList<PersonRotationModelChild> adapterCollection = new
                    List<PersonRotationModelChild>();
                adapterCollection.Add(adapter1);
                adapterCollection.Add(adapter2);

                grid.Tag = adapterCollection;

                model.GridControl = grid;

                model.ResetCanBoldPropertyOfChildAdapters();

                IList<PersonRotationModelChild> childAdapters = model.GridControl.Tag as
                                                                IList<PersonRotationModelChild>;


                Assert.IsNotNull(childAdapters);
                Assert.AreEqual(2, childAdapters.Count);
                Assert.IsFalse(childAdapters[0].CanBold);
                Assert.IsFalse(childAdapters[1].CanBold);
            }
        }

		[Test]
		public void ShouldGetCanBoldOnAdapterAndChildAdaptersWhenChildCanBold()
		{
			var modelParent = new PersonRotationModelParent(person1, null);

			using (var grid = new GridControl())
			{
				var adapter1 = new PersonRotationModelChild(person1, null);
				adapter1.CanBold = true;
				IList<PersonRotationModelChild> adapterCollection = new List<PersonRotationModelChild>();
				adapterCollection.Add(adapter1);
				grid.Tag = adapterCollection;
				modelParent.GridControl = grid;

				modelParent.AdapterOrChildCanBold().Should().Be.True();
			}
		}

		[Test]
		public void ShouldGetCanBoldOnAdapterAndChildAdaptersWhenParentCanBold()
		{
			var modelParent = new PersonRotationModelParent(person1, null);

			using (var grid = new GridControl())
			{
				var adapter1 = new PersonRotationModelChild(person1, null);
				IList<PersonRotationModelChild> adapterCollection = new List<PersonRotationModelChild>();
				adapterCollection.Add(adapter1);
				grid.Tag = adapterCollection;
				modelParent.GridControl = grid;
				modelParent.CanBold = true;
				modelParent.AdapterOrChildCanBold().Should().Be.True();
			}
		}

		[Test]
		public void ShouldNotGetCanBoldOnAdapterAndChildAdaptersWhenParentOrChildCantBold()
		{
			var modelParent = new PersonRotationModelParent(person1, null);

			using (var grid = new GridControl())
			{
				var adapter1 = new PersonRotationModelChild(person1, null);
				IList<PersonRotationModelChild> adapterCollection = new List<PersonRotationModelChild>();
				adapterCollection.Add(adapter1);
				grid.Tag = adapterCollection;
				modelParent.GridControl = grid;
				modelParent.AdapterOrChildCanBold().Should().Be.False();
			}
		}
	}
}
