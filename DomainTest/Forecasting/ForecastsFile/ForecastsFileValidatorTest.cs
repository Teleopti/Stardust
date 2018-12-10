using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.Forecasting.Import;

namespace Teleopti.Ccc.DomainTest.Forecasting.ForecastsFile
{
    [TestFixture]
    public class ForecastsFileValidatorTest
    {
        [Test]
        public void ShouldValidateValidSkillName()
        {
            var target = new ForecastsFileSkillNameValidator();

            ForecastParseResult<string> parseResult;
            var result = target.TryParse("Insurance", out parseResult);

            Assert.That(result, Is.True);
            Assert.That(parseResult.ErrorMessage, Is.Null);
        }

        [Test]
        public void ShouldValidateInvalidSkillName()
        {
            var target = new ForecastsFileSkillNameValidator();

            ForecastParseResult<string> parseResult;
            var result = target.TryParse("", out parseResult);

            Assert.That(result, Is.False);
            Assert.That(parseResult.ErrorMessage, Is.Not.Null);
        }

        [Test]
        public void ShouldValidateInvalidSkillNameExceedsMaxLength()
        {
            var target = new ForecastsFileSkillNameValidator();

            ForecastParseResult<string> parseResult;
            var result = target.TryParse("InsuranceInsuranceInsuranceInsuranceInsuranceInsurance", out parseResult);

            Assert.That(result, Is.False);
            Assert.That(parseResult.ErrorMessage, Is.Not.Null);
        }

        [Test]
        public void ShouldValidateValidDateTime()
        {
            var target = new ForecastsFileDateTimeValidator();

            ForecastParseResult<DateTime> parseResult;
            var result = target.TryParse("20110907 18:15", out parseResult);

            Assert.That(result, Is.True);
            Assert.That(parseResult.ErrorMessage, Is.Null);
        }

        [Test]
        public void ShouldValidateInvalidDateTime()
        {
            var target = new ForecastsFileDateTimeValidator();

            ForecastParseResult<DateTime> parseResult;
            var result = target.TryParse("2011090718:15", out parseResult);

            Assert.That(result, Is.False);
            Assert.That(parseResult.ErrorMessage, Is.Not.Null);

            result = target.TryParse("20110907 25:15", out parseResult);

            Assert.That(result, Is.False);
            Assert.That(parseResult.ErrorMessage, Is.Not.Null);

            result = target.TryParse("110907 25:15", out parseResult);

            Assert.That(result, Is.False);
            Assert.That(parseResult.ErrorMessage, Is.Not.Null);
        }

        [Test]
        public void ShouldValidateValidDoubleValue()
        {
            var target = new ForecastsFileDoubleValueValidator();

            ForecastParseResult<double> parseResult;
            var result = target.TryParse("1000.1", out parseResult);

            Assert.That(result, Is.True);
            Assert.That(parseResult.ErrorMessage, Is.Null);
        }

        [Test]
        public void ShouldValidateInvalidDoubleValue()
        {
            var target = new ForecastsFileDoubleValueValidator();

            ForecastParseResult<double> parseResult;
            var result = target.TryParse("100,1", out parseResult);

            Assert.That(result, Is.True);
	        parseResult.Value.Should().Be.EqualTo(100.1);
            Assert.That(parseResult.ErrorMessage, Is.Null);
        }

    }
}
