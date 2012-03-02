using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;

namespace Teleopti.Ccc.DomainTest.Forecasting.ForecastsFile
{
    [TestFixture]
    public class ForecastsFileValidatorTest
    {
        private IForecastsFileValidator _target;

        [Test]
        public void ShouldValidateValidSkillName()
        {
            _target = new ForecastsFileSkillNameValidator();

            var result = _target.Validate("Insurance");

            Assert.That(result, Is.True);
            Assert.That(_target.ErrorMessage, Is.Null);
        }

        [Test]
        public void ShouldValidateInvalidSkillName()
        {
            _target = new ForecastsFileSkillNameValidator();

            var result = _target.Validate("");

            Assert.That(result, Is.False);
            Assert.That(_target.ErrorMessage, Is.Not.Null);
        }

        [Test]
        public void ShouldValidateInvalidSkillNameExceedsMaxLength()
        {
            _target = new ForecastsFileSkillNameValidator();

            var result = _target.Validate("InsuranceInsuranceInsuranceInsuranceInsuranceInsurance");

            Assert.That(result, Is.False);
            Assert.That(_target.ErrorMessage, Is.Not.Null);
        }

        [Test]
        public void ShouldValidateValidDateTime()
        {
            _target = new ForecastsFileDateTimeValidator();

            var result = _target.Validate("20110907 18:15");

            Assert.That(result, Is.True);
            Assert.That(_target.ErrorMessage, Is.Null);
        }
        
        [Test]
        public void ShouldValidateInvalidDateTime()
        {
            _target = new ForecastsFileDateTimeValidator();

            var result = _target.Validate("2011090718:15");

            Assert.That(result, Is.False);
            Assert.That(_target.ErrorMessage, Is.Not.Null);

            result = _target.Validate("20110907 25:15");

            Assert.That(result, Is.False);
            Assert.That(_target.ErrorMessage, Is.Not.Null);

            result = _target.Validate("110907 25:15");

            Assert.That(result, Is.False);
            Assert.That(_target.ErrorMessage, Is.Not.Null);
        }

        [Test]
        public void ShouldValidateValidIntegerValue()
        {
            _target = new ForecastsFileIntegerValueValidator();

            var result = _target.Validate("1000");

            Assert.That(result, Is.True);
            Assert.That(_target.ErrorMessage, Is.Null);
        } 
        
        [Test]
        public void ShouldValidateInvalidIntegerValue()
        {
            _target = new ForecastsFileIntegerValueValidator();

            var result = _target.Validate("1000.1");

            Assert.That(result, Is.False);
            Assert.That(_target.ErrorMessage, Is.Not.Null);
        }

        [Test]
        public void ShouldValidateValidDoubleValue()
        {
            _target = new ForecastsFileDoubleValueValidator();

            var result = _target.Validate("1000.1");

            Assert.That(result, Is.True);
            Assert.That(_target.ErrorMessage, Is.Null);
        } 
        
        [Test]
        public void ShouldValidateInvalidDoubleValue()
        {
            _target = new ForecastsFileDoubleValueValidator();

            var result = _target.Validate("100,1");

            Assert.That(result, Is.False);
            Assert.That(_target.ErrorMessage, Is.Not.Null);
        }
    }
}
