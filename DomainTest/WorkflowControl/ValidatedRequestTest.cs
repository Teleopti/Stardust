using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class ValidatedRequestTest
    {
        private IValidatedRequest target;

        [SetUp]
        public void Setup()
        {
            target = new ValidatedRequest();
            target.IsValid = true;
            target.ValidationErrors = string.Empty;
        }

        [Test]
        public void VerifyThatThePropertiesAreSetOk()
        {
            target.IsValid = false;
            target.ValidationErrors = "Invalid";
            
            Assert.IsFalse(target.IsValid);
            Assert.IsNotNullOrEmpty(target.ValidationErrors);

        }
    }
}
