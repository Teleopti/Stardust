using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationSteps
{
    [TestFixture]
    public class AuthorizationStepTest
    {
        private AuthorizationStepTestClass _target;
        private AuthorizationStepTestClass _parent;
        private string _nameValue = "Text";

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _target = new AuthorizationStepTestClass(_nameValue);
            _parent = new AuthorizationStepTestClass("Parent");
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(_nameValue, _target.PanelName);
        }

        [Test]
        public void VerifyConstructorOverload1()
        {
            string descriptionValue = "DescriptionText";
            _target = new AuthorizationStepTestClass(_nameValue, descriptionValue);
            Assert.IsNotNull(_target);
            Assert.AreEqual(_nameValue, _target.PanelName);
            Assert.AreEqual(descriptionValue, _target.PanelDescription);
        }

        [Test]
        public void VerifyConstructorOverload2()
        {
            string descriptionValue = "DescriptionText";
            _target = new AuthorizationStepTestClass(_parent, _nameValue, descriptionValue);
            Assert.IsNotNull(_target);
            Assert.AreEqual(_nameValue, _target.PanelName);
            Assert.AreEqual(descriptionValue, _target.PanelDescription);
            Assert.AreSame(_target.Parents[0], _parent);
        }

        [Test]
        public void VerifyConstructorOverload3()
        {
            string descriptionValue = "DescriptionText";
            IList<IAuthorizationStep> list = new List<IAuthorizationStep>();
            list.Add(_parent);
            _target = new AuthorizationStepTestClass(list, _nameValue, descriptionValue);
            Assert.IsNotNull(_target);
            Assert.AreEqual(_nameValue, _target.PanelName);
            Assert.AreEqual(descriptionValue, _target.PanelDescription);
            Assert.AreSame(_target.Parents[0], _parent);
        }

        [Test]
        public void VerifyConstructorOverload4()
        {
            _target = new AuthorizationStepTestClass(_parent, _nameValue);
            Assert.IsNotNull(_target);
            Assert.AreEqual(_nameValue, _target.PanelName);
            Assert.AreSame(_target.Parents[0], _parent);
        }

        [Test]
        public void VerifyConstructorOverload5()
        {
            IList<IAuthorizationStep> list = new List<IAuthorizationStep>();
            list.Add(_parent);
            _target = new AuthorizationStepTestClass(list, _nameValue);
            Assert.IsNotNull(_target);
            Assert.AreEqual(_nameValue, _target.PanelName);
            Assert.AreSame(_target.Parents[0], _parent);
        }

        [Test]
        public void VerifyParentListIsNeverNull()
        {
            _target.SetParents(null);
            Assert.IsNotNull(_target.Parents);
        }

        [Test]
        public void VerifyCanSetParent()
        {
            Assert.AreEqual(0, _target.Parents.Count);
            IAuthorizationStep setValue = _parent;
            IList<IAuthorizationStep> parents = new List<IAuthorizationStep>();
            parents.Add(setValue);
            _target.SetParents(parents);
            IAuthorizationStep getValue = _target.Parents[0];
            Assert.AreSame(setValue, getValue);

        }

        [Test]
        public void VerifyCanSetPanelName()
        {
            string setValue = "NewPanelName";
            _target.SetPanelName(setValue);
            string getValue = _target.PanelName;
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyCanSetPanelDescription()
        {
            string setValue = "NewPanelDescription";
            _target.SetPanelDescription(setValue);
            string getValue = _target.PanelDescription;
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyRefreshList()
        {
            _target.Parents.Add(_parent);
            _target.SetRefreshOwnListResult(AuthorizationObjectFactory.CreateAuthorizationEntityList());
            _target.RefreshList();
            Assert.IsTrue(_target.RefreshedOwnListCalled);
            Assert.IsTrue(_parent.RefreshedOwnListCalled);
            Assert.IsNotNull(_target.ProvidedList<IAuthorizationEntity>());
        }

        [Test]
        public void VerifyRefreshListWhenDisabled()
        {
            // if disabled then the local RefreshOwnList should not, but the parent's RefreshOwnList
            // should be called

            _target.Parents.Add(_parent);
            _target.SetRefreshOwnListResult(AuthorizationObjectFactory.CreateAuthorizationEntityList());
            _target.Enabled = false;
            _parent.Enabled = true;
            _target.RefreshList();
            Assert.IsFalse(_target.RefreshedOwnListCalled);
            Assert.IsTrue(_parent.RefreshedOwnListCalled);

        }

        [Test]
        public void VerifyProvidedList()
        {
            IList<IAuthorizationEntity> expectedList = AuthorizationObjectFactory.CreateAuthorizationEntityList();
            _target.SetRefreshOwnListResult(expectedList);
            _target.RefreshList();
            IList<IAuthorizationEntity> resultList = _target.ProvidedList<IAuthorizationEntity>();
            for (int counter = 0; counter < expectedList.Count; counter++)
            {
                Assert.AreSame(expectedList[counter], resultList[counter]);
            }
        }

        [Test]
        public void VerifyProvidedListIsNeverNull()
        {
            _target.SetRefreshOwnListResult(null);
            _target.RefreshList();
            Assert.IsNotNull(_target.ProvidedList<IAuthorizationEntity>());
        }

        [Test]
        public void VerifyInnerExceptionProperty()
        {
            Exception setException = new Exception("TestException");
            _target.SetInnerException(setException);
            Assert.AreEqual(setException, _target.InnerException);
        }

        [Test]
        public void VerifyEnabledProperty()
        {
            bool setValue = !_target.Enabled;
            _target.Enabled = setValue;
            bool getValue = _target.Enabled;
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyWarningMessageProperty()
        {
            string setValue = "Message";
            _target.WarningMessage = setValue;
            string getValue = _target.WarningMessage;
            Assert.AreEqual(setValue, getValue);
        }
    }
}
