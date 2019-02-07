using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.AuthorizationEntities
{
    [TestFixture]
    public class ApplicationFunctionTest
    {
	    private ApplicationFunction _target;
        private IApplicationFunction _parent;

        [SetUp]
        public void Setup()
        {
            _parent = new ApplicationFunction("Root");
            _target = new ApplicationFunction("Function", _parent);
        }

	    [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
            Assert.AreSame(_parent, _target.Parent);
            Assert.AreEqual("Function", _target.FunctionCode);
            Assert.AreEqual("Root/Function", _target.FunctionPath);
        }

        [Test]
        public void VerifyConstructorOverload1()
        {
            _target = new ApplicationFunction();
            Assert.IsNotNull(_target);
            Assert.AreEqual("ApplicationFunction", _target.FunctionCode);
            Assert.AreEqual("ApplicationFunction", _target.FunctionPath);
        }

        [Test]
        public void VerifyConstructorOverload2()
        {
            _target = new ApplicationFunction("Function");
            Assert.IsNotNull(_target);
            Assert.IsNull(_target.Parent);
            Assert.AreEqual(_target.FunctionCode, "Function");
            Assert.AreEqual(_target.FunctionPath, "Function");
        }

        [Test]
        public void VerifyConstructorOverload3()
        {
            Assert.Throws<ArgumentException>(() => _target = new ApplicationFunction(string.Empty, _parent));
        }

        [Test]
        public void VerifyConstructorOverload4()
        {
			Assert.Throws<ArgumentNullException>(() => _target = new ApplicationFunction("Function", null));
        }

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void VerifyFindByForeignId()
        {
            var idOne = "09DB7510-ED3C-49CE-B49C-D43D94EC7263";
            var idNotExists = "2CF43425-2C18-41D8-8897-2F8BD5B60323";

            IList<IApplicationFunction> list = ApplicationFunctionFactory.CreateApplicationFunctionWithMatrixReports();
            Assert.IsNotNull(ApplicationFunction.FindByForeignId(list, DefinedForeignSourceNames.SourceMatrix, idOne));
            Assert.IsNull(ApplicationFunction.FindByForeignId(list, DefinedForeignSourceNames.SourceMatrix, idNotExists));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void VerifyFindByPath()
        {
            IList<IApplicationFunction> list = ApplicationFunctionFactory.CreateApplicationFunctionStructure();
            Assert.IsNotNull(ApplicationFunction.FindByPath(list, "Raptor/LEVEL1ITEM1/LEVEL2ITEM1"));
            Assert.IsNull(ApplicationFunction.FindByPath(list, "LEVEL2/ITEM1"));
        }

        [Test]
        public void VerifyAddChildRemoveChild()
        {

            // Declare return variable to hold property get method

            // call get method
            IList<IApplicationFunction> getChildCollection = new List<IApplicationFunction>(_parent.ChildCollection.OfType<IApplicationFunction>());

            int childNumber = getChildCollection.Count;

            ApplicationFunction newApp = new ApplicationFunction("NEW");
            _parent.AddChild(newApp);
            getChildCollection = new List<IApplicationFunction>(_parent.ChildCollection.OfType<IApplicationFunction>());

            Assert.AreSame(_parent, newApp.Parent);

            // Assert result
            // if property is reference type
            Assert.AreEqual(childNumber + 1, getChildCollection.Count);

            _parent.RemoveChild(newApp);
            getChildCollection = new List<IApplicationFunction>(_parent.ChildCollection.OfType<IApplicationFunction>());
            Assert.AreEqual(childNumber, getChildCollection.Count);
            Assert.IsNull(newApp.Parent);
        }

        [Test]
        public void VerifyRemoveApplicationFunctionFromContext()
        {
            IList<IApplicationRole> roles = new List<IApplicationRole>();
            ApplicationRole adminRole =
                ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AdministratorRole,
                                                  ShippedApplicationRoleNames.AdministratorRole);
            roles.Add(adminRole);
            ApplicationRole agentRole =
                ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole,
                                      ShippedApplicationRoleNames.AgentRole);
            roles.Add(agentRole);
            ApplicationRole businessUnitCoordinatorRole =
                ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.BusinessUnitAdministratorRole,
                          ShippedApplicationRoleNames.BusinessUnitAdministratorRole);
            roles.Add(businessUnitCoordinatorRole);

            adminRole.AddApplicationFunction(_target);
            businessUnitCoordinatorRole.AddApplicationFunction(_target);

            Assert.AreEqual(0, agentRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(1, businessUnitCoordinatorRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(1, adminRole.ApplicationFunctionCollection.Count);


            _target.RemoveApplicationRoleFromContext(roles);

            Assert.AreEqual(0, agentRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(0, businessUnitCoordinatorRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(0, adminRole.ApplicationFunctionCollection.Count);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyGetParentPath()
        {
            string path = "RAPTOR/PARENT/METHOD";
            string result = ApplicationFunction.GetParentPath(path);
            Assert.AreEqual("RAPTOR/PARENT", result);

            path = "RAPTOR";
            result = ApplicationFunction.GetParentPath(path);
            Assert.AreEqual(string.Empty, result);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyGetCode()
        {
            string path = "RAPTOR/PARENT/METHOD";
            string result = ApplicationFunction.GetCode(path);
            Assert.AreEqual("METHOD", result);

            path = "METHOD";
            result = ApplicationFunction.GetCode(path);
            Assert.AreEqual("METHOD", result);
        }
		
        [Test]
        public void VerifyBelongsToBusinessUnit()
        {
            Assert.IsFalse(_target is IFilterOnBusinessUnit);
        }

        [Test]
        public void VerifyClone()
        {
            var clonedEntity = (ApplicationFunction)_target.Clone();
            clonedEntity.FunctionCode += "Cloned";
            Assert.AreNotEqual(clonedEntity, _target);
            Assert.AreNotSame(clonedEntity, _target);
        }

        [Test]
        public void VerifyGetHashCode()
        {
            Assert.AreEqual(_target.FunctionPath.GetHashCode(), _target.GetHashCode());
        }

	    [Test]
        public void VerifyParent()
        {
            var setParent = new ApplicationFunction("NEWPARENT");

            _target.Parent = setParent;

		    var getParent = _target.Parent as ApplicationFunction;

            Assert.AreSame(setParent, getParent);

            IList<ApplicationFunction> getChildCollection = new List<ApplicationFunction>(getParent.ChildCollection.OfType<ApplicationFunction>());
            Assert.AreSame(_target, getChildCollection[0]);
        }

        [Test]
        public void VerifyChildCollection()
        {
	        IList<ApplicationFunction> getChildCollection = new List<ApplicationFunction>(_parent.ChildCollection.OfType<ApplicationFunction>());

	        Assert.AreSame(_target, getChildCollection[0]);
        }

	    [Test]
        public void VerifyFunctionCode()
        {
            string setFunctionCode = "TestCode";

            _target.FunctionCode = setFunctionCode;

            string getFunctionCode = _target.FunctionCode;

            Assert.AreEqual(setFunctionCode, getFunctionCode);
        }

        [Test]
        public void VerifyOrder()
        {
            int? orderValue = null;

            _target.SortOrder = orderValue;

            Assert.IsFalse(_target.SortOrder.HasValue);

            orderValue = 2;
            _target.SortOrder = orderValue;
            Assert.IsTrue(_target.SortOrder.HasValue);
            Assert.AreEqual(orderValue.Value, _target.SortOrder.Value);
        }

        [Test]
        public void VerifyIsPermitted()
        {
            bool? isPermitted = null;

            _target.IsPermitted = isPermitted;

            Assert.IsFalse(_target.IsPermitted.HasValue);

            isPermitted = true;
            _target.IsPermitted = isPermitted;
            Assert.IsTrue(_target.IsPermitted.HasValue);
            Assert.AreEqual(isPermitted.Value, _target.IsPermitted.Value);
        }

        [Test]
        public void VerifyIsPreliminary()
        {
            bool isPreliminary = true;
            _target.IsPreliminary = isPreliminary;
            Assert.IsTrue(_target.IsPreliminary);
            isPreliminary = false;
            _target.IsPreliminary = isPreliminary;
            Assert.IsFalse(_target.IsPreliminary);
        }

        [Test]
        public void VerifyLevel()
        {
            int getValue = _target.Level;
            Assert.AreEqual(getValue, 1);

            getValue = _parent.Level;
            Assert.AreEqual(getValue, 0);
        }

        [Test]
        public void VerifyFunctionPath()
        {
	        string expFunctionPath = "Root/Function";

            string getFunctionPath = _target.FunctionPath;

            Assert.AreEqual(expFunctionPath, getFunctionPath);
        }

        [Test]
        public void VerifyFunctionDescription()
        {
            string setFunctionDescription = "DESC";

            _target.FunctionDescription = setFunctionDescription;

            string getFunctionDescription = _target.FunctionDescription;

            Assert.AreEqual(setFunctionDescription, getFunctionDescription);
        }

        [Test]
        public void VerifyForeignId()
        {
            string setId = null;
            _target.ForeignId = setId;

            Assert.IsNull(_target.ForeignId);
            Assert.AreEqual(setId, _target.ForeignId);

            setId = "2";
            _target.ForeignId = setId;

            Assert.AreEqual(setId, _target.ForeignId);
        }

        [Test]
        public void VerifyForeignSource()
        {
            string setSource = null;
            _target.ForeignSource = setSource;

            Assert.IsNull(_target.ForeignSource);
            Assert.AreEqual(setSource, _target.ForeignSource);

            setSource = DefinedForeignSourceNames.SourceMatrix;
            _target.ForeignSource = setSource;

            Assert.AreEqual(setSource, _target.ForeignSource);
        }

        [Test]
        public void VerifyLocalizedFunctionDescription()
        {
            string setFunctionDescription = "functionDescription";
            _target.FunctionDescription = setFunctionDescription;
            _target.UserTextTranslator = new FakeUserTextTranslator();
            string getFunctionDescription = _target.LocalizedFunctionDescription;
            Assert.AreEqual(setFunctionDescription, getFunctionDescription);
        }

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyEqual()
        {
            var root = new ApplicationFunction("Root");
            var child1 = new ApplicationFunction("ChildOfRoot", root);
            var child2 = new ApplicationFunction("ChildOfRoot", root);

            Assert.IsFalse(child1.Equals(null));
            Assert.IsFalse(child1.Equals(new object()));
            Assert.IsTrue(child1.Equals(child2));
        }

	    [Test]
        public void VerifyDeleted()
        {
            bool isDeleted = _target.IsDeleted;
            Assert.IsFalse(isDeleted);
            _target.SetDeleted();
            isDeleted = _target.IsDeleted;
            Assert.IsTrue(isDeleted);
        }

        [Test]
        public void VerifyUserTextTranslator()
        {
            IUserTextTranslator userTextTranslator = new UserTextTranslator();
            _target.UserTextTranslator = userTextTranslator;
            Assert.AreEqual(userTextTranslator, _target.UserTextTranslator);
        }
    }
}
