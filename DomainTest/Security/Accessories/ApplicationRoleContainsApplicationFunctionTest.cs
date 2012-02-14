using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Accessories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Accessories
{
    /// <summary>
    /// Tests for ApplicationRoleContainsApplicationFunctionSpecification
    /// </summary>
    [TestFixture]
    public class ApplicationRoleContainsApplicationFunctionTest
    {
        private ApplicationRoleContainsApplicationFunctionSpecification _target;

        private IList<IApplicationRole> _applicationRoles;

        [SetUp]
        public void Setup()
        {
            _applicationRoles = ApplicationRoleFactory.CreateShippedRoles();

        }

        [Test]
        public void VerifySetup()
        {
            Assert.AreEqual(5, _applicationRoles.Count);
        }

        [Test]
        public void VerifyIsSatisfy()
        {
            ApplicationFunction okFunction = new ApplicationFunction("OK");
            _applicationRoles[0].AddApplicationFunction(okFunction);
            _target = new ApplicationRoleContainsApplicationFunctionSpecification(okFunction);

            Assert.IsFalse(_target.IsSatisfiedBy(null));

            Assert.IsTrue(_target.IsSatisfiedBy(_applicationRoles[0]));
            Assert.IsFalse(_target.IsSatisfiedBy(_applicationRoles[1]));

        }

        [Test]
        public void VerifyFilterList()
        {
            IApplicationFunction okFunction = new ApplicationFunction("OK");
            _applicationRoles[0].AddApplicationFunction(okFunction);
            _applicationRoles[2].AddApplicationFunction(okFunction);

            IList<IApplicationRole> filteredApplicationRoleList = new List<IApplicationRole>(_applicationRoles)
            .FindAll(new ApplicationRoleContainsApplicationFunctionSpecification(okFunction).IsSatisfiedBy);

            Assert.AreEqual(2, filteredApplicationRoleList.Count);
            Assert.AreSame(_applicationRoles[0], filteredApplicationRoleList[0]);
            Assert.AreSame(_applicationRoles[2], filteredApplicationRoleList[1]);
            
        }
    }
}