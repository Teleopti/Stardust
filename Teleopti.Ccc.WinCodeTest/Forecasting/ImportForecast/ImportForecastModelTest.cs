using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.ImportForecast
{
    [TestFixture]
    public class ImportForecastModelTest
    {
        private ImportForecastModel _target;
        private ISkill _skill;
        private byte[] _fileContent;

        [SetUp]
        public void Setup()
        {
            _skill = _skill = SkillFactory.CreateSkillWithWorkloadAndSources();
            _fileContent = new byte[200];
            _target = new ImportForecastModel();
            _target.ImportMode = ImportForecastsMode.ImportWorkloadAndStaffing;
            _target.SelectedSkill = _skill;
        }

        [Test]
        public void ShouldReturnSelectedSkill()
        {
            Assert.IsNotNull(_target.SelectedSkill);   
        }

        [Test]
        public void ShouldGetFileContents()
        {
            _target.FileContent = _fileContent;
            Assert.IsNotEmpty(_fileContent);
        }

        [Test]
        public void ShouldSetImportMode()
        {
           Assert.That(_target.ImportMode, Is.EqualTo(ImportForecastsMode.ImportWorkloadAndStaffing));
        }

        [Test]
        public void ShouldGetSelectedWorkload()
        {
            Assert.That(_target.SelectedWorkload(), Is.EqualTo(_skill.WorkloadCollection.FirstOrDefault()));
        }
    }
}
