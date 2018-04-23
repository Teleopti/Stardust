﻿using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for RootPersonGroupTest
    /// </summary>
    [TestFixture]
    public class RootPersonGroupTest
    {
        private const int groupNameLengthLimit = 255;
        private IRootPersonGroup _root;
        private IChildPersonGroup _child1;
        private IChildPersonGroup _child2;
        private IChildPersonGroup _child3;
        private IRootPersonGroup _roottest;

        [SetUp]
        public void Setup()
        {
            _root = new RootPersonGroup("Group Unit");
            _child1 = new ChildPersonGroup("Sub Group Unit1");
            _child2 = new ChildPersonGroup("Sub Group Unit2");
            _child3 = new ChildPersonGroup("Sub Group Unit3");
            _roottest = new RootPersonGroup();

            _child1.AddChildGroup(_child2);
            _root.AddChildGroup(_child1);
            _roottest.AddChildGroup(_child3);
        }

        [Test]
        public void ShouldIdentifyAsTeamWithTeamEntity()
        {
            _child2.Entity = TeamFactory.CreateSimpleTeam();
            Assert.IsTrue(_child2.IsTeam);
        }

        [Test]
        public void ShouldIdentifyAsSiteWithSiteEntity()
        {
            _child2.Entity = SiteFactory.CreateSimpleSite();
            Assert.IsTrue(_child2.IsSite);
        }

        [Test]
        public void VerifyCheckProperties()
        {
            Assert.IsNotNull(_root.Name);
            Assert.IsNotNull(_root.PersonCollection);
            Assert.IsNotNull(_root.ChildGroupCollection);
            Assert.IsFalse(_root.IsTeam);
            Assert.IsFalse(_root.IsSite);
        }

        [Test]
        public void VerifyCanAddRemoveChildGroups()
        {
            // Check Parent
            Assert.IsNull(_root.Parent);
            Assert.AreEqual(_root, _child1.Parent);
            Assert.AreEqual(_child1, _child2.Parent);

            Assert.AreEqual(1, _root.ChildGroupCollection.Count);
            Assert.AreEqual(_child1, _root.ChildGroupCollection[0]);

            IChildPersonGroup groupUnit = _root.ChildGroupCollection[0];
            
            Assert.IsNotNull(groupUnit);
            Assert.AreEqual(1, groupUnit.ChildGroupCollection.Count);
            Assert.AreEqual(_child2, groupUnit.ChildGroupCollection[0]);

            groupUnit.RemoveChildGroup(_child2);
            Assert.AreEqual(0, groupUnit.ChildGroupCollection.Count);
        }

        [Test]
        public void VerifyNameCanSet()
        {
            _root.Name = "FunGroup";

            Assert.AreEqual("FunGroup", _root.Name);

        }

	    [Test]
	    public void ShouldHandleNameWith100Characters()
	    {
		    _root.Name = new String('n', 100);
		    _root.Name.Length.Should().Be.EqualTo(100);
	    }

		[Test]
		public void ShouldThrowErrorWhenNameExceedsLengthLimit()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => _root.Name = new string('n', groupNameLengthLimit + 1));
		}

	    [Test]
	    public void ShouldThrowErrorFromConstructorWhenNameExceedsLengthLimit()
	    {
		    Assert.Throws<ArgumentOutOfRangeException>(() => _root = new RootPersonGroup(new string('n', groupNameLengthLimit + 1)));
	    }

		[Test]
        public void VerifyCanAddRemovePersonsFromGroup()
        {
            IPerson person1 = PersonFactory.CreatePerson("Person1");
            IPerson person2 = PersonFactory.CreatePerson("Person2");
            IPerson person3 = PersonFactory.CreatePerson("Person3");
            IPerson person4 = PersonFactory.CreatePerson("Person4");
            IPerson person5 = PersonFactory.CreatePerson("Person5");

            #region Target GroupUnit Tests

            // Get Target group unit and add persons
            Assert.AreEqual(0, _root.PersonCollection.Count);
            _root.AddPerson(person1);
            _root.AddPerson(person2);
            _root.AddPerson(person3);
            Assert.AreEqual(3, _root.PersonCollection.Count);

            // Remove person from target group unit
            _root.RemovePerson(person3);
            Assert.AreEqual(2, _root.PersonCollection.Count);
            Assert.IsFalse(_root.PersonCollection.Contains(person3));

            #endregion

            #region SubGroupUnit Tests

            // Get sub groups add persons
            IList<IChildPersonGroup> subGroupUnits = new List<IChildPersonGroup>(_root.ChildGroupCollection.OfType<IChildPersonGroup>());
            Assert.AreEqual(0, subGroupUnits[0].PersonCollection.Count);
            subGroupUnits[0].AddPerson(person4);
            Assert.AreEqual(1, subGroupUnits[0].PersonCollection.Count);

            // Check the scenario user can add person already in person collection
            subGroupUnits[0].AddPerson(person4);
            Assert.AreEqual(1, subGroupUnits[0].PersonCollection.Count);

            // Remove person from sub group unit1
            subGroupUnits[0].RemovePerson(person4);
            Assert.AreEqual(0, subGroupUnits[0].PersonCollection.Count);
 
            #endregion

            #region SubGroupUnit 2 Tests

            // Get sub groups
            IList<IChildPersonGroup> subGroupUnits1 = new List<IChildPersonGroup>(subGroupUnits[0].ChildGroupCollection.OfType<IChildPersonGroup>());
            Assert.AreEqual(0, subGroupUnits1[0].PersonCollection.Count);
            subGroupUnits1[0].AddPerson(person5);
            Assert.AreEqual(1, subGroupUnits1[0].PersonCollection.Count);

            // Remove person from sub group unit2
            subGroupUnits1[0].RemovePerson(person5);
            Assert.AreEqual(0, subGroupUnits1[0].PersonCollection.Count);
            
            #endregion

        }

		[Test]
		public void ShouldIdentifyAsOptionalColumnEntity()
		{
			_child3.Entity = new OptionalColumn("");
			Assert.IsTrue(_child3.IsOptionalColumn);
		}
	}
}
