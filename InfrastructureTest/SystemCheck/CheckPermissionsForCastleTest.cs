using System;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.SystemCheck;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.InfrastructureTest.SystemCheck
{
    public class CheckPermissionsForCastleTest
    {
        private CheckPermissionsForCastle target;

        [SetUp]
        public void Setup()
        {
            target = new CheckPermissionsForCastle();
            CheckPermissionsForCastle.Reset();
        }

        [Test]
        public void VerifyWarningText()
        {
            Assert.AreEqual(Resources.CheckPermissionsForCastle, target.WarningText);
        }

        [Test]
        public void VerifySufficientPermissions()
        {
            Assert.IsTrue(target.IsRunningOk());
        }

        [Test]
        public void VerifyInsufficientPermissions()
        {
            //not nice but...
            target = new insufficentPermission();
            Assert.IsFalse(target.IsRunningOk());
        }

        [Test]
        public void VerifyInternalState()
        {
            target = new countCheckCalls();
            Assert.IsTrue(target.IsRunningOk());
            Assert.IsTrue(target.IsRunningOk());
            Assert.IsTrue(target.IsRunningOk());
            Assert.IsTrue(target.IsRunningOk());
            Assert.AreEqual(1, ((countCheckCalls)target).NumberOfChecks);
        }

        private class insufficentPermission : CheckPermissionsForCastle
        {
            protected override void TryCreateTempPerson()
            {
                throw new ArgumentException("just to simulate castle perm problem");
            }
        }

        private class countCheckCalls : CheckPermissionsForCastle
        {


            protected override void TryCreateTempPerson()
            {
                NumberOfChecks++;
            }

            public int NumberOfChecks { get; private set; }
        }

    }

}