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
	        target = new ValidatedRequest
	        {
		        IsValid = true,
		        ValidationErrors = string.Empty
	        };
        }

        [Test]
        public void VerifyThatThePropertiesAreSetOk()
        {
            target.IsValid = false;
            target.ValidationErrors = "Invalid";
            
            Assert.IsFalse(target.IsValid);
			Assert.That(target.ValidationErrors, Is.Not.Null.Or.Empty);

        }
    }
}
