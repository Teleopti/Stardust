using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.ImportForecast
{
    [TestFixture]
    public class ImportForecastModelTest
    {
        private IImportForecastModel _target;
        private MockRepository _mocks;
        private ISkill _skill;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IImportForecastsRepository _importForecastsRepository;
        
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _skill = SkillFactory.CreateSkillWithWorkloadAndSources();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _importForecastsRepository = _mocks.StrictMock<IImportForecastsRepository>();
            _target = new ImportForecastModel(_skill, _unitOfWorkFactory, _importForecastsRepository, new ForecastsRowExtractor());
        }

        [Test]
        public void ShouldGetSkillName()
        {
            Assert.That(_target.GetSelectedSkillName(), Is.EqualTo("TestSkill"));   
        }

        [Test]
        public void ShouldLoadWorkload()
        {
            Assert.That(_target.LoadWorkload(), Is.EqualTo(_skill.WorkloadCollection.FirstOrDefault()));
        }

        [Test]
        public void ShouldSaveFileToServer()
        {
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            var forecastFile = _mocks.StrictMock<IForecastFile>();
            var fileId = Guid.NewGuid();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(() => _importForecastsRepository.Add(forecastFile));
                Expect.Call(forecastFile.Id).Return(fileId);
                Expect.Call(unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
            }
            using (_mocks.Playback())
            {
                Assert.That(_target.SaveValidatedForecastFile(forecastFile), Is.EqualTo(fileId));
            }
        }

        [Test]
        public void ShouldValidateValidFile()
        {
            const string file = "Insurance,20120301 12:45,20120301 13:00,17,179,0,4.05";
            var fileContent = Encoding.UTF8.GetBytes(file);
            var streamReader = new StreamReader(new MemoryStream(fileContent));

            _target.ValidateFile(streamReader);
            Assert.That(_target.FileContent, Is.EqualTo(fileContent));
        }

        [Test]
        [ExpectedException(typeof(ValidationException))]
        public void ShouldValidateInvalidFile()
        {
            const string file = "Insurance,20120301 12:45,20120301 13:00,17,179";
            var fileContent = Encoding.UTF8.GetBytes(file);
            var streamReader = new StreamReader(new MemoryStream(fileContent));
            _target.ValidateFile(streamReader);
        }

        [Test]
        [ExpectedException(typeof(ValidationException))]
        public void ShouldValidateInvalidEmptyFile()
        {
            const string file = "";
            var fileContent = Encoding.UTF8.GetBytes(file);
            var streamReader = new StreamReader(new MemoryStream(fileContent));
            _target.ValidateFile(streamReader);
        }
    }
}
