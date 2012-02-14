﻿using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{
    [TestFixture]
    public class ApplicationRoleTest
    {
        private ApplicationRole _target;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _target = new ApplicationRole();
        }

        [Test]
        public void VerifyAddRemoveApplicationFunction()
        {
            ApplicationFunction fkn = new ApplicationFunction("heja gnaget!");
            Assert.AreEqual(0, _target.ApplicationFunctionCollection.Count);
            _target.AddApplicationFunction(fkn);
            Assert.AreEqual(1, _target.ApplicationFunctionCollection.Count);
            Assert.IsTrue(_target.ApplicationFunctionCollection.Contains(fkn));
            _target.RemoveApplicationFunction(fkn);
            Assert.AreEqual(0, _target.ApplicationFunctionCollection.Count);
            Assert.IsFalse(_target.ApplicationFunctionCollection.Contains(fkn));
        }

        [Test]
        public void VerifyDoesNotAddApplicationFunctionTwice()
        {
            ApplicationFunction fkn = new ApplicationFunction("heja gnaget!");
            Assert.AreEqual(0, _target.ApplicationFunctionCollection.Count);
            _target.AddApplicationFunction(fkn);
            _target.AddApplicationFunction(fkn);
            Assert.AreEqual(1, _target.ApplicationFunctionCollection.Count);
        }

        [Test]
        public void VerifyBuiltInProperty()
        {
            bool setValue = true;
            _target.BuiltIn = setValue;
            bool getValue = _target.BuiltIn;

            Assert.AreEqual(setValue, getValue);

            setValue = false;
            _target.BuiltIn = setValue;
            getValue = _target.BuiltIn;

            Assert.AreEqual(setValue, getValue);

        }

        [Test]
        public void VerifyAvailableDataProperty()
        {
            IAvailableData setValue = new AvailableData();
            _target.AvailableData = setValue;
            IAvailableData getValue = _target.AvailableData;
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyAuthorizationValueProperty()
        {
            bool setValue = true;
            _target.BuiltIn = setValue;
            string getValue = _target.AuthorizationValue;

            Assert.IsFalse(string.IsNullOrEmpty(getValue));

            setValue = false;
            _target.BuiltIn = setValue;
            getValue = _target.AuthorizationValue;

            Assert.IsTrue(string.IsNullOrEmpty(getValue));
        }

        /// <summary>
        /// Verifies the set business unit.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), 
        Test, 
        Explicit("Tamas! Have a look at this one!")]
        public void VerifyBusinessUnitCanBeSet()
        {
            Assert.IsNull(_target.BusinessUnit);
            _target.SetBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
            Assert.AreSame(BusinessUnitFactory.BusinessUnitUsedInTest, _target.BusinessUnit);
        }

        [Test]
        public void VerifyICloneableEntity()
        {
            ((IEntity)_target).SetId(Guid.NewGuid());
            _target.DescriptionText = "Original Description";
            ApplicationFunction func1 = new ApplicationFunction("func1"),
                func2 = new ApplicationFunction("func2");
            _target.AddApplicationFunction(func1);
            _target.AddApplicationFunction(func2);

            // Entity clone testing.

            ApplicationRole targetCloned = _target.EntityClone();
            Assert.IsNotNull(targetCloned);
            Assert.AreEqual(_target.Id, targetCloned.Id);
            Assert.AreEqual(_target.DescriptionText, targetCloned.DescriptionText);
            // 
            targetCloned.RemoveApplicationFunction(func1);
            targetCloned.RemoveApplicationFunction(func2);
            Assert.AreEqual(2, _target.ApplicationFunctionCollection.Count);
            Assert.AreEqual(0, targetCloned.ApplicationFunctionCollection.Count);

            // None entity clone testing.

            targetCloned = _target.NoneEntityClone();
            Assert.IsNotNull(targetCloned);
            Assert.IsNull(targetCloned.Id);
        }

        [Test]
        public void VerifyClone()
        {
            ApplicationRole clonedEntity = (ApplicationRole)_target.Clone();
            Assert.AreNotEqual(clonedEntity, _target);
            Assert.AreNotSame(clonedEntity, _target);
        }

        [Test]
        public void VerifyUserTextTranslator()
        {
            UserTextTranslator explicitUserTextTranslator = new UserTextTranslator();
            _target.UserTextTranslator = explicitUserTextTranslator;
            Assert.AreSame(explicitUserTextTranslator, _target.UserTextTranslator);
        }
    }
}
