using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Accessories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Accessories
{
    /// <summary>
    /// Tests for AvailableDataContainsApplicationRoleSpecification
    /// </summary>
    [TestFixture]
    public class AvailableDataContainsApplicationRoleSpecificationTest
    {
        private AvailableDataContainsApplicationRoleSpecification _target;
        private IAvailableData _availableDataOk1;
        private IAvailableData _availableDataNotOk;
        private IAvailableData _availableDataOk2;
        private IApplicationRole _applicationRole1;
        private IApplicationRole _applicationRole2;
        private IApplicationRole _applicationRole3;
        private IApplicationRole _applicationRole4;
        private IApplicationRole _applicationRole5;
        private IList<IApplicationRole> _applicationRoleList;
        private IList<IAvailableData> _availableDataList;

        [SetUp]
        public void Setup()
        {
            _applicationRoleList = ApplicationRoleFactory.CreateShippedRoles(out _applicationRole1, out _applicationRole2, out _applicationRole3, out _applicationRole4, out _applicationRole5);
            // redefine
            _applicationRole2 = new ApplicationRole();

            _availableDataList = new List<IAvailableData>();

            _availableDataOk1 = new AvailableData();
            _availableDataOk1.ApplicationRole = _applicationRole1;
            _availableDataList.Add(_availableDataOk1);

            _availableDataNotOk = new AvailableData();
            _availableDataNotOk.ApplicationRole = _applicationRole2;
            _availableDataList.Add(_availableDataNotOk);

            _availableDataOk2 = new AvailableData();
            _availableDataOk2.ApplicationRole = _applicationRole3;
            _availableDataList.Add(_availableDataOk2);
        }

        [Test]
        public void VerifySetup()
        {
            Assert.AreEqual(3, _availableDataList.Count);
            Assert.AreEqual(5, _applicationRoleList.Count);
            Assert.AreSame(_availableDataOk1, _availableDataList[0]);
            Assert.AreSame(_availableDataNotOk, _availableDataList[1]);
            Assert.AreSame(_availableDataOk2, _availableDataList[2]);
            Assert.AreSame(_availableDataOk1.ApplicationRole, _applicationRole1);
            Assert.AreSame(_availableDataOk2.ApplicationRole, _applicationRole3);
        }

        [Test]
        public void VerifyIsSatisfy()
        {
            _target = new AvailableDataContainsApplicationRoleSpecification(_applicationRoleList);

            Assert.IsFalse(_target.IsSatisfiedBy(null));

            Assert.IsFalse(_target.IsSatisfiedBy(_availableDataNotOk));
            Assert.IsTrue(_target.IsSatisfiedBy(_availableDataOk1));
            Assert.IsTrue(_target.IsSatisfiedBy(_availableDataOk2));
        }

        [Test]
        public void VerifyFilterList()
        {
            IList<IAvailableData> filteredAvailableDataList = new List<IAvailableData>(_availableDataList)
            .FindAll(new AvailableDataContainsApplicationRoleSpecification(_applicationRoleList).IsSatisfiedBy);

            Assert.AreEqual(2, filteredAvailableDataList.Count);
            Assert.AreSame(filteredAvailableDataList[0], _availableDataOk1);
            Assert.AreSame(filteredAvailableDataList[1], _availableDataOk2);
            
        }
    }
}
