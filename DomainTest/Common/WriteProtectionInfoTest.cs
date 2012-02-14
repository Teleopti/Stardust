using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class WriteProtectionInfoTest
    {
        private PersonWriteProtectionInfo _target;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _person = new Person();
            _target = new PersonWriteProtectionInfo(_person);
        }

        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.AreSame(_person, _target.BelongsTo);
            Assert.IsNull(_target.UpdatedBy);
            Assert.IsNull(_target.UpdatedOn);
            Assert.IsNull(_target.PersonWriteProtectedDate);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyDefaultConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(typeof(PersonWriteProtectionInfo)));
        }

        [Test]
        public void CanSetBelongsTo()
        {
            IPerson per = new Person();
            _target.BelongsTo = per;
            Assert.AreSame(per, _target.BelongsTo);
        }
        
        [Test]
        public void VerifyWriteProtectedUntilWhenOnlyTeam()
        {
            IWorkflowControlSet workflowControlSet = new WorkflowControlSet("sö") {WriteProtection = 12};
            _person.WorkflowControlSet = workflowControlSet;
            
            Assert.IsNull(_target.PersonWriteProtectedDate);
            Assert.AreEqual(new DateOnly(DateTime.Now.AddDays(-12)), _target.WriteProtectedUntil());
        }

        [Test]
        public void VerifyWriteProtectedUntilWhenNotSet()
        {
            IWorkflowControlSet workflowControlSet = new WorkflowControlSet("sö");
            Assert.AreEqual(new DateOnly(DateTime.Now.AddDays(-10000)), _target.WriteProtectedUntil());

            _person.WorkflowControlSet = workflowControlSet;

            Assert.IsNull(_target.PersonWriteProtectedDate);
            Assert.AreEqual(new DateOnly(DateTime.Now.AddDays(-10000)), _target.WriteProtectedUntil());
        }

        [Test]
        public void VerifyWriteProtectedTeamMostImportant()
        {
            IWorkflowControlSet workflowControlSet = new WorkflowControlSet("sö") {WriteProtection = 12};
            _person.WorkflowControlSet = workflowControlSet;

            _target.PersonWriteProtectedDate = new DateOnly(1800,1,1);

            Assert.AreEqual(new DateOnly(1800,1,1), _target.PersonWriteProtectedDate);
            Assert.AreEqual(new DateOnly(DateTime.Now.AddDays(-12)), _target.WriteProtectedUntil());
        }

        [Test]
        public void VerifyWriteProtectedPersonMostImportant()
        {
            IWorkflowControlSet workflowControlSet = new WorkflowControlSet("sö") {WriteProtection = 12};
            _person.WorkflowControlSet = workflowControlSet;

            _target.PersonWriteProtectedDate = new DateOnly(DateTime.Today.AddDays(-1));

            Assert.AreEqual(new DateOnly(DateTime.Today.AddDays(-1)), _target.WriteProtectedUntil());
        }

        [Test]
        public void VerifyIsWriteProtected()
        {
            //simple test, internaly writeprotecteduntil is used
            _target.PersonWriteProtectedDate = new DateOnly(2000,1,1);
            Assert.IsTrue(_target.IsWriteProtected(new DateOnly(1900, 1, 1)));
            Assert.IsFalse(_target.IsWriteProtected(new DateOnly(2001, 1, 1)));
        }

        [Test]
        [ExpectedException(typeof(PermissionException))]
        public void CannotSetDateIfNoPermission()
        {
            MockRepository mocks = new MockRepository();
            IPrincipalAuthorization principalAuthorization = mocks.StrictMock<IPrincipalAuthorization>();
            using(mocks.Record())
            {
                Expect.Call(principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.SetWriteProtection))
                    .Return(false);
            }
            using(mocks.Playback())
            {
                using(new CustomAuthorizationContext(principalAuthorization))
                {
                    _target.PersonWriteProtectedDate = new DateOnly(2000,1,1);
                }
            }
        }
    }
}
