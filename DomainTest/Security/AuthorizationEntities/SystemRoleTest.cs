﻿using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{
    [TestFixture]
    public class SystemRoleTest
    {
        private SystemRole _target;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _target = new SystemRole();
        }

        [Test]
        public void CanSetName()
        {
            string desc = "test";
            _target.DescriptionText =  desc;
            Assert.AreEqual(desc, _target.DescriptionText);
        }

        [Test]
        public void VerifyProperties()
        {
            string name = "NameMember";
            string description = "DescriptionMember";
            _target = new SystemRole();
            _target.Name = name;
            _target.DescriptionText = description;
            Assert.AreEqual(name, _target.Name);
            Assert.AreEqual(name, _target.AuthorizationName);
            Assert.AreEqual(description, _target.DescriptionText);
            Assert.AreEqual(description, _target.AuthorizationDescription);
            Assert.AreEqual(string.Empty, _target.AuthorizationValue);
            Assert.AreEqual(description, _target.AuthorizationKey);

            // check null value
            _target = new SystemRole();
            Assert.IsTrue(string.IsNullOrEmpty(_target.Name));
            Assert.IsTrue(string.IsNullOrEmpty(_target.DescriptionText));
        }

        [Test]
        public void VerifyDeletedProperty()
        {
            Assert.IsFalse(_target.IsDeleted);

            _target.SetDeleted();

            Assert.IsTrue(_target.IsDeleted);
        }
    }
}
