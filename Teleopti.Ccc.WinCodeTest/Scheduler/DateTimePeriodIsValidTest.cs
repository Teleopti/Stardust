using System.Windows.Controls;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class ViewModelIsValidRuleTest
    {
        private DateTimePeriodViewModel _target;
        private ValidationRule _validationRule;

        [SetUp]
        public void Setup()
        {
            _target = new DateTimePeriodViewModel();
            _validationRule = new ViewModelIsValidRule();
        }


        [Test]
        public void VerifyValidator()
        {
            Assert.IsTrue(_target.IsValid,"Make sure its valid");
            Assert.IsTrue(_validationRule.Validate(_target,null).IsValid);

            //set to invalid
            _target.AutoUpdate = false;
            _target.Start = _target.End.AddDays(1);
            Assert.IsFalse(_target.IsValid,"Make sure its invalid");
            Assert.IsFalse(_validationRule.Validate(_target, null).IsValid);
        }

        [Test]
        public void VerifyValidationMessage()
        {
            _target.AutoUpdate = false;
            _target.Start = _target.End.AddDays(1);
            Assert.IsFalse(_target.IsValid, "Make sure its invalid");
            Assert.AreEqual(_target.InvalidMessage, _validationRule.Validate(_target, null).ErrorContent);
        }

        [Test]
        public void VerifyReturnValidIfNull()
        {
            Assert.IsTrue(_validationRule.Validate(null,null).IsValid);
        }

        
    }
}
