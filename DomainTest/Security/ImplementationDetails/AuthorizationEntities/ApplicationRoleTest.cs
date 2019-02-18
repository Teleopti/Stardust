using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.AuthorizationEntities
{
	[TestFixture]
    public class ApplicationRoleTest
    {
        private ApplicationRole _target;

        [SetUp]
        public void Setup()
        {
            _target = new ApplicationRole();
        }

        [Test]
        public void VerifyAddRemoveApplicationFunction()
        {
            var fkn = new ApplicationFunction("heja gnaget!");
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
            var fkn = new ApplicationFunction("heja gnaget!");
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
        public void VerifyBusinessUnitCanBeSet()
        {
            Assert.IsNull(_target.BusinessUnit);
            _target.SetBusinessUnit(BusinessUnitUsedInTests.BusinessUnit);
            Assert.AreSame(BusinessUnitUsedInTests.BusinessUnit, _target.BusinessUnit);
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
            var clonedEntity = (ApplicationRole)_target.Clone();
            Assert.AreNotEqual(clonedEntity, _target);
            Assert.AreNotSame(clonedEntity, _target);
        }

        [Test]
        public void VerifyUserTextTranslator()
        {
            var explicitUserTextTranslator = new UserTextTranslator();
            _target.UserTextTranslator = explicitUserTextTranslator;
            Assert.AreSame(explicitUserTextTranslator, _target.UserTextTranslator);
        }
    }
}
