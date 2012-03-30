using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Presenters;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.ImportForecast
{
    [TestFixture]
    public class ImportForecastModelTest
    {
        private ImportForecastModel _target;
        private MockRepository _mocks;
        private ISkill _skill;
        private byte[] _fileContent;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
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
