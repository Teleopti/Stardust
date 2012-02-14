using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationSteps
{
    [TestFixture]
    public class SingleEntityBasedAuthorizationStepTest
    {
        private SingleEntityBasedAuthorizationStepTestClass _target;
        private AuthorizationStepTestClass _parent;
        private AuthorizationEntityProviderTestClass _entityProvider;
        private IList<AuthorizationEntity> _expectedEntityList;

        [SetUp]
        public void Setup()
        {
            _parent = new AuthorizationStepTestClass("ParentStep");
            _entityProvider = new AuthorizationEntityProviderTestClass();
            // fill up expected result list
            _expectedEntityList = new List<AuthorizationEntity>();
            _expectedEntityList.Add(new AuthorizationEntity("KeyOne", "NameOne", "InfoOne", "Value1"));
            _expectedEntityList.Add(new AuthorizationEntity("KeyTwo", "NameTwo", "InfoTwo", "Value2"));
            _entityProvider.setResultEntityList(_expectedEntityList);

            _target = new SingleEntityBasedAuthorizationStepTestClass
                (_entityProvider,
                _parent, 
                "TargetStep");
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyConstructorOverload1()
        {
            string name = "TargetName";
            _target = new SingleEntityBasedAuthorizationStepTestClass
            (_entityProvider, null, name);
            Assert.IsNotNull(_target);
            Assert.AreEqual(name, _target.PanelName);
        }

        [Test]
        public void VerifyConstructorOverload2()
        {
            string name = "TargetName";
            string desc = "TargetDescription";
            _target = new SingleEntityBasedAuthorizationStepTestClass
            (_entityProvider, null, name, desc);
            Assert.IsNotNull(_target);
            Assert.AreEqual(name, _target.PanelName);
            Assert.AreEqual(desc, _target.PanelDescription);
        }

        [Test]
        public void VerifyRefreshList()
        {
            _target.RefreshList();
            IList<IAuthorizationEntity> resultList = _target.StoredResultListFromRefreshOwnListMethod;
            for (int counter = 0; counter < _expectedEntityList.Count; counter++)
            {
                Assert.AreSame(_expectedEntityList[counter], resultList[counter]);
            }
        }

        [Test]
        public void VerifyResultListNeverNullButEmptyList()
        {
            _entityProvider.setResultEntityList(null);
            _target.RefreshList();

            // Expectations
            Assert.IsNotNull(_target.ProvidedList<AuthorizationEntity>());
            Assert.AreEqual(0, _target.ProvidedList<AuthorizationEntity>().Count);
        }

    }
}
