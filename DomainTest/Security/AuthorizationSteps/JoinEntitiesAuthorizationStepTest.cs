using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.AuthorizationSteps;
using Teleopti.Ccc.TestCommon.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationSteps
{
    [TestFixture]
    public class JoinEntitiesAuthorizationStepTest
    {
        private JoinEntitiesAuthorizationStep _target;
        private AuthorizationStepTestClass _firstParent;
        private AuthorizationStepTestClass _secondParent;

        [SetUp]
        public void Setup()
        {
            IList<AuthorizationStepTestClass> steps = AuthorizationObjectFactory.CreateTestAuthorizationStepsStructure();
            _firstParent = steps[0];
            _secondParent = steps[1];            
            _target = new JoinEntitiesAuthorizationStep
                (_firstParent, _secondParent, "TargetStep", JoinStrategyOption.Union);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(2, _target.Parents.Count);
        }

        [Test]
        public void VerifyConstructorOverload1()
        {
            IList<IAuthorizationStep> parents = new List<IAuthorizationStep>();
            parents.Add(_firstParent);
            parents.Add(_secondParent);
            string name = "TargetName";
            _target = new JoinEntitiesAuthorizationStep(parents, name, JoinStrategyOption.Union);
            Assert.IsNotNull(_target);
            Assert.AreEqual(name, _target.PanelName);
        }

        [Test]
        public void VerifyConstructorOverload2()
        {
            IList<IAuthorizationStep> parents = new List<IAuthorizationStep>();
            parents.Add(_firstParent);
            parents.Add(_secondParent);
            string name = "TargetName";
            string desc = "TargetDescription";
            _target = new JoinEntitiesAuthorizationStep(parents, name, JoinStrategyOption.Union, desc);
            Assert.IsNotNull(_target);
            Assert.AreEqual(name, _target.PanelName);
            Assert.AreEqual(desc, _target.PanelDescription);
        }

        [Test]
        public void VerifyRefreshList()
        {
            _target.RefreshList();
            IList<AuthorizationEntity> resultList = _target.ProvidedList<AuthorizationEntity>();
            Assert.AreEqual(3, resultList.Count);

            _target.JoinStrategy = JoinStrategyOption.Subtract;
            _target.RefreshList();
            resultList = _target.ProvidedList<AuthorizationEntity>();
            Assert.AreEqual(1, resultList.Count);

        }

        [Test]
        public void VerifyRefreshListWithNullParentProvidedListResult()
        {
            _secondParent.SetRefreshOwnListResult(null);
            _target.RefreshList();
            Assert.IsNull(_target.InnerException);
            Assert.AreEqual(_target.ProvidedList<AuthorizationEntity>().Count, _firstParent.ProvidedList<AuthorizationEntity>().Count);
        }

    }
}
