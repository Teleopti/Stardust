using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.GroupPageCreator
{
    /// <summary>
    /// Tests PersonNoteGroupPage
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 7/8/2008
    /// </remarks>
    [TestFixture]
    public class PersonNoteGroupPageTest
    {
        private IGroupPage _target;
        private ICollection<IPerson> _personCollection;
        private PersonNoteGroupPage _obj;
        private string _groupPageName;
        private string _groupPageNameKey;
        private IGroupPage _groupPage;
        private const string noteUpperCase = "Note";
        private const string noteLowerCase = "note";

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/8/2008
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2008, 07, 07));

            // setup Persons
            IPerson person1 = PersonFactory.CreatePerson("Name1");
            person1.Note = noteUpperCase;
            person1.AddPersonPeriod(personPeriod);
            IPerson person2 = PersonFactory.CreatePerson("Name2");
            person2.Note = noteLowerCase;
            person2.AddPersonPeriod(personPeriod);

            // setup RootPersonGroup
            IRootPersonGroup rootPersonGroup1 = new RootPersonGroup(noteUpperCase);
            rootPersonGroup1.AddPerson(person1);
            IRootPersonGroup rootPersonGroup2 = new RootPersonGroup(noteLowerCase);
            rootPersonGroup2.AddPerson(person2);

            // Initializes _target
            _target = new GroupPage("Name");
            _target.AddRootPersonGroup(rootPersonGroup1);
            _target.AddRootPersonGroup(rootPersonGroup2);


            // Initializes _personCollection
            _personCollection = new List<IPerson> { person1, person2 };

            _obj = new PersonNoteGroupPage();

            _groupPageName = "CurrentGroupPageName";
            _groupPageNameKey = "CurrentGroupPageNameKey";
            _groupPage = _obj.CreateGroupPage(null, new GroupPageOptions(_personCollection) { CurrentGroupPageName = _groupPageName, CurrentGroupPageNameKey = _groupPageNameKey });
        }

	    [Test]
	    public void ShouldHandleWhiteSpaceNote()
	    {
		    var person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2015,3,17));
		    person1.Note = "              ";
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2015, 3, 17));
		    person2.Note = "hej";
			var options = new GroupPageOptions(new List<IPerson> { person1, person2 }) { CurrentGroupPageName = _groupPageName, CurrentGroupPageNameKey = _groupPageNameKey, SelectedPeriod = new DateOnlyPeriod(2015,3,17,2015,3,17)};
			var groupPage = _obj.CreateGroupPage(new List<IPerson> { person1, person2 }, options);
			Assert.AreEqual(1, groupPage.RootGroupCollection.Count);
	    }

        /// <summary>
        /// Verifies the root person group count.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/8/2008
        /// </remarks>
        [Test]
        public void VerifyRootPersonGroupCount()
        {
            int expectedRootPageCount = _groupPage.RootGroupCollection.Count;
            int actualRootPageCount = _target.RootGroupCollection.Count;
            Assert.AreEqual(expectedRootPageCount, actualRootPageCount);
        }


        /// <summary>
        /// Verifies the person count.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/8/2008
        /// </remarks>
        [Test]
        public void VerifyPersonCount()
        {
            int expectedRootPageCount = _groupPage.RootGroupCollection[0].PersonCollection.Count;
            int actualRootPageCount = _target.RootGroupCollection[0].PersonCollection.Count;
            Assert.AreEqual(expectedRootPageCount, actualRootPageCount);
        }

        [Test]
        public void VerifyDescription()
        {
            Assert.AreEqual(_groupPageName, _groupPage.Description.Name);
            Assert.AreEqual(_groupPageNameKey, _groupPage.DescriptionKey);
        }

        [Test]
        public void ShouldCreateNoteGroupsBasedOnCaseSensitive()
        {
            Assert.AreEqual(1, _groupPage.RootGroupCollection.Count(n => String.Compare(n.Name, noteUpperCase, StringComparison.Ordinal) == 0));
            Assert.AreEqual(1, _groupPage.RootGroupCollection.Count(n => String.Compare(n.Name, noteLowerCase, StringComparison.Ordinal) == 0));
        }

		[Test]
		public void ShouldEachRootPersonGroupHasAGuid()
		{
			Assert.That(_groupPage.RootGroupCollection[0].Id.ToString(), Is.EqualTo("ca53d6aa-e63e-6369-5f29-38b73098b6d7"));
			Assert.That(_groupPage.RootGroupCollection[1].Id.ToString(), Is.EqualTo("c749063b-5026-13c3-a357-338dcdfb64ec"));
		}
    }
}