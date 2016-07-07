using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Presenters;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.ImportForecast
{
    [TestFixture]
    public class ValidateImportForecastFileCommandTest
    {
        private IValidateImportForecastFileCommand target;
        private ImportForecastModel model;

        [SetUp]
        public void Setup()
        {
            model = new ImportForecastModel{SelectedSkill = SkillFactory.CreateSkill("My Skill")};
            target = new ValidateImportForecastFileCommand(new ForecastsRowExtractor(), model);
        }

        [Test]
        public void ShouldHandleEmptyFile()
        {
            target.Execute("Forecasting\\ImportForecast\\EmptyFile.txt");

            model.HasValidationError.Should().Be.True();
            model.ValidationMessage.Should().Not.Be.Empty();
            model.FileContent.Should().Be.Null();
        }

        [Test]
        public void ShouldHandleValidFile()
        {
            target.Execute("Forecasting\\ImportForecast\\ValidFile.txt");

            model.HasValidationError.Should().Be.False();
            model.ValidationMessage.Should().Be.Empty();
            model.FileContent.Should().Not.Be.Null();
        }

        [Test]
        public void ShouldHandleInvalidFile()
        {
            target.Execute("Forecasting\\ImportForecast\\InvalidFile.txt");

            model.HasValidationError.Should().Be.True();
            model.ValidationMessage.Should().Not.Be.Empty();
            model.FileContent.Should().Be.Null();
        }
    }

}

